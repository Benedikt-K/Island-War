using System.Collections.Generic;
using System.Drawing;
using Common.com.game.settings;

namespace Common.com.objects.immovables.Buildings
{
    public  class StoneMine : ResourceBuilding
    {

        public override Size TileSize => new Size(NumberManager.Three, NumberManager.Three);
        public override List<Terrain> TerrainRequirement => new List<Terrain> { Terrain.Mountains };
        public override int VisionRange => NumberManager.Five;
        public override int MaxHp => NumberManager.TwoHundred;
        public override Items ResourceCost => new Items(new [] { Item.Plank }, new [] { NumberManager.Ten });
        public override int ClassNumber => NumberManager.Sixteen;

        public override Item ActiveProvider => Item.RawStone;
        public override Item[] ActiveRequester => null;

        public override Items MaxResourcesStorable => new Items(
            new [] { Item.RawStone },
            new [] { NumberManager.Fifty });
        
    }
}