using System.Collections.Generic;
using System.Drawing;
using Common.com.game.settings;

namespace Common.com.objects.immovables.Buildings
{
    public class ForestersLodge : ResourceBuilding
    {

        public ForestersLodge()
        {
            Plants = true;
        }



        public static readonly int sRadius= 7;
        public override Size TileSize => new Size(NumberManager.Two, NumberManager.Two);
        public override List<Terrain> TerrainRequirement => new List<Terrain> { Terrain.Grass };
        public override int VisionRange => NumberManager.Five;
        public override int MaxHp => NumberManager.Forty;
        public override Items ResourceCost => new Items(new [] { Item.Plank }, new [] { NumberManager.Ten });
        public override int ClassNumber => NumberManager.Eighteen;
        public override Item ActiveProvider => Item.Wood;
        public override Item[] ActiveRequester => null;
        public override Items MaxResourcesStorable => new Items(
           new [] { Item.Wood },
           new [] { NumberManager.Fifty });
        public bool Plants { get; set; }

    }
}