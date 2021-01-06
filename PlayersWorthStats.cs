using System;

namespace MPGApp
{
    public class AuPlayers : PlayersTimeStats
    {
        public string Au { get; set; }

        public AuPlayers(PlayersTimeStats inp)
            : base(inp)
        {
        }
    }

    public class PlayersTimeStats
    {
        public string id { get; set; }
        public string Name { get; set; }

        public PlayersWorthStats CurrentSeasonStats { get; set; }
        public PlayersWorthStats PastSeason1Stats { get; set; }
        public PlayersWorthStats PastSeason2Stats { get; set; }

        public double CurrentRating { get { return CurrentSeasonStats.OurRating; } }
        public double PastRating1 { get { return (null != PastSeason1Stats) ? PastSeason1Stats.OurRating : 0; } }
        public double PastRating2 { get { return (null != PastSeason2Stats) ? PastSeason2Stats.OurRating : 0; } }

        public double OurRating
        {
            get
            {
                double wCurrent = 10;
                double wPast1 = 4;
                double wPast2 = 2;

                double weight = wCurrent;

                if (null != PastSeason1Stats)
                {
                    if (4 == PastSeason1Stats.championship && CurrentSeasonStats.championship != 4)
                        wPast1 = 1;

                    weight += wPast1;
                }

                if (null != PastSeason2Stats)
                {
                    if (4 == PastSeason2Stats.championship && CurrentSeasonStats.championship != 4)
                        wPast2 = 1;

                    weight += wPast2;
                }

                double ret = CurrentSeasonStats.OurRating * wCurrent / weight + PastRating1 * wPast1 / weight + PastRating2 * wPast2 / weight;
                return ret;
            }
        }

        public double Consistency
        {
            get { return CurrentSeasonStats.OurRating - OurRating; }
        }

        public PlayersTimeStats(PlayersTimeStats inp)
        {
            //OnlyUsed in Clone
            id = inp.id;
            Name = inp.Name;
            CurrentSeasonStats = inp.CurrentSeasonStats.Clone();
            if (null != inp.PastSeason1Stats)
                PastSeason1Stats = inp.PastSeason1Stats.Clone();
            if (null != inp.PastSeason2Stats)
                PastSeason2Stats = inp.PastSeason2Stats.Clone();
        }

        public PlayersTimeStats(string idIn, string nameIn)
        {
            id = idIn;
            Name = nameIn;
        }

        internal PlayersTimeStats Clone()
        {
            return new PlayersTimeStats(this);
        }
    }

    public class PlayersWorthStats
    {
        public string id { get; set; }
        public string Name { get; set; }
        public int Quotation { get; set; }
        public int Position { get; set; }
        public int RealNbMatch { get; set; }
        public int LastPlayedM { get; set; }
        public double Rate { get; set; }
        public int Goals { get; set; }
        public int RedC { get; set; }
        public int TotalPlayed { get; set; }
        public int championship { get; set; }

        public double PStarter { get; set; }
        public double MinM { get; set; }
        public double MistakeM { get; set; }
        public double ShotM { get; set; }
        public double PShotTargetM { get; set; }
        public double CrossSucM { get; set; }
        public double PCrossSucM { get; set; }
        public int PGoalChances { get; set; }
        public int PenaltyNb { get; set; }
        public int PPenaltyConv { get; set; }
        public int AssistsNb { get; set; }
        public int ChancesCreated { get; set; }
        public int ChancesMiss { get; set; }
        //Specific Keeper
        public int KCleanSheet { get; set; }
        public double KPSaveShot { get; set; }
        public int KSaves { get; set; }
        public int KDeflect { get; set; }
        public int KPenaltySave { get; set; }

        //For consecutive computing
        public int ConsecutivePlay { get; set; }
        public double CRate { get; set; }
        public int MinPlayed { get; set; }
        public int CGoals { get; set; }
        public int CAssists { get; set; }
        public int CChancesCreated { get; set; }

        public double OurRating
        {
            get
            {
                double ret = 0;
                if (RealNbMatch > 0)
                {
                    ret += 2 * (10 - 2 * (LastPlayedM));
                    ret += 10 * (double)TotalPlayed / RealNbMatch;
                    ret += 10 * Rate;

                    //ret -= RedC * 10;
                    ret += Goals;
                }

                return ret;
            }
        }

        public double Momentum
        {
            get { return (OurRating + Math.Min(5, ConsecutivePlay)) * CRate / Rate; }
        }

        public PlayersWorthStats(PlayersWorthStats inp)
        {
            id = inp.id;
            Name = inp.Name;
            Quotation = inp.Quotation;
            Position = inp.Position;
            RealNbMatch = inp.RealNbMatch;
            LastPlayedM = inp.LastPlayedM;
            PStarter = inp.PStarter;
            Rate = inp.Rate;
            Goals = inp.Goals;
            RedC = inp.RedC;
            TotalPlayed = inp.TotalPlayed;
            MinM = inp.MinM;
            MistakeM = inp.MistakeM;
            ShotM = inp.ShotM;
            PShotTargetM = inp.PShotTargetM;
            CrossSucM = inp.CrossSucM;
            PCrossSucM = inp.PCrossSucM;
            PGoalChances = inp.PGoalChances;
            PenaltyNb = inp.PenaltyNb;
            PPenaltyConv = inp.PPenaltyConv;
            AssistsNb = inp.AssistsNb;
            ChancesCreated = inp.ChancesCreated;
            ChancesMiss = inp.ChancesMiss;
            KCleanSheet = inp.KCleanSheet;
            KPSaveShot = inp.KPSaveShot;
            KSaves = inp.KSaves;
            KDeflect = inp.KDeflect;
            KPenaltySave = inp.KPenaltySave;
            ConsecutivePlay = inp.ConsecutivePlay;
            CRate = inp.CRate;
            MinPlayed = inp.MinPlayed;
            CGoals = inp.CGoals;
            CAssists = inp.CAssists;
            CChancesCreated = inp.CChancesCreated;
        }

        //public PlayersWorthStats(string id, string name, int quotation, int position, int currentMDay, int lastPlayedM, double pStarter, double rate,
        //    int goals, int redC, int totalPlayed, double minM, double mistakeM, double shotM, double pShotTargetM, double crossSucM, double pCrossSucM,
        //    int pGoalChances, int penaltyNb, int pPenaltyConv, int assistsNb, int chancesCreated, int chancesMiss,
        //    int kCleanSheet, double kPSaveShot, int kSaves, int kDeflect, int kPenaltySave)
        //{
        //    this.id = id;
        //    Name = name;
        //    Quotation = quotation;
        //    Position = position;
        //    RealNbMatch = currentMDay;
        //    LastPlayedM = lastPlayedM;
        //    PStarter = pStarter;
        //    Rate = rate;
        //    Goals = goals;
        //    RedC = redC;
        //    TotalPlayed = totalPlayed;
        //    MinM = minM;
        //    MistakeM = mistakeM;
        //    ShotM = shotM;
        //    PShotTargetM = pShotTargetM;
        //    CrossSucM = crossSucM;
        //    PCrossSucM = pCrossSucM;
        //    PGoalChances = pGoalChances;
        //    PenaltyNb = penaltyNb;
        //    PPenaltyConv = pPenaltyConv;
        //    AssistsNb = assistsNb;
        //    ChancesCreated = chancesCreated;
        //    ChancesMiss = chancesMiss;
        //    KCleanSheet = kCleanSheet;
        //    KPSaveShot = kPSaveShot;
        //    KSaves = kSaves;
        //    KDeflect = kDeflect;
        //    KPenaltySave = kPenaltySave;
        //}

        public PlayersWorthStats(string id, string name, int quotation, int position, int nbMatch, int lastPlayedM, double rate,
            int goals, int redC, int totalPlayed)
        {
            this.id = id;
            Name = name;
            Quotation = quotation;
            Position = position;
            RealNbMatch = nbMatch;
            LastPlayedM = lastPlayedM;
            Rate = rate;
            Goals = goals;
            RedC = redC;
            TotalPlayed = totalPlayed;
        }

        internal PlayersWorthStats Clone()
        {
            return new PlayersWorthStats(this);
        }
    }
}
