using System.Drawing;
using System.Collections.Generic;
using Common.com.game;
using Common.com.objects;
using Priority_Queue;

namespace Common.com.path
{
    public class PathAbstract
    {
        private static readonly Point[] sDirections = { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1), new Point(0,0) };
        //private static readonly Point[] sDirectionsW = { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1)};
        private static int sMaxWaitTime=1;
        public static int mTimeToWait = 12;
        // ReSharper disable once MemberCanBeProtected.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public Point Start { get; set; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignoredl
        public Point End { get; set; }
        public static readonly int sMaxSpeed=6;
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        // ReSharper disable once MemberCanBeProtected.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public bool InWater { get; set; }

        public PathAbstract(Point start, Point end, bool inWater)
        {
            Start = start;
            End = end;
            InWater = inWater;
        }

        private void CalculatePositions(int time, IReadOnlyCollection<Path> intersecting, GameMap map, IList<LinkedListNode<Point>[]> pathPositions,IList<int[]> pathProgress, bool isBoat) 
        {
            for (var i = pathPositions.Count; i <= time; i++)
            {
                var nextPoints = new LinkedListNode<Point>[intersecting.Count];
                
                pathProgress.Add(new int[intersecting.Count]);
                for (var j=0;j<intersecting.Count;j++)
                {
                    
                    var pathPosition = pathPositions[i-1][j];

                        if (pathPosition.Next == null)
                        {
                            nextPoints[j] = pathPosition;
                            pathProgress[i][j] = 0;
                            continue;
                        }
                        var progress = pathProgress[i - 1][j] + map.GetSpeed(pathPosition.Value.X, pathPosition.Value.Y, isBoat);

                        if (progress >= sMaxSpeed)
                        {
                            progress -= sMaxSpeed;
                            nextPoints[j] = pathPosition.Next;

                        }
                        else
                        {
                            nextPoints[j] = pathPosition;
                        }
                        pathProgress[i][j] = progress;
                    
                }
                pathPositions.Add(nextPoints);
            }
        }
        /*
        private int FindSimplePath(GameMap map,
            HashSet<Point> endPoints = null, bool avoidRoads = false)
        {
            var openList = new SimplePriorityQueue<Node>();
            var closedList = new HashSet<Point>();
            var current = new Node(map, Start, null,InWater,null,0,false,avoidRoads);
            openList.Enqueue(current,0);
            while (openList.Count != 0)
            {
                current=openList.Dequeue();
                foreach (var p in sDirectionsW)
                {
                    var newX = p.X + current.NodeCoordinates.X;
                    var newY = p.Y + current.NodeCoordinates.Y;
                    var newPoint = new Point(newX,newY);
                    if (closedList.Contains(newPoint))
                    {
                        continue;
                    }
                    //openList.Enqueue(newPoint,);
                    closedList.Add(newPoint);
                }
            }

            return -1;
        }*/
        public Path FindPath(GameMap map, Path[] intersecting, HashSet<int> ignoreMoving=null, HashSet<Point> endPoints=null,bool avoidRoads=false)
        {
            if (0==map.GetSpeed(Start.X, Start.Y,InWater))
            {
                return null;
            }
            var representative = new Dictionary<Node, Node>();
            var pathPositions=new List<LinkedListNode<Point>[]>();
            pathPositions.Add(new LinkedListNode<Point>[intersecting.Length]);
            var pathProgress = new List<int[]>();
            pathProgress.Add(new int[intersecting.Length]);
            var oldPath = -1;
            for (var j = 0; j < intersecting.Length; j++)
            {
                pathPositions[0][j] = intersecting[j].WayPoints.First;
                pathProgress[0][j] = 0;
                if (intersecting[j].Start == Start)
                {
                    oldPath = j;
                }
            }
            var openList = new SimplePriorityQueue<Node>();
            var closedList = new HashSet<Node>();
        
            var currentNode = new Node(map, Start, null,InWater,null,0,false,avoidRoads);
            openList.Enqueue(currentNode, 0);

            while (openList.Count != 0)
            {
                var res=OneStep();
                if (res != null)
                {
                    return res;
                }
            }
            return null;

            Path OneStep()
            {
                currentNode = openList.Dequeue();
                if (currentNode.NodeCoordinates.Equals(End)||endPoints!=null&&endPoints.Contains(currentNode.NodeCoordinates))
                {
                    var result = new LinkedList<Point>();
                    while (currentNode.NodeParent != null)
                    {
                        result.AddFirst(currentNode.NodeCoordinates);
                        currentNode = currentNode.NodeParent;
                    }
                    result.AddFirst(currentNode.NodeCoordinates);
                    return new Path(result, InWater);
                }

                closedList.Add(currentNode);
                ExpandNode(currentNode);
                return null;
            }
            void ExpandNode(Node current) {
                foreach (var p in sDirections)
                {
                    var newX = p.X + current.NodeCoordinates.X;
                    var newY = p.Y + current.NodeCoordinates.Y;
                    var newPoint = new Point(newX,newY);
                    var speedAtTile = map.GetSpeed(newX, newY, InWater);
                    if (map.GetSecondaryObject(newX, newY) is ObjectMoving objectMoving)
                    {
                        if ((ignoreMoving == null || !ignoreMoving.Contains(objectMoving.Id))&&Start==current.NodeCoordinates)
                        {
                            continue;
                        }
                        if (objectMoving.CurrentPath == null ||objectMoving.CurrentPath.WayPoints==null||
                            objectMoving.CurrentPath.WayPoints.First == objectMoving.CurrentPath.WayPoints.Last)
                        {
                            if (ignoreMoving == null || !ignoreMoving.Contains(objectMoving.Id))
                            {
                                continue;
                            }
                        }
                    }
                    if(speedAtTile != 0)
                    {
                        CalculatePositions(current.CostToNode,intersecting,map,pathPositions,pathProgress,InWater);

                        var possible=true;
                        for (var i = 0; i < pathPositions[current.CostToNode].Length;i++)
                        {
                            if (intersecting[i]==null||intersecting[i].WayPoints==null||intersecting[i].End==intersecting[i].Start)
                            {
                                continue;
                            }
                            var collision = newPoint.Equals(pathPositions[current.CostToNode][i].Value) ||
                                            (pathPositions[current.CostToNode][i].Previous!=null&&
                                             pathPositions[current.CostToNode][i].Next!=null&&
                                             pathPositions[current.CostToNode][i].Previous.Value.Equals(newPoint))||
                                            (pathPositions[current.CostToNode][i].Next!=null&&(
                                             pathPositions[current.CostToNode][i].Next.Value.Equals(newPoint)||
                                             pathPositions[current.CostToNode][i].Next.Value.Equals(current.NodeCoordinates)));
                            if (i!=oldPath&&collision)
                            {
                                possible = false;
                            }
                        }

                        if (!possible)
                        {
                            continue;
                        }

                        var waitNode = p == new Point();
                        if (waitNode && current.WaitTime >= sMaxWaitTime)
                        {
                            continue;
                        }
                        var successor = new Node(map, newPoint, current, InWater,null,current.WaitTime,waitNode,avoidRoads);
                        if(closedList.Contains(successor)) {
                            continue;
                        }

                        var hasKey = false;
                        if (representative.ContainsKey(successor))
                        {
                            hasKey = true;
                            successor = representative[successor];
                        }
                        if (successor.NodeChild!=null && successor.NodeChild.CostToNode < current.CostToNode)
                        {
                            continue;
                        }

                        successor.NodeChild = current;
                        if (openList.Contains(successor))
                        {
                            openList.Remove(successor);
                        }

                        if (hasKey)
                        {
                            representative.Remove(successor);
                        }
                        representative.Add(successor,successor);
                        openList.Enqueue(successor, (float) successor.GetHeuristicCost(End)/sMaxSpeed + successor.CostToNode);
                    }
                }
            }
        }

    }
    }
