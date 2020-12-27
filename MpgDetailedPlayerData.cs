using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MPGApp
{
    public class MpgDetailedPlayersData
    {
        public class Championship
        {
            public int active { get; set; }
            public DateTimeOffset availableSince { get; set; }
            public int championshipChampionship { get; set; }
            public string club { get; set; }
            public DateTimeOffset joinDate { get; set; }
            public int quotation { get; set; }
            public string teamId { get; set; }
        }

        public class DetailedAppearances
        {
            public int total { get; set; }
            public int standBy { get; set; }
            public int standIn { get; set; }
            public int starter { get; set; }
        }

        public class LastFiveRate
        {
            public string matchId { get; set; }
            public int day { get; set; }
        }

        public class DetailedMatch
        {
            public class DetailedMatchInfo
            {
                public int goals { get; set; }
                public int sub { get; set; }
                public int minsPlayed { get; set; }
                public double rate { get; set; }
            }
            public class DetailedMatchScore
            {
                public int home { get; set; }
                public int away { get; set; }
                public int scoreHome { get; set; }
                public int scoreAway { get; set; }
            }
            public class DetailedMatchStats
            {
                public int shot_off_target { get; set; }
                public int lost_contest { get; set; }
                public int failed_cross { get; set; }
                [JsonConverter(typeof(StringToDoubleConverter))]
                public double percentage_pass { get; set; }
                public int was_fouled { get; set; }
                public int clean_sheet { get; set; }
                public int touches { get; set; }
                public int interception_won { get; set; }
                public int poss_lost_ctrl { get; set; }
                public int duel_won { get; set; }
                public int duel_lost { get; set; }
                public int fouls { get; set; }
                public int yellow_card { get; set; }
                public int red_card { get; set; }
                public int own_goals { get; set; }
                public int error_lead_to_goal { get; set; }
                public int goals_conceded { get; set; }
                public int goals { get; set; }
                public int big_chance_created { get; set; }
                public int won_contest { get; set; }
                public int goal_assist { get; set; }
                public int big_chance_missed { get; set; }
                public int att_pen_miss { get; set; }
                public int att_pen_post { get; set; }
                public int att_pen_target { get; set; }
                public int att_pen_goal { get; set; }
                public int total_scoring_att { get; set; }
                public int total_pass { get; set; }
                public int total_contest { get; set; }
                public int total_tackle { get; set; }
                public int total_back_zone_pass { get; set; }
                public int total_fwd_zone_pass { get; set; }
                public int total_int_balls { get; set; }
                public int total_cross_nocorner { get; set; }
                public int accurate_cross_nocorner { get; set; }
                public int accurate_back_zone_pass { get; set; }
                public int accurate_fwd_zone_pass { get; set; }
                public int accurate_int_balls { get; set; }
                public int accurate_pass { get; set; }

                //Specific Keeper
                public int penalty_save { get; set; }
                public int penalty_faced { get; set; }
                public int saves { get; set; }
                public int dive_save { get; set; }
                public int stand_save { get; set; }
            }

            public DetailedMatchInfo info { get; set; }
            public DetailedMatchScore score { get; set; }
            public DetailedMatchStats stats { get; set; }
            public string id { get; set; }
            public DateTimeOffset date { get; set; }
            public int day { get; set; }
        }


        public class DetailedStats
        {
            public DetailedAppearances appearances { get; set; }
            public string id { get; set; }
            public int currentChampionship { get; set; }
            public DetailedMatch[] matches { get; set; }
            public Dictionary<string, LastFiveRate> lastFiveRate { get; set; }
            public double percentageStarter { get; set; }
            [JsonConverter(typeof(StringToDoubleConverter))]
            public double avgRate { get; set; }
            public int sumGoals { get; set; }
            public int sumYellowCard { get; set; }
            public int sumRedCard { get; set; }
            public double goalByMatch { get; set; }
            public double shotByMatch { get; set; }
            public double minutesByMatch { get; set; }
            public double interceptByMatch { get; set; }
            public double tackleByMatch { get; set; }
            public double mistakeByMatch { get; set; }
            public double wonContestByMatch { get; set; }
            public double wonDuelByMatch { get; set; }
            public double lostBallByMatch { get; set; }
            public double foulsByMatch { get; set; }
            public double succeedPassByMatch { get; set; }
            public double goalsConcededByMatch { get; set; }
            public double shotOnTargetByMatch { get; set; }
            public double succeedPassBackZoneByMatch { get; set; }
            public double succeedPassFwdZoneByMatch { get; set; }
            public double succeedintPassByMatch { get; set; }
            public double succeedCrossByMatch { get; set; }
            public double foulsEnduredByMatch { get; set; }

            [JsonConverter(typeof(StringToIntConverter))]
            public int percentageAccuratePassBackZone { get; set; }

            [JsonConverter(typeof(StringToIntConverter))]
            public int percentageAccurateFwdZone { get; set; }

            [JsonConverter(typeof(StringToIntConverter))]
            public int percentageAccurateintPass { get; set; }

            [JsonConverter(typeof(StringToIntConverter))]
            public int percentageSucceedPass { get; set; }

            [JsonConverter(typeof(StringToIntConverter))]
            public int minutesByGoal { get; set; }

            [JsonConverter(typeof(StringToIntConverter))]
            public int percentageShotOnTarget { get; set; }

            [JsonConverter(typeof(StringToIntConverter))]
            public int percentageCrossSuccess { get; set; }

            [JsonConverter(typeof(StringToIntConverter))]
            public int percentageGoalByOpportunity { get; set; }

            [JsonConverter(typeof(StringToIntConverter))]
            public int percentagePenaltiesScored { get; set; }

            [JsonConverter(typeof(StringToIntConverter))]
            public int percentageWonContest { get; set; }

            [JsonConverter(typeof(StringToIntConverter))]
            public int percentageWonDuel { get; set; }

            [JsonConverter(typeof(StringToIntConverter))]
            public int percentageLostBall { get; set; }
            public int sumPenalties { get; set; }
            public int sumGoalAssist { get; set; }
            public int sumBigChanceMissed { get; set; }
            public int sumBigChanceCreated { get; set; }

            //Specific Keeper
            [JsonConverter(typeof(StringToDoubleConverter))]
            public double percentageSaveShot { get; set; }
            public int sumCleanSheet { get; set; }
            public int sumSaves { get; set; }
            public int sumDeflect { get; set; }
            public int sumPenaltySave { get; set; }
            public int sumPenaltyFaced { get; set; }
        }

        public DetailedStats stats { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public int position { get; set; }
        public int ultraPosition { get; set; }
        public int teamId { get; set; }
        public int quotation { get; set; }
        public string club { get; set; }
        public string calendar { get; set; }
        public DateTimeOffset updatedAt { get; set; }
        public int nbMatch { get; set; }
        public int active { get; set; }
        public DateTimeOffset birthDate { get; set; }
        public int championship { get; set; }
        public Dictionary<string, Championship> championships { get; set; }
        [JsonConverter(typeof(StringToIntConverter))]
        public int jerseyNum { get; set; }
        public DateTimeOffset joinDate { get; set; }
        public string twitter { get; set; }
        public string[] availableSeasons { get; set; }
    }
}

