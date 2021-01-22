using System;
using System.Collections.Generic;

namespace MPGApp
{
    class MpgCoach
    {
        public class Data
        {
            public class TeamSide
            {
                public class Jersey
                {
                    public class Zones
                    {
                        public string z1 { get; set; }
                    }

                    public int id { get; set; }
                    public int sponsor { get; set; }
                    public Zones zones { get; set; }
                }

                public string id { get; set; }
                public string userId { get; set; }
                public string name { get; set; }
                public string abbr { get; set; }
                public string coach { get; set; }
                public Jersey jersey { get; set; }
                public string jerseyHome { get; set; }
                public string jerseyAway { get; set; }
                public string jerseyUrl { get; set; }
            }

            public class Bonus
            {
                public int _1 { get; set; }
                public int _2 { get; set; }
                public int _3 { get; set; }
                public int _4 { get; set; }
                public int _5 { get; set; }
                public int _6 { get; set; }
                public int _7 { get; set; }
            }

            public class Playersonpitch
            {
                public string _1 { get; set; }
                public string _2 { get; set; }
                public string _3 { get; set; }
                public string _4 { get; set; }
                public string _5 { get; set; }
                public string _6 { get; set; }
                public string _7 { get; set; }
                public string _8 { get; set; }
                public string _9 { get; set; }
                public string _10 { get; set; }
                public string _11 { get; set; }
                public string _12 { get; set; }
                public string _13 { get; set; }
                public string _14 { get; set; }
                public string _15 { get; set; }
                public string _16 { get; set; }
                public string _17 { get; set; }
                public string _18 { get; set; }
            }

            public class Bonusselected
            {
            }
            public class Tds
            {
                public class TdsPlayer
                {
                    public int pending { get; set; }
                    public int reported { get; set; }
                    public int injured { get; set; }
                    public int suspended { get; set; }
                }

                public DateTime update { get; set; }
                public Dictionary<string, TdsPlayer> players { get; set; }
            }

            public int realday { get; set; }
            public long dateMatch { get; set; }
            public int timetogame { get; set; }
            public int champid { get; set; }
            public string matchId { get; set; }
            public string stadium { get; set; }
            public TeamSide teamHome { get; set; }
            public TeamSide teamAway { get; set; }
            public int nbPlayers { get; set; }
            public Bonus bonus { get; set; }
            public int nanardpaid { get; set; }
            public int money { get; set; }
            public bool fridayBrief { get; set; }
            public object[] tacticalsubstitutes { get; set; }
            public int composition { get; set; }
            public Playersonpitch playersOnPitch { get; set; }
            public Bonusselected bonusSelected { get; set; }
            public Tds tds { get; set; }
        }
    }
}
