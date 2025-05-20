using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
// ReSharper disable PossibleNullReferenceException

namespace Common.com.path
{
    public sealed class Path:PathAbstract,IComparable
    {
        [JsonIgnore]
        // ReSharper disable once MemberCanBePrivate.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public LinkedList<Point> WayPoints { get; set; }

        
        public IEnumerable<Point> Points
        {
            get => WayPoints;
            // ReSharper disable once UnusedMember.Global
            //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
            set => WayPoints = new LinkedList<Point>(value);
        }

        public Path Clone()
        {
            return new Path(new LinkedList<Point>(WayPoints), InWater);
        }
        
        public Path(LinkedList<Point> wayPoints,bool inWater) :base(wayPoints.First.Value,wayPoints.Last.Value,inWater)
        {
            WayPoints = wayPoints;
        }
        // ReSharper disable once UnusedMember.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public Path() : base(new Point(-1, -1), new Point(-1, -1), false)
        {
            WayPoints = new LinkedList<Point>();
        }
        public void RemoveFirst()
        {
            WayPoints.RemoveFirst();
            Start = WayPoints.First.Value; // was null
        }
        public int CompareTo(object obj)
        {
            return obj.GetHashCode() - GetHashCode();
        }
    }
}