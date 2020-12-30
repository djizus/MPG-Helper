using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MPGApp
{
    public class CommonDataAnalyser
    {
        private LiteDatabase Db { get; set; }
        public ChampData Championship { get; set; }
        public List<PlayersTimeStats> PlayersSeasonsStats { get; set; }
        public List<PlayersTimeStats> PlayersToCheck { get; set; }

        public CommonDataAnalyser(LiteDatabase commonDb)
        {
            Db = commonDb;
            PlayersSeasonsStats = new List<PlayersTimeStats>();
            PlayersToCheck = new List<PlayersTimeStats>();
        }
        public void AnalyzeData()
        {
            var dData = Db.GetCollection<MpgDetailedPlayersData>(string.Concat(Championship.ToString(), "DetailedPlayersData"));
            var allPlayers = dData.FindAll();

            AnalysePlayerStats(allPlayers);

            var pData = Db.GetCollection<PlayersTimeStats>(string.Concat(Championship.champName, "PlayersToCheck"));
            pData.DeleteAll();
            pData.InsertBulk(PlayersToCheck);

            var sData = Db.GetCollection<PlayersTimeStats>(string.Concat(Championship.champName, "PlayersCalcStats"));
            sData.DeleteAll();
            sData.InsertBulk(PlayersSeasonsStats);
        }

        private void AnalysePlayerStats(IEnumerable<MpgDetailedPlayersData> playersIn)
        {
            List<PlayersTimeStats> allPlayersStats = new List<PlayersTimeStats>();
            List<PlayersTimeStats> playersToCheck = new List<PlayersTimeStats>();

            foreach (var p in playersIn)
            {
                var pTimeStats = new PlayersTimeStats
                (
                    p.id,
                    p.firstname != null ? string.Concat(p.firstname, " ", p.lastname) : p.lastname
                );

                pTimeStats.CurrentSeasonStats = GetSeasonPlayerStats(p, Championship.currentSeason);
                pTimeStats.PastSeason1Stats = GetPastPlayerStats(p.id, Championship.currentSeason - 1);
                pTimeStats.PastSeason2Stats = GetPastPlayerStats(p.id, Championship.currentSeason - 2);

                AnalysePlayerMomentum(p, pTimeStats, playersToCheck);

                allPlayersStats.Add(pTimeStats);
            }

            PlayersSeasonsStats = new List<PlayersTimeStats>(allPlayersStats.OrderByDescending(x => x.CurrentRating));
            PlayersToCheck = new List<PlayersTimeStats>(playersToCheck.OrderByDescending(x => x.CurrentRating));
        }

        private PlayersWorthStats GetSeasonPlayerStats(MpgDetailedPlayersData p, int season)
        {
            var calData = Db.GetCollection<MpgCalendar>(string.Concat(Championship.champName, season, "Calendar"));
            var allCal = calData.FindAll();

            var avSince = p.championships[Championship.champNb.ToString()].joinDate;
            var realNbMatch = allCal.Where(x => (x.matches.First().date > avSince) && (x.matches.First().date < DateTime.Now)).Count();

            var seasonPlayerStats = new PlayersWorthStats
            (
                string.Concat(p.id, "_", season),
                p.firstname != null ? string.Concat(p.firstname, " ", p.lastname) : p.lastname,
                p.quotation,
                p.position,
                realNbMatch,
                p.nbMatch - p.stats.matches.First().day,
                p.stats.avgRate,
                p.stats.sumGoals,
                p.stats.sumRedCard,
                p.stats.appearances.total
            );

            return seasonPlayerStats;
        }

        private PlayersWorthStats GetPastPlayerStats(string pId, int season)
        {
            var pData = Db.GetCollection<MpgDetailedPlayersData>(string.Concat(Championship.champName, season, "DetailedPlayersData"));
            var p = pData.FindById(pId);

            if (null != p)
            {
                var calData = Db.GetCollection<MpgCalendar>(string.Concat(Championship.champName, season, "Calendar"));
                var allCal = calData.FindAll();

                int realNbMatch = 0;
                if (p.championships.First().Value.club == p.club)
                {
                    var avSince = p.championships.First().Value.joinDate;
                    realNbMatch = allCal.Where(x => (x.matches.First().date > avSince) && (x.matches.First().date < DateTime.Now)).Count();
                }

                if (0 == realNbMatch) realNbMatch = p.nbMatch;

                var seasonPlayerStats = new PlayersWorthStats
                (
                    string.Concat(p.id, "_", season),
                    p.firstname != null ? string.Concat(p.firstname, " ", p.lastname) : p.lastname,
                    4 == p.stats.currentChampionship ? p.quotation / 2 : p.quotation,
                    p.position,
                    realNbMatch,
                    p.nbMatch - p.stats.matches.First().day,
                    p.stats.avgRate,
                    p.stats.sumGoals,
                    p.stats.sumRedCard,
                    p.stats.appearances.total
                )
                {
                    championship = p.stats.currentChampionship
                };

                return seasonPlayerStats;
            }
            else
                return null;
        }

        private void AnalysePlayerMomentum(MpgDetailedPlayersData p, PlayersTimeStats currentPlayerStats, List<PlayersTimeStats> playToChk)
        {
            var matchList = p.stats.matches;
            int consecutivePlay = 0;
            int goals = 0;
            int minP = 0;
            int assists = 0;
            int chancesC = 0;
            List<double> avgrate = new List<double>();

            foreach (var m in matchList)
            {
                var mDay = p.nbMatch - m.day;

                if (mDay == consecutivePlay)
                {
                    consecutivePlay++;
                    goals += m.info.goals;
                    minP += m.info.minsPlayed;
                    assists += m.stats.goal_assist;
                    chancesC += m.stats.big_chance_created;
                    avgrate.Add(m.info.rate);
                }
            }

            if (consecutivePlay != 0)
            {
                currentPlayerStats.CurrentSeasonStats.ConsecutivePlay = consecutivePlay;

                if (consecutivePlay > 5)
                    currentPlayerStats.CurrentSeasonStats.CRate = avgrate.Take(5).Average();
                else
                    currentPlayerStats.CurrentSeasonStats.CRate = avgrate.Average();

                currentPlayerStats.CurrentSeasonStats.MinPlayed = minP;
                currentPlayerStats.CurrentSeasonStats.CGoals = goals;
                currentPlayerStats.CurrentSeasonStats.CAssists = assists;
                currentPlayerStats.CurrentSeasonStats.CChancesCreated = chancesC;

                if (p.quotation < 10)
                    playToChk.Add(currentPlayerStats);
            }
        }

        public void WriteConsoleOutput()
        {
            Console.WriteLine("");

            var header = string.Format("{0,-25}{1,6}{2,10}{3,12}{4,12}\n",
                  "5 players to check", "Quote", "Momentum", "Our Rating", "Season Imp");
            Console.WriteLine(header);
            foreach (var p in PlayersToCheck.OrderByDescending(x => x.CurrentSeasonStats.Momentum).Take(5))
            {
                Console.WriteLine(GetConsoleDisplay(p));
            }
            Console.WriteLine("");

            Console.WriteLine(header);
            foreach (var p in PlayersToCheck.OrderBy(x => x.CurrentSeasonStats.Quotation).Take(5))
            {
                Console.WriteLine(GetConsoleDisplay(p));
            }
            Console.WriteLine("");

            header = string.Format("{0,-25}{1,6}{2,10}{3,12}{4,12}\n",
                              "Best 5 Keepers", "Quote", "Momentum", "Our Rating", "Season Imp");
            Console.WriteLine(header);
            foreach (var p in PlayersSeasonsStats.Where(x => (x.CurrentSeasonStats.Position == 1)).Take(5))
            {
                Console.WriteLine(GetConsoleDisplay(p));
            }
            Console.WriteLine("");

            header = string.Format("{0,-25}{1,6}{2,10}{3,12}{4,12}\n",
                              "Best 10 Defenders", "Quote", "Momentum", "Our Rating", "Season Imp");
            Console.WriteLine(header);
            foreach (var p in PlayersSeasonsStats.Where(x => (x.CurrentSeasonStats.Position == 2)).Take(10))
            {
                Console.WriteLine(GetConsoleDisplay(p));
            }
            Console.WriteLine("");

            header = string.Format("{0,-25}{1,6}{2,10}{3,12}{4,12}\n",
                              "Best 10 Midfielders", "Quote", "Momentum", "Our Rating", "Season Imp");
            Console.WriteLine(header);
            foreach (var p in PlayersSeasonsStats.Where(x => (x.CurrentSeasonStats.Position == 3)).Take(10))
            {
                Console.WriteLine(GetConsoleDisplay(p));
            }
            Console.WriteLine("");

            header = string.Format("{0,-25}{1,6}{2,10}{3,12}{4,12}\n",
                              "Best 10 Attackers", "Quote", "Momentum", "Our Rating", "Season Imp");
            Console.WriteLine(header);
            foreach (var p in PlayersSeasonsStats.Where(x => (x.CurrentSeasonStats.Position == 4)).Take(10))
            {
                Console.WriteLine(GetConsoleDisplay(p));
            }
            Console.WriteLine("");
        }

        private string GetConsoleDisplay(PlayersTimeStats p)
        {
            string seasonImp = "=";
            if (p.Consistency > 10)
                seasonImp = "+++";
            else if (p.Consistency > 5)
                seasonImp = "++";
            else if (p.Consistency > 1)
                seasonImp = "+";
            else if (p.Consistency < -10)
                seasonImp = "---";
            else if (p.Consistency < -5)
                seasonImp = "--";
            else if (p.Consistency < -1 && p.Consistency < 0)
                seasonImp = "-";

            var output = string.Format("{0,-25}{1,6}{2,10:F2}{3,12:F2}{4,12}",
                          p.Name, p.CurrentSeasonStats.Quotation, p.CurrentSeasonStats.Momentum,
                          p.CurrentRating, seasonImp);

            return output;
        }
    }
}
