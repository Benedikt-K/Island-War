using System.Collections.Generic;
using Common.com.game.settings;
using Common.com.Menu;
using Common.com.objects;

namespace Common.com.game.achievments
{
    
    public sealed class StatisticsManager
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public Dictionary<Statistic,int>[] StatisticsPlayer { get; set; } //Dont make private

        public StatisticsManager()
        {
            StatisticsPlayer = new Dictionary<Statistic, int>[NumberManager.Two];
            StatisticsPlayer[0] = new Dictionary<Statistic, int>();
            StatisticsPlayer[1] = new Dictionary<Statistic, int>();
        }

        public void OnNewLoad()
        {
            for (var i=0;i< NumberManager.Two;i++)
            {
                StatisticsPlayer[i][Statistic.BlueprintSpaces] = 0;
            }
        }
        public StatisticTracker GetTracker(Statistic statistic, int playerNum,string extra="")
        {
            return new StatisticTracker(statistic,playerNum,this,extra);
        }
        private Dictionary<Statistic,int> GetStat(int playerNum)
        {
            return StatisticsPlayer[playerNum - 1];
        }

        public sealed class StatisticTracker:INumber
        {
            private readonly Statistic mStatistic;
            private readonly int mPlayerNum;
            private readonly StatisticsManager mStatisticsManager;
            private readonly string mExtra;
            public StatisticTracker(Statistic statistic, int playerNum,StatisticsManager statisticsManager,string extra="")
            {
                mPlayerNum = playerNum;
                mStatistic = statistic;
                mStatisticsManager = statisticsManager;
                mExtra = extra;
            }

            public string Number => ""+mStatisticsManager.GetStatistic(mStatistic, mPlayerNum)+mExtra;
        }
        public int GetStatistic(Statistic statistic,int playerNum)
        {
            if (GetStat(playerNum).ContainsKey(statistic))
            {
                return GetStat(playerNum)[statistic];
            }

            return 0;
        }
        public void AddStatistic(Statistic statistic,int playerNum,int amount=1)
        {
            var stat = GetStat(playerNum);
            if (!stat.ContainsKey(statistic))
            {
                stat.Add(statistic,amount);
            }
            else
            {
                stat[statistic] +=amount;
            }
            AchievementManager.GetManager().OnStatisticUpdate(playerNum);
        }

        public void MaxStat(Statistic statistic, int playerNum, int newStat)
        {
            var stat = GetStat(playerNum);
            if (!stat.ContainsKey(statistic))
            {
                stat.Add(statistic,newStat);
            }
            else
            {
                if (stat[statistic]<newStat)
                {
                    stat[statistic] = newStat;
                }
            }
        }
        public static Statistic GetStatistic(Item resource)
        {
            switch (resource)
            {
                case Item.Iron:
                    return Statistic.IronProduced;
                case Item.Plank:
                    return Statistic.PlanksProduced;
                case Item.Stone:
                    return Statistic.StonesProduced;
                case Item.Wood:
                    return Statistic.WoodCollected;
                case Item.RawStone:
                    return Statistic.StonesCollected;
                case Item.IronOre:
                    return Statistic.OreCollected;
            }

            return Statistic.BridgesBuilt;
        }

        public void RemoveStatistic(Statistic statistic, int playerNum,int amount=1)
        {
            GetStat(playerNum)[statistic]-=amount;
        }

        public int MoreTowers()
        {
            var diff=StatisticsPlayer[1][Statistic.TowersStanding]-StatisticsPlayer[0][Statistic.TowersStanding];
            if (diff > 0)
            {
                return NumberManager.Two;
            }

            if (diff < 0)
            {
                return 1;
            }

            return 0;
        }
        public void OnEndGame(GameState gameState)
        {
            MaxStat(Statistic.GameTime,1,gameState.TickNr);
            MaxStat(Statistic.GameTime, NumberManager.Two,gameState.TickNr);
        }
    }
}