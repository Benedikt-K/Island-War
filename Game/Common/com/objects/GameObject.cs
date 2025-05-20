using System;
using System.Drawing;
using System.Numerics;
using Common.com.game.Map;
using Common.com.serialization;
using Rectangle = System.Drawing.Rectangle;

namespace Common.com.objects
{
    public abstract class GameObject : JsonSerializable,IComparable
    {
        public int PlayerNumber { get; set; }
        public int Id { get; set; }

        protected GameObject(int playerNumber, int id)
        {
            PlayerNumber = playerNumber;
            Id = id;
        }

        public override int GetHashCode()
        {
            return GetId();
        }

        public override bool Equals(object obj)
        {
            return obj is GameObject gameObject&&gameObject.Id==Id;
        }

        public bool PartiallyVisible(FogOfWar fogOfWar, int playerNumber)
        {
            var bounds = GetBounds();
            for (var x = bounds.Left; x < bounds.Right; x++)
            {
                for (var y=bounds.Top; y<bounds.Bottom; y++)
                {
                    if (fogOfWar.FogOfWarTiles[x][y][playerNumber-1]!=0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private int GetId()
        {
            return Id;
        }
        public bool Adjacent(GameObject other)
        {
            var rect = other.GetBounds();
            rect.Inflate(new Size(1,1));
            return rect.IntersectsWith(GetBounds());
        }

        public float EuclideanDistance(GameObject other)
        {
            return EuclideanDistance(other.GetBounds(),GetBounds());
        }

        private static float EuclideanDistance(Rectangle rectangle1, Rectangle rectangle2)
        {
            var middle1 = new Vector2(rectangle1.Left + rectangle1.Width / 2f,rectangle1.Top + rectangle1.Height / 2f);
            var middle2 = new Vector2(rectangle2.Left + rectangle2.Width / 2f,rectangle2.Top + rectangle2.Height / 2f);
            return Vector2.Distance(middle1, middle2);
        }
        protected GameObject()
        {
            
        }

        protected void Override(GameObject gameObject)
        {
            PlayerNumber = gameObject.PlayerNumber;
            Id = gameObject.Id;
        }

        public abstract Rectangle GetBounds();
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return -1;
            }
            if (obj is GameObject gameObject)
            {
                return gameObject.Id - Id;
            }
            return obj.GetHashCode() - GetHashCode();
            
        }
    }
}