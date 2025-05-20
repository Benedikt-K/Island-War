using Common.com.game;
using Common.com.game.achievments;
using Common.com.objects;

namespace Common.com.rollbacks.changes
{
    public sealed class ProductionChange:IChange
    {
        private readonly Item mItem1;
        private readonly Item mItem2;
        private readonly int mBuildingId;
        private readonly int mAmount1;
        private readonly int mAmount2;
        private bool mHappened;
        public ProductionChange(Item item1,Item item2, int buildingId,int amount1,int amount2)
        {
            mAmount1 = amount1;
            mAmount2 = amount2;
            mItem1 = item1;
            mItem2 = item2;
            mBuildingId = buildingId;
        }
        public ProductionChange(Item item, int buildingId,int amount)
        {
            mAmount1 = amount;
            mAmount2 = 0;
            mItem1 = item;
            mItem2 = Item.Nothing;
            mBuildingId = buildingId;
        }
        public void DoChange(GameState gameState)
        {
            if(gameState.Map.GetObject(mBuildingId) is ResourceBuilding resourceBuilding && resourceBuilding.CanChangeItem(mItem1, mAmount1) && resourceBuilding.CanChangeItem(mItem2, mAmount2))
            {
                GameMap.StatisticsManager.AddStatistic(StatisticsManager.GetStatistic(mItem1),resourceBuilding.PlayerNumber);
                resourceBuilding.ChangeItem(mItem1, mAmount1);
                resourceBuilding.ChangeItem(mItem2, mAmount2);
                mHappened = true;
            }
        }

        public void RevertChange(GameState gameState)
        {
            if(gameState.Map.GetObject(mBuildingId) is ResourceBuilding resourceBuilding&&mHappened)
            {
                GameMap.StatisticsManager.RemoveStatistic(StatisticsManager.GetStatistic(mItem1),resourceBuilding.PlayerNumber);
                resourceBuilding.ChangeItem(mItem1, -mAmount1);
                resourceBuilding.ChangeItem(mItem2, -mAmount2);
            }
        }
    }
}