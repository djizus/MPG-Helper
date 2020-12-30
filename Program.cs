using LiteDB;
using System;
using System.Net.Http;

namespace MPGApp
{
    class Program
    {
        public static HttpClient client = new HttpClient();
        public static LiteDatabase Db { get; set; }

        static void Main(string[] args)
        {
            var championship = new ChampData();
            DoChamp(championship, false);

            var lAnalyser = new LeagueAnalyser(Db, client, "MLNHJ9T8") { Championship = championship };
            lAnalyser.GetLeaguePlayerData();

            lAnalyser = new LeagueAnalyser(Db, client, "MLNHJ9UC") { Championship = championship };
            lAnalyser.GetLeaguePlayerData();

            Db.Dispose();

            //championship.champName = "EPL";
            //championship.champNb = 2;
            //DoChamp(championship);

            //championship.champName = "Ligue2";
            //championship.champNb = 4;
            //DoChamp(championship);

            //championship.champName = "Calcio";
            //championship.champNb = 5;
            //DoChamp(championship);
        }

        static void DoChamp(ChampData champ, bool disposeDb = true)
        {
            Db = new LiteDatabase(string.Concat("MPG", champ.champName, "Data.db"));

            DataRetriever dR = new DataRetriever(Db, client) { Championship = champ };

            dR.GetData();

            CommonDataAnalyser cDA = new CommonDataAnalyser(Db) { Championship = champ };

            cDA.AnalyzeData();
            cDA.WriteConsoleOutput();

            if (disposeDb) Db.Dispose();
        }
    }
}
