using System.Collections.Generic;
using System.Drawing;
using Common.com.game.settings;

namespace Common.com.objects.immovables.Buildings
{
    public  class WorkerTrainingCenter : ResourceBuilding
    {
        public override Size TileSize => new Size(NumberManager.Three, NumberManager.Three);
        public override List<Terrain> TerrainRequirement => new List<Terrain> { Terrain.Grass };
        public override int VisionRange => NumberManager.Five;
        public override Item ActiveProvider => Item.Nothing;
        public override Item[] ActiveRequester => new []{Item.Stone,Item.Plank};
        public override int MaxHp => NumberManager.OneHundredFifty;
        public override Items ResourceCost => new Items(new [] { Item.Wood, Item.Stone }, new [] { NumberManager.Five, NumberManager.Ten });
        public override int ClassNumber => NumberManager.Ten;

        

        public override Items MaxResourcesStorable => new Items(
            new [] { Item.Plank, Item.Stone},
            new [] { NumberManager.Twenty, NumberManager.Twenty });
        
    }
}