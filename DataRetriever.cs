using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace MPGApp
{
    public class ChampData
    {
        public int currentSeason;
        public String champName;
        public int champNb;

        public ChampData()
        {
            currentSeason = 2020;
            champName = "Ligue1";
            champNb = 1;
        }

        public override String ToString()
        {
            return String.Concat(champName, currentSeason);
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

        public void setNewDb(LiteDatabase dBin)
        {
            Db = dBin;
        }
        public void GetData()
        {
            var cData = Db.GetCollection<MpgChampionshipPlayers>(String.Concat(Championship.champName, "Players"));

            if (0 == cData.Count())
            {
                Console.WriteLine("Get " + Championship.champName + " Players ... Start");
                var vUrl = String.Concat("https://api.monpetitgazon.com/stats/championship/", Championship.champNb, "/", Championship.currentSeason);
                var response = Client.GetStringAsync(vUrl).Result;
                var jsonData = System.Text.Json.JsonSerializer.Deserialize<List<MpgChampionshipPlayers>>(response);
                cData.InsertBulk(jsonData);
                Console.WriteLine("Get " + Championship.champName + " Players ... Done");
            }

            //cData.EnsureIndex(x => x.id);

            var dData = Db.GetCollection<MpgDetailedPlayersData>(String.Concat(Championship.ToString(), "DetailedPlayersData"));
            var dData1 = Db.GetCollection<MpgDetailedPlayersData>(String.Concat(Championship.champName, Championship.currentSeason - 1, "DetailedPlayersData"));
            var dData2 = Db.GetCollection<MpgDetailedPlayersData>(String.Concat(Championship.champName, Championship.currentSeason - 2, "DetailedPlayersData"));

            if (0 == dData.Count())
            {
                Console.WriteLine("Get " + Championship.champName + " Players Detailed Stats ... Start");
                var tot = cData.Count();
                Console.WriteLine("{0} Players to get   ", tot);
                List<MpgDetailedPlayersData> dPlayersData = new List<MpgDetailedPlayersData>();
                List<MpgDetailedPlayersData> dPlayersData1 = new List<MpgDetailedPlayersData>();
                List<MpgDetailedPlayersData> dPlayersData2 = new List<MpgDetailedPlayersData>();

                int count = 0;
                foreach (var p in cData.FindAll())
                {
                    Console.Write("\r{0}%   ", (double)++count / tot * 100d);
                    if (0 != p.stats.avgRate)
                    {
                        var Id = p.id.Split('_');
                        var vUrl = String.Concat("https://api.monpetitgazon.com/stats/player/", Id.Last(), "?season=", Championship.currentSeason);
                        var response = Client.GetStringAsync(vUrl).Result;
                        var jsonData = System.Text.Json.JsonSerializer.Deserialize<MpgDetailedPlayersData>(response);
                        dPlayersData.Add(jsonData);

                        if (0 == dData1.Count())
                            GetPastPlayerStats(Id.Last(), Championship.currentSeason - 1, dPlayersData1);

                        if (0 == dData2.Count())
                            GetPastPlayerStats(Id.Last(), Championship.currentSeason - 2, dPlayersData2);
                    }
                }

                dData.InsertBulk(dPlayersData);
                dData1.InsertBulk(dPlayersData1);
                dData2.InsertBulk(dPlayersData2);
                Console.WriteLine("");
                Console.WriteLine("Get " + Championship.champName + " Players Detailed Stats ... Done");
            }

            GetCurrentCalendar();
            GetPastCalendars(Championship.currentSeason - 1);
            GetPastCalendars(Championship.currentSeason - 2);
        }

        private void GetPastPlayerStats(string id, int season, List<MpgDetailedPlayersData> list)
        {
            try
            {
                var vUrl = String.Concat("https://api.monpetitgazon.com/stats/player/", id, "?season=", season);
                var response = Client.GetStringAsync(vUrl).Result;
                var jsonData = System.Text.Json.JsonSerializer.Deserialize<MpgDetailedPlayersData>(response);
                list.Add(jsonData);
            }
            catch { /*In case of error it means no Data available for that player for the specified season*/ }
        }

        private void GetCurrentCalendar()
        {
            var calendarData = Db.GetCollection<MpgCalendar>(String.Concat(Championship.ToString(), "Calendar"));

            if (0 == calendarData.Count())
            {
                Console.WriteLine("Get " + Championship.champName + " Calendar ... Start");
                var vUrl = String.Concat("https://api.monpetitgazon.com/championship/", Championship.champNb, "/calendar");

                Console.WriteLine("38 Match Day to get");
                for (var i = 38; i > 0; i--)
                {
                    Console.Write("\r{0}%   ", 100d - (i / 38d * 100d));
                    var dUrl = String.Concat(vUrl, "/", i);
                    var response = Client.GetStringAsync(dUrl).Result;
                    var jsonData = System.Text.Json.JsonSerializer.Deserialize<MpgCalendar>(response);
                    calendarData.Insert(jsonData);
                }
                Console.WriteLine("");
                Console.WriteLine("Get " + Championship.champName + " Calendar ... Done");
            }
        }

        private void GetPastCalendars(int season)
        {
            var pastSeasonId = String.Concat(Championship.champName, season);
            var cData = Db.GetCollection<MpgCalendar>(String.Concat(pastSeasonId, "Calendar"));

            if (0 == cData.Count())
            {
                var dData = Db.GetCollection<MpgDetailedPlayersData>(String.Concat(pastSeasonId, "DetailedPlayersData"));
                var allPlayers = dData.FindAll();
                var tst = allPlayers.Where(x => x.stats.matches.Length == x.nbMatch).FirstOrDefault();

                if (null != tst)
                {
                    var pastCal = new List<MpgCalendar>();
                    int i = tst.nbMatch;

                    foreach (var m in tst.stats.matches)
                    {
                        var fakeListMatch = new List<MpgCalendar.Match>();
                        var fakeCalMatch = new MpgCalendar.Match { date = m.date.DateTime };
                        fakeListMatch.Add(fakeCalMatch);
                        var calDate = new MpgCalendar { day = i, matches = fakeListMatch };
                        pastCal.Add(calDate);
                        i--;
                    }

                    cData.InsertBulk(pastCal);
                }
            }
        }
    }
}
