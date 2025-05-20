using System.Collections.Generic;
using System.Drawing;
using Common.com.game.settings;

namespace Common.com.objects.immovables.Buildings
{
    public  class Sawmill : ResourceBuilding
    {

        public override Size TileSize => new Size(NumberManager.Four, NumberManager.Four);
        public override List<Terrain> TerrainRequirement => new List<Terrain> { Terrain.Grass };
        public override Item ActiveProvider => Item.Plank;
        public override Item[] ActiveRequester => new []{Item.Wood};
        public override int VisionRange => NumberManager.Five;
        public override int MaxHp => NumberManager.OneHundred;
        public override Items ResourceCost => new Items(new [] { Item.Wood }, new[] { NumberManager.Fifteen });
        public override int ClassNumber => NumberManager.TwentyOne;
        

        public override Items MaxResourcesStorable => new Items(
            new [] { Item.Wood, Item.Plank },
            new [] { NumberManager.Fifty, NumberManager.Fifty });
        
    }
}