using System.Collections.Generic;
using System.Drawing;
using Common.com.game.settings;

namespace Common.com.objects.immovables.Buildings
{
    public class Warehouse : ResourceBuilding
    {

        public override Size TileSize => new Size(NumberManager.Three, NumberManager.Three);
        public override List<Terrain> TerrainRequirement => new List<Terrain> { Terrain.Grass };
        public override int VisionRange => NumberManager.Five;
        public override int MaxHp => NumberManager.TwoHundred;
        public override Items ResourceCost => new Items(new [] { Item.Plank, Item.Stone }, new [] { NumberManager.Twenty, NumberManager.Twenty });
        public override int ClassNumber => NumberManager.Eleven;
        public override Item ActiveProvider => Item.Nothing;
        public override Item[] ActiveRequester => null;

        public override Items MaxResourcesStorable => new Items(
            new[] { Item.Wood, Item.RawStone, Item.IronOre, Item.Plank, Item.Stone, Item.Iron },
            new[] { NumberManager.TwoHundred, NumberManager.TwoHundred, NumberManager.TwoHundred, NumberManager.TwoHundred, NumberManager.TwoHundred, NumberManager.TwoHundred });
    }
}