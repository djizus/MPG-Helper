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
        private LiteDatabase Db { get; set; }
        private HttpClient Client { get; set; }
        public ChampData Championship { get; set; }
        public string LeagueCode { get; set; }

        List<AuPlayers> UserAvailablePlayers = new List<AuPlayers>();

        public LeagueAnalyser(LiteDatabase commonDb, HttpClient commonClient, string leagueCode)
        {
            Db = commonDb;
            Client = commonClient;
            LeagueCode = leagueCode;

            UserAvailablePlayers = new List<AuPlayers>();
        }
        public void GetLeaguePlayerData()
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(jnToken);

            string vUrl = String.Concat("https://api.monpetitgazon.com/league/", LeagueCode, "/transfer/buy");
            var response = Client.GetStringAsync(vUrl).Result;
            var mercatoData = System.Text.Json.JsonSerializer.Deserialize<MpgMercato>(response);

            //TO DO JNM Automatic naming from the championship to get the correct Colletction
            var allPayers = Db.GetCollection<PlayersWorthStats>("Ligue1PlayersCalcStats");

            var auPlayers = new List<AuPlayers>();

            foreach (var p in mercatoData.userPlayers)
            {
                var cPlayer = allPayers.FindById(p.id);
                if (null != cPlayer)
                {
                    var auPlayer = new AuPlayers(cPlayer) { Au = "User" };
                    auPlayers.Add(auPlayer);
                }
            }

            foreach (var p in mercatoData.availablePlayers)
            {
                var cPlayer = allPayers.FindById(p.id);
                if (null != cPlayer)
                {
                    var auPlayer = new AuPlayers(cPlayer) { Au = "Available" };
                    auPlayers.Add(auPlayer);
                }
            }

            UserAvailablePlayers = new List<AuPlayers>(auPlayers.OrderByDescending(x => x.CalcWorth));

            var auBd = Db.GetCollection<AuPlayers>(String.Concat(LeagueCode, "UserAvailablePlayers"));
            auBd.DeleteAll();
            auBd.InsertBulk(UserAvailablePlayers);

            //var oK = UserAvailablePlayers.Where(x => (1 == x.Position));
            //var oD = UserAvailablePlayers.Where(x => (2 == x.Position));
            //var oM = UserAvailablePlayers.Where(x => (3 == x.Position));
            //var oS = UserAvailablePlayers.Where(x => (4 == x.Position));
        }
    }
}
