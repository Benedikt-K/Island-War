using Common.com.game;
using Common.com.game.achievments;
using Common.com.objects;

namespace Common.com.rollbacks.changes
{
    public class ImmovableChange:IChange
    {
        private readonly ObjectImmovable mObjectImmovableOld;
        private readonly ObjectImmovable mObjectImmovableNew;
        private readonly bool mNewObject;
        private bool mDone;
        public ImmovableChange(ObjectImmovable objectImmovableOld,ObjectImmovable objectImmovableNew,bool newObject)
        {
            mObjectImmovableOld = objectImmovableOld;
            mObjectImmovableNew = objectImmovableNew;
            mNewObject = newObject;
        }
        public void DoChange(GameState gameState)
        {
            if (mDone)
            {
                return;
            }
            GameMap.StatisticsManager.AddStatistic(Statistic.BuildingsBuilt, mObjectImmovableOld.PlayerNumber);
            mDone = true;
            gameState.Map.RemoveObject(mObjectImmovableOld);
            gameState.Map.AddObject(mObjectImmovableNew,mNewObject);
        }

        public void RevertChange(GameState gameState)
        {
            GameMap.StatisticsManager.RemoveStatistic(Statistic.BuildingsBuilt, mObjectImmovableOld.PlayerNumber);
            gameState.Map.RemoveObject(mObjectImmovableNew);
            gameState.Map.AddObject(mObjectImmovableOld,mNewObject);
        }
    }
}