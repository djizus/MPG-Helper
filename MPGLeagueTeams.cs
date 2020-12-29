﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MPGApp
{

    public class MPGLeagueStatus
    {
        public string leagueName { get; set; }
        public int leagueStatus { get; set; }
        public int players { get; set; }
        public int mode { get; set; }
        public int championship { get; set; }
        public bool userInLeague { get; set; }
        //public Options options { get; set; }
        public string masterLeagueId { get; set; }
        public string teamId { get; set; }
        public int teamStatus { get; set; }
    }

    public class MPGLeagueTeams
    {
        public class Teams
        {
            public class Mpg_Team
            {
                public class Player
                {
                    public string id { get; set; }
                    public string club { get; set; }
                    public string firstname { get; set; }
                    public string lastname { get; set; }
                    public int position { get; set; }
                    public int ultraPosition { get; set; }
                    public string teamid { get; set; }
                    public int price_paid { get; set; }
                }
                public string id { get; set; }
                public string name { get; set; }
                public object president { get; set; }
                public string stadium { get; set; }
                public List<Player> players { get; set; }
            }
            public List<Mpg_Team> mpg_teams { get; set; }
        }

        public class Teamsid
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        public string current_mpg_team { get; set; }
        public List<Teamsid> teamsid { get; set; }

        [JsonConverter(typeof(TeamConverter))]
        public Teams teams { get; set; }
    }

}
