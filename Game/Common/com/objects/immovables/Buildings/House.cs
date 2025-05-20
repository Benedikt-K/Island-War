using System.Collections.Generic;
using System.Drawing;
using Common.com.game.settings;

namespace Common.com.objects.immovables.Buildings
{
    public sealed class House : Building
    {
        public override Size TileSize => new Size(NumberManager.Two, NumberManager.Two);
        public override List<Terrain> TerrainRequirement => new List<Terrain> { Terrain.Grass };
        public override int VisionRange => NumberManager.Five;
        public override int MaxHp => NumberManager.Fifty;
        public override Items ResourceCost => new Items(new [] { Item.Plank, Item.Stone }, new [] { NumberManager.Ten, NumberManager.Three });
        public override int ClassNumber => NumberManager.Twelve;
        public const int HousingSpace=5;
    }
}