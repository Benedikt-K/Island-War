using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Common.com.objects;
using Common.com.objects.entities;
using Common.com.rollbacks;

namespace Common.com.game
{
    public sealed class TaskPossibilitySystem
    {
        private static readonly LinkedList<TaskPossibilitySystem> sTaskPossibilitySystems=new LinkedList<TaskPossibilitySystem>();
        private int mCurrentIslandNumber;
        private readonly int mTick;
        private Dictionary<int, HashSet<int>> mIslandNumberToBuildings;
        private Dictionary<Point, int> mPointToIslandNumber;
        private static readonly Point[] sDirections = { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1) };

        public bool IsTick(int tick)
        {
            return mTick == tick;
        }
        public TaskPossibilitySystem(int tick)
        {
            mTick = tick;
            mPointToIslandNumber = new Dictionary<Point, int>();
            mIslandNumberToBuildings = new Dictionary<int, HashSet<int>>();
        }
        
        public static void IsTaskDoable(GameMap gameMap, Worker worker,  GameObject resourceBuilding,int tick)
        {
            
            while (sTaskPossibilitySystems.Last!=null&&sTaskPossibilitySystems.Last.Value.mTick>tick)
            {
                sTaskPossibilitySystems.RemoveLast();
            }
            while (sTaskPossibilitySystems.First != null && 
                   sTaskPossibilitySystems
                       .First
                       .Value
                       .mTick < 
                   tick-RollbackManager.DefaultSize&&
                   sTaskPossibilitySystems.Count>=1)
            {
                sTaskPossibilitySystems.RemoveFirst();
            }
            if (sTaskPossibilitySystems.Last==null)
            {
                sTaskPossibilitySystems.AddLast(new TaskPossibilitySystem(tick));
            }

            sTaskPossibilitySystems.Last?.Value.IsTaskDoable(gameMap, worker, resourceBuilding);
        }

        private TaskPossibilitySystem Clone()
        {
            return new TaskPossibilitySystem(mTick)
            {
                mCurrentIslandNumber = mCurrentIslandNumber,
                mIslandNumberToBuildings = mIslandNumberToBuildings.ToDictionary(entry => entry.Key, 
                    entry => new HashSet<int>(entry.Value)),
                mPointToIslandNumber = mPointToIslandNumber.ToDictionary(entry => entry.Key,
                    entry => entry.Value)
            };
        }
        public bool IsTaskDoable(GameMap gameMap, Worker worker,  GameObject resourceBuilding)
        {
            if (resourceBuilding == null)
            {
                return false;
            }
            var p = new Point((int)worker.X,(int)worker.Y);
            Update(p,gameMap);
            return mIslandNumberToBuildings[mPointToIslandNumber[p]].Contains(resourceBuilding.Id);
            
        }
        private void Update(Point p, GameMap gameMap)
        {
            var newIslandNumber=-1;
            var checkedPoints = new HashSet<Point>();
            var toCheck = new HashSet<Point>();
            var resourceBuildings = new HashSet<int>();
            toCheck.Add(p);
            checkedPoints.Add(p);
            while (toCheck.Count != 0)
            {
                var p2 = toCheck.First();
                toCheck.Remove(p2);
                foreach (var dar in sDirections)
                {
                    var p2Neighbor = new Point(p2.X + dar.X, p2.Y + dar.Y);
                    if (checkedPoints.Contains(p2Neighbor))
                    {
                        continue;
                    }
                    if(!gameMap.InBounds(p2Neighbor.X, p2Neighbor.Y))
                    {
                        continue;
                    }
                    var terrain = gameMap.GetTerrainAt((uint)p2Neighbor.X, (uint)p2Neighbor.Y);
                    

                    var gameObject = gameMap.GetObject(p2Neighbor.X, p2Neighbor.Y);
                    if (gameMap.GetSecondaryObject(p2Neighbor.X, p2Neighbor.Y)!=null)
                    {
                        continue;
                    }
                    if (gameObject is ResourceBuilding)
                    {
                        resourceBuildings.Add(gameObject.Id);
                    }
                    if (gameObject != null&&!(gameObject is INonCollisional)&&!(gameObject is ObjectMoving))
                    {
                        
                        continue;
                    }
                    checkedPoints.Add(p2Neighbor);
                    if (terrain == Terrain.Water)
                    {
                        continue;
                    }
                    if (mPointToIslandNumber.ContainsKey(p2Neighbor)&&newIslandNumber==-1)
                    {
                        newIslandNumber = mPointToIslandNumber[p2Neighbor];
                    }

                    if (mPointToIslandNumber.ContainsKey(p2Neighbor)&&mPointToIslandNumber[p2Neighbor]==newIslandNumber)
                    {
                        continue;
                    }
                    toCheck.Add(p2Neighbor);
                    
                }

                if (newIslandNumber == -1)
                {
                    newIslandNumber = mCurrentIslandNumber++;
                }

                
            }

            TaskPossibilitySystem res = null;
            foreach (var point in checkedPoints)
            {
                if ((!mPointToIslandNumber.ContainsKey(point)||mPointToIslandNumber[point]!=newIslandNumber)&&res==null)
                {
                    res = Clone();
                }
                mPointToIslandNumber[point] = newIslandNumber;
            }

            if(!mIslandNumberToBuildings.ContainsKey(newIslandNumber))
            {
                mIslandNumberToBuildings[newIslandNumber] = resourceBuildings;
            }
            else
            {
                mIslandNumberToBuildings[newIslandNumber].UnionWith(resourceBuildings);
            }
        }
    }
}