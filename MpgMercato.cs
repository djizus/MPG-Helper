using System;
using System.Collections.Generic;

namespace MPGApp
{
    public class MpgMercato
    {
        public class Availableplayer
        {
            public string id { get; set; }
            public string lastname { get; set; }
            public string firstname { get; set; }
            public int position { get; set; }
            public int quotation { get; set; }
            public int ultraPosition { get; set; }
            public string club { get; set; }
            public string teamid { get; set; }
            public string joinDate { get; set; }
            public DateTime availableSince { get; set; }
        }

        public class Userplayer
        {
            public string id { get; set; }
            public string lastname { get; set; }
            public string firstname { get; set; }
            public int position { get; set; }
            public int quotation { get; set; }
            public int ultraPosition { get; set; }
            public string club { get; set; }
            public string teamid { get; set; }
            public string joinDate { get; set; }
            public DateTime availableSince { get; set; }
            public int pricePaid { get; set; }
            public DateTime buying_date { get; set; }
            public int status { get; set; }
        }

        public string leagueId { get; set; }
        public int turn { get; set; }
        public int season { get; set; }
        public int statusLeague { get; set; }
        public string currentUser { get; set; }
        public int championship { get; set; }
        public int statusTeam { get; set; }
        public int leagueMode { get; set; }
        public int budget { get; set; }
        public string currentTeam { get; set; }
        public List<Availableplayer> availablePlayers { get; set; }
        public List<Userplayer> userPlayers { get; set; }
    }
}
