using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Data.Sqlite;
using SQLite;
using System.Diagnostics;
using Microsoft.VisualBasic;

namespace MPG_WPF_App
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        //Token de l'api MPG
        private string globalToken = "0";
        //Instances
        private MpgUser UserInstance = new MpgUser();
        private MpgUserDashboard UserDashboardInstance = new MpgUserDashboard();
        private MpgLeagueDashboard LeagueDashboardInstance = new MpgLeagueDashboard();
        private MpgLeagueMercato LeagueMercatoInstance = new MpgLeagueMercato();
        private MpgTransferts TransfertsInstance = new MpgTransferts();
        private MpgCoach CoachInstance = new MpgCoach();
        //Requêteurs http et bdd
        private static readonly HttpClient client = new HttpClient();
        private DbCreator dbApi = new DbCreator();
        //Nos données à exploiter et à afficher
        private Dictionary<List<MpgPlayerSeasonStats>, string> PlayerDataBySeason = new Dictionary<List<MpgPlayerSeasonStats>, string>();
        private List<PlayerData> MpgFullListPlayerData = new List<PlayerData>();
        private List<TeamData> MpgFullListTeamData = new List<TeamData>();
        private List<Match> MpgListMatches = new List<Match>();
        private List<TeamFormationData> ListTeamFormationData = new List<TeamFormationData>();
        private Image selectedImage = new Image();
        public List<PlayerData> TeamPlayersData { get; set; }
        private string bestFormation = "0";
        //Variables statiques
        private static readonly String saveFileLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "mpg_token.txt");
        private static readonly String dbFileLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MpgApi.db");
        private static IReadOnlyList<string> seasonsToSelect = new List<string>
        {
             {"2020" },
             {"2019" },
        };
        private static IReadOnlyDictionary<long?, string> Championnats = new Dictionary<long?, string>
        {
             { 1, "Ligue 1" },
             { 2, "PL" },
             { 3, "La Liga" },
             { 4, "Ligue 2" },
             { 5, "Serie A" },
             { 6, "LDC" },
        };
        private static IReadOnlyDictionary<long?, string> Modes = new Dictionary<long?, string>
        {
             { 0, "Normal" },
             { 1, "Normal" },
             { 2, "Expert" },
             { 3, "Ultra" },
        };
        private static IReadOnlyDictionary<long?, string> StatutLigue = new Dictionary<long?, string>
        {
             { 0, "Inconnu" },
             { 1, "Création de la ligue" },
             { 2, "Création de ton équipe" },
             { 3, "Mercato en cours" },
             { 4, "Saison en cours" },
        };

        public MainWindow()
        {
            InitializeComponent(); 
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            dbApi.createDbConnection();
            //On met à jour les data
            MPG_RefreshPlayerData();
            //On recupere tous les matches de la prochaine journée
            MPG_GetCalendar();
            try
            {
                // On essaie de savoir si le token est créé, et si oui on se connecte
                using (StreamReader sr = new StreamReader(saveFileLocation))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        globalToken = line;
                        MPG_GetUserInfo();
                    }
                }
            }
            catch (Exception)
            {
                // Token non créé, faut se connecter
                MPG_GetUserInfo();
            }
        }

        public class DbCreator
        {
            SqliteConnection dbConnection;
            SQLiteAsyncConnection database;
            SqliteCommand command;

            public string createDbConnection()
            {
                string strCon = string.Format("Data Source={0};", dbFileLocation);
                database = new SQLiteAsyncConnection(dbFileLocation);
                dbConnection = new SqliteConnection(strCon);
                dbConnection.Open();
                command = dbConnection.CreateCommand();
                createTable("PlayerData");
                createTable("TeamData");
                return strCon;
            }

            public async void createTable(string tablename)
            {
                if (!checkIfExist(tablename))
                {
                    await database.CreateTableAsync<PlayerData>();
                    await database.CreateTableAsync<TeamData>();
                }
            }

            public bool checkIfExist(string tableName)
            {
                command.CommandText = "SELECT name FROM sqlite_master WHERE name='" + tableName + "'";
                var result = command.ExecuteScalar();

                return result != null && result.ToString() == tableName ? true : false;
            }

            public void executeQuery(string sqlCommand)
            {
                SqliteCommand triggerCommand = dbConnection.CreateCommand();
                triggerCommand.CommandText = sqlCommand;
                triggerCommand.ExecuteNonQuery();
            }

            public bool checkIfTableContainsData(string tableName)
            {
                command.CommandText = "SELECT count(*) FROM " + tableName;
                var result = command.ExecuteScalar();

                return Convert.ToInt32(result) > 0 ? true : false;
            }

            public async void saveMpgPlayerData(List<PlayerData> mpgPlayerData)
            {
                foreach (PlayerData p in mpgPlayerData)
                {
                    await database.RunInTransactionAsync(tran =>
                    {
                        {
                            tran.InsertOrReplace(p);
                        }
                    });
                }
            }
            public async void saveMpgTeamData(List<TeamData> mpgTeamData)
            {
                foreach (TeamData p in mpgTeamData)
                {
                    await database.RunInTransactionAsync(tran =>
                    {
                        {
                            tran.InsertOrReplace(p);
                        }
                    });
                }
            }

            public async Task<IEnumerable<PlayerData>> GetMpgPlayerData()
            {
                return await database.Table<PlayerData>().ToListAsync();                
            }
        }

        private async void MPG_RefreshDetailedPlayerData()
        {
            List<PlayerDetailedData> AllDetailedPlayerData = new List<PlayerDetailedData>();

            foreach (MpgPlayerSeasonStats p in PlayerDataBySeason.Keys.First())
            {
                //This works but is a bit slow maybe think about getting several players in //
                if ("-" != p.Stats.AvgRate)
                {
                    var Id = p.Id.Split('_');
                    var vUrl = String.Concat("https://api.monpetitgazon.com/stats/player/", Id.Last(), "?season=2020");
                    var response = await client.GetAsync(vUrl);
                    string result = response.Content.ReadAsStringAsync().Result;
                    if (response.IsSuccessStatusCode)
                    {
                        AllDetailedPlayerData.Add(JsonConvert.DeserializeObject<PlayerDetailedData>(result));
                    }
                }
            }

            foreach (var p in AllDetailedPlayerData)
            {
                var nbMatch = p.NbMatch;
                Dictionary<long, double> rates = new Dictionary<long, double>();
                foreach (var m in p.Stats.Matches)
                {
                    rates.Add(m.Day, m.Info.Rate);
                }

                long nbMatchPlayed = p.Stats.Appearances.Total;
            }
        }

        private async void MPG_RefreshPlayerData()
        {
            foreach (string s in seasonsToSelect)
            {
                List<MpgPlayerSeasonStats> AllPlayerData = new List<MpgPlayerSeasonStats>();

                foreach (long l in Championnats.Keys)
                {
                    if (l != 6)
                    {
                        var vUrl = String.Concat("https://api.monpetitgazon.com/stats/championship/", l.ToString(), "/", s);
                        var response = await client.GetAsync(vUrl);
                        string result = response.Content.ReadAsStringAsync().Result;

                        if (response.IsSuccessStatusCode)
                        {
                            AllPlayerData.AddRange(JsonConvert.DeserializeObject<List<MpgPlayerSeasonStats>>(result));
                        }
                    }
                    else { } //impossible
                }
                PlayerDataBySeason.Add(AllPlayerData, s);
            }

            //algo à améliorer
            foreach (MpgPlayerSeasonStats p in PlayerDataBySeason.Keys.First())
            {
                MpgFullListPlayerData.Add(p.convertToPlayerData(1));
            }

            
            foreach (MpgPlayerSeasonStats p in PlayerDataBySeason.Keys.Last())
                    MpgFullListPlayerData.Add(p.convertToPlayerData(2));

            MpgFullListPlayerData = MpgFullListPlayerData.GroupBy(l => l.Id).Select(cl => new PlayerData
            {
                PlayerName = cl.First().PlayerName,
                Position = cl.First().Position,
                TeamId = cl.First().TeamId,
                Club = cl.First().Club,
                AvgRate = cl.Sum(c => c.AvgRate) / cl.Count(),
                SumGoals = cl.Sum(c => c.SumGoals) / cl.Count(),
                PercentageStarter = cl.Sum(c => c.PercentageStarter) / cl.Count(),
                Quotation = cl.First().Quotation,
                EV = cl.Sum(c => c.EV) / cl.Count(),
                Enchere = cl.Sum(c => c.Enchere) / cl.Count(),
                Id = cl.First().Id,
            }).ToList();

            if (globalToken != "0")
                list_Ligues.SelectedItem = list_Ligues.Items[0];
            dbApi.saveMpgPlayerData(MpgFullListPlayerData);
            MPG_RefreshTeamData();
        }

        private void MPG_RefreshTeamData()
        {
            MpgFullListTeamData = MpgFullListPlayerData.GroupBy(l => l.TeamId).Select(cl => new PlayerData
            {
                PlayerName = cl.First().PlayerName,
                Position = cl.First().Position,
                Club = cl.First().Club,
                TeamId = cl.First().TeamId,
                AvgRate = cl.Sum(c => c.AvgRate),
                SumGoals = cl.Sum(c => c.SumGoals),
                PercentageStarter = cl.Sum(c => c.PercentageStarter),
                Quotation = cl.First().Quotation,
                EV = cl.Sum(c => c.EV),
                Enchere = cl.Sum(c => c.Enchere),
                Id = cl.First().Id,
            }.convertToTeamData()).ToList();

            dbApi.saveMpgTeamData(MpgFullListTeamData);

        }

        private void btn_connexion_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                btn_connexion_Click(sender, e);
            }
        }

        private async void btn_connexion_Click(object sender, RoutedEventArgs e)
        {
            string username = tb_email.Text.Trim();
            string password = tb_password.Password.ToString().Trim();
            var vUserLogin = new MpgUserForm
            {
                email = username,
                password = password,
                language = "fr-FR"
            };
            await MPG_Connexion(vUserLogin);
        }

        private async Task MPG_Connexion(MpgUserForm vUserLogin)
        {
            var vJsonLogin = JsonConvert.SerializeObject(vUserLogin);
            var vUrl = "https://api.monpetitgazon.com/user/signIn";
            var vRequestLogin = new StringContent(vJsonLogin, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(vUrl, vRequestLogin);
            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                MpgUserToken tokenClassUser = JsonConvert.DeserializeObject<MpgUserToken>(result);

                File.WriteAllText(saveFileLocation, tokenClassUser.token);
                globalToken = tokenClassUser.token;
                MPG_GetUserInfo();
                MessageBox.Show("La connexion a été effectuée avec succès !", "Connexion", MessageBoxButton.OK);
            }
            else
            {
                MessageBox.Show("La connexion a échoué, veuillez réessayer", "Connexion", MessageBoxButton.OK);
            }
        }

        private async void MPG_GetUserInfo()
        {
            var vUrl = "https://api.monpetitgazon.com/user";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(globalToken);
            var response = await client.GetAsync(vUrl);
            string result = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                UserInstance = JsonConvert.DeserializeObject<MpgUser>(result);
                tb_email.Visibility = Visibility.Hidden;
                tb_password.Visibility = Visibility.Hidden;
                btn_connexion.Visibility = Visibility.Hidden;
                lb_User.Content = string.Concat("Bienvenue ", UserInstance.firstname, " ", UserInstance.lastname, " !");
                MPG_GetUserDashboard();
            }
            else
            {
                tb_email.Text = "Email";
                tb_password.Password = "";
                tb_email.Visibility = Visibility.Visible;
                tb_password.Visibility = Visibility.Visible;
                btn_connexion.Visibility = Visibility.Visible;
                lb_User.Content = "Login";
                MessageBox.Show("Le token a expiré, veuillez vous reconnecter svp", "Connexion", MessageBoxButton.OK);
            }
        }

        private async void MPG_GetUserDashboard()
        {
            var vUrl = "https://api.monpetitgazon.com/user/dashboard";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(globalToken);
            var response = await client.GetAsync(vUrl);
            var result = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                UserDashboardInstance = JsonConvert.DeserializeObject<MpgUserDashboard>(result);
                foreach (MpgLeagueElement element in UserDashboardInstance.Data.Leagues)
                {
                    ListBoxItem listBoxItem = new ListBoxItem();
                    if (element.LeaguesIds != null && element.LeaguesIds.Count() == 0)
                    {
                        //Nous sommes dans une ligue
                        listBoxItem.Content = element.Name;
                        listBoxItem.Name = element.League.Id;
                        list_Ligues.Items.Add(listBoxItem);
                    }
                    else if (element.LeaguesIds != null && element.LeaguesIds.Count() > 0)
                    {
                        //Nous sommes dans une division
                        listBoxItem.Content = element.Name;
                        listBoxItem.Name = element.League.Id;
                        list_Ligues.Items.Add(listBoxItem);
                    }
                    else
                    {
                        //Nouvelle ligue pas encore finalisée
                        listBoxItem.Content = element.Name;
                        listBoxItem.Name = element.Id;
                        list_Ligues.Items.Add(listBoxItem);
                    }
                }
                if (MpgFullListPlayerData.Count > 0)
                    list_Ligues.SelectedItem = list_Ligues.Items[0];
            }
            else
            {
                tb_email.Text = "Email";
                tb_password.Password = "";
                tb_email.Visibility = Visibility.Visible;
                tb_password.Visibility = Visibility.Visible;
                btn_connexion.Visibility = Visibility.Visible;
                lb_User.Content = "Login";
                MessageBox.Show("Le token a expiré, veuillez vous reconnecter svp", "Connexion", MessageBoxButton.OK);
            }
        }

        private async void MPG_GetLeagueInfo(string vLeague)
        {
            var vUrl = string.Concat("https://api.monpetitgazon.com/user/", vLeague, "/team");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(globalToken);
            var response = await client.GetAsync(vUrl);
            string result = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                LeagueDashboardInstance = JsonConvert.DeserializeObject<MpgLeagueDashboard>(result);
                lb_infoLigue3.Content = "Equipe : " + LeagueDashboardInstance.Team.Name;

                List<MpgLeagueElement> mpgLeagueElements = UserDashboardInstance.Data.Leagues.Where(p => p.Id == vLeague || (p.League != null && p.League.Id == vLeague)).ToList();
                if (mpgLeagueElements[0].Mode != null)
                {
                    Modes.TryGetValue(mpgLeagueElements[0].Mode, out string varMode);
                    Championnats.TryGetValue(mpgLeagueElements[0].Championship, out string varCham);
                    StatutLigue.TryGetValue(mpgLeagueElements[0].LeagueStatus, out string varStatus);
                    lb_infoLigue1.Content = "Championnat : " + varCham;
                    lb_infoLigue2.Content = "Mode : " + varMode;
                    lb_infoLigue4.Content = "Ligue de " + mpgLeagueElements[0].Players + " joueurs";

                    if (varMode == "Normal" && mpgLeagueElements[0].LeagueStatus != 3)
                    {
                        dg_mercato.Visibility = Visibility.Hidden;                        
                        lb_Mercato.Content = "Mercato fermé dans cette ligue, pas de bol !";
                    }
                    else
                    {
                        dg_mercato.Visibility = Visibility.Visible;
                        lb_Mercato.Content = varStatus + " :";
                        MPG_GetMercato(vLeague);
                    }
                }
                else
                {
                    Modes.TryGetValue(mpgLeagueElements[0].League.Mode, out string varMode);
                    Championnats.TryGetValue(mpgLeagueElements[0].League.Championship, out string varCham);
                    StatutLigue.TryGetValue(mpgLeagueElements[0].League.LeagueStatus, out string varStatus);
                    lb_infoLigue1.Content = "Championnat : " + varCham;
                    lb_infoLigue2.Content = "Mode : " + varMode;
                    lb_infoLigue4.Content = "Ligue de " + mpgLeagueElements[0].League.Players + " joueurs";
                    if (varMode == "Normal" && mpgLeagueElements[0].Status != 3)
                    {
                        dg_mercato.Visibility = Visibility.Hidden;
                        lb_Mercato.Content = "Mercato fermé dans cette ligue, pas de bol !";
                    }
                    else
                    {
                        dg_mercato.Visibility = Visibility.Visible;
                        lb_Mercato.Content = varStatus + " :";
                        MPG_GetMercato(vLeague);
                    }
                }
            }
            else
            {
                tb_email.Text = "Email";
                tb_password.Password = "";
                tb_email.Visibility = Visibility.Visible;
                tb_password.Visibility = Visibility.Visible;
                btn_connexion.Visibility = Visibility.Visible;
                lb_User.Content = "Login";
                MessageBox.Show("Le token a expiré, veuillez vous reconnecter svp", "Connexion", MessageBoxButton.OK);
            }
        }

        private async void MPG_GetMercato(string vLeague)
        {
            var vUrl = string.Concat("https://api.monpetitgazon.com/league/", vLeague, "/mercato");
            client.DefaultRequestHeaders.Add("client-version", "6.6.0");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(globalToken);
            var response = await client.GetAsync(vUrl);
            string result = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                //Mercato en cours !
                LeagueMercatoInstance = JsonConvert.DeserializeObject<MpgLeagueMercato>(result);
                List<MpgAvailablePlayer> AP = LeagueMercatoInstance.AvailablePlayers.ToList();
                List<PlayerData> ListPlayerDataToBind = MpgFullListPlayerData.Join(AP,
                    ap => ap.Id,
                    pd => pd.Id,
                    (ap, pd) => ap).ToList();
                dg_mercato.ItemsSource = null;
                dg_mercato.ItemsSource = ListPlayerDataToBind;
                dg_mercato.Columns.Last().IsHidden = true;
            }
            else
            {
                //On teste si on est en mode expert
                vUrl = string.Concat("https://api.monpetitgazon.com/league/", vLeague, "/transfer/buy");
                client.DefaultRequestHeaders.Add("client-version", "6.6.0");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(globalToken);
                response = await client.GetAsync(vUrl);
                result = response.Content.ReadAsStringAsync().Result;
                if (response.IsSuccessStatusCode && !result.Contains("false"))
                {
                    //Woohoo on est en mode expert lets go !
                    TransfertsInstance = JsonConvert.DeserializeObject<MpgTransferts>(result);
                    List<MpgAvailablePlayer> AP = TransfertsInstance.AvailablePlayers.Select(p => p.convertToAvailablePlayer()).ToList();
                    //on recupere nos joueurs
                    List<PlayerData> ListPlayerDataToBind = MpgFullListPlayerData.Join(AP,
                        ap => ap.Id,
                        pd => pd.Id,
                        (ap, pd) => ap).ToList();
                    dg_mercato.ItemsSource = null;
                    dg_mercato.ItemsSource = ListPlayerDataToBind;
                    dg_mercato.Columns.Last().IsHidden = true;
                }
                else
                {
                    //Pas de mercato possible mon bon monsieur !
                    dg_mercato.ItemsSource = null;
                }
            }
        }
                
        private async void MPG_GetTeam(string vLeague)
        {
            var vUrl = string.Concat("https://api.monpetitgazon.com/league/", vLeague, "/coach");
            client.DefaultRequestHeaders.Add("client-version", "6.6.0");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(globalToken);
            var response = await client.GetAsync(vUrl);
            string result = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                //on enlève le timestamp de mort
                result = result.Remove(result.IndexOf("\"tds\"") + 7,36);
                //récupération des données pour coacher
                CoachInstance = JsonConvert.DeserializeObject<MpgCoach>(result);
                //On récupère la formation et la force de l'équipe
                lb_infoFormation_lbl.Content = "Formation actuelle :";
                cbFormationMyTeam.SelectedValue = CoachInstance.Data.Composition.ToString();
                MpgCoachPlayer[] AP = CoachInstance.Data.Players;
                TeamPlayersData = MpgFullListPlayerData.Join(AP,
                    ap => ap.Id,
                    pd => pd.Playerid,
                    (ap, pd) => ap).ToList();
                //Calcul de la force de la team
                Mpg_CalculateTeamStrength();
                TeamFormationData tmpFormationData = ListTeamFormationData.First(p => p.Formation == cbFormationMyTeam.SelectedValue.ToString());
                lb_Force.Content = tmpFormationData.Force;
            }
            else
            {
                //Pas de team
                //dg_myTeam.ItemsSource = null;
                MyTeamGrid.Visibility = Visibility.Hidden;
                cbFormationMyTeam.SelectedItem = "-";
                lb_Force.Content = "-";
            }
        }


        private async void MPG_GetCalendar()
        {
            for(int i=1; i<6; ++i)
            {
                var vUrl = string.Concat("https://api.monpetitgazon.com/championship/", i, "/calendar/");
                client.DefaultRequestHeaders.Add("client-version", "6.6.0");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(globalToken);
                var response = await client.GetAsync(vUrl);
                string result = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    //récupération du calendrier
                    Calendar MPG_Calendar = JsonConvert.DeserializeObject<Calendar>(result);
                    //On teste si on est sur la prochaine journée
                    if (MPG_Calendar.Matches.Count(p => p.DefinitiveRating == true) > 1)
                    {
                        //On n'est pas sur la prochaine journée, on récupère la prochaine du coup
                        var nextUrl = string.Concat("https://api.monpetitgazon.com/championship/", i, "/calendar/", MPG_Calendar.Day + 1);
                        client.DefaultRequestHeaders.Add("client-version", "6.6.0");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(globalToken);
                        var nextResponse = await client.GetAsync(nextUrl);
                        string nextResult = nextResponse.Content.ReadAsStringAsync().Result;

                        if (nextResponse.IsSuccessStatusCode)
                        {
                            //récupération du calendrier
                            MPG_Calendar = new Calendar();
                            MPG_Calendar = JsonConvert.DeserializeObject<Calendar>(nextResult);
                            //On ajoute a notre liste de calendrier
                            MpgListMatches.AddRange(MPG_Calendar.Matches);
                        }
                        else
                        {
                            //Pas de prochaine journée dans cette ligue
                        }
                    }
                    else
                    {
                        //On ajoute a notre liste de calendrier
                        MpgListMatches.AddRange(MPG_Calendar.Matches);
                    }
                }
                else
                {
                    //Pas de calendrier
                }
            }
        }

        private void Mpg_CalculateTeamStrength()
        {
            //On nettoie tout
            ListTeamFormationData.Clear();
            if (CoachInstance.Data != null)
            {
                for (int i = 0; i < 7; ++i)
                {
                    TeamFormationData tmpFormationData = new TeamFormationData();
                    tmpFormationData.Initiate();
                    tmpFormationData.TeamPlayersData = TeamPlayersData;
                    //on recupere les joueurs indisponibles sur la journée
                    List<KeyValuePair<string, MpgTdValue>> currentIndisponibilities = CoachInstance.Data.Tds.Where(p => p.Value.Disponibilite == 1).ToList();
                    //on matche avec les joueurs de notre equipe pour ne garder que les disponibles
                    List<PlayerData> playerDispo = TeamPlayersData.Where(l => !currentIndisponibilities.Any(c => c.Key == l.Id)).ToList();
                    //Dans notre liste des matches il y a les prochaines rencontres des  5 championnats mpg - fuck la C1
                    
                    var coeff = (from ml in MpgListMatches
                                 join ltdHome in MpgFullListTeamData on ml.Home.Id.ToString() equals ltdHome.Id
                                 join ltdAway in MpgFullListTeamData on ml.Away.Id.ToString() equals ltdAway.Id
                                 select new
                                 {
                                     ClubHome = ltdHome.Id,
                                     ClubAway = ltdAway.Id,
                                     coefficientHome = (ltdHome.Force + ltdHome.Force * 5 / 100) / ltdAway.Force,
                                     coefficientAway = ltdAway.Force / (ltdHome.Force + ltdHome.Force * 5 / 100),
                                 }).ToList();
                    //on applique le coefficient de match à la valeur des joueurs
                    tmpFormationData.TeamPlayersForce = (from pd in playerDispo
                                                         join cfHome in coeff on pd.TeamId equals cfHome.ClubHome
                                                         select new PlayerData
                                                         {
                                                             PlayerName = pd.PlayerName,
                                                             Position = pd.Position,
                                                             Club = pd.Club,
                                                             TeamId = pd.TeamId,
                                                             AvgRate = pd.AvgRate,
                                                             SumGoals = pd.SumGoals,
                                                             PercentageStarter = pd.PercentageStarter,
                                                             Quotation = pd.Quotation,
                                                             EV = (long)Convert.ToDouble(Decimal.Round(pd.Enchere * cfHome.coefficientHome, 0)),
                                                             Enchere = pd.Enchere,
                                                             Id = pd.Id,
                                                         }).Union(from pd in playerDispo
                                                                  join cfAway in coeff on pd.TeamId equals cfAway.ClubAway
                                                                  select new PlayerData
                                                                  {
                                                                      PlayerName = pd.PlayerName,
                                                                      Position = pd.Position,
                                                                      Club = pd.Club,
                                                                      TeamId = pd.TeamId,
                                                                      AvgRate = pd.AvgRate,
                                                                      SumGoals = pd.SumGoals,
                                                                      PercentageStarter = pd.PercentageStarter,
                                                                      Quotation = pd.Quotation,
                                                                      EV = (long)Convert.ToDouble(Decimal.Round(pd.Enchere * cfAway.coefficientAway, 0)),
                                                                      Enchere = pd.Enchere,
                                                                      Id = pd.Id,
                                                                  }).ToList();

                    tmpFormationData.FilterAndSortPlayerByPosition();
                    switch (i)
                    {

                        case 0:
                            //541
                            tmpFormationData.Formation = "541";
                            tmpFormationData.Force = tmpFormationData.gks.Take(1).Sum(p => p.EV) +
                                tmpFormationData.defs.Take(5).Sum(p => p.EV) +
                                tmpFormationData.mfs.Take(4).Sum(p => p.EV) +
                                tmpFormationData.bts.Take(1).Sum(p => p.EV);
                            int cpt = 0;
                            for(int j = 0; j < 1; j++)
                            {
                                if(tmpFormationData.gks.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.gks.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt] = tmpFormationData.gks.ElementAtOrDefault(j).Id;
                                }                                    
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt++;                                    
                            }
                            for (int j = 0; j < 5; j++)
                            {
                                if (tmpFormationData.defs.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.defs.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt] = tmpFormationData.defs.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt++;
                            }
                            for (int j = 0; j < 4; j++)
                            {
                                if (tmpFormationData.mfs.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.mfs.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt] = tmpFormationData.mfs.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt++;
                            }
                            for (int j = 0; j < 1; j++)
                            {
                                if (tmpFormationData.bts.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.bts.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt] = tmpFormationData.bts.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt++;
                            }
                            break;

                        case 1:
                            //532
                            tmpFormationData.Formation = "532";
                            tmpFormationData.Force = tmpFormationData.gks.OrderByDescending(p => p.EV).Take(1).Sum(p => p.EV) +
                                tmpFormationData.defs.OrderByDescending(p => p.EV).Take(5).Sum(p => p.EV) +
                                tmpFormationData.mfs.OrderByDescending(p => p.EV).Take(3).Sum(p => p.EV) +
                                tmpFormationData.bts.OrderByDescending(p => p.EV).Take(2).Sum(p => p.EV);
                            int cpt2 = 0;
                            for (int j = 0; j < 1; j++)
                            {
                                if (tmpFormationData.gks.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.gks.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt2] = tmpFormationData.gks.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt2++;
                            }
                            for (int j = 0; j < 5; j++)
                            {
                                if (tmpFormationData.defs.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.defs.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt2] = tmpFormationData.defs.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt2++;
                            }
                            for (int j = 0; j < 3; j++)
                            {
                                if (tmpFormationData.mfs.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.mfs.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt2] = tmpFormationData.mfs.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt2++;
                            }
                            for (int j = 0; j < 2; j++)
                            {
                                if (tmpFormationData.bts.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.bts.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt2] = tmpFormationData.bts.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt2++;
                            }
                            break;

                        case 2:
                            //451
                            tmpFormationData.Formation = "451";
                            tmpFormationData.Force = tmpFormationData.gks.OrderByDescending(p => p.EV).Take(1).Sum(p => p.EV) +
                                tmpFormationData.defs.OrderByDescending(p => p.EV).Take(4).Sum(p => p.EV) +
                                tmpFormationData.mfs.OrderByDescending(p => p.EV).Take(5).Sum(p => p.EV) +
                                tmpFormationData.bts.OrderByDescending(p => p.EV).Take(1).Sum(p => p.EV);
                            int cpt3 = 0;
                            for (int j = 0; j < 1; j++)
                            {
                                if (tmpFormationData.gks.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.gks.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt3] = tmpFormationData.gks.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt3++;
                            }
                            for (int j = 0; j < 4; j++)
                            {
                                if (tmpFormationData.defs.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.defs.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt3] = tmpFormationData.defs.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt3++;
                            }
                            for (int j = 0; j < 5; j++)
                            {
                                if (tmpFormationData.mfs.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.mfs.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt3] = tmpFormationData.mfs.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt3++;
                            }
                            for (int j = 0; j < 1; j++)
                            {
                                if (tmpFormationData.bts.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.bts.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt3] = tmpFormationData.bts.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt3++;
                            }
                            break;

                        case 3:
                            //442
                            tmpFormationData.Formation = "442";
                            tmpFormationData.Force = tmpFormationData.gks.OrderByDescending(p => p.EV).Take(1).Sum(p => p.EV) +
                                tmpFormationData.defs.OrderByDescending(p => p.EV).Take(4).Sum(p => p.EV) +
                                tmpFormationData.mfs.OrderByDescending(p => p.EV).Take(4).Sum(p => p.EV) +
                                tmpFormationData.bts.OrderByDescending(p => p.EV).Take(2).Sum(p => p.EV);
                            int cpt4 = 0;
                            for (int j = 0; j < 1; j++)
                            {
                                if (tmpFormationData.gks.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.gks.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt4] = tmpFormationData.gks.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt4++;
                            }
                            for (int j = 0; j < 4; j++)
                            {
                                if (tmpFormationData.defs.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.defs.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt4] = tmpFormationData.defs.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt4++;
                            }
                            for (int j = 0; j < 4; j++)
                            {
                                if (tmpFormationData.mfs.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.mfs.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt4] = tmpFormationData.mfs.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt4++;
                            }
                            for (int j = 0; j < 2; j++)
                            {
                                if (tmpFormationData.bts.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.bts.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt4] = tmpFormationData.bts.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt4++;
                            }
                            break;

                        case 4:
                            //433
                            tmpFormationData.Formation = "433";
                            tmpFormationData.Force = tmpFormationData.gks.OrderByDescending(p => p.EV).Take(1).Sum(p => p.EV) +
                                tmpFormationData.defs.OrderByDescending(p => p.EV).Take(4).Sum(p => p.EV) +
                                tmpFormationData.mfs.OrderByDescending(p => p.EV).Take(3).Sum(p => p.EV) +
                                tmpFormationData.bts.OrderByDescending(p => p.EV).Take(3).Sum(p => p.EV);
                            int cpt5 = 0;
                            for (int j = 0; j < 1; j++)
                            {
                                if (tmpFormationData.gks.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.gks.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt5] = tmpFormationData.gks.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt5++;
                            }
                            for (int j = 0; j < 4; j++)
                            {
                                if (tmpFormationData.defs.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.defs.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt5] = tmpFormationData.defs.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt5++;
                            }
                            for (int j = 0; j < 3; j++)
                            {
                                if (tmpFormationData.mfs.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.mfs.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt5] = tmpFormationData.mfs.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt5++;
                            }
                            for (int j = 0; j < 3; j++)
                            {
                                if (tmpFormationData.bts.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.bts.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt5] = tmpFormationData.bts.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt5++;
                            }
                            break;

                        case 5:
                            //352
                            tmpFormationData.Formation = "352";
                            tmpFormationData.Force = tmpFormationData.gks.OrderByDescending(p => p.EV).Take(1).Sum(p => p.EV) +
                                tmpFormationData.defs.OrderByDescending(p => p.EV).Take(3).Sum(p => p.EV) +
                                tmpFormationData.mfs.OrderByDescending(p => p.EV).Take(5).Sum(p => p.EV) +
                                tmpFormationData.bts.OrderByDescending(p => p.EV).Take(2).Sum(p => p.EV);
                            int cpt6 = 0;
                            for (int j = 0; j < 1; j++)
                            {
                                if (tmpFormationData.gks.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.gks.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt6] = tmpFormationData.gks.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt6++;
                            }
                            for (int j = 0; j < 3; j++)
                            {
                                if (tmpFormationData.defs.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.defs.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt6] = tmpFormationData.defs.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt6++;
                            }
                            for (int j = 0; j < 5; j++)
                            {
                                if (tmpFormationData.mfs.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.mfs.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt6] = tmpFormationData.mfs.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt6++;
                            }
                            for (int j = 0; j < 2; j++)
                            {
                                if (tmpFormationData.bts.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.bts.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt6] = tmpFormationData.bts.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt6++;
                            }
                            break;

                        case 6:
                            //343
                            tmpFormationData.Formation = "343";
                            tmpFormationData.Force = tmpFormationData.gks.OrderByDescending(p => p.EV).Take(1).Sum(p => p.EV) +
                                tmpFormationData.defs.OrderByDescending(p => p.EV).Take(3).Sum(p => p.EV) +
                                tmpFormationData.mfs.OrderByDescending(p => p.EV).Take(4).Sum(p => p.EV) +
                                tmpFormationData.bts.OrderByDescending(p => p.EV).Take(3).Sum(p => p.EV);
                            int cpt7 = 0;
                            for (int j = 0; j < 1; j++)
                            {
                                if (tmpFormationData.gks.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.gks.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt7] = tmpFormationData.gks.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt7++;
                            }
                            for (int j = 0; j < 3; j++)
                            {
                                if (tmpFormationData.defs.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.defs.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt7] = tmpFormationData.defs.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt7++;
                            }
                            for (int j = 0; j < 4; j++)
                            {
                                if (tmpFormationData.mfs.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.mfs.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt7] = tmpFormationData.mfs.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt7++;
                            }
                            for (int j = 0; j < 3; j++)
                            {
                                if (tmpFormationData.bts.ElementAtOrDefault(j) != null)
                                {
                                    tmpFormationData.Best11.Add(tmpFormationData.bts.ElementAtOrDefault(j));
                                    tmpFormationData.PlayersOnPitch[cpt7] = tmpFormationData.bts.ElementAtOrDefault(j).Id;
                                }
                                else
                                    tmpFormationData.Best11.Add(new PlayerData());
                                cpt7++;
                            }
                            break;

                        default:
                            tmpFormationData.Formation = "-";
                            break;
                    }
                    tmpFormationData.SetRemplacants();
                    tmpFormationData.SetReserve();
                    ListTeamFormationData.Add(tmpFormationData);
                }
                TeamFormationData tmp = ListTeamFormationData.First(p => p.Force == ListTeamFormationData.Max(a => a.Force));
                bestFormation = tmp.Formation;               
                //Maintenant qu'on a la formation et la force remplie, on affiche nos joueurs dans la data viz !
                MPG_DrawTeamOnBoard();

            }
        }

        private void MPG_DrawTeamOnBoard()
        {
            if (ListTeamFormationData.Count > 0 && cbFormationMyTeam.SelectedValue.ToString() != "-")
            {
                TeamFormationData tmpFormationData = ListTeamFormationData.First(p => p.Formation == cbFormationMyTeam.SelectedValue.ToString());
                IMG_GK.Visibility = Visibility.Visible;
                lbl_GK.Visibility = Visibility.Visible;
                lbl_GK.Content = tmpFormationData.Best11[0].PlayerName;
                int i = 0;
                List<string> Full = new List<string> { "D1", "D2", "D3", "D2", "D4", "D5", "M1", "M2", "M3", "M4", "M5", "B1", "B2", "B3",
                "R1", "R2", "R3", "R4", "R5", "R6", "R7",
                "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "CA", "CB", "CC" };
                List<string> F541 = new List<string> { "D1", "D2", "D3", "D4", "D5", "M1", "M2", "M3", "M4", "B1" };
                List<string> F532 = new List<string> { "D1", "D2", "D3", "D4", "D5", "M1", "M2", "M3", "B1", "B2" };
                List<string> F451 = new List<string> { "D1", "D2", "D3", "D4", "M1", "M2", "M3", "M4", "M5", "B1" };
                List<string> F442 = new List<string> { "D1", "D2", "D3", "D4", "M1", "M2", "M3", "M4", "B1", "B2" };
                List<string> F433 = new List<string> { "D1", "D2", "D3", "D4", "M1", "M2", "M3", "B1", "B2", "B3" };
                List<string> F352 = new List<string> { "D1", "D2", "D3", "M1", "M2", "M3", "M4", "M5", "B1", "B2" };
                List<string> F343 = new List<string> { "D1", "D2", "D3", "M1", "M2", "M3", "M4", "B1", "B2", "B3" };
                List<string> FRemplacants = new List<string> { "R1", "R2", "R3", "R4", "R5", "R6", "R7" };
                List<string> FReserve = new List<string> { "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "CA", "CB", "CC" };
                switch (tmpFormationData.Formation)
                {

                    case "541":
                        foreach (string s in F541)
                        {
                            ++i;
                            Image tmpIMG = this.FindName("IMG_" + s) as Image;
                            Label tmpLBL = this.FindName("lbl_" + s) as Label;
                            tmpIMG.Visibility = Visibility.Visible;
                            tmpLBL.Visibility = Visibility.Visible;
                            if (i < tmpFormationData.Best11.Count())
                                tmpLBL.Content = tmpFormationData.Best11[i].PlayerName;
                        }
                        foreach (string s in Full.Except(F541).ToList())
                        {
                            Image tmpIMG = this.FindName("IMG_" + s) as Image;
                            Label tmpLBL = this.FindName("lbl_" + s) as Label;
                            tmpIMG.Visibility = Visibility.Hidden;
                            tmpLBL.Visibility = Visibility.Hidden;
                        }
                        break;

                    case "532":
                        foreach (string s in F532)
                        {
                            ++i;
                            Image tmpIMG = this.FindName("IMG_" + s) as Image;
                            Label tmpLBL = this.FindName("lbl_" + s) as Label;
                            tmpIMG.Visibility = Visibility.Visible;
                            tmpLBL.Visibility = Visibility.Visible;
                            if (i < tmpFormationData.Best11.Count())
                                tmpLBL.Content = tmpFormationData.Best11[i].PlayerName;
                        }
                        foreach (string s in Full.Except(F532).ToList())
                        {
                            Image tmpIMG = this.FindName("IMG_" + s) as Image;
                            Label tmpLBL = this.FindName("lbl_" + s) as Label;
                            tmpIMG.Visibility = Visibility.Hidden;
                            tmpLBL.Visibility = Visibility.Hidden;
                        }
                        break;

                    case "451":
                        foreach (string s in F451)
                        {
                            ++i;
                            Image tmpIMG = this.FindName("IMG_" + s) as Image;
                            Label tmpLBL = this.FindName("lbl_" + s) as Label;
                            tmpIMG.Visibility = Visibility.Visible;
                            tmpLBL.Visibility = Visibility.Visible;
                            if (i < tmpFormationData.Best11.Count())
                                tmpLBL.Content = tmpFormationData.Best11[i].PlayerName;
                        }
                        foreach (string s in Full.Except(F451).ToList())
                        {
                            Image tmpIMG = this.FindName("IMG_" + s) as Image;
                            Label tmpLBL = this.FindName("lbl_" + s) as Label;
                            tmpIMG.Visibility = Visibility.Hidden;
                            tmpLBL.Visibility = Visibility.Hidden;
                        }
                        break;

                    case "442":
                        foreach (string s in F442)
                        {
                            ++i;
                            Image tmpIMG = this.FindName("IMG_" + s) as Image;
                            Label tmpLBL = this.FindName("lbl_" + s) as Label;
                            tmpIMG.Visibility = Visibility.Visible;
                            tmpLBL.Visibility = Visibility.Visible;
                            if (i < tmpFormationData.Best11.Count())
                                tmpLBL.Content = tmpFormationData.Best11[i].PlayerName;
                        }
                        foreach (string s in Full.Except(F442).ToList())
                        {
                            Image tmpIMG = this.FindName("IMG_" + s) as Image;
                            Label tmpLBL = this.FindName("lbl_" + s) as Label;
                            tmpIMG.Visibility = Visibility.Hidden;
                            tmpLBL.Visibility = Visibility.Hidden;
                        }

                        break;

                    case "433":
                        foreach (string s in F433)
                        {
                            ++i;
                            Image tmpIMG = this.FindName("IMG_" + s) as Image;
                            Label tmpLBL = this.FindName("lbl_" + s) as Label;
                            tmpIMG.Visibility = Visibility.Visible;
                            tmpLBL.Visibility = Visibility.Visible;
                            if (i < tmpFormationData.Best11.Count())
                                tmpLBL.Content = tmpFormationData.Best11[i].PlayerName;
                        }
                        foreach (string s in Full.Except(F433).ToList())
                        {
                            Image tmpIMG = this.FindName("IMG_" + s) as Image;
                            Label tmpLBL = this.FindName("lbl_" + s) as Label;
                            tmpIMG.Visibility = Visibility.Hidden;
                            tmpLBL.Visibility = Visibility.Hidden;
                        }
                        break;

                    case "352":
                        foreach (string s in F352)
                        {
                            ++i;
                            Image tmpIMG = this.FindName("IMG_" + s) as Image;
                            Label tmpLBL = this.FindName("lbl_" + s) as Label;
                            tmpIMG.Visibility = Visibility.Visible;
                            tmpLBL.Visibility = Visibility.Visible;
                            if (i < tmpFormationData.Best11.Count())
                                tmpLBL.Content = tmpFormationData.Best11[i].PlayerName;
                        }
                        foreach (string s in Full.Except(F352).ToList())
                        {
                            Image tmpIMG = this.FindName("IMG_" + s) as Image;
                            Label tmpLBL = this.FindName("lbl_" + s) as Label;
                            tmpIMG.Visibility = Visibility.Hidden;
                            tmpLBL.Visibility = Visibility.Hidden;
                        }
                        break;

                    case "343":
                        foreach (string s in F343)
                        {
                            ++i;
                            Image tmpIMG = this.FindName("IMG_" + s) as Image;
                            Label tmpLBL = this.FindName("lbl_" + s) as Label;
                            tmpIMG.Visibility = Visibility.Visible;
                            tmpLBL.Visibility = Visibility.Visible;
                            if (i < tmpFormationData.Best11.Count())
                                if (tmpFormationData.Best11[i] != null)
                                    tmpLBL.Content = tmpFormationData.Best11[i].PlayerName;
                                else
                                    tmpLBL.Content = null;

                        }
                        foreach (string s in Full.Except(F343).ToList())
                        {
                            Image tmpIMG = this.FindName("IMG_" + s) as Image;
                            Label tmpLBL = this.FindName("lbl_" + s) as Label;
                            tmpIMG.Visibility = Visibility.Hidden;
                            tmpLBL.Visibility = Visibility.Hidden;
                        }
                        break;

                    default:
                        break;
                }
                int j = 0;
                foreach (string s in FRemplacants)
                {
                    Image tmpIMG = this.FindName("IMG_" + s) as Image;
                    Label tmpLBL = this.FindName("lbl_" + s) as Label;
                    if (j < tmpFormationData.BestSubs.Count())
                    {
                        if (tmpFormationData.BestSubs[j] != null)
                        {
                            tmpLBL.Content = tmpFormationData.BestSubs[j].PlayerName;
                            tmpIMG.Visibility = Visibility.Visible;
                            tmpLBL.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        tmpIMG.Visibility = Visibility.Hidden;
                        tmpLBL.Visibility = Visibility.Hidden;
                    }
                    ++j;
                }
                foreach (string s in FReserve)
                {
                    Image tmpIMG = this.FindName("IMG_" + s) as Image;
                    Label tmpLBL = this.FindName("lbl_" + s) as Label;
                    tmpIMG.Visibility = Visibility.Hidden;
                    tmpLBL.Visibility = Visibility.Hidden;
                }
                int k = 0;
                foreach(PlayerData pd in tmpFormationData.Reserve)
                {
                    Image tmpIMG = this.FindName("IMG_" + FReserve[k]) as Image;
                    Label tmpLBL = this.FindName("lbl_" + FReserve[k]) as Label;
                    tmpIMG.Visibility = Visibility.Visible;
                    tmpLBL.Visibility = Visibility.Visible;
                    tmpLBL.Content = pd.PlayerName;
                    k++;
                }

                Label tmplbl = this.FindName("lbl_" + Strings.Right(selectedImage.Name, 2)) as Label;
                if (selectedImage.Visibility == Visibility.Hidden)
                {
                    if (selectedImage.Name.Contains("R"))
                        selectedImage.Source = new BitmapImage(new Uri(@"/img/MaillotRem.png", UriKind.Relative));
                    else if (selectedImage.Name.Contains("C"))
                        selectedImage.Source = new BitmapImage(new Uri(@"/img/MaillotRes.png", UriKind.Relative));
                    else
                        selectedImage.Source = new BitmapImage(new Uri(@"/img/MaillotTitu.png", UriKind.Relative));
                    selectedImage = new Image();
                    dgPlayer.ItemsSource = null;
                    dgPlayer.Visibility = Visibility.Hidden;
                    dgCibles.Visibility = Visibility.Hidden;
                }
                else if (selectedImage.Name != "" && tmplbl.Content != null)
                {
                    dgPlayer.ItemsSource = null;
                    if (tmpFormationData.TeamPlayersForce.Where(p => p.PlayerName == tmplbl.Content.ToString()).ToList().Count > 0)
                        dgPlayer.ItemsSource = tmpFormationData.TeamPlayersForce.Where(p => p.PlayerName == tmplbl.Content.ToString()).ToList();
                    else
                        dgPlayer.ItemsSource = tmpFormationData.TeamPlayersData.Where(p => p.PlayerName == tmplbl.Content.ToString()).ToList();
                }
                MyTeamGrid.Visibility = Visibility.Visible;
            }
            else
            {
                MyTeamGrid.Visibility = Visibility.Hidden;
            }
        }

        private void LBL_Select_Player(object sender, MouseEventArgs e)
        {
            Label tmplbl = sender as Label;
            IMG_Select_Player(this.FindName("IMG_" + Strings.Right(tmplbl.Name, 2)), e);
        }

        private void IMG_Select_Player(object sender, MouseEventArgs e)
        {            
            IMG_Do_Select_Player(sender as Image);
        }

        private void IMG_Do_Select_Player(Image input)
        {
            if (selectedImage.Name.Contains("R"))
                selectedImage.Source = new BitmapImage(new Uri(@"/img/MaillotRem.png", UriKind.Relative));
            else if (selectedImage.Name.Contains("C"))
                selectedImage.Source = new BitmapImage(new Uri(@"/img/MaillotRes.png", UriKind.Relative));
            else
                selectedImage.Source = new BitmapImage(new Uri(@"/img/MaillotTitu.png", UriKind.Relative));

            input.Source = new BitmapImage(new Uri(@"/img/MaillotSelect.png", UriKind.Relative));
            selectedImage = input;
            Label tmplbl = this.FindName("lbl_" + Strings.Right(selectedImage.Name, 2)) as Label;
            if (tmplbl.Content != null)
            {
                TeamFormationData tmpFormationData = ListTeamFormationData.First(p => p.Formation == cbFormationMyTeam.SelectedValue.ToString());
                if (tmpFormationData.TeamPlayersForce.Where(p => p.PlayerName == tmplbl.Content.ToString()).ToList().Count > 0)
                    dgPlayer.ItemsSource = tmpFormationData.TeamPlayersForce.Where(p => p.PlayerName == tmplbl.Content.ToString()).ToList();
                else
                    dgPlayer.ItemsSource = tmpFormationData.TeamPlayersData.Where(p => p.PlayerName == tmplbl.Content.ToString()).ToList();
                dgPlayer.Visibility = Visibility.Visible;
                dgCibles.Visibility = Visibility.Visible;
            }
            else
            {
                dgPlayer.ItemsSource = null;
                dgPlayer.Visibility = Visibility.Hidden;
                dgCibles.Visibility = Visibility.Hidden;
            }
        }

        private void list_Ligues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Ici on récupère toutes les infos sur la ligue sélectionnée
            ListBoxItem listBoxItem = (ListBoxItem) list_Ligues.SelectedItem;
            //Infos Ligue
            MPG_GetLeagueInfo(listBoxItem.Name);
            //Infos Mercato
            MPG_GetMercato(listBoxItem.Name);
            //Infos Equipe
            MPG_GetTeam(listBoxItem.Name);
        }

        private async void btn_BtnMercato_L1_Click(object sender, RoutedEventArgs e)
        {
            MercatoPlayers mercatoPlayersL1 = new MercatoPlayers();
            List<mercatoPlayerPrice> tmplist = new List<mercatoPlayerPrice>
            {
                new mercatoPlayerPrice { id="player_204739", price=400 }
            };
            mercatoPlayersL1.playerPrice = tmplist;
            var vJsonMercatoL1 = JsonConvert.SerializeObject(mercatoPlayersL1);
            var vUrl = String.Concat("https://api.monpetitgazon.com/mercato/1");

            var vRequestMercatoL1 = new StringContent(vJsonMercatoL1, Encoding.UTF8, "application/json");
            //vRequestCoach.Headers.Add("Authorization", globalToken);
            var response = await client.PostAsync(vUrl, vRequestMercatoL1);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("On a bien posté le mercato, on te lance MPG pour que tu voies ça !", "Mercato L1", MessageBoxButton.OK);
                Process.Start("https://mpg.football/mercato/1");
            }
            else
            {
                MessageBox.Show("La connexion a échoué, veuillez réessayer", "Connexion", MessageBoxButton.OK);
            }
            //Process.Start("https://mpg.football/mercato/1");
        }

        private void btn_BtnMercato_L2_Click(object sender, RoutedEventArgs e)
        {
            //Process.Start("https://mpg.football/mercato/4");
        }

        private void btn_BtnMercato_PL_Click(object sender, RoutedEventArgs e)
        {
            //Process.Start("https://mpg.football/mercato/2");
        }

        private void btn_BtnMercato_SA_Click(object sender, RoutedEventArgs e)
        {
            //Process.Start("https://mpg.football/mercato/5");
        }

        private void btn_BtnMercato_Liga_Click(object sender, RoutedEventArgs e)
        {
            //Process.Start("https://mpg.football/mercato/3");
        }

        private void cbFormationMyTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CoachInstance.Data != null)
            {
                //On récupère la formation et la force de l'équipe
                lb_infoFormation_lbl.Content = "Formation choisie :";
                if (ListTeamFormationData.Count > 0 && cbFormationMyTeam.SelectedValue.ToString() != "-")
                { 
                    TeamFormationData tmpFormationData = ListTeamFormationData.First(p => p.Formation == cbFormationMyTeam.SelectedValue.ToString());
                    lb_Force.Content = tmpFormationData.Force;
                }
                else
                {
                    lb_Force.Content = 0;
                }
                //Maintenant qu'on a la formation et la force remplie, on affiche nos joueurs dans la data viz !
                MPG_DrawTeamOnBoard();
            }
            else
            {
                cbFormationMyTeam.SelectedValue = "-";
            }
        }

        private void btn_BtnSelectFormation_Click(object sender, RoutedEventArgs e)
        {
            if (CoachInstance.Data != null)
            {
                //Mpg_CalculateTeamStrength();
                lb_infoFormation_lbl.Content = "Meilleure formation : ";
                cbFormationMyTeam.SelectedValue = bestFormation;
                TeamFormationData tmpFormationData = ListTeamFormationData.First(p => p.Formation == cbFormationMyTeam.SelectedValue.ToString());
                lb_Force.Content = tmpFormationData.Force;

                //on reaffiche la compo sur le board
                MPG_DrawTeamOnBoard();
            }
        }

        private async void btn_BtnSubmitCompo_Click(object sender, RoutedEventArgs e)
        {
            if (CoachInstance.Data != null && MyTeamGrid.Visibility == Visibility.Visible)
            {
                ListBoxItem listBoxItem = (ListBoxItem)list_Ligues.SelectedItem;
                TeamFormationData tmpFormationData = ListTeamFormationData.First(p => p.Formation == cbFormationMyTeam.SelectedValue.ToString());
                MpgCoachSubmit submitComp = new MpgCoachSubmit
                {
                    bonusSelected = new bonusSelected(),
                    composition = (long)Convert.ToDouble(tmpFormationData.Formation),
                    matchId = CoachInstance.Data.MatchId,
                    playersOnPitch = new pOnPitch
                    {
                        //GK Titulaire
                        p1 = tmpFormationData.PlayersOnPitch[0],
                        p2 = tmpFormationData.PlayersOnPitch[1],
                        p3 = tmpFormationData.PlayersOnPitch[2],
                        p4 = tmpFormationData.PlayersOnPitch[3],
                        p5 = tmpFormationData.PlayersOnPitch[4],
                        p6 = tmpFormationData.PlayersOnPitch[5],
                        p7 = tmpFormationData.PlayersOnPitch[6],
                        p8 = tmpFormationData.PlayersOnPitch[7],
                        p9 = tmpFormationData.PlayersOnPitch[8],
                        p10 = tmpFormationData.PlayersOnPitch[9],
                        p11 = tmpFormationData.PlayersOnPitch[10],
                        p12 = tmpFormationData.PlayersOnPitch[11],
                        p13 = tmpFormationData.PlayersOnPitch[12],
                        p14 = tmpFormationData.PlayersOnPitch[13],
                        p15 = tmpFormationData.PlayersOnPitch[14],
                        p16 = tmpFormationData.PlayersOnPitch[15],
                        p17 = tmpFormationData.PlayersOnPitch[16],
                        //GK remplacant
                        p18 = tmpFormationData.PlayersOnPitch[17]
                    },
                    realday = CoachInstance.Data.Realday,
                    tacticalsubstitutes = new TacticalSubs[0]
                };

                var vJsonCoach = JsonConvert.SerializeObject(submitComp);
                var vUrl = String.Concat("https://api.monpetitgazon.com/league/", listBoxItem.Name, "/coach");

                var vRequestCoach = new StringContent(vJsonCoach, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(vUrl, vRequestCoach);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("On a bien posté ta compo, on te lance MPG pour que tu voies ça !", "Compo soumise", MessageBoxButton.OK);
                    Process.Start("https://mpg.football/league/" + listBoxItem.Name + "/coach");
                }
                else
                {
                    MessageBox.Show("La connexion a échoué, veuillez réessayer", "Connexion", MessageBoxButton.OK);
                }
            }
        }

    }

}
