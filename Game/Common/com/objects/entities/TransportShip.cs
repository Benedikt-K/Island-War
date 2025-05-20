using System.Collections.Generic;
using Common.com.game;
using Common.com.game.settings;
using Common.com.networking.Messages;
using Common.com.networking.Messages.CommonMessages;
using Common.com.objects.entities.FightingUnit;
using Microsoft.Xna.Framework;

namespace Common.com.objects.entities
{
    public class TransportShip : ObjectMoving
    {
        // ReSharper disable once MemberCanBePrivate.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public List<ObjectMoving> UnitsInside { get; set; }

        public TransportShip()
        {
            UnitsInside = new List<ObjectMoving>();
        }

        public override int VisionRange => NumberManager.Five;
        protected override int MaxHp => 1;
        public override int BaseSpeed => 1;
        public override int RequiredCap => NumberManager.Five;

        public override Items ResourceCost =>
            new Items(new[] {Item.Plank, Item.Stone}, new[] { NumberManager.Five, NumberManager.Three });

        public override bool OnLand => false;
        public override int ClassNumber => NumberManager.Two;
        public override int AttackDamage => 1;

        public override ObjectMoving Clone()
        {
            var transportShip = new TransportShip();
            transportShip.Override(this);
            transportShip.UnitsInside = new List<ObjectMoving>();
            foreach (var objectMoving in UnitsInside)
            {
                var newObjectMoving = objectMoving.Clone();
                transportShip.UnitsInside.Add(newObjectMoving);
            }
            return transportShip;
        }

        public void AddUnit(ObjectMoving unit)
        {
            UnitsInside.Add(unit);
        }

        public Message UnloadTransportShip(int number)
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
                    var message = new UnloadTransportShipMessage(0, objectMoving, Id);
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

        public string WorkerInsideString()
        {
            var count = 0;
            foreach (var objectMoving in UnitsInside)
            {
                if (objectMoving is Worker)
                {
                    count += 1;
                }
            }

            var res = "" + count;
            return res;
        }
        public int WorkerInsideInt()
        {

            var count = 0;
            foreach (var objectMoving in UnitsInside)
            {
                if (objectMoving is Worker)
                {
                    count += 1;
                }
            }

            return count;
        }

        public string SpiesInsideString()
        {
            var count = 0;
            foreach (var objectMoving in UnitsInside)
            {
                if (objectMoving is Spy)
                {
                    count += 1;
                }
            }

            var res = "" + count;
            return res;
        }
        public int SpiesInsideInt()
        {

            var count = 0;
            foreach (var objectMoving in UnitsInside)
            {
                if (objectMoving is Spy)
                {
                    count += 1;
                }
            }

            return count;
        }

        public static Vector2 GetLandSpawnLocation(TransportShip transportShip, GameMap map)
        {
            var spawnPoint = new Vector2(-1, -1);
            for (var j = -1; j <= 1; j++)
            {
                for (var i = -1; i <= 1; i++)
                {
                    var locationX = (int) transportShip.X + i;
                    var locationY = (int) transportShip.Y + j;
                    if (map.GetObject(locationX, locationY) == null &&
                        map.GetTerrainAt((uint) locationX, (uint) locationY) != Terrain.Water &&
                        map.GetTerrainAt((uint) locationX, (uint) locationY) != Terrain.OutOfBounds)
                    {
                        spawnPoint.X = transportShip.X + i;
                        spawnPoint.Y = transportShip.Y + j;
                        return spawnPoint;
                    }
                }
            }

            return spawnPoint;
        }
    }
}

