using Common.com.game;
using Common.com.objects;

namespace Common.com.rollbacks.changes
{
    public sealed class ExpectedItemChange:IChange
    {
        private readonly int mBuildingId;
        private readonly int mAmount;
        private readonly Item mItem;
        private bool mDone;
        
        public ExpectedItemChange(Item item, int buildingId, int amount)
        {
            mItem = item;
            mBuildingId = buildingId;
            mAmount = amount;
        }
        public void DoChange(GameState gameState)
        {
            if (mDone)
            {
                return;
            }
            mDone = true;
            gameState.Map.GetLogisticsSystem().AddTaskItemAmount(mBuildingId,mItem,mAmount);
        }

        public void RevertChange(GameState gameState)
        {
            gameState.Map.GetLogisticsSystem().AddTaskItemAmount(mBuildingId,mItem,-mAmount);
        }
    }
}