using Common.com.game;
using Common.com.objects;

namespace Common.com.rollbacks.changes
{
    class ObjectRemoveChange : IChange
    {
        private readonly ObjectMoving mObjectMoving;
        public ObjectRemoveChange(ObjectMoving objectMoving)
        {
            mObjectMoving = objectMoving;
        }
        public void DoChange(GameState gameState)
        {
            gameState.Map.RemoveObject(mObjectMoving);
        }

        public void RevertChange(GameState gameState)
        {
            // empty
        }
    }
}
