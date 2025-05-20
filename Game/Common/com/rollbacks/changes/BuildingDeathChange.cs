using Common.com.game;
using Common.com.game.achievments;
using Common.com.game.settings;
using Common.com.objects;


namespace Common.com.rollbacks.changes
{
    public sealed class BuildingDeathChange:IChange
    {
        private readonly Building mDeadBuilding;
        private bool mDone;
        private readonly int mDamage;
        public BuildingDeathChange(Building building,int damage)
        {
            mDeadBuilding = building;
            mDamage = damage;
        }

        public void DoChange(GameState gameState)
        {
            if (mDone)
            {
                return;
            }

            mDeadBuilding.CurrentHp -= mDamage;
            if (mDeadBuilding.CurrentHp <= 0)
            {
                GameMap.StatisticsManager.AddStatistic(Statistic.EnemyBuildingsDestroyed,
                    NumberManager.Three - mDeadBuilding.PlayerNumber);
                gameState.Map.RemoveObject(mDeadBuilding);
            }

            mDone = true;

        }

        public void RevertChange(GameState gameState)
        {
            if (mDeadBuilding.CurrentHp <= 0)
            {
                GameMap.StatisticsManager.RemoveStatistic(Statistic.EnemyBuildingsDestroyed,
                    NumberManager.Three - mDeadBuilding.PlayerNumber);
                gameState.Map.AddObject(mDeadBuilding);
            }

            mDeadBuilding.CurrentHp += mDamage;
        }
    }
}