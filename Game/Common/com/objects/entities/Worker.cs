using Common.com.game;
using Common.com.game.settings;
using Common.com.path;
using Microsoft.Xna.Framework;

namespace Common.com.objects.entities
{
    public class Worker : ObjectMoving
    {

        public Item HoldingItem { get; set; }
        public override int RequiredCap => 1;
        public Worker(int playerNumber, int id, Vector2 position, Path currentPath, double currentHp) : base(playerNumber, id, position, currentPath, currentHp)
        {
        }

        public Worker()
        {
        }

        public override ObjectMoving Clone()
        {
            var worker = new Worker();
            worker.Override(this);
            worker.HoldingItem = HoldingItem;
            return worker;
        }

        public override void RedoPath(GameMap gameMap,int radius=4)
        {
            if (CurrentAction.TransportingFromId != -1)
            {

                if (CurrentAction.TransportingToId == -1)
                {
                    if (!PathTo(gameMap, CurrentAction.TransportingFromId, false))
                    {
                        gameMap.GetLogisticsSystem().RemoveTask(this,gameMap,false);
                    }
                }
                else
                {
                    if (!PathTo(gameMap,
                        HasItemRequested()
                            ? CurrentAction.TransportingToId
                            : CurrentAction.TransportingFromId,
                        false))
                    {
                        gameMap.GetLogisticsSystem().RemoveTask(this,gameMap,false);
                    }
                }
            }
            else
            {
                base.RedoPath(gameMap,0);
            }

            
        }

        public bool HasItemRequested()
        {
            return CurrentAction.ItemTransportIntent == HoldingItem;
        }
        public override int VisionRange => NumberManager.Five;
        protected override int MaxHp => NumberManager.Three;
        public override int BaseSpeed => 1;
        public override int AttackDamage => 1;
        public override Items ResourceCost => new Items(new []{Item.Stone,Item.Plank},new []{ NumberManager.Three, NumberManager.Three });
        public override bool OnLand => true;
        public override int ClassNumber => 1;
    }
}