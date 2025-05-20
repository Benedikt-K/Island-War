using System.Drawing;
using Common.com.game.settings;

namespace Common.com.objects.immovables.Resources
{
    public sealed class IronDeposit : Resource, INonCollisional
    {

        public override Size TileSize => new Size(NumberManager.Two, NumberManager.Two);
        public override int ClassNumber => NumberManager.TwentyTwo;
    }
}