using System;
using System.Collections.Generic;
using System.Drawing;
using Common.com.objects;
using Common.com.objects.immovables.Buildings;

namespace Common.com.game.Map
{
    public class IslandMapper
    {
        private static readonly Point[] sDirections = { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1), new Point(0,0) };
        private int IslandCount => mIslandUsed.Count;
        private readonly List<Point> mIslandUsed;
        public Point BestSpawnTop { get; private set; }
        public Point BestSpawnBottom { get; private set; }
        private readonly Dictionary<Point, int> mIslandMapping=new Dictionary<Point, int>();

        private const int Four = 4,Two=2;
        public IslandMapper(GameMap gameMap)
        {
            mIslandUsed = new List<Point>();
            CreateMapping(gameMap);
        }
        
        private void CreateMapping(GameMap gameMap)
        {
            for (var x = 0; x < gameMap.GetSize().Width; x++)
            {
                for (var y = 0; y < gameMap.GetSize().Height; y++)
                {
                    ExtendMapping(new Point(x,y),gameMap);
                }
            }
        }

        private double SpawnVal(GameMap gameMap,Point point,bool top)
        {
            return Math.Pow(gameMap.GetSize().Width/Two-point.X,Two)+Math.Pow(top?gameMap.GetSize().Height-point.Y:point.Y,Two);
        }

        private Point GetBestPoint(bool top)
        {
            return top ? BestSpawnTop : BestSpawnBottom;
        }
        private void UpdateBestSpawn(GameMap gameMap,Point point,bool top)
        {
            if (CanBeSpawn(gameMap, point)&&SpawnVal(gameMap,point,top)<SpawnVal(gameMap,GetBestPoint(top),top))
            {
                if (top)
                {
                    BestSpawnTop = point;
                }
                else
                {
                    BestSpawnBottom = point;
                }
            }
        }
        private bool CanBeSpawn(GameMap gameMap,Point point)
        {
            for (var x = -Four; x < new MainBuilding().GetBounds().Width-1; x++)
            {
                for (var y = -Four; y < new MainBuilding().GetBounds().Width-1; y++)
                {
                    if (!gameMap.InBounds(x + point.X,y + point.Y)||gameMap.GetTerrainAt((uint)(x + point.X), (uint)(y + point.Y))!=Terrain.Grass||gameMap.GetObject(x+ point.X,y+ point.Y)!=null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        private void SetMapping(Point p, ISet<Point> newEvaluating, GameMap gameMap)
        {
            UpdateBestSpawn(gameMap,p,true);
            UpdateBestSpawn(gameMap,p,false);
            mIslandMapping[p] = IslandCount;
            foreach (var dir in sDirections)
            {
                var res = new Point(p.X + dir.X, p.Y + dir.Y);
                if (gameMap.InBounds(res.X,res.Y)&&GetIslandNum(res) == -1)
                {
                    var terrain = gameMap.GetTerrainAt((uint) res.X, (uint) res.Y);
                    if (terrain == Terrain.Grass || terrain == Terrain.Mountains || terrain==Terrain.Road)
                    {
                        newEvaluating.Add(res);
                    }
                }
            }
        }
        private void ExtendMapping(Point p, GameMap gameMap)
        {
            if (!gameMap.InBounds(p.X,p.Y)||GetIslandNum(p) != -1)
            {
                return;
                
            }
            var terrain = gameMap.GetTerrainAt((uint) p.X, (uint) p.Y);
            if (terrain != Terrain.Grass && terrain != Terrain.Mountains && terrain!=Terrain.Road)
            {
                return;
            }
            var evaluating=new HashSet<Point> {p};
            while (evaluating.Count != 0)
            {
                var newEval = new HashSet<Point>();
                foreach (var t in evaluating)
                {
                    SetMapping(t,newEval,gameMap);
                }

                evaluating = newEval;
            }
            mIslandUsed.Add(new Point(-1,-1));
        }

        public bool CanAddTower(GameMap gameMap,ObjectImmovable tower)
        {
            if (GetIslandNum(tower.Location)==-1)
            {
                return false;
            }
            var islandTower=mIslandUsed[GetIslandNum(tower.Location)];
            if (islandTower.X!=-1&&(gameMap.GetObject(islandTower.X, islandTower.Y) is Tower||gameMap.GetObject(islandTower.X, islandTower.Y) is MainBuilding))
            {
                return false;
            }
            return true;
        }

        public bool OnAddTower(GameMap gameMap,ObjectImmovable tower)
        {
            if (CanAddTower(gameMap,tower))
            {
                mIslandUsed[GetIslandNum(tower.Location)] = tower.Location;
                return true;
            }

            return false;
        }

        private int GetIslandNum(Point point)
        {
            if (!mIslandMapping.ContainsKey(point))
            {
                return -1;
            }
            return mIslandMapping[point];
        }
    }
}