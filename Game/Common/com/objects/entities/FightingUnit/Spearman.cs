using Common.com.game.settings;
using Common.com.path;
using Microsoft.Xna.Framework;

namespace Common.com.objects.entities.FightingUnit
{
    public class Spearman : ObjectMoving
    {
        public Spearman()
        {
        }
        public Spearman(int playerNumber, int id, Vector2 position, Path currentPath, double currentHp) : base(playerNumber, id, position, currentPath, currentHp)
        {
        }

        public override ObjectMoving Clone()
        {
            var spearman = new Spearman();
            spearman.Override(this);
            return spearman;
        }
        public override int VisionRange => NumberManager.Five;
        protected override int MaxHp => NumberManager.Three * NumberManager.Five;
        public override int BaseSpeed => 1;
        public override int AttackDamage => 1;
        public override int RequiredCap => NumberManager.Three;
        public override Items ResourceCost => new Items(new [] { Item.Plank, Item.Iron }, new [] { NumberManager.Three, NumberManager.One });
        public override bool OnLand => true;

        public override int ClassNumber => NumberManager.Five;
    }
}
