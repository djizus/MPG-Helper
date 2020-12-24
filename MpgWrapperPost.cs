using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Linq;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SQLite;

namespace MPG_WPF_App
{
    public class MpgUserForm
    {
        public string email { get; set; }
        public string password { get; set; }
        public string language { get; set; }

        public override string ToString()
        {
            return $"{email}: {password}: {email}";
        }
    }

    public class MpgCoachSubmit
    {
        [JsonProperty("bonusSelected")]
        public bonusSelected bonusSelected { get; set; }

        [JsonProperty("composition")]
        public long? composition { get; set; }

        [JsonProperty("matchId")]
        public string matchId { get; set; }

        [JsonProperty("playersOnPitch")]
        public pOnPitch playersOnPitch { get; set; }

        [JsonProperty("realday")]
        public long? realday { get; set; }

        [JsonProperty("tacticalsubstitutes")]
        public TacticalSubs[] tacticalsubstitutes { get; set; }
    }
    
    public class pOnPitch
    {
        [JsonProperty("1")]
        public string p1 { get; set; }

        [JsonProperty("2")]
        public string p2 { get; set; }

        [JsonProperty("3")]
        public string p3 { get; set; }

        [JsonProperty("4")]
        public string p4 { get; set; }

        [JsonProperty("5")]
        public string p5 { get; set; }

        [JsonProperty("6")]
        public string p6 { get; set; }

        [JsonProperty("7")]
        public string p7 { get; set; }

        [JsonProperty("8")]
        public string p8 { get; set; }

        [JsonProperty("9")]
        public string p9 { get; set; }

        [JsonProperty("10")]
        public string p10 { get; set; }

        [JsonProperty("11")]
        public string p11 { get; set; }

        [JsonProperty("12")]
        public string p12 { get; set; }

        [JsonProperty("13")]
        public string p13 { get; set; }

        [JsonProperty("14")]
        public string p14 { get; set; }

        [JsonProperty("15")]
        public string p15 { get; set; }

        [JsonProperty("16")]
        public string p16 { get; set; }

        [JsonProperty("17")]
        public string p17 { get; set; }

        [JsonProperty("18")]
        public string p18 { get; set; }

    }

    public class TacticalSubs
    {
        [JsonProperty("rating", NullValueHandling = NullValueHandling.Ignore)]
        public long? rating { get; set; }

        [JsonProperty("start", NullValueHandling = NullValueHandling.Ignore)]
        public string start { get; set; }

        [JsonProperty("subs", NullValueHandling = NullValueHandling.Ignore)]
        public string subs { get; set; }
    }
    
    public class bonusSelected
    {
        [JsonProperty("type")]
        public long? type { get; set; }
    }

    public class MercatoPlayers
    {
        [JsonProperty("players")]
        public List<mercatoPlayerPrice> playerPrice { get; set; }
    }

    public class mercatoPlayerPrice
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("price")]
        public long price { get; set; }
    }

}
