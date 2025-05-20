using System;
using System.Drawing;
using Common.com.game;
using Common.com.game.settings;
using Common.com.objects;

namespace Common.com.path
{
    class Node
    {
        public Point NodeCoordinates { get; }
        public Node NodeParent { get; }
        public Node NodeChild { get; set; }
        public int CostToNode { get; }
        public int WaitTime { get; }
        public Node(GameMap map, Point point, Node parent,bool isShip,Node nodeChild,int waitTime,bool waitNode,bool avoidRoads)
        {
            WaitTime = waitTime+(waitNode?1:0);
            NodeChild = nodeChild;
            NodeCoordinates = point;
            NodeParent = parent;
            var terrain = map.GetTerrainAt((uint) point.X, (uint) point.Y);
            
            var additionalCost = waitNode ? PathAbstract.mTimeToWait:PathAbstract.sMaxSpeed / map.GetSpeed(point.X, point.Y, isShip);
            if (avoidRoads&&(terrain==Terrain.Road||terrain==Terrain.Bridge))
            {
                additionalCost=PathAbstract.sMaxSpeed * NumberManager.Two;
            }
            if (parent != null)
            {
                CostToNode = parent.CostToNode + additionalCost;
            }
        }

        public double GetHeuristicCost(Point endPoint)
        {
            return Math.Sqrt(Math.Pow(NodeCoordinates.X - endPoint.X, NumberManager.Two) + Math.Pow(NodeCoordinates.Y - endPoint.Y, NumberManager.Two)) + CostToNode;
        }

        public static bool operator == (Node obj1, Node obj2)
        {
            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }
            if (ReferenceEquals(obj1, null))
            {
                return false;
            }
            if (ReferenceEquals(obj2, null))
            {
                return false;
            }

            return obj1.Equals(obj2);
        }

        public static bool operator !=(Node obj1, Node obj2)
        {
            return !(obj1 == obj2);
        }

        private bool Equals(Node other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return NodeCoordinates.Equals(other.NodeCoordinates)&&WaitTime==other.WaitTime;
        }

        public override bool Equals(object other)
        {
            return other is Node n && Equals(n);
        }
        public override int GetHashCode()
        {
            return NodeCoordinates.X + 1000*NodeCoordinates.Y+1000000*WaitTime;
        }
    }
}
