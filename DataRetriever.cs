using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace MPGApp
{
    public class ChampData
    {
        public string currentSeason;
        public string champName;
        public int champNb;

        public ChampData()
        {
            currentSeason = "2020";
            champName = "Ligue1";
            champNb = 1;
        }
    }

    public class DataRetriever
    {
        private LiteDatabase Db { get; set; }
        private HttpClient Client { get; set; }
        public ChampData Championship { get; set; }

        public DataRetriever(LiteDatabase commonDb, HttpClient commonHttpClient)
        {
            Db = commonDb;
            Client = commonHttpClient;
        }
        public void GetData()
        {
            var cData = Db.GetCollection<MpgChampionshipPlayers>(String.Concat(Championship.champName, "Players"));

            if (0 == cData.Count())
            {
                Console.WriteLine("Get " + Championship.champName + " Players ... Start");
                string vUrl = String.Concat("https://api.monpetitgazon.com/stats/championship/", Championship.champNb, "/", Championship.currentSeason);
                var response = Client.GetStringAsync(vUrl).Result;
                var jsonData = System.Text.Json.JsonSerializer.Deserialize<List<MpgChampionshipPlayers>>(response);
                cData.InsertBulk(jsonData);
                Console.WriteLine("Get " + Championship.champName + " Players ... Done");
            }

            //cData.EnsureIndex(x => x.id);

            var dData = Db.GetCollection<MpgDetailedPlayersData>(String.Concat(Championship.champName, "DetailedPlayersData"));

            if (0 == dData.Count())
            {
                Console.WriteLine("Get " + Championship.champName + " Players Detailed Stats ... Start");
                var tot = cData.Count();
                Console.WriteLine("{0} Players to get   ", tot);
                List<MpgDetailedPlayersData> dPlayersData = new List<MpgDetailedPlayersData>();

                int count = 0;
                foreach (var p in cData.FindAll())
                {
                    Console.Write("\r{0}%   ", (double)++count / tot * 100d);
                    if (0 != p.stats.avgRate)
                    {
                        var Id = p.id.Split('_');
                        string vUrl = String.Concat("https://api.monpetitgazon.com/stats/player/", Id.Last(), "?season=", Championship.currentSeason);
                        var response = Client.GetStringAsync(vUrl).Result;
                        var jsonData = System.Text.Json.JsonSerializer.Deserialize<MpgDetailedPlayersData>(response);
                        dPlayersData.Add(jsonData);
                        //foreach(var s in jsonData.availableSeasons)
                        //{
                        //    if("2020" != s)
                        //    {
                        //        vUrl = String.Concat("https://api.monpetitgazon.com/stats/player/", Id.Last(), "?season=", s);
                        //        response = client.GetStringAsync(vUrl).Result;
                        //        jsonData = System.Text.Json.JsonSerializer.Deserialize<MpgDetailedPlayersData>(response);
                        //        dPlayersData.Add(jsonData);
                        //    }
                        //}
                    }
                }

                dData.InsertBulk(dPlayersData);
                Console.WriteLine("Get " + Championship.champName + " Players Detailed Stats ... Done");
            }

            var calendarData = Db.GetCollection<MpgCalendar>(String.Concat(Championship.champName, "Calendar"));

            if (0 == calendarData.Count())
            {
                Console.WriteLine("Get " + Championship.champName + " Calendar ... Start");
                string vUrl = String.Concat("https://api.monpetitgazon.com/championship/", Championship.champNb, "/calendar");
                var response = Client.GetStringAsync(vUrl).Result;
                var jsonData = System.Text.Json.JsonSerializer.Deserialize<MpgCalendar>(response);
                calendarData.Insert(jsonData);

                Console.WriteLine("{0} Match Day to get   ", jsonData.day - 1);
                for (var i = jsonData.day - 1; i > 0; i--)
                {
                    string dUrl = String.Concat(vUrl, "/", i);
                    response = Client.GetStringAsync(dUrl).Result;
                    jsonData = System.Text.Json.JsonSerializer.Deserialize<MpgCalendar>(response);
                    calendarData.Insert(jsonData);
                }

                Console.WriteLine("Get " + Championship.champName + " Calendar ... Done");
            }
        }
    }
}
