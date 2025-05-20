using System.Drawing;
using Common.com.game.settings;

namespace Common.com.objects.immovables
{
    public sealed class Corpse : ObjectImmovable
    {
        public override Size TileSize => new Size(1, 1);
        public override int ClassNumber => NumberManager.TwentyFour;
    }
}