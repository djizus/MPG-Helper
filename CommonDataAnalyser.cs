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
        public List<PlayersWorthStats> AllPlayers { get; set; }
        public List<PlayersWorthStats> PlayersToCheck { get; set; }

        public CommonDataAnalyser(LiteDatabase commonDb)
        {
            Db = commonDb;
            AllPlayers = new List<PlayersWorthStats>();
            PlayersToCheck = new List<PlayersWorthStats>();
        }
        public void AnalyzeData()
        {
            var dData = Db.GetCollection<MpgDetailedPlayersData>(String.Concat(Championship.champName, "DetailedPlayersData"));
            var allPlayers = dData.FindAll();
            var currentDayMatch = allPlayers.First().nbMatch;

            var calData = Db.GetCollection<MpgCalendar>(String.Concat(Championship.champName, "Calendar"));
            var allCal = calData.FindAll();

            List<PlayersWorthStats> aP = new List<PlayersWorthStats>();
            List<PlayersWorthStats> playToChk = new List<PlayersWorthStats>();

            foreach (var p in allPlayers)
            {
                var avSince = p.championships[Championship.champNb.ToString()].joinDate;
                var realNbMatch = allCal.Where(x => x.matches.First().date > avSince).Count();
                //First get the player's worth Stats
                var currentPlayerStats = new PlayersWorthStats
                (
                    p.id,
                    String.Concat(p.firstname, " ", p.lastname),
                    p.quotation,
                    p.position,
                    realNbMatch,
                    p.nbMatch - p.stats.matches.First().day,
                    p.stats.percentageStarter,
                    p.stats.avgRate,
                    p.stats.sumGoals,
                    p.stats.sumRedCard,
                    p.stats.appearances.total,
                    p.stats.minutesByMatch,
                    p.stats.mistakeByMatch,
                    p.stats.shotByMatch,
                    p.stats.percentageShotOnTarget,
                    p.stats.succeedCrossByMatch,
                    p.stats.percentageCrossSuccess,
                    p.stats.percentageGoalByOpportunity,
                    p.stats.sumPenalties,
                    p.stats.percentagePenaltiesScored,
                    p.stats.sumGoalAssist,
                    p.stats.sumBigChanceCreated,
                    p.stats.sumBigChanceMissed,
                    p.stats.sumCleanSheet,
                    p.stats.percentageSaveShot,
                    p.stats.sumSaves,
                    p.stats.sumDeflect,
                    p.stats.sumPenaltySave
                );

                //Then check the player's last Results
                var matchList = p.stats.matches;
                int consecutivePlay = 0;
                int goals = 0;
                int minP = 0;
                int assists = 0;
                int chancesC = 0;
                List<double> avgrate = new List<double>();

                foreach (var m in matchList)
                {
                    var mDay = currentDayMatch - m.day;

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
                    currentPlayerStats.ConsecutivePlay = consecutivePlay;

                    if (consecutivePlay > 5)
                        currentPlayerStats.CRate = avgrate.Take(5).Average();
                    else
                        currentPlayerStats.CRate = avgrate.Average();

                    currentPlayerStats.MinPlayed = minP;
                    currentPlayerStats.CGoals = goals;
                    currentPlayerStats.CAssists = assists;
                    currentPlayerStats.CChancesCreated = chancesC;

                    if (p.quotation < 10)
                        playToChk.Add(currentPlayerStats);
                }

                aP.Add(currentPlayerStats);
            }

            AllPlayers = new List<PlayersWorthStats>(aP.OrderByDescending(x => x.CalcWorth));
            PlayersToCheck = new List<PlayersWorthStats>(playToChk.OrderByDescending(x => x.CalcWorth));


            var pData = Db.GetCollection<PlayersWorthStats>(String.Concat(Championship.champName, "PlayersToCheck"));
            pData.DeleteAll();
            pData.InsertBulk(PlayersToCheck);

            var sData = Db.GetCollection<PlayersWorthStats>(String.Concat(Championship.champName, "PlayersCalcStats"));
            sData.DeleteAll();
            sData.InsertBulk(PlayersToCheck);
        }

        public void WriteConsoleOutput()
        {
            Console.WriteLine("Best 5 players to check");
            foreach (var p in PlayersToCheck.OrderByDescending(x => x.Momentum).Take(5))
            {
                Console.WriteLine("{0} : {1}, {2}, {3}", p.Name, p.Quotation, p.Momentum, p.CalcWorth);
            }
            Console.WriteLine("");

            Console.WriteLine("Min Quotation 5 players to check");
            foreach (var p in PlayersToCheck.OrderBy(x => x.Quotation).Take(5))
            {
                Console.WriteLine("{0} : {1}, {2}, {3}", p.Name, p.Quotation, p.Momentum, p.CalcWorth);
            }
            Console.WriteLine("");

            Console.WriteLine("Best 3 Keepers");
            foreach (var p in AllPlayers.Where(x => (x.Position == 1)).Take(3))
            {
                Console.WriteLine("{0} : {1}, {2}", p.Name, p.Quotation, p.CalcWorth);
            }
            Console.WriteLine("");

            Console.WriteLine("Best 6 Defenders");
            foreach (var p in AllPlayers.Where(x => (x.Position == 2)).Take(6))
            {
                Console.WriteLine("{0} : {1}, {2}", p.Name, p.Quotation, p.CalcWorth);
            }
            Console.WriteLine("");

            Console.WriteLine("Best 6 Midfielders");
            foreach (var p in AllPlayers.Where(x => (x.Position == 3)).Take(6))
            {
                Console.WriteLine("{0} : {1}, {2}", p.Name, p.Quotation, p.CalcWorth);
            }
            Console.WriteLine("");

            Console.WriteLine("Best 6 Attackers");
            foreach (var p in AllPlayers.Where(x => (x.Position == 4)).Take(6))
            {
                Console.WriteLine("{0} : {1}, {2}", p.Name, p.Quotation, p.CalcWorth);
            }
            Console.WriteLine("");
        }
    }
}
