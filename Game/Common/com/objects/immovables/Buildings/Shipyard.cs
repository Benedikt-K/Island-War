using System.Collections.Generic;
using System.Drawing;
using Common.com.game.settings;

namespace Common.com.objects.immovables.Buildings
{
    public  class Shipyard : ResourceBuilding
    {

        public override Item ActiveProvider => Item.Nothing;
        public override Item[] ActiveRequester => new []{Item.Plank, Item.Stone, Item.Iron};
        public override Size TileSize => new Size(NumberManager.Three, NumberManager.Three);
        public override List<Terrain> TerrainRequirement => new List<Terrain> { Terrain.Grass, Terrain.Water };
        public override int VisionRange => NumberManager.Five;
        public override int MaxHp => NumberManager.FiveHundred;
        public override Items ResourceCost => new Items(new [] { Item.Plank, Item.Stone }, new [] {NumberManager.Five, NumberManager.Twenty });
        public override int ClassNumber => NumberManager.Fourteen;
        public override Items MaxResourcesStorable => new Items(
            new [] { Item.Plank, Item.Stone, Item.Iron },
            new [] { NumberManager.Twenty, NumberManager.Twenty, NumberManager.Twenty });
    }
}