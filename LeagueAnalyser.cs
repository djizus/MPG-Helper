using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MPGApp
{
    public class LeagueAnalyser
    {
        private const string jnToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6Im1wZ191c2VyXzU0MzQ2MSIsImNoZWNrIjoiNmY5NDdiYWY3MmY2ZDE3YyIsImlhdCI6MTYwNDc1MjIyOH0.JZIMpUpwo37eQWY2NCm70vrCUndPb9hcIoU8sryqGh4";
        private LiteDatabase _dB { get; set; }
        private HttpClient _client { get; set; }
        public ChampData Championship { get; set; }
        public string LeagueCode { get; set; }

        private MpgLeagueTeams _allTeams;
        private MpgLeagueStatus _leagueStatus;
        private MpgMercato _leagueMercato;
        private ILiteCollection<PlayersTimeStats> _allPlayers;
        private IOrderedEnumerable<AuPlayers> _userAvailablePlayers;
        private List<LeagueTeam> _lTeams;

        public LeagueAnalyser(LiteDatabase commonDb, HttpClient commonClient, string leagueCode)
        {
            _dB = commonDb;
            _client = commonClient;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(jnToken);
            _client.DefaultRequestHeaders.Add("client-version", "6.9.1");
            LeagueCode = leagueCode;

            _lTeams = new List<LeagueTeam>();

            //_userAvailablePlayers = new List<AuPlayers>();
        }

        public void InitializeLeagueData()
        {
            var vUrl = string.Concat("https://api.monpetitgazon.com/league/", LeagueCode, "/status");
            var response = _client.GetStringAsync(vUrl).Result;
            _leagueStatus = System.Text.Json.JsonSerializer.Deserialize<MpgLeagueStatus>(response);

            if (_leagueStatus.leagueStatus == 3)
            {
                try
                {
                    vUrl = string.Concat("https://api.monpetitgazon.com/league/", LeagueCode, "/mercato");
                    response = _client.GetStringAsync(vUrl).Result;
                    _leagueMercato = System.Text.Json.JsonSerializer.Deserialize<MpgMercato>(response);
                }
                catch
                {
                    vUrl = string.Concat("https://api.monpetitgazon.com/league/", LeagueCode, "/pending_mercato");
                    response = _client.GetStringAsync(vUrl).Result;
                    _leagueMercato = System.Text.Json.JsonSerializer.Deserialize<MpgMercato>(response);
                }

            }
            else
            {
                vUrl = string.Concat("https://api.monpetitgazon.com/league/", LeagueCode, "/teams");
                response = _client.GetStringAsync(vUrl).Result;
                _allTeams = System.Text.Json.JsonSerializer.Deserialize<MpgLeagueTeams>(response);

                if (_leagueStatus.mode > 1 && _leagueStatus.userInLeague)
                {
                    vUrl = string.Concat("https://api.monpetitgazon.com/league/", LeagueCode, "/transfer/buy");
                    response = _client.GetStringAsync(vUrl).Result;
                    _leagueMercato = System.Text.Json.JsonSerializer.Deserialize<MpgMercato>(response);
                }

            }

            _allPlayers = _dB.GetCollection<PlayersTimeStats>(string.Concat(Championship.champName, "PlayersCalcStats"));
        }

        public void AnalyzeMercato()
        {
            var auPlayers = new List<AuPlayers>();
            foreach (var p in _leagueMercato.userPlayers)
            {
                var cPlayer = _allPlayers.FindById(p.id);
                if (null != cPlayer)
                {
                    var auPlayer = new AuPlayers(cPlayer) { Au = "User" };
                    auPlayers.Add(auPlayer);
                }
            }

            foreach (var p in _leagueMercato.availablePlayers)
            {
                var cPlayer = _allPlayers.FindById(p.id);
                if (null != cPlayer)
                {
                    var auPlayer = new AuPlayers(cPlayer) { Au = "Available" };
                    auPlayers.Add(auPlayer);
                }
            }

            _userAvailablePlayers = auPlayers.OrderByDescending(x => x.CurrentRating);

            if (_userAvailablePlayers.Count() > 0)
            {
                var auBd = _dB.GetCollection<AuPlayers>(string.Concat(LeagueCode, "MercatoPlayers"));
                auBd.DeleteAll();
                auBd.InsertBulk(_userAvailablePlayers);
            }

            //var oK = UserAvailablePlayers.Where(x => (1 == x.Position));
            //var oD = UserAvailablePlayers.Where(x => (2 == x.Position));
            //var oM = UserAvailablePlayers.Where(x => (3 == x.Position));
            //var oS = UserAvailablePlayers.Where(x => (4 == x.Position));
        }

        public class LeagueTeam
        {
            private IOrderedEnumerable<PlayersTimeStats> _players;
            private double _teamStrength;
            private string _bestCompo;
            private string _tName;
            private bool _isUserTeam;

            public IOrderedEnumerable<PlayersTimeStats> Players
            {
                get { return _players; }
            }

            public double TeamStrength
            {
                get { return _teamStrength; }
                set { _teamStrength = value; }
            }

            public bool IsUserTeam
            {
                get { return _isUserTeam; }
                set { _isUserTeam = value; }
            }

            public string BestCompo
            {
                get { return _bestCompo; }
            }

            public LeagueTeam(IOrderedEnumerable<PlayersTimeStats> p, string n)
            {
                _players = p;
                //_teamStrength = s;
                //_bestCompo = c;
                _tName = n;
                _isUserTeam = false;
            }

            public string GetConsoleDisplay()
            {
                var output = string.Format("{0,-40}{1,12}{2,10:F2}{3,20}{4,15:F2}",
                              _tName, _bestCompo, _teamStrength, _players.First().Name, _players.Average(x => x.CurrentRating));

                return output;
            }

            private void ComputeSrength(string c, PlayersTimeStats k)
            {
                int nbDef = int.Parse(c.Substring(0, 1));
                int nbMid = int.Parse(c.Substring(1, 1));
                int nbAtt = int.Parse(c.Substring(2, 1));

                var bestDef = _players.Where(x => 2 == x.CurrentSeasonStats.Position).Take(nbDef).Select(x => x.Clone()).ToList();
                var bestMid = _players.Where(x => 3 == x.CurrentSeasonStats.Position).Take(nbMid);
                var bestAtt = _players.Where(x => 4 == x.CurrentSeasonStats.Position).Take(nbAtt);

                if (5 == nbDef)
                {
                    //bestDef = bestDef.Select(x => x.Clone());
                    bestDef.ForEach(x => x.CurrentSeasonStats.Rate += 0.5);//bonus 1 with 5 defenders
                }
                else if (4 == nbDef)
                {
                    //bestDef = bestDef.Select(x => x.Clone());
                    bestDef.ToList().ForEach(x => x.CurrentSeasonStats.Rate += 0.25);//bonus 0.5 with 4 defenders
                }

                var tStrength = k.CurrentRating + bestDef.Sum(x => x.CurrentRating) + bestMid.Sum(x => x.CurrentRating) + bestAtt.Sum(x => x.CurrentRating);
                var nbGoals = k.CurrentSeasonStats.Goals + bestDef.Sum(x => x.CurrentSeasonStats.Goals) + bestMid.Sum(x => x.CurrentSeasonStats.Goals) + bestAtt.Sum(x => x.CurrentSeasonStats.Goals);

                var teamEval = (tStrength + 2 * nbGoals);

                if (teamEval > _teamStrength)
                {
                    _teamStrength = teamEval;
                    _bestCompo = c;
                }
            }

            public void ComputeBestTeam()
            {
                var bestKee = _players.Where(x => 1 == x.CurrentSeasonStats.Position).First();

                ComputeSrength("541", bestKee);
                ComputeSrength("532", bestKee);

                ComputeSrength("451", bestKee);
                ComputeSrength("442", bestKee);
                ComputeSrength("433", bestKee);

                ComputeSrength("352", bestKee);
                ComputeSrength("343", bestKee);
            }
        }

        private void AnalyzeLeagueTeams()
        {
            foreach (var t in _allTeams.teams.mpg_teams)
            {
                var playerList = new List<PlayersTimeStats>();

                foreach (var p in t.players)
                {
                    var pStats = _allPlayers.FindById(p.id);
                    if (null != pStats)
                        playerList.Add(pStats);
                    //else
                    //{
                    //    Console.WriteLine("WARNING");
                    //    Console.WriteLine("Player " + p.ToString() + " not found in all Stats for team " + t.name);
                    //}
                }

                var oPlayerList = playerList.OrderByDescending(x => x.CurrentRating);

                var currentTeam = new LeagueTeam(oPlayerList, t.name);
                currentTeam.ComputeBestTeam();

                if (t.id == _allTeams.current_mpg_team)
                    currentTeam.IsUserTeam = true;

                _lTeams.Add(currentTeam);
            }
        }

        public void GetLeaguePlayerData()
        {
            InitializeLeagueData();

            if (null != _leagueMercato)
                AnalyzeMercato();

            if (4 == _leagueStatus.leagueStatus)
                AnalyzeLeagueTeams();

            var tst = _lTeams.Where(x => true == x.IsUserTeam).First();

            var matchDays = _dB.GetCollection<MpgCalendar>(string.Concat(Championship.ToString(), "Calendar")).FindAll();
            var tst2 = matchDays.Where(x => x.matches.First().quotationPreGame != null).First();

            var tst3 = _dB.GetCollection<MpgChampionshipPlayers>(string.Concat(Championship.champName, "Players")).FindAll();

            var teamQuotes = new Dictionary<string, double>();

            foreach (var p in tst.Players)
            {
                var t = tst3.Where(x => x.id == p.id).First().teamId.ToString();

                if (!teamQuotes.ContainsKey(t))
                {
                    foreach (var m in tst2.matches)
                    {
                        if (m.away.id == t)
                        {
                            teamQuotes.Add(t, m.quotationPreGame.Away);
                            break;
                        }
                        else if (m.home.id == t)
                        {
                            teamQuotes.Add(t, m.quotationPreGame.Home);
                            break;
                        }
                    }
                }

                p.QuotationPreGame = teamQuotes[t];
            }

            WriteConsoleOutput();
        }

        public void WriteConsoleOutput()
        {
            Console.WriteLine("");
            Console.WriteLine("League " + _leagueStatus.leagueName + " Analysis");
            Console.WriteLine("");

            var header = string.Format("{0,-40}{1,12}{2,10}{3,20}{4,15}\n",
                  "Teams", "Best Set Up", "Strength", "Best Player", "Average Rating");
            Console.WriteLine(header);

            var oTeams = _lTeams.OrderByDescending(x => x.TeamStrength);
            foreach (var t in oTeams)
            {
                Console.WriteLine(t.GetConsoleDisplay());
            }

            var ut = oTeams.Where(x => x.IsUserTeam).First();
            var players = ut.Players.OrderByDescending(x => x.NextGameRating);

            Console.WriteLine("");
            header = string.Format("{0,-25}{1,10}{2,10}\n", "Keeper", "Rating", "Quote");
            Console.WriteLine(header);

            var k = players.Where(x => x.CurrentSeasonStats.Position == 1).First();
            var output = string.Format("{0,-25}{1,10:F2}{2,10:F2}", k.Name, k.CurrentRating, k.QuotationPreGame);
            Console.WriteLine(output);

            Console.WriteLine("");
            header = string.Format("{0,-25}{1,10}{2,10}\n", "Defenders", "Rating", "Quote");
            Console.WriteLine(header);
            var ptd = players.Where(x => x.CurrentSeasonStats.Position == 2).Take(int.Parse(ut.BestCompo.Substring(0, 1)) + 2);
            foreach (var p in ptd)
            {
                output = string.Format("{0,-25}{1,10:F2}{2,10:F2}", p.Name, p.CurrentRating, p.QuotationPreGame);
                Console.WriteLine(output);
            }

            Console.WriteLine("");
            header = string.Format("{0,-25}{1,10}{2,10}\n", "Midfielders", "Rating", "Quote");
            ptd = players.Where(x => x.CurrentSeasonStats.Position == 3).Take(int.Parse(ut.BestCompo.Substring(1, 1)) + 2);
            Console.WriteLine(header);
            foreach (var p in ptd)
            {
                output = string.Format("{0,-25}{1,10:F2}{2,10:F2}", p.Name, p.CurrentRating, p.QuotationPreGame);
                Console.WriteLine(output);
            }

            Console.WriteLine("");
            header = string.Format("{0,-25}{1,10}{2,10}\n", "Attackers", "Rating", "Quote");
            ptd = players.Where(x => x.CurrentSeasonStats.Position == 4).Take(int.Parse(ut.BestCompo.Substring(2, 1)) + 2);
            Console.WriteLine(header);
            foreach (var p in ptd)
            {
                output = string.Format("{0,-25}{1,10:F2}{2,10:F2}", p.Name, p.CurrentRating, p.QuotationPreGame);
                Console.WriteLine(output);
            }
        }
    }
}
