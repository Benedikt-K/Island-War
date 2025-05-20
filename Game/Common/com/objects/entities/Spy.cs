using Common.com.game.settings;
using Common.com.path;
using Microsoft.Xna.Framework;

namespace Common.com.objects.entities
{
    public class Spy : ObjectMoving
    {
        public Spy(int playerNumber, int id, Vector2 position, Path currentPath, double currentHp) : base(playerNumber, id, position, currentPath, currentHp)
        {
        }

        public Spy()
        {

        }

        public override ObjectMoving Clone()
        {
            var spy = new Spy();
            spy.Override(this);
            return spy;
        }

        public override int VisionRange => NumberManager.Ten;
        protected override int MaxHp => 1;
        public override int BaseSpeed => 1;
        public override int AttackDamage => 1;
        public override int RequiredCap => NumberManager.Three;
        public override Items ResourceCost => new Items(new [] { Item.Plank, Item.Stone, Item.Iron }, new [] { NumberManager.One, NumberManager.One, NumberManager.Six});
        public override bool OnLand => true;
        public override int ClassNumber => NumberManager.Four;
    }
}
