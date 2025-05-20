using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Common.com.game;
using Common.com.game.settings;
using Common.com.objects.entities;
using Common.com.objects.entities.FightingUnit;
using Common.com.path;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace Common.com.objects
{
    public abstract class ObjectMoving:GameObject
    {
        [JsonIgnore]
        public Vector2 Position => new Vector2(X,Y);
        public float X { get; set; }
        public float Y{ get; set; }


        public ObjectAction CurrentAction { get; set; }
        [JsonIgnore]
        public Path CurrentPath
        {
            get => CurrentAction.CurrentPath;
            set => CurrentAction.CurrentPath = value;
        }
        [JsonIgnore] public abstract int VisionRange { get; }
        [JsonIgnore] protected abstract int MaxHp { get; }

        public double CurrentHp { get; set; }
        [JsonIgnore]
        public abstract int BaseSpeed { get; }
        [JsonIgnore]
        public abstract int AttackDamage { get; }
        [JsonIgnore]
        public abstract int RequiredCap { get; }
        [JsonIgnore]
        public abstract Items ResourceCost { get;}
        [JsonIgnore]
        public abstract bool OnLand { get; } // Determines which kind of terrain a unit can move across (land/water).
        public int StartedWaitingAtTick { get; set; }
        private Size TileSize => new Size(1, 1);
        protected ObjectMoving()
        {
            CurrentHp=GetMaxHp(this);
            CurrentAction = new ObjectAction();
        }

        protected void Override(ObjectMoving objectMoving)
        {
            base.Override(objectMoving);
            X = objectMoving.X;
            Y = objectMoving.Y;
            CurrentAction = objectMoving.CurrentAction.Clone();
            CurrentHp = objectMoving.CurrentHp;
            StartedWaitingAtTick = objectMoving.StartedWaitingAtTick;
        }

        public abstract ObjectMoving Clone();

        public bool PathDone()
        {
            return CurrentPath == null || CurrentPath.Start == CurrentPath.End;
        }
        public virtual void RedoPath(GameMap gameMap,int radius=4)
        {   
            if (CurrentAction.GoingToFightWithId!=-1)
            {
                var res=PathTo(gameMap,gameMap.GetObject(CurrentAction.GoingToFightWithId).GetBounds(),!OnLand);
                gameMap.Paths.Remove(CurrentPath);
                CurrentAction.CurrentPath = res;
                if (CurrentPath != null)
                {
                    gameMap.Paths.Add(CurrentPath);
                }
            }
            else
            {
                
                if (CurrentPath != null)
                {
                    var redoRectangle = new Rectangle(CurrentPath.End.X -radius, CurrentPath.End.Y - radius, 2*radius+1, 2*radius+1);
                    var possibleEnds = new HashSet<Point>();
                    for (var pathX = redoRectangle.Left; pathX < redoRectangle.Right; pathX++)
                    {
                        for (var pathY = redoRectangle.Top; pathY < redoRectangle.Bottom; pathY++)
                        {
                            possibleEnds.Add(new Point(pathX, pathY));
                        }
                    }
                    var newPath=PathToRectangle(gameMap, redoRectangle, possibleEnds,  !OnLand);
                    gameMap.Paths.Remove(CurrentPath);
                    CurrentPath = newPath;
                    if (newPath != null)
                    {
                        
                        gameMap.Paths.Add(CurrentPath);
                    }else
                    {
                        if (radius < NumberManager.Four)
                        {
                            RedoPath(gameMap, radius + 1);
                        }
                    }
                }
            }
        }
        //Just some funny stuff here
        protected ObjectMoving(int playerNumber,int id,Vector2 position, Path currentPath, double currentHp) : base(playerNumber,id)
        {
            X = position.X;
            Y = position.Y;
            CurrentAction = new ObjectAction();
            CurrentPath = currentPath;
            CurrentHp = currentHp;
            var points = new LinkedList<Point>();
            points.AddFirst(new Point((int)X,(int)Y));
            if (currentPath == null)
            {
                // ReSharper disable once VirtualMemberCallInConstructor
                CurrentPath = new Path(points, !OnLand);
            }

        }

        public double FightWinChance (ObjectMoving enemy)
        {
            if (GetType() == enemy.GetType())
            {
                return NumberManager.ZeroPointFiveD;
            }

            if (GetType() == typeof(Worker))
            {
                return NumberManager.ZeroPointZeroOneD;
            }

            if (enemy is Worker)
            {
                return NumberManager.ZeroPointNineNineD;
            }

            if ((GetType() == typeof(Spearman) && enemy.GetType() == typeof(Swordsman))) 
            {
                return NumberManager.ZeroPointNineFiveD;
            } 
            else if (GetType() == typeof(Shieldman) && enemy.GetType() == typeof(Spearman))
            {
                return NumberManager.ZeroPointNineNineD;
            }
            else if (GetType() == typeof(Swordsman) && enemy.GetType() == typeof(Shieldman))
            {
                return NumberManager.ZeroPointNineFiveD;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(VisionRange.ToString());
                System.Diagnostics.Debug.WriteLine(MaxHp.ToString());
                System.Diagnostics.Debug.WriteLine(TileSize.ToString());
                return NumberManager.ZeroPointZeroFiveD;
            }

        }

        private static Rectangle GetBoundsAt(Vector2 vector)
        {
            return new Rectangle((int) vector.X, (int) vector.Y, 1, 1);
        }
        
        public override Rectangle GetBounds()
        {
            return GetBoundsAt(Position);
        }

        public bool Train(ResourceBuilding resourceBuilding)
        {
            return resourceBuilding.HasResources(ResourceCost);
        }
        public bool PathTo(GameMap gameMap, int id,bool water)
        {
            var gameObject = gameMap.GetObject(id);
            if (gameObject == null)
            {
                return false;
            }
            var res= PathTo(gameMap,gameObject.GetBounds(),water);
            if (res==null)
            {
                return false;
            }
            gameMap.Paths.Remove(CurrentPath);
            CurrentPath = res;
            gameMap.Paths.Add(CurrentPath);
            return true;
        }
        public Path PathToRectangle(GameMap gameMap,  Rectangle bounds,
            HashSet<Point> possibleEnds,bool water, List<Path> paths = null, HashSet<int> objects = null)
        {

            paths ??= gameMap.Paths.ToList();
            var pathAbstract = new PathAbstract(new Point((int)X, (int)Y), new Point (bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2), water);
            var path = pathAbstract.FindPath(gameMap, paths.ToArray(), objects, possibleEnds);
            return path;
        }

        private Path PathTo(GameMap gameMap, Rectangle rectangle,bool water,List<Path> paths = null, HashSet<int> objects=null)
        {
            var possibleEnds=new HashSet<Point>();
            for (var x = rectangle.Left-1; x < rectangle.Right+1; x++)
            {
                if (x == rectangle.Left - 1 || x == rectangle.Right + 1 - 1) 
                {
                    for (var y = rectangle.Top - 1; y < rectangle.Bottom + 1; y++)
                    {
                        possibleEnds.Add(new Point(x, y));
                    }
                }
                else
                {
                    possibleEnds.Add(new Point(x, rectangle.Top-1));
                    possibleEnds.Add(new Point(x, rectangle.Bottom));
                }
            }
            paths ??= gameMap.Paths.ToList();
            var pathAbstract = new PathAbstract(new Point((int)X,(int)Y),new Point(rectangle.Left+rectangle.Width/2,rectangle.Top+rectangle.Height/2),water);
            return pathAbstract.FindPath(gameMap,paths.ToArray(),objects,possibleEnds);
        }
        public int GetMaxHp(ObjectMoving objectMoving)
        {
            return objectMoving.MaxHp;
        }
    }
}