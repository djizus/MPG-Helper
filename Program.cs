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

            Db = new LiteDatabase(String.Concat("MPGData.db"));

            DataRetriever dR = new DataRetriever(Db, client){Championship = championship};

            dR.GetData();

            CommonDataAnalyser cDA = new CommonDataAnalyser(Db){ Championship = championship};

            cDA.AnalyzeData();
            cDA.WriteConsoleOutput();

            var lAnalyser = new LeagueAnalyser(Db, client, "MLNHJ9T8");
            lAnalyser.GetLeaguePlayerData();

            championship.champName = "EPL";
            championship.champNb = 2;

            dR.GetData();

            cDA.AnalyzeData();
            cDA.WriteConsoleOutput();

            championship.champName = "Ligue2";
            championship.champNb = 4;

            dR.GetData();

            cDA.AnalyzeData();
            cDA.WriteConsoleOutput();

            championship.champName = "Calcio";
            championship.champNb = 5;

            dR.GetData();

            cDA.AnalyzeData();
            cDA.WriteConsoleOutput();

            Db.Dispose();
        }
    }
}
