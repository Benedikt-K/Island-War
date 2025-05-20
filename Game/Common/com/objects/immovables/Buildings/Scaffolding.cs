using System.Collections.Generic;
using System.Drawing;
using Common.com.game.settings;
using Common.com.objects.entities;
using Newtonsoft.Json;

namespace Common.com.objects.immovables.Buildings
{
    public sealed class Scaffolding : ResourceBuilding, INonCollisional
    {
        public const int MaxBlueprintSpaces=200;

        public Building TurnsInto { get; set; }
        public bool IsRoad { get; set; }

        public bool IsBridge { get; set; }
        

        public override Item ActiveProvider => Item.Nothing;
        [JsonIgnore]
        public override Item[] ActiveRequester
        {
            get
            {
                if (IsRoad || IsBridge)
                {
                    var res = new Item[ResourceCost.ItemAmounts.Count];
                    ResourceCost.ItemAmounts.Keys.CopyTo(res, 0);
                    return res;
                }
                else
                {
                    var res = new Item[TurnsInto.ResourceCost.ItemAmounts.Count];
                    TurnsInto.ResourceCost.ItemAmounts.Keys.CopyTo(res, 0);
                    return res;
                }
            }
        }

        [JsonIgnore]
        public override Size TileSize 
        {
            get
            {
                if (IsRoad || IsBridge)
                {
                    return new Size(1, 1);
                }
                
                return TurnsInto.TileSize;
            }
        }

        [JsonIgnore]
        public override List<Terrain> TerrainRequirement
        {
            get
            {
                if (IsRoad)
                {
                    return new List<Terrain> { Terrain.Grass, Terrain.Mountains };
                }

                if (IsBridge)
                {
                    return new List<Terrain> {Terrain.Water};
                }
                return TurnsInto.TerrainRequirement;
            }
        }
        [JsonIgnore]
        public override int VisionRange => 0;
        public override int MaxHp => NumberManager.Fifty;
        [JsonIgnore]
        public override Items ResourceCost
        {
            get
            {
                if (IsRoad)
                {
                    return new Items(new[] {Item.Plank}, new[] {NumberManager.One});
                }
                if (IsBridge)
                {
                    return new Items(new[] {Item.Stone, Item.Plank}, new[] {NumberManager.One, NumberManager.One});
                }

                return TurnsInto.ResourceCost;
            }
        }

        public override int ClassNumber => NumberManager.Thirteen;
        [JsonIgnore]
        public override Items MaxResourcesStorable
        {
            get
            {
                if (IsRoad || IsBridge)
                {
                    return ResourceCost;
                }
                return TurnsInto.ResourceCost;
            }
        }

        public override bool DepositResource(Worker worker)
        {
            
            var res= base.DepositResource(worker);
            if (res)
            {
                IsBlueprint = false;
            }
            return res;
        }
    }
}