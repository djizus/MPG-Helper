using System.Text.Json.Serialization;

namespace MPGApp
{
    public class MpgChampionshipPlayers
    {
        public class Stats
        {
            [JsonConverter(typeof(StringToDoubleConverter))]
            public double avgRate { get; set; }
            public int sumGoals { get; set; }
            public int currentChampionship { get; set; }
            public double percentageStarter { get; set; }
        }
        public string id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public int position { get; set; }
        public int ultraPosition { get; set; }
        public int teamId { get; set; }
        public int quotation { get; set; }
        public string club { get; set; }
        public Stats stats { get; set; }
    }

}
