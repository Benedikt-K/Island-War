using System.Collections.Generic;
using System.Drawing;
using Common.com.game.settings;

namespace Common.com.objects.immovables.Buildings
{
    public  class MainBuilding : ResourceBuilding
    {


        public MainBuilding()
        {
            CurrentResourcesStored.ItemAmounts[Item.Plank] = NumberManager.OneHundred;
            CurrentResourcesStored.ItemAmounts[Item.Stone] = NumberManager.OneHundred;
        }

        public override Item ActiveProvider => Item.Nothing;
        public override Item[] ActiveRequester => null;
        public override Size TileSize => new Size(NumberManager.Six, NumberManager.Six);
        public override List<Terrain> TerrainRequirement => new List<Terrain> { Terrain.Grass };
        public override int VisionRange => NumberManager.Five;
        public override int MaxHp => NumberManager.OneThousand;
        public override Items ResourceCost => new Items(new Item[] {}, new int[] {});
        public override int ClassNumber => NumberManager.Eight;



        public override Items MaxResourcesStorable => new Items(
            new [] { Item.Wood, Item.RawStone, Item.IronOre, Item.Plank, Item.Stone, Item.Iron }, 
            new [] { NumberManager.FiveThousand, NumberManager.FiveThousand, NumberManager.FiveThousand, NumberManager.FiveThousand, NumberManager.FiveThousand, NumberManager.FiveThousand });
        }
}