using System.Collections.Generic;
using System.Drawing;
using Common.com.game.settings;

namespace Common.com.objects.immovables.Buildings
{
    public class IronMine : ResourceBuilding
    {

        public override Size TileSize => new Size(NumberManager.Three, NumberManager.Three);
        public override List<Terrain> TerrainRequirement => new List<Terrain> { Terrain.Grass, Terrain.Mountains };
        public override int VisionRange => NumberManager.Five;
        public override int MaxHp => NumberManager.TwoHundred;
        public override Items ResourceCost => new Items(new [] { Item.Stone }, new [] { NumberManager.Twenty });
        public override int ClassNumber => NumberManager.Seventeen;
        public override Item ActiveProvider => Item.IronOre;
        public override Item[] ActiveRequester => null;

        public override Items MaxResourcesStorable => new Items(
            new [] { Item.IronOre },
            new[] { NumberManager.Fifty });
        
    }
}