using System.Collections.Generic;
using System.Drawing;
using Common.com.game;
using Common.com.game.settings;
using Common.com.networking.Messages;
using Common.com.networking.Messages.CommonMessages;
using Common.com.objects.entities.FightingUnit;
using Microsoft.Xna.Framework;

namespace Common.com.objects.immovables.Buildings
{
    public class Tower : Building
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public List<ObjectMoving> UnitsInside { get; set; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public List<ObjectMoving> SpyInside { get; set; }
        public Tower()
        {
            UnitsInside = new List<ObjectMoving>();
            SpyInside = new List<ObjectMoving>(1);
        }
        public override Size TileSize => new Size(NumberManager.Three, NumberManager.Three);
        public override List<Terrain> TerrainRequirement => new List<Terrain> { Terrain.Grass };
        public override int VisionRange => NumberManager.Five;
        public override int MaxHp => NumberManager.FiveHundred;
        public override Items ResourceCost => new Items(new [] { Item.Stone }, new [] { NumberManager.Ten });
        public override int ClassNumber => NumberManager.Fifteen;

        public void AddSpy(ObjectMoving spy)
        {
            SpyInside.Add(spy);
        }

        public Message LeaveTower()
        {
            var objectMoving = SpyInside[0];
            if (objectMoving != null)
            {
                var message = new LeaveTowerMessage(0, objectMoving, Id, false);
                return message;
            }
            return null;
        }

        public Message KillSpy()
        {
            var objectMoving = SpyInside[0];
            if (objectMoving != null)
            {
                var message = new NewObjectActionMessage(0, objectMoving.Id, new ObjectAction());
                return message;
            }

            return null;
        }
        public void RemoveSpy()
        {
            SpyInside.RemoveAt(0);
        }


        public void AddUnit(ObjectMoving unit)
        {
            UnitsInside.Add(unit);
        }

        public Message UnmanTower(int number)
        {
            if (UnitsInside.Count == 0)
            {
                return null;
            }
            if (UnitsInside.Count > number)
            {
                var objectMoving = UnitsInside?[number];
                if (objectMoving != null)
                {
                    var message = new UnloadTowerMessage(0, objectMoving, Id);
                    return message;
                }
            }
            return null;
        }

        public void RemoveUnit(int id)
        {
            for (var i = UnitsInside.Count - 1; i >= 0; i--)
            {
                if (UnitsInside[i].Id == id)
                {
                    UnitsInside.RemoveAt(i);
                }
            }
        }

        public string PikemenInsideString()
        {
            var count = 0;
            foreach (var objectMoving in UnitsInside)
            {
                if (objectMoving is Shieldman)
                {
                    count += 1;
                }
            }
            var res = "" + count;
            return res;
        }

        public string SpearmenInsideString()
        {
            var count = 0;
            foreach (var objectMoving in UnitsInside)
            {
                if (objectMoving is Spearman)
                {
                    count += 1;
                }
            }
            var res = "" + count;
            return res;
        }

        public string SwordsmenInsideString()
        {
            var count = 0;
            foreach (var objectMoving in UnitsInside)
            {
                if (objectMoving is Swordsman)
                {
                    count += 1;
                }
            }
            var res = "" + count;
            return res;
        }

        public int PikemenInsideInt()
        {
            var count = 0;
            foreach (var objectMoving in UnitsInside)
            {
                if (objectMoving is Shieldman)
                {
                    count += 1;
                }
            }
            return count;
        }

        public int SpearmenInsideInt()
        {
            var count = 0;
            foreach (var objectMoving in UnitsInside)
            {
                if (objectMoving is Spearman)
                {
                    count += 1;
                }
            }
            return count;
        }

        public int SwordsmenInsideInt()
        {
            var count = 0;
            foreach (var objectMoving in UnitsInside)
            {
                if (objectMoving is Swordsman)
                {
                    count += 1;
                }
            }
            return count;
        }

        public static Vector2 GetLandSpawnLocation(Tower tower, GameMap map)
        {
            var spawnPoint = new Vector2(-1, -1);
            for (var j = 0; j <= tower.TileSize.Height; j++)
            {

                for (var i = 0; i <= tower.TileSize.Width; i++) // 
                {
                    var yAdd = j - 1;
                    var xAdd = i;
                    if (j >= 1)
                    {
                        xAdd = tower.TileSize.Width;
                    }

                    var convertedLocationX = (uint)tower.Location.X + xAdd;
                    var convertedLocationY = (uint)tower.Location.Y + yAdd;
                    if (map.GetObject(tower.Location.X + xAdd,
                            tower.Location.Y + yAdd) == null &&
                        map.GetTerrainAt((uint)convertedLocationX,
                            (uint)convertedLocationY) != Terrain.Water &&
                        map.GetTerrainAt((uint)convertedLocationX,
                            (uint)convertedLocationY) != Terrain.OutOfBounds)
                    {
                        spawnPoint.X = (tower.Location.X + xAdd);
                        spawnPoint.Y = (tower.Location.Y + yAdd);
                        return spawnPoint;
                    }

                }
            }

            for (var j = tower.TileSize.Height + 1; j >= 0; j--)
            {

                for (var i = tower.TileSize.Width; i >= -1; i--)
                {
                    var yAdd = j - 1;
                    var xAdd = i;
                    if (j < tower.TileSize.Height + 1)
                    {
                        xAdd = -1;
                    }

                    var convertedLocationX = (uint)tower.Location.X + xAdd;
                    var convertedLocationY = (uint)tower.Location.Y + yAdd;
                    if (map.GetObject(tower.Location.X + xAdd,
                            tower.Location.Y + yAdd) == null &&
                        map.GetTerrainAt((uint)convertedLocationX,
                            (uint)convertedLocationY) != Terrain.Water &&
                        map.GetTerrainAt((uint)convertedLocationX,
                            (uint)convertedLocationY) != Terrain.OutOfBounds)
                    {
                        spawnPoint.X = (tower.Location.X + xAdd);
                        spawnPoint.Y = (tower.Location.Y + yAdd);
                        return spawnPoint;
                    }

                }
            }

            return spawnPoint;
        }
    }
}