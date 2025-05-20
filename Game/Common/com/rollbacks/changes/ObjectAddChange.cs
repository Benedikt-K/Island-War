using Common.com.game;
using Common.com.objects;
using Common.com.objects.entities;
using Common.com.objects.entities.FightingUnit;
using Common.com.objects.immovables.Buildings;

namespace Common.com.rollbacks.changes
{
    class ObjectAddChange : IChange
    {
        private readonly ObjectMoving mObjectMoving;
        private readonly GameObject mGameObject;
        public ObjectAddChange(ObjectMoving objectMoving, GameObject gameObject)
        {
            mObjectMoving = objectMoving;
            mGameObject = gameObject;
        }
        public void DoChange(GameState gameState)
        {
            if (mGameObject is Tower tower && (mObjectMoving is Shieldman || mObjectMoving is Spearman || mObjectMoving is Swordsman))
            {
                tower.AddUnit(mObjectMoving);
            }
            else if (mGameObject is TransportShip transportShip)
            {
                transportShip.AddUnit(mObjectMoving);
            }
            else if (mGameObject is Tower tower2 && mObjectMoving is Spy)
            {
                tower2.AddSpy(mObjectMoving);
            }
        }

        public void RevertChange(GameState gameState)
        {
            if (mGameObject is Tower tower && (mObjectMoving is Shieldman || mObjectMoving is Spearman || mObjectMoving is Swordsman))
            {
                tower.RemoveUnit(mObjectMoving.Id);
            }
            else if (mGameObject is TransportShip transportShip)
            {
                transportShip.RemoveUnit(mObjectMoving.Id);
            }
            else if (mGameObject is Tower tower2 && mObjectMoving is Spy)
            {
                tower2.RemoveSpy();
            }
        }
    }
}