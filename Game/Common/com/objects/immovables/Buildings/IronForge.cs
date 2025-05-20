using System.Collections.Generic;
using System.Drawing;
using Common.com.game.settings;

namespace Common.com.objects.immovables.Buildings
{
    public class IronForge : ResourceBuilding
    {

        public override Size TileSize => new Size(NumberManager.Four, NumberManager.Four);
        public override List<Terrain> TerrainRequirement => new List<Terrain> { Terrain.Grass };
        public override int VisionRange => NumberManager.Five;
        public override int MaxHp => NumberManager.OneHundred;
        public override Items ResourceCost => new Items(new [] { Item.Stone }, new [] { NumberManager.Twenty });
        public override int ClassNumber => NumberManager.Twenty;
        public override Item ActiveProvider => Item.Iron;
        public override Item[] ActiveRequester => new []{Item.IronOre};

        public override Items MaxResourcesStorable => new Items(
            new [] { Item.IronOre, Item.Iron },
            new [] { NumberManager.Fifty, NumberManager.Fifty });
        
    }
}