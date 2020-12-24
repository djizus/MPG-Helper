using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Linq;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace MPG_WPF_App
{
    public class MpgUserDashboard
    {
        public MpgLeagueData Data { get; set; }
    }

    public class MpgLeagueData
    {
        public MpgLeagueElement[] Leagues { get; set; }
        public object[] Follow { get; set; }
    }

    public class MpgLeagueElement
    {
        public string Id { get; set; }
        public string AdminMpgUserId { get; set; }
        public string CurrentMpgUser { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public long? LeagueStatus { get; set; }
        public long? Championship { get; set; }
        public long? Mode { get; set; }
        public long? TeamStatus { get; set; }
        public long? Players { get; set; }
        public string Admin { get; set; }
        public Dictionary<string, MpgLeagueDivision> Divisions { get; set; }
        public Uri Image { get; set; }
        public string[] LeaguesIds { get; set; }
        public string Rating { get; set; }
        public long? Season { get; set; }
        public long? Status { get; set; }
        public string[] UsersIds { get; set; }
        public bool? IsMasterLeague { get; set; }
        public MpgLeagueElementData League { get; set; }
    }

    public class MpgLeagueDivision
    {
        public string Name { get; set; }
        public MpgLeagueDivisionUser[] Users { get; set; }
        public long Id { get; set; }
        public string LeagueId { get; set; }
        public bool? MercatoClosed { get; set; }
    }

    public class MpgLeagueDivisionUser
    {
        public string Id { get; set; }
        public long Movement { get; set; }
    }

    public class MpgLeagueElementData
    {
        public string Id { get; set; }
        public string AdminMpgUserId { get; set; }
        public string CurrentMpgUser { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public long LeagueStatus { get; set; }
        public long Championship { get; set; }
        public long Mode { get; set; }
        public long? TeamStatus { get; set; }
        public long Players { get; set; }
        public string MasterLeagueId { get; set; }
    }

    public class MpgLeagueDashboard
    {
        public long Hide { get; set; }
        public MpgTemplateJersey TemplateJersey { get; set; }
        public MpgLeagueTeam Team { get; set; }
        public MpgLeagueAdmin Admin { get; set; }
    }

    public class MpgLeagueAdmin
    {
        public MpgLeagueUser[] Users { get; set; }
    }

    public class MpgLeagueUser
    {
        public string Lastname { get; set; }
        public string Firstname { get; set; }
        public string Id { get; set; }
    }

    public class MpgLeagueTeam
    {
        public string Name { get; set; }
        public string Stadium { get; set; }
        public string Abbr { get; set; }
        public long OptinAlerte { get; set; }
        public long OptinMur { get; set; }
    }

    public class MpgLeagueMercato
    {
        public string LeagueId { get; set; }
        public long Turn { get; set; }
        public long Season { get; set; }
        public long StatusLeague { get; set; }
        public string CurrentUser { get; set; }
        public long Championship { get; set; }
        public long StatusTeam { get; set; }
        public long LeagueMode { get; set; }
        public long Budget { get; set; }
        public string CurrentTeam { get; set; }
        public MpgAvailablePlayer[] AvailablePlayers { get; set; }
        public object[] UserPlayers { get; set; }
        public MpgPreparation[] Preparation { get; set; }
        public long NextMatchIn { get; set; }
        public long MatchesToComeMercato { get; set; }
        public long MatchesToComeReal { get; set; }
        public long NextTurnIn { get; set; }
    }

    public class MpgAvailablePlayer
    {
        public string Id { get; set; }

        public string Lastname { get; set; }

        public string Firstname { get; set; }

        public long Position { get; set; }

        public long Quotation { get; set; }

        public long UltraPosition { get; set; }

        public string Club { get; set; }

        public long Teamid { get; set; }

        public DateTimeOffset JoinDate { get; set; }

        public DateTimeOffset AvailableSince { get; set; }
    }

    public class MpgPreparation
    {
        public string Id { get; set; }
        public long Price { get; set; }
    }
    
    public class MpgTemplateJersey{}

    public class MpgUserToken
    {
        public string token { get; set; }
        public string userId { get; set; }
        public string language { get; set; }
        public string action { get; set; }
        public DateTime createdAt { get; set; }
        public bool onboarded { get; set; }
    }

    public class MpgUser
    {
        public string id { get; set; }
        public string email { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string country { get; set; }
        public int teamId { get; set; }
        public DateTime dob { get; set; }
        public bool onboarded { get; set; }
        public int push_wall { get; set; }
        public int optin_nl { get; set; }
        public int optin_partner { get; set; }
        public IList<object> jerseySkin { get; set; }
        public int money { get; set; }
        public int bounce { get; set; }
        public bool hasBeenExpert { get; set; }
        public int optinNlMpp { get; set; }
        public int pushNlMpp { get; set; }
        public DateTime liveRating { get; set; }
    }

    public class MpgPlayerSeasonStats
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("firstname")]
        public string Firstname { get; set; }

        [JsonProperty("lastname")]
        public string Lastname { get; set; }

        [JsonProperty("position")]
        public long Position { get; set; }

        [JsonProperty("ultraPosition")]
        public long UltraPosition { get; set; }

        [JsonProperty("teamId")]
        public long TeamId { get; set; }

        [JsonProperty("quotation")]
        public long Quotation { get; set; }

        [JsonProperty("club")]
        public string Club { get; set; }

        [JsonProperty("stats")]
        public MpgPlayerStats Stats { get; set; }
        
        private static IReadOnlyDictionary<long?, string> UltraPositions = new Dictionary<long?, string>
        {
             { 10, "GK" },
             { 20, "DC" },
             { 21, "DL" },
             { 30, "MD" },
             { 31, "MC" },
             { 32, "MO" },
             { 40, "BT" },
        };
        public PlayerData convertToPlayerData(Decimal weightSeason)
        {
            decimal tmpAvgRate;
            decimal tmpPercentageStarter;
            UltraPositions.TryGetValue(UltraPosition, out string parsePos);
            var ci = CultureInfo.InvariantCulture.Clone() as CultureInfo;
            ci.NumberFormat.NumberDecimalSeparator = ".";
            if (Stats.AvgRate == "-")
                tmpAvgRate = (Decimal) 2.5;
            else 
                tmpAvgRate = decimal.Parse(Stats.AvgRate, ci);
            if (Stats.PercentageStarter == null)
                tmpPercentageStarter = 0;
            else
                tmpPercentageStarter = decimal.Parse(Stats.PercentageStarter, ci);
            decimal tmpEV1 = (Decimal)0.31 * tmpAvgRate + (Decimal) 0.4/Position * Stats.SumGoals + (Decimal)1.2 * (decimal.Parse("0.01", ci) + tmpPercentageStarter);
            decimal tmpEV2 = (tmpEV1* tmpEV1* tmpEV1 + tmpEV1*tmpEV1 + tmpEV1) / weightSeason;
            if (Stats.CurrentChampionship == (long) 4)
                tmpEV2 = tmpEV2 / 2;
            long tmpEV = (long)Convert.ToDouble(Decimal.Round(tmpEV2, 0));
            return new PlayerData
            {
                PlayerName = String.Concat(Firstname, " ", Lastname),
                Position = parsePos,
                Club = Club,
                TeamId = TeamId.ToString(),
                AvgRate = tmpAvgRate,
                SumGoals = Stats.SumGoals,
                PercentageStarter = tmpPercentageStarter,                    
                Quotation = Quotation,
                EV = tmpEV - Quotation,
                Enchere = tmpEV,
                Id = Id,
            };
        }
    }

    public class MpgPlayerStats
    {
        [JsonProperty("avgRate")]
        public string AvgRate { get; set; }

        [JsonProperty("sumGoals")]
        public long SumGoals { get; set; }

        [JsonProperty("currentChampionship")]
        public long CurrentChampionship { get; set; }

        [JsonProperty("percentageStarter")]
        public string PercentageStarter { get; set; }
    }

    public class MpgTransferts
    {
        [JsonProperty("leagueId")]
        public string LeagueId { get; set; }

        [JsonProperty("turn")]
        public long Turn { get; set; }

        [JsonProperty("season")]
        public long Season { get; set; }

        [JsonProperty("statusLeague")]
        public long StatusLeague { get; set; }

        [JsonProperty("currentUser")]
        public string CurrentUser { get; set; }

        [JsonProperty("championship")]
        public long Championship { get; set; }

        [JsonProperty("statusTeam")]
        public long StatusTeam { get; set; }

        [JsonProperty("leagueMode")]
        public long LeagueMode { get; set; }

        [JsonProperty("budget")]
        public long Budget { get; set; }

        [JsonProperty("currentTeam")]
        public string CurrentTeam { get; set; }

        [JsonProperty("availablePlayers")]
        public MpgPlayerTransfert[] AvailablePlayers { get; set; }

        [JsonProperty("userPlayers")]
        public MpgPlayerTransfert[] UserPlayers { get; set; }
    }

    public class MpgPlayerTransfert
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("lastname")]
        public string Lastname { get; set; }

        [JsonProperty("firstname")]
        public string Firstname { get; set; }

        [JsonProperty("position")]
        public long Position { get; set; }

        [JsonProperty("quotation")]
        public long Quotation { get; set; }

        [JsonProperty("ultraPosition")]
        public long UltraPosition { get; set; }

        [JsonProperty("club")]
        public string Club { get; set; }

        [JsonProperty("teamid")]
        public long Teamid { get; set; }

        [JsonProperty("joinDate")]
        public DateTimeOffset JoinDate { get; set; }

        [JsonProperty("availableSince")]
        public DateTimeOffset AvailableSince { get; set; }

        [JsonProperty("pricePaid", NullValueHandling = NullValueHandling.Ignore)]
        public long? PricePaid { get; set; }

        [JsonProperty("buying_date", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? BuyingDate { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public long? Status { get; set; }

        public MpgAvailablePlayer convertToAvailablePlayer()
        {
            MpgAvailablePlayer newPlayer = new MpgAvailablePlayer();
            newPlayer.AvailableSince = AvailableSince;
            newPlayer.Club = Club;
            newPlayer.Firstname = Firstname;
            newPlayer.Id = Id;
            newPlayer.JoinDate = JoinDate;
            newPlayer.Lastname = Lastname;
            newPlayer.Position = Position;
            newPlayer.Quotation = Quotation;
            newPlayer.Teamid = Teamid;
            newPlayer.UltraPosition = UltraPosition;
            return newPlayer;
        }

    }

    public class MpgCoach
    {
        [JsonProperty("data")]
        public MpgCoachData Data { get; set; }
    }

    public class MpgCoachData
    {
        [JsonProperty("realday")]
        public long Realday { get; set; }

        [JsonProperty("dateMatch")]
        public long DateMatch { get; set; }

        [JsonProperty("timetogame")]
        public long Timetogame { get; set; }

        [JsonProperty("champid")]
        public long Champid { get; set; }

        [JsonProperty("matchId")]
        public string MatchId { get; set; }

        [JsonProperty("stadium")]
        public string Stadium { get; set; }

        [JsonProperty("teamHome")]
        public MpgTeam TeamHome { get; set; }

        [JsonProperty("teamAway")]
        public MpgTeam TeamAway { get; set; }

        [JsonProperty("nbPlayers")]
        public long NbPlayers { get; set; }

        [JsonProperty("players")]
        public MpgCoachPlayer[] Players { get; set; }

        [JsonProperty("bonus", NullValueHandling = NullValueHandling.Ignore)]
        public KeyValuePair<string, long> Bonus { get; set; }

        [JsonProperty("nanardpaid")]
        public long Nanardpaid { get; set; }

        [JsonProperty("money")]
        public long Money { get; set; }

        [JsonProperty("fridayBrief")]
        public bool FridayBrief { get; set; }

        [JsonProperty("tacticalsubstitutes")]
        public object[] Tacticalsubstitutes { get; set; }

        [JsonProperty("composition")]
        public long Composition { get; set; }

        [JsonProperty("playersOnPitch")]
        public Dictionary<string, string> PlayersOnPitch { get; set; }

        [JsonProperty("teams")]
        public Dictionary<string, MpgTeamValue> Teams { get; set; }

        [JsonProperty("tds")]
        public Dictionary<string, MpgTdValue> Tds { get; set; }
    }

    public class MpgCoachPlayer
    {
        [JsonProperty("playerid")]
        public string Playerid { get; set; }

        [JsonProperty("firstname")]
        public string Firstname { get; set; }

        [JsonProperty("lastname")]
        public string Lastname { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("teamid")]
        public long Teamid { get; set; }

        [JsonProperty("position")]
        public long Position { get; set; }

        [JsonProperty("quotation")]
        public long Quotation { get; set; }

        [JsonProperty("ultraPosition")]
        public long UltraPosition { get; set; }

        [JsonProperty("lastFiveRate")]
        public Dictionary<string, MpgLastFiveRate> LastFiveRate { get; set; }
    }

    public class MpgLastFiveRate
    {
        [JsonProperty("matchId")]
        public string MatchId { get; set; }

        [JsonProperty("day")]
        public long Day { get; set; }

        [JsonProperty("rate", NullValueHandling = NullValueHandling.Ignore)]
        public double? Rate { get; set; }
    }

    public struct MpgTdValue
    {
        [JsonProperty("injured", NullValueHandling = NullValueHandling.Ignore)]
        public long? Disponibilite { get; set; }

        [JsonProperty("pending", NullValueHandling = NullValueHandling.Ignore)]
        private long? Pending { set { Disponibilite = 0; } }

        [JsonProperty("suspended", NullValueHandling = NullValueHandling.Ignore)]
        private long? Suspended { set { Disponibilite = value; } }

        [JsonProperty("reported", NullValueHandling = NullValueHandling.Ignore)]
        private long? Reported { set { Disponibilite = 0; } }
    }

    public class MpgTeam
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("abbr")]
        public string Abbr { get; set; }

        [JsonProperty("coach")]
        public string Coach { get; set; }

        [JsonProperty("jersey")]
        public MpgJersey Jersey { get; set; }

        [JsonProperty("jerseyHome")]
        public string JerseyHome { get; set; }

        [JsonProperty("jerseyAway")]
        public string JerseyAway { get; set; }

        [JsonProperty("jerseyUrl")]
        public Uri JerseyUrl { get; set; }
    }

    public class MpgJersey
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("sponsor")]
        public long Sponsor { get; set; }

        [JsonProperty("zones")]
        public MpgZones Zones { get; set; }
    }

    public class MpgZones
    {
        [JsonProperty("z1")]
        public string Z1 { get; set; }
    }

    public class MpgTeamValue
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("against")]
        public string Against { get; set; }
    }


    public partial class Calendar
    {
        [JsonProperty("day")]
        public long Day { get; set; }

        [JsonProperty("matches")]
        public Match[] Matches { get; set; }
    }

    public partial class Match
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("period")]
        public string Period { get; set; }

        [JsonProperty("matchTime", NullValueHandling = NullValueHandling.Ignore)]
        public long MatchTime { get; set; }

        [JsonProperty("quotationLink")]
        public Uri QuotationLink { get; set; }

        [JsonProperty("matchData")]
        public MatchData MatchData { get; set; }

        [JsonProperty("definitiveRating")]
        public bool DefinitiveRating { get; set; }

        [JsonProperty("home")]
        public MatchAway Home { get; set; }

        [JsonProperty("away")]
        public MatchAway Away { get; set; }
    }

    public partial class MatchAway
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("club")]
        public string Club { get; set; }

        [JsonProperty("players")]
        public Player[] Players { get; set; }

        [JsonProperty("score")]
        public long Score { get; set; }
    }

    public partial class Player
    {
        [JsonProperty("lastname")]
        public string Lastname { get; set; }

        [JsonProperty("own_goals")]
        public long OwnGoals { get; set; }

        [JsonProperty("goals")]
        public long Goals { get; set; }
    }

    public partial class MatchData
    {
        [JsonProperty("home")]
        public MatchDataAway Home { get; set; }

        [JsonProperty("away")]
        public MatchDataAway Away { get; set; }
    }

    public partial class MatchDataAway
    {
        [JsonProperty("goals")]
        public Booking[] Goals { get; set; }

        [JsonProperty("bookings")]
        public Booking[] Bookings { get; set; }

        [JsonProperty("substitutions")]
        public Substitution[] Substitutions { get; set; }
    }

    public partial class Booking
    {
        [JsonProperty("playerId")]
        public long PlayerId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("lastname")]
        public string Lastname { get; set; }

        [JsonProperty("assistPlayerId", NullValueHandling = NullValueHandling.Ignore)]
        public long? AssistPlayerId { get; set; }
    }

    public partial class Substitution
    {
        [JsonProperty("subOff")]
        public long SubOff { get; set; }

        [JsonProperty("subOn")]
        public string SubOn { get; set; }

        [JsonProperty("reason", NullValueHandling = NullValueHandling.Ignore)]
        public string Reason { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }
    }

    public partial class PlayerDetailedData
    {
        [JsonProperty("stats")]
        public Stats Stats { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("firstname")]
        public string Firstname { get; set; }

        [JsonProperty("lastname")]
        public string Lastname { get; set; }

        [JsonProperty("position")]
        public long Position { get; set; }

        [JsonProperty("ultraPosition")]
        public long UltraPosition { get; set; }

        [JsonProperty("teamId")]
        public long TeamId { get; set; }

        [JsonProperty("quotation")]
        public long Quotation { get; set; }

        [JsonProperty("club")]
        public string Club { get; set; }

        [JsonProperty("calendar")]
        public string Calendar { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("nbMatch")]
        public long NbMatch { get; set; }

        [JsonProperty("active")]
        public long Active { get; set; }

        [JsonProperty("birthDate")]
        public DateTimeOffset BirthDate { get; set; }

        [JsonProperty("championship")]
        public long Championship { get; set; }

        [JsonProperty("championships")]
        public Dictionary<string, Championship> Championships { get; set; }

        [JsonProperty("jerseyNum")]
        [JsonConverter(typeof(StringToLongConverter))]
        public long JerseyNum { get; set; }

        [JsonProperty("joinDate")]
        public DateTimeOffset JoinDate { get; set; }

        [JsonProperty("twitter")]
        public string Twitter { get; set; }

        [JsonProperty("availableSeasons")]
        public List<long> AvailableSeasons { get; set; }
    }

    public partial class Championship
    {
        [JsonProperty("active")]
        public long Active { get; set; }

        [JsonProperty("availableSince")]
        public DateTimeOffset AvailableSince { get; set; }

        [JsonProperty("championship")]
        public long ChampionshipChampionship { get; set; }

        [JsonProperty("club")]
        public string Club { get; set; }

        [JsonProperty("joinDate")]
        public DateTimeOffset JoinDate { get; set; }

        [JsonProperty("quotation")]
        public long Quotation { get; set; }

        [JsonProperty("teamId")]
        public long TeamId { get; set; }
    }

    public class StringToLongConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(string))
            {
                return true;
            }

            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            long ret = 0;

            if (reader.TokenType != JsonToken.Null)
            {
                if (reader.TokenType == JsonToken.StartArray)
                {
                    JToken token = JToken.Load(reader);
                    List<string> items = token.ToObject<List<string>>();
                    //myCustomType = new MyCustomType(items);
                }
                else
                {
                    JValue jValue = new JValue(reader.Value);
                    switch (reader.TokenType)
                    {
                        case JsonToken.String:
                            Int64.TryParse((string)jValue, out ret);
                            break;
                        //case JsonToken.Date:
                        //    myCustomType = new MyCustomType((DateTime)jValue);
                        //    break;
                        //case JsonToken.Boolean:
                        //    myCustomType = new MyCustomType((bool)jValue);
                        //    break;
                        case JsonToken.Integer:
                        case JsonToken.Float:
                            ret = Convert.ToInt64(jValue);
                            //myCustomType = new MyCustomType(i);
                            break;
                        default:
                            Console.WriteLine("Default case");
                            Console.WriteLine(reader.TokenType.ToString());
                            break;
                    }
                }
            }
            return ret;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public partial class Stats
    {
        [JsonProperty("appearances")]
        public Appearances Appearances { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("currentChampionship")]
        public long CurrentChampionship { get; set; }

        [JsonProperty("matches")]
        public List<PlayerDataMatch> Matches { get; set; }

        [JsonProperty("lastFiveRate")]
        public Dictionary<string, LastFiveRate> LastFiveRate { get; set; }

        [JsonProperty("percentageStarter")]
        public double PercentageStarter { get; set; }

        [JsonProperty("avgRate")]
        public double AvgRate { get; set; }

        [JsonProperty("sumGoals")]
        public long SumGoals { get; set; }

        [JsonProperty("sumYellowCard")]
        public long SumYellowCard { get; set; }

        [JsonProperty("sumRedCard")]
        public long SumRedCard { get; set; }

        [JsonProperty("goalByMatch")]
        public double GoalByMatch { get; set; }

        [JsonProperty("shotByMatch")]
        public double ShotByMatch { get; set; }

        [JsonProperty("minutesByMatch")]
        public double MinutesByMatch { get; set; }

        [JsonProperty("interceptByMatch")]
        public long InterceptByMatch { get; set; }

        [JsonProperty("tackleByMatch")]
        public double TackleByMatch { get; set; }

        [JsonProperty("mistakeByMatch")]
        public long MistakeByMatch { get; set; }

        [JsonProperty("wonContestByMatch")]
        public long WonContestByMatch { get; set; }

        [JsonProperty("wonDuelByMatch")]
        public double WonDuelByMatch { get; set; }

        [JsonProperty("lostBallByMatch")]
        public double LostBallByMatch { get; set; }

        [JsonProperty("foulsByMatch")]
        public double FoulsByMatch { get; set; }

        [JsonProperty("succeedPassByMatch")]
        public double SucceedPassByMatch { get; set; }

        [JsonProperty("goalsConcededByMatch")]
        public double GoalsConcededByMatch { get; set; }

        [JsonProperty("shotOnTargetByMatch")]
        public double ShotOnTargetByMatch { get; set; }

        [JsonProperty("succeedPassBackZoneByMatch")]
        public double SucceedPassBackZoneByMatch { get; set; }

        [JsonProperty("succeedPassFwdZoneByMatch")]
        public long SucceedPassFwdZoneByMatch { get; set; }

        [JsonProperty("succeedLongPassByMatch")]
        public double SucceedLongPassByMatch { get; set; }

        [JsonProperty("succeedCrossByMatch")]
        public double SucceedCrossByMatch { get; set; }

        [JsonProperty("foulsEnduredByMatch")]
        public double FoulsEnduredByMatch { get; set; }

        [JsonProperty("percentageAccuratePassBackZone")]
        [JsonConverter(typeof(StringToLongConverter))]
        public long PercentageAccuratePassBackZone { get; set; }

        [JsonProperty("percentageAccurateFwdZone")]
        [JsonConverter(typeof(StringToLongConverter))]
        public long PercentageAccurateFwdZone { get; set; }

        [JsonProperty("percentageAccurateLongPass")]
        [JsonConverter(typeof(StringToLongConverter))]
        public long PercentageAccurateLongPass { get; set; }

        [JsonProperty("percentageSucceedPass")]
        [JsonConverter(typeof(StringToLongConverter))]
        public long PercentageSucceedPass { get; set; }

        [JsonProperty("minutesByGoal")]
        [JsonConverter(typeof(StringToLongConverter))]
        public long MinutesByGoal { get; set; }

        [JsonProperty("percentageShotOnTarget")]
        [JsonConverter(typeof(StringToLongConverter))]
        public long PercentageShotOnTarget { get; set; }

        [JsonProperty("percentageCrossSuccess")]
        [JsonConverter(typeof(StringToLongConverter))]
        public long PercentageCrossSuccess { get; set; }

        [JsonProperty("percentageGoalByOpportunity")]
        [JsonConverter(typeof(StringToLongConverter))]
        public long PercentageGoalByOpportunity { get; set; }

        [JsonProperty("percentagePenaltiesScored")]
        [JsonConverter(typeof(StringToLongConverter))]
        public long PercentagePenaltiesScored { get; set; }

        [JsonProperty("percentageWonContest")]
        [JsonConverter(typeof(StringToLongConverter))]
        public long PercentageWonContest { get; set; }

        [JsonProperty("percentageWonDuel")]
        [JsonConverter(typeof(StringToLongConverter))]
        public long PercentageWonDuel { get; set; }

        [JsonProperty("percentageLostBall")]
        [JsonConverter(typeof(StringToLongConverter))]
        public long PercentageLostBall { get; set; }

        [JsonProperty("sumPenalties")]
        public long SumPenalties { get; set; }

        [JsonProperty("sumGoalAssist")]
        public long SumGoalAssist { get; set; }

        [JsonProperty("sumBigChanceMissed")]
        public long SumBigChanceMissed { get; set; }

        [JsonProperty("sumBigChanceCreated")]
        public long SumBigChanceCreated { get; set; }
    }

    public partial class Appearances
    {
        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("standBy")]
        public long StandBy { get; set; }

        [JsonProperty("standIn")]
        public long StandIn { get; set; }

        [JsonProperty("starter")]
        public long Starter { get; set; }
    }

    public partial class LastFiveRate
    {
        [JsonProperty("matchId")]
        public string MatchId { get; set; }

        [JsonProperty("day")]
        public long Day { get; set; }
    }

    public partial class PlayerDataMatch
    {
            [JsonProperty("info")]
            public Info Info { get; set; }

            [JsonProperty("score")]
            public Score Score { get; set; }

            [JsonProperty("stats")]
            public MatchStats Stats { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("date")]
            public DateTimeOffset Date { get; set; }

            [JsonProperty("day")]
            public long Day { get; set; }
    }

    public partial class Info
    {
        [JsonProperty("goals")]
        public long Goals { get; set; }

        [JsonProperty("sub")]
        public long Sub { get; set; }

        [JsonProperty("minsPlayed")]
        public long MinsPlayed { get; set; }

        [JsonProperty("rate")]
        public double Rate { get; set; }
    }

    public partial class Score
    {
        [JsonProperty("home")]
        public long Home { get; set; }

        [JsonProperty("away")]
        public long Away { get; set; }

        [JsonProperty("scoreHome")]
        public long ScoreHome { get; set; }

        [JsonProperty("scoreAway")]
        public long ScoreAway { get; set; }
    }

    public partial class MatchStats
    {
        [JsonProperty("shot_off_target")]
        public long ShotOffTarget { get; set; }

        [JsonProperty("lost_contest")]
        public long LostContest { get; set; }

        [JsonProperty("failed_cross")]
        public long FailedCross { get; set; }

        [JsonProperty("percentage_pass")]
        [JsonConverter(typeof(StringToLongConverter))]
        public long PercentagePass { get; set; }

        [JsonProperty("was_fouled")]
        public long WasFouled { get; set; }

        [JsonProperty("clean_sheet")]
        public long CleanSheet { get; set; }

        [JsonProperty("touches")]
        public long Touches { get; set; }

        [JsonProperty("interception_won")]
        public long InterceptionWon { get; set; }

        [JsonProperty("poss_lost_ctrl")]
        public long PossLostCtrl { get; set; }

        [JsonProperty("duel_won")]
        public long DuelWon { get; set; }

        [JsonProperty("duel_lost")]
        public long DuelLost { get; set; }

        [JsonProperty("fouls")]
        public long Fouls { get; set; }

        [JsonProperty("yellow_card")]
        public long YellowCard { get; set; }

        [JsonProperty("red_card")]
        public long RedCard { get; set; }

        [JsonProperty("own_goals")]
        public long OwnGoals { get; set; }

        [JsonProperty("error_lead_to_goal")]
        public long ErrorLeadToGoal { get; set; }

        [JsonProperty("goals_conceded")]
        public long GoalsConceded { get; set; }

        [JsonProperty("goals")]
        public long Goals { get; set; }

        [JsonProperty("big_chance_created")]
        public long BigChanceCreated { get; set; }

        [JsonProperty("won_contest")]
        public long WonContest { get; set; }

        [JsonProperty("goal_assist")]
        public long GoalAssist { get; set; }

        [JsonProperty("big_chance_missed")]
        public long BigChanceMissed { get; set; }

        [JsonProperty("att_pen_miss")]
        public long AttPenMiss { get; set; }

        [JsonProperty("att_pen_post")]
        public long AttPenPost { get; set; }

        [JsonProperty("att_pen_target")]
        public long AttPenTarget { get; set; }

        [JsonProperty("att_pen_goal")]
        public long AttPenGoal { get; set; }

        [JsonProperty("total_scoring_att")]
        public long TotalScoringAtt { get; set; }

        [JsonProperty("total_pass")]
        public long TotalPass { get; set; }

        [JsonProperty("total_contest")]
        public long TotalContest { get; set; }

        [JsonProperty("total_tackle")]
        public long TotalTackle { get; set; }

        [JsonProperty("total_back_zone_pass")]
        public long TotalBackZonePass { get; set; }

        [JsonProperty("total_fwd_zone_pass")]
        public long TotalFwdZonePass { get; set; }

        [JsonProperty("total_long_balls")]
        public long TotalLongBalls { get; set; }

        [JsonProperty("total_cross_nocorner")]
        public long TotalCrossNocorner { get; set; }

        [JsonProperty("accurate_cross_nocorner")]
        public long AccurateCrossNocorner { get; set; }

        [JsonProperty("accurate_back_zone_pass")]
        public long AccurateBackZonePass { get; set; }

        [JsonProperty("accurate_fwd_zone_pass")]
        public long AccurateFwdZonePass { get; set; }

        [JsonProperty("accurate_long_balls")]
        public long AccurateLongBalls { get; set; }

        [JsonProperty("accurate_pass")]
        public long AccuratePass { get; set; }
    }
}
