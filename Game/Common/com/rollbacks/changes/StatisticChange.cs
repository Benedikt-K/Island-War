using Common.com.game;
using Common.com.game.achievments;

namespace Common.com.rollbacks.changes
{
    public sealed class StatisticChange:IChange
    {
        private readonly int mPlayerNum;
        private readonly Statistic mStatistic;
        public StatisticChange(int playerNum, Statistic statistic)
        {
            mPlayerNum = playerNum;
            mStatistic = statistic;
        }
        public void DoChange(GameState gameState)
        {
            GameMap.StatisticsManager.AddStatistic(mStatistic,mPlayerNum);
        }

        public void RevertChange(GameState gameState)
        {
            GameMap.StatisticsManager.RemoveStatistic(mStatistic,mPlayerNum);
        }
    }
}