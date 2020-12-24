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

    [Table("PlayerData")]
    public class PlayerData
    {
        [Column("Joueur")]
        public string PlayerName { get; set; }

        [Column("Poste")]
        public string Position { get; set; }

        [Column("Club")]
        public string Club { get; set; }

        [Column("TeamId")]
        public string TeamId { get; set; }

        [Column("Prix")]
        public long Quotation { get; set; }

        [Column("Enchere")]
        public long Enchere { get; set; }

        [Column("Valeur")]
        public long EV { get; set; }

        [Column("Moyenne")]
        public decimal AvgRate { get; set; }

        [Column("Buts")]
        public long SumGoals { get; set; }

        [Column("Titus")]
        public decimal PercentageStarter { get; set; }

        [PrimaryKey]
        [Column("mpg_id")]
        [JsonProperty("id")]
        public string Id { get; set; }

        public TeamData convertToTeamData()
        {
            return new TeamData
            {
                Id = TeamId,
                Club = Club,
                Valeur = Enchere,
                EV = EV,
                Force = Enchere + EV,
            };
        }
    }

    [Table("TeamData")]
    public class TeamData
    {
        [PrimaryKey]
        [Column("id")]
        public string Id { get; set; }

        [Column("Club")]
        public string Club { get; set; }

        [Column("Valeur")]
        public decimal Valeur { get; set; }

        [Column("Indicateur de performance")]
        public long EV { get; set; }

        [Column("Force")]
        public decimal Force { get; set; }
    }

    public class TeamFormationData
    {
        public string Formation { get; set; }

        public long Force { get; set; }
        
        public string[] PlayersOnPitch { get; set; }

        public List<PlayerData> Best11 { get; set; }

        public List<PlayerData> BestSubs { get; set; }

        public List<PlayerData> Reserve { get; set; }

        public List<PlayerData> TeamPlayersData { get; set; }

        public List<PlayerData> TeamPlayersForce { get; set; }

        public List<PlayerData> gks { get; set; }

        public List<PlayerData> defs { get; set; }

        public List<PlayerData> mfs { get; set; }

        public List<PlayerData> bts { get; set; }

        public TeamFormationData Initiate()
        {
            Formation = "0";
            Force = 0;
            Best11 = new List<PlayerData>();
            BestSubs = new List<PlayerData>();
            Reserve = new List<PlayerData>();
            TeamPlayersData = new List<PlayerData>();
            TeamPlayersForce = new List<PlayerData>();
            gks = new List<PlayerData>();
            defs = new List<PlayerData>();
            mfs = new List<PlayerData>();
            bts = new List<PlayerData>();
            PlayersOnPitch = new string[18];
            return this;
        }

        public void FilterAndSortPlayerByPosition()
        {
            gks = TeamPlayersForce.Where(p => p.Position == "GK").OrderByDescending(b => b.EV).ThenByDescending(c => c.Enchere).ToList();
            defs = TeamPlayersForce.Where(p => p.Position == "DC" || p.Position == "DL").OrderByDescending(b => b.EV).ThenByDescending(c => c.Enchere).ToList();
            mfs = TeamPlayersForce.Where(p => p.Position == "MC" || p.Position == "MO").OrderByDescending(b => b.EV).ThenByDescending(c => c.Enchere).ToList();
            bts = TeamPlayersForce.Where(p => p.Position == "BT").OrderByDescending(b => b.EV).ThenByDescending(c => c.Enchere).ToList();
        }

        public void SetRemplacants()
        {
            //On récupère les joueurs qui sont disponibles et ne sont pas dans le meilleur 11
            List<PlayerData> playerDisposNotTitus = TeamPlayersForce.Where(l => !Best11.Any(c => c.Id == l.Id)).ToList();
            gks = playerDisposNotTitus.Where(p => p.Position == "GK").OrderByDescending(b => b.EV).ThenByDescending(c => c.Enchere).ToList();
            if (gks.Count() > 0)
            {
                BestSubs.AddRange(gks.Take(1).ToList());
                PlayersOnPitch[17] = BestSubs[0].Id;
            }
            else
                BestSubs.Add(new PlayerData());

            //On retire les gardiens de la liste des remplacants potentiels
            playerDisposNotTitus = playerDisposNotTitus.Where(l => !gks.Any(c => c.Id == l.Id)).OrderByDescending(b => b.EV).ThenByDescending(c => c.Enchere).Take(6).ToList();
            
            //playerDisposNotTitus = playerDisposNotTitus.Where(l => !gks.Any(c => c.Id == l.Id)).ToList();
            int i = 0;
            foreach(PlayerData pd in playerDisposNotTitus)
            {
                BestSubs.Add(playerDisposNotTitus.ElementAtOrDefault(i));
                PlayersOnPitch[i+11] = playerDisposNotTitus.ElementAtOrDefault(i).Id;
                ++i;
            }
        }

        public void SetReserve()
        { 
            //On récupère le reste des joueurs, ceux qui ne sont pas dans le 11 ni dans les remplacants
            List<PlayerData> playerReserve = TeamPlayersData.Where(l => !Best11.Any(c => c.Id == l.Id)).ToList();
            playerReserve = playerReserve.Where(l => !BestSubs.Any(c => c.Id == l.Id)).ToList();
            Reserve = playerReserve;
        }
    }

    internal static class MyExtensions
    {
        internal static T KeyByValue<T, W>(this Dictionary<T, W> dict, W val)
        {
            T key = default;
            foreach (KeyValuePair<T, W> pair in dict)
            {
                if (EqualityComparer<W>.Default.Equals(pair.Value, val))
                {
                    key = pair.Key;
                    break;
                }
            }
            return key;
        }
    }

}
