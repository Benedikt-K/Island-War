using Common.com.game.settings;

namespace Common.com.objects.entities
{
    public class ScoutShip : ObjectMoving
    {
        public override ObjectMoving Clone()
        {
            var scoutShip = new ScoutShip();
            scoutShip.Override(this);
            return scoutShip;
        }
        public override int VisionRange => NumberManager.Ten;
        protected override int MaxHp => 1;
        public override int BaseSpeed => 1;
        public override int RequiredCap => NumberManager.Four;
        public override Items ResourceCost => new Items(new [] {Item.Plank, Item.Iron}, new [] { NumberManager.Two, NumberManager.Five });
        public override bool OnLand => false;
        public override int ClassNumber => NumberManager.Three;
        public override int AttackDamage => 1;
    }
}
