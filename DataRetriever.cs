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
        public string champName;
        public int champNb;
        public ChampData()
        {
            currentSeason = 2020;
            champName = "Ligue1";
            champNb = 1;
        }

        public override string ToString()
        {
            return string.Concat(champName, currentSeason);
        }
    }

    public class DataRetriever
    {
        private LiteDatabase _dB;
        private HttpClient _client;
        public ChampData Championship { get; set; }

        public DataRetriever(LiteDatabase commonDb, HttpClient commonHttpClient)
        {
            _dB = commonDb;
            _client = commonHttpClient;
        }

        public void setNewDb(LiteDatabase dBin)
        {
            _dB = dBin;
        }
        public void GetData()
        {
            var cData = _dB.GetCollection<MpgChampionshipPlayers>(string.Concat(Championship.champName, "Players"));

            var vUrl = string.Concat("https://api.monpetitgazon.com/championship/", Championship.champNb, "/calendar");
            var response = _client.GetStringAsync(vUrl).Result;
            var currentMatchDay = System.Text.Json.JsonSerializer.Deserialize<MpgCalendar>(response);

            if (0 == cData.Count())
            {
                Console.WriteLine("Get " + Championship.champName + " Players ... Start");
                vUrl = string.Concat("https://api.monpetitgazon.com/stats/championship/", Championship.champNb, "/", Championship.currentSeason);
                response = _client.GetStringAsync(vUrl).Result;
                var jsonData = System.Text.Json.JsonSerializer.Deserialize<List<MpgChampionshipPlayers>>(response);
                cData.InsertBulk(jsonData);
                Console.WriteLine("Get " + Championship.champName + " Players ... Done");
            }

            //cData.EnsureIndex(x => x.id);

            var dData = _dB.GetCollection<MpgDetailedPlayersData>(string.Concat(Championship.ToString(), "DetailedPlayersData"));
            var dData1 = _dB.GetCollection<MpgDetailedPlayersData>(string.Concat(Championship.champName, Championship.currentSeason - 1, "DetailedPlayersData"));
            var dData2 = _dB.GetCollection<MpgDetailedPlayersData>(string.Concat(Championship.champName, Championship.currentSeason - 2, "DetailedPlayersData"));

            if (0 == dData.Count())
            {
                GetDetailedPlayerStats(cData, dData, dData1, dData2);
            }
            else
            {
                if (currentMatchDay.matches.First().date > DateTime.Now)
                {
                    var firstPlayer = dData.Query().Limit(1).First();
                    if (currentMatchDay.day > firstPlayer.nbMatch)
                    {
                        dData.DeleteAll();
                        GetDetailedPlayerStats(cData, dData, dData1, dData2);
                    }
                }
            }

            GetCurrentCalendar(currentMatchDay);
            GetPastCalendars(Championship.currentSeason - 1);
            GetPastCalendars(Championship.currentSeason - 2);
        }

        private void GetDetailedPlayerStats(ILiteCollection<MpgChampionshipPlayers> cData,
                                            ILiteCollection<MpgDetailedPlayersData> dData,
                                            ILiteCollection<MpgDetailedPlayersData> dData1,
                                            ILiteCollection<MpgDetailedPlayersData> dData2)
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
                    var vUrl = string.Concat("https://api.monpetitgazon.com/stats/player/", Id.Last(), "?season=", Championship.currentSeason);
                    var response = _client.GetStringAsync(vUrl).Result;
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

        private void GetPastPlayerStats(string id, int season, List<MpgDetailedPlayersData> list)
        {
            try
            {
                var vUrl = string.Concat("https://api.monpetitgazon.com/stats/player/", id, "?season=", season);
                var response = _client.GetStringAsync(vUrl).Result;
                var jsonData = System.Text.Json.JsonSerializer.Deserialize<MpgDetailedPlayersData>(response);
                list.Add(jsonData);
            }
            catch { /*In case of error it means no Data available for that player for the specified season*/ }
        }

        private void GetCurrentCalendar(MpgCalendar currentMatchDay)
        {
            var calendarData = _dB.GetCollection<MpgCalendar>(string.Concat(Championship.ToString(), "Calendar"));

            bool updateCalendar = false;
            if(calendarData.Count() != 0)
            {
                var tst2 = calendarData.FindAll().Where(x => x.matches.First().quotationPreGame != null).First();
                if(currentMatchDay.matches.Last().definitiveRating && (tst2.day != currentMatchDay.day + 1))
                {
                    updateCalendar = true;
                    calendarData.DeleteAll();
                }
                else if((currentMatchDay.matches.First().quotationPreGame != null) && (tst2.day != currentMatchDay.day))
                {
                    updateCalendar = true;
                    calendarData.DeleteAll();
                }

            }

            if (0 == calendarData.Count() || updateCalendar)
            {
                Console.WriteLine("Get " + Championship.champName + " Calendar ... Start");
                var vUrl = string.Concat("https://api.monpetitgazon.com/championship/", Championship.champNb, "/calendar");

                var calList = new List<MpgCalendar>();

                Console.WriteLine("38 Match Day to get");
                for (var i = 38; i > 0; i--)
                {
                    Console.Write("\r{0}%   ", 100d - (i / 38d * 100d));
                    var dUrl = string.Concat(vUrl, "/", i);
                    var response = _client.GetStringAsync(dUrl).Result;
                    var jsonData = System.Text.Json.JsonSerializer.Deserialize<MpgCalendar>(response);
                    calList.Add(jsonData);
                }

                calendarData.InsertBulk(calList);
                Console.WriteLine("");
                Console.WriteLine("Get " + Championship.champName + " Calendar ... Done");
            }
        }

        private void GetPastCalendars(int season)
        {
            var pastSeasonId = string.Concat(Championship.champName, season);
            var cData = _dB.GetCollection<MpgCalendar>(string.Concat(pastSeasonId, "Calendar"));

            if (0 == cData.Count())
            {
                var dData = _dB.GetCollection<MpgDetailedPlayersData>(string.Concat(pastSeasonId, "DetailedPlayersData"));
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
