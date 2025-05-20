using System.Drawing;
using Common.com.game.settings;

namespace Common.com.objects.immovables.Resources
{
    public class Tree : Resource
    {
        public Tree()
        {
            ForestersLodgeId = -1;
        }
        public int ForestersLodgeId { get; set; }
        public override Size TileSize => new Size(NumberManager.Two, NumberManager.Two);
        public override int ClassNumber => NumberManager.TwentyThree;
    }
}