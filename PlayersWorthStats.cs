using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPGApp
{
    public class PlayersWorthStats
    {
        public string id { get; set; }
        public string Name { get; set; }
        public int Quotation { get; set; }
        public int Position { get; set; }
        public int RealNbMatch { get; set; }
        public int LastPlayedM { get; set; }
        public double PStarter { get; set; }
        public double Rate { get; set; }
        public int Goals { get; set; }
        public int RedC { get; set; }
        public int TotalPlayed { get; set; }
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

        public double CalcWorth
        {
            get
            {
                double ret = 0;
                ret += 2 * (10 - 2 * (LastPlayedM));
                //ret += 10 * PStarter;
                ret += 10 * (double)TotalPlayed / RealNbMatch;
                //ret += 5 * MinM / 90d * 10d;
                ret += 5 * Rate;

                //ret -= RedC * 10;
                //ret -= MistakeM * 5;

                ret += Goals;
                //ret += ChancesCreated;

                //if (Position > 1)
                //{
                //    ret += ShotM;
                //    ret += CrossSucM;
                //    ret += AssistsNb;

                //    if (PenaltyNb > 0)
                //    {
                //        ret += PenaltyNb;
                //        ret += 5 * PPenaltyConv / 10d;
                //    }

                //    ret += 2 * PShotTargetM / 10d;
                //    ret += 2 * PCrossSucM / 10d;
                //    ret += 5 * PGoalChances / 10d;

                //    ret -= ChancesMiss;
                //}
                //else
                //{
                //    ret += KCleanSheet * 2;
                //    ret += KPSaveShot / 10d;
                //    //ret += KSaves;
                //    ret += KPenaltySave * 3;
                //    //ret += KDeflect;
                //}

                return ret;
            }
        }

        public double Momentum
        {
            get { return (CalcWorth + Math.Min(5, ConsecutivePlay)) * CRate / Rate; }
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

        public PlayersWorthStats(string id, string name, int quotation, int position, int currentMDay, int lastPlayedM, double pStarter, double rate,
            int goals, int redC, int totalPlayed, double minM, double mistakeM, double shotM, double pShotTargetM, double crossSucM, double pCrossSucM,
            int pGoalChances, int penaltyNb, int pPenaltyConv, int assistsNb, int chancesCreated, int chancesMiss,
            int kCleanSheet, double kPSaveShot, int kSaves, int kDeflect, int kPenaltySave)
        {
            this.id = id;
            Name = name;
            Quotation = quotation;
            Position = position;
            RealNbMatch = currentMDay;
            LastPlayedM = lastPlayedM;
            PStarter = pStarter;
            Rate = rate;
            Goals = goals;
            RedC = redC;
            TotalPlayed = totalPlayed;
            MinM = minM;
            MistakeM = mistakeM;
            ShotM = shotM;
            PShotTargetM = pShotTargetM;
            CrossSucM = crossSucM;
            PCrossSucM = pCrossSucM;
            PGoalChances = pGoalChances;
            PenaltyNb = penaltyNb;
            PPenaltyConv = pPenaltyConv;
            AssistsNb = assistsNb;
            ChancesCreated = chancesCreated;
            ChancesMiss = chancesMiss;
            KCleanSheet = kCleanSheet;
            KPSaveShot = kPSaveShot;
            KSaves = kSaves;
            KDeflect = kDeflect;
            KPenaltySave = kPenaltySave;
        }
    }

    public class AuPlayers : PlayersWorthStats
    {
        public string Au { get; set; }

        public AuPlayers(PlayersWorthStats inp)
            : base(inp)
        {
        }
    }

}
