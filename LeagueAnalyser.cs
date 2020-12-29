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
        public const String jnToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6Im1wZ191c2VyXzU0MzQ2MSIsImNoZWNrIjoiNmY5NDdiYWY3MmY2ZDE3YyIsImlhdCI6MTYwNDc1MjIyOH0.JZIMpUpwo37eQWY2NCm70vrCUndPb9hcIoU8sryqGh4";
        private LiteDatabase Db { get; set; }
        private HttpClient Client { get; set; }
        public ChampData Championship { get; set; }
        public String LeagueCode { get; set; }

        private MPGLeagueTeams allTeams;

        private MPGLeagueTeams GetAllTeams()
        {
            return allTeams;
        }

        private void SetAllTeams(MPGLeagueTeams value)
        {
            allTeams = value;
        }

        private MPGLeagueStatus leagueStatus;

        private MPGLeagueStatus GetLeagueStatus()
        {
            return leagueStatus;
        }

        private void SetLeagueStatus(MPGLeagueStatus value)
        {
            leagueStatus = value;
        }

        private MpgMercato leagueMercato;

        private MpgMercato GetLeagueMercato()
        {
            return leagueMercato;
        }

        private void SetLeagueMercato(MpgMercato value)
        {
            leagueMercato = value;
        }

        private List<AuPlayers> UserAvailablePlayers { get; set; }

        public LeagueAnalyser(LiteDatabase commonDb, HttpClient commonClient, String leagueCode)
        {
            Db = commonDb;
            Client = commonClient;
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(jnToken);
            LeagueCode = leagueCode;

            UserAvailablePlayers = new List<AuPlayers>();
        }

        public void InitializeLeagueData()
        {
            var vUrl = String.Concat("https://api.monpetitgazon.com/league/", LeagueCode, "/status");
            var response = Client.GetStringAsync(vUrl).Result;
            SetLeagueStatus(System.Text.Json.JsonSerializer.Deserialize<MPGLeagueStatus>(response));

            vUrl = String.Concat("https://api.monpetitgazon.com/league/", LeagueCode, "/teams");
            response = Client.GetStringAsync(vUrl).Result;
            SetAllTeams(System.Text.Json.JsonSerializer.Deserialize<MPGLeagueTeams>(response));

            if (leagueStatus.mode > 1)
            {
                vUrl = String.Concat("https://api.monpetitgazon.com/league/", LeagueCode, "/transfer/buy");
                response = Client.GetStringAsync(vUrl).Result;
                SetLeagueMercato(System.Text.Json.JsonSerializer.Deserialize<MpgMercato>(response));
            }
        }
        public void GetLeaguePlayerData()
        {
            InitializeLeagueData();

            var allPayers = Db.GetCollection<PlayersTimeStats>(String.Concat(Championship.champName, "PlayersCalcStats"));

            var auPlayers = new List<AuPlayers>();

            if (null != leagueMercato)
            {
                foreach (var p in leagueMercato.userPlayers)
                {
                    var cPlayer = allPayers.FindById(p.id);
                    if (null != cPlayer)
                    {
                        var auPlayer = new AuPlayers(cPlayer) { Au = "User" };
                        auPlayers.Add(auPlayer);
                    }
                }

                foreach (var p in leagueMercato.availablePlayers)
                {
                    var cPlayer = allPayers.FindById(p.id);
                    if (null != cPlayer)
                    {
                        var auPlayer = new AuPlayers(cPlayer) { Au = "Available" };
                        auPlayers.Add(auPlayer);
                    }
                }

                UserAvailablePlayers = new List<AuPlayers>(auPlayers.OrderByDescending(x => x.CurrentSeasonStats.OurRating));

                var auBd = Db.GetCollection<AuPlayers>(String.Concat(LeagueCode, "MercatoPlayers"));
                auBd.DeleteAll();
                auBd.InsertBulk(UserAvailablePlayers);
            }

            foreach (var t in allTeams.teams.mpg_teams)
            {
                foreach (var p in t.players)
                {
                    var cPlayer = allPayers.FindById(p.id);
                }
            }

            //var oK = UserAvailablePlayers.Where(x => (1 == x.Position));
            //var oD = UserAvailablePlayers.Where(x => (2 == x.Position));
            //var oM = UserAvailablePlayers.Where(x => (3 == x.Position));
            //var oS = UserAvailablePlayers.Where(x => (4 == x.Position));
        }
    }
}
