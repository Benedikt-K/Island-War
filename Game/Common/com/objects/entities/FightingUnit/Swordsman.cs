using Common.com.game.settings;
using Common.com.path;
using Microsoft.Xna.Framework;

namespace Common.com.objects.entities.FightingUnit
{
    public class Swordsman : ObjectMoving
    {
        public Swordsman()
        {
        }

        public Swordsman(int playerNumber, int id, Vector2 position, Path currentPath, double currentHp) : base(playerNumber, id, position, currentPath, currentHp)
        {
        }

        public override ObjectMoving Clone()
        {
            var swordsman = new Swordsman();
            swordsman.Override(this);
            return swordsman;
        }

        public override int VisionRange => NumberManager.Five;
        protected override int MaxHp => NumberManager.Two * NumberManager.Five;
        public override int BaseSpeed => 1;
        public override int AttackDamage => 1;
        public override int RequiredCap => NumberManager.Two;
        public override Items ResourceCost => new Items(new [] { Item.Plank, Item.Iron }, new [] { NumberManager.One, NumberManager.Six });
        public override bool OnLand => true;

        public override int ClassNumber => NumberManager.Six;
    }
}
