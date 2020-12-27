using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MPGApp
{
    class MpgCalendar
    {
        public class Match
        {
            public class Matchdata
            {
                public class MatchSide
                {
                    public class Event
                    {
                        public string playerId { get; set; }
                        public string type { get; set; }
                        public string time { get; set; }
                        public string lastname { get; set; }
                        public string assistPlayerId { get; set; }
                    }

                    public class Substitution
                    {
                        public string subOff { get; set; }
                        public string subOn { get; set; }
                        public string reason { get; set; }
                        public string time { get; set; }
                    }

                    public List<Event> goals { get; set; }
                    public List<Event> bookings { get; set; }
                    public List<Substitution> substitutions { get; set; }
                }

                public MatchSide home { get; set; }
                public MatchSide away { get; set; }
            }

            public class Side
            {
                public class Player
                {
                    public string lastname { get; set; }
                    public int own_goals { get; set; }
                    public int goals { get; set; }
                }

                public string id { get; set; }
                public string club { get; set; }
                public List<Player> players { get; set; }
                public string score { get; set; }
            }

            public class Quotationpregame
            {
                public float Home { get; set; }
                public float Draw { get; set; }
                public float Away { get; set; }
            }
            public string id { get; set; }
            public DateTime date { get; set; }
            public string period { get; set; }
            [JsonConverter(typeof(StringToIntConverter))]
            public int matchTime { get; set; }
            public string quotationLink { get; set; }
            public Matchdata matchData { get; set; }
            public bool definitiveRating { get; set; }
            public Side home { get; set; }
            public Side away { get; set; }
            public Quotationpregame quotationPreGame { get; set; }
        }
        public int day { get; set; }
        public List<Match> matches { get; set; }
    }
}
