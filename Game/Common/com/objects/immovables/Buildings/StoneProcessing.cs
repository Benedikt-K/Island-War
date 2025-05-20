using System.Collections.Generic;
using System.Drawing;
using Common.com.game.settings;

namespace Common.com.objects.immovables.Buildings
{
    public  class StoneProcessing : ResourceBuilding
    {
        public override Size TileSize => new Size(NumberManager.Four, NumberManager.Four);
        public override List<Terrain> TerrainRequirement => new List<Terrain> { Terrain.Grass };
        public override int VisionRange => NumberManager.Five;
        public override int MaxHp => NumberManager.OneHundred;
        public override Items ResourceCost => new Items(new [] { Item.Plank, Item.RawStone }, new [] { NumberManager.Five, NumberManager.Ten });
        public override int ClassNumber => NumberManager.Nineteen;
        public override Item ActiveProvider => Item.Stone;
        public override Item[] ActiveRequester => new []{Item.RawStone};
        
        public override Items MaxResourcesStorable => new Items(
            new [] { Item.RawStone, Item.Stone },
            new [] { NumberManager.Fifty, NumberManager.Fifty });
        
    }
}