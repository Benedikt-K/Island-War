using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Common.com.game.achievments;
using Common.com.game.Map;
using Common.com.game.settings;
using Common.com.Menu;
using Common.com.networking.Messages.serverToClient;
using Common.com.objects;
using Common.com.objects.entities;
using Common.com.objects.entities.FightingUnit;
using Common.com.objects.immovables;
using Common.com.objects.immovables.Buildings;
using Common.com.objects.immovables.Resources;
using Common.com.path;
using Common.com.rollbacks;
using Common.com.rollbacks.changes;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Common.com.game
{
    public sealed class CapTracker:INumber
    {
        private readonly int mPlayerNum;
        
        public CapTracker(int playerNum)
        {
            mPlayerNum = playerNum;
        }
        public string Number =>  "Unit cap:" + GameMap.mFilledCap[mPlayerNum-1]+ " / " + GameMap.GetCap(mPlayerNum);
    }
    public sealed class GameMap
    {
        public static StatisticsManager StatisticsManager { private set; get; }

        private static int[] CurrentCap { get; set; }

        private static void AddCap(int amount,int playerNumber)
        {
            CurrentCap[playerNumber - 1] += amount;
            StatisticsManager.MaxStat(Statistic.HightestCapReached,1,CurrentCap[playerNumber - 1]);
        }
        public static int GetCap(int playerNumber)
        {
            return CurrentCap[playerNumber - 1];
        }
        public static int[] mFilledCap;
        public static FogOfWar FogOfWarListener { set; get; }
        public static IslandMapper IslandMapper { get; set; }
        private static Terrain[][] LastTerrains { get; set; }
        public static List<IGameEndListener> mGameEndListeners;
        private static SortedDictionary<int,GameObject> LastImmovable { get; set; }
        private static GameObject[,] LastImmovablePos { get; set; }
        private readonly Terrain[][] mTerrains;
        public static readonly HashSet<Point> sMountains=new HashSet<Point>();
        private readonly GameObject[,] mObjects;
        private GameObject[,] mObjectsImmovable;
        private static float sBaseSpeed = 0.024f;
        private LogisticsSystem mLogisticsSystem = new LogisticsSystem();
        public static List<int> Added { get; private set; }
        public static List<int> Removed { get; private set; }
        public static List<Tree> ChangedTree { get; private set; }
        public List<IChange> Changes { get; set; } 
        public SortedSet<Path> Paths { get; }
        private readonly SortedDictionary<int, GameObject> mIdToGameObject;
        private readonly SortedSet<GameObject> mMovings;
        private SortedDictionary<int, GameObject> mImmovables;
        private readonly SortedDictionary<int, int> mMovingsInTowerRange;
        private readonly IReadOnlyList<Corpse> mCorpses;
        public static readonly HashSet<Point> sRoads=new HashSet<Point>();

        public static void OnStart()
        {
            ChangedTree = null;
            Removed = null;
            Added = null;
            StatisticsManager = new StatisticsManager();
            CurrentCap = new []{ NumberManager.Ten, NumberManager.Ten };
            StatisticsManager.MaxStat(Statistic.HightestCapReached,1, NumberManager.Ten);
            StatisticsManager.MaxStat(Statistic.HightestCapReached, NumberManager.Two, NumberManager.Ten);
            mGameEndListeners = new List<IGameEndListener>();
            mFilledCap = new []{0,0};
            IslandMapper = null;
        }

        public static void OnEntityReset()
        {
            mFilledCap = new []{0,0};
        }
        private GameMap(Terrain[][] terrains,bool addRoads)
        {
            FogOfWarListener ??= FogOfWar.CreateNewFogOfWar(new Size(terrains.GetLength(0), terrains[0].GetLength(0)));
            mImmovables = new SortedDictionary<int, GameObject>();
            mMovings = new SortedSet<GameObject>();
            mTerrains = terrains;
            if(addRoads)
            {
                for (int x = 0; x < mTerrains.Length; x++)
                {
                    for (int y = 0; y < mTerrains[x].Length; y++)
                    {
                        if (mTerrains[x][y]==Terrain.Bridge||mTerrains[x][y]==Terrain.Road)
                        {
                            sRoads.Add(new Point(x,y));
                        }
                    }
                }
            }

            mObjects = new GameObject[terrains.GetLength(0), terrains[0].GetLength(0)];
            mObjectsImmovable = new GameObject[terrains.GetLength(0), terrains[0].GetLength(0)];
            Paths = new SortedSet<Path>();
            mIdToGameObject = new SortedDictionary<int, GameObject>();
            IslandMapper ??= new IslandMapper(this); 
            mCorpses = new List<Corpse>();
            mMovingsInTowerRange = new SortedDictionary<int, int>();
        }

        public GameMap(Size size)
        {
            FogOfWarListener ??= FogOfWar.CreateNewFogOfWar(size);
            mImmovables = new SortedDictionary<int, GameObject>();
            mMovings = new SortedSet<GameObject>();
            mTerrains = new Terrain[size.Width][];

            for (var i = 0; i < size.Width; i++)
            {
                mTerrains[i] = new Terrain[size.Height];
            }
            mObjects = new GameObject[size.Width, size.Height];
            mObjectsImmovable = new GameObject[size.Width, size.Height];
            Paths = new SortedSet<Path>();
            mIdToGameObject = new SortedDictionary<int, GameObject>();
            mCorpses = new List<Corpse>();
            mMovingsInTowerRange = new SortedDictionary<int, int>();
        }

        public IReadOnlyList<Corpse> GetCorpses()
        {
            return mCorpses;
        }
        internal int GetSpeed(int x, int y, bool ship)
        {
            return !InBounds(x, y) ? 0 : TrueSpeed(x, y, ship);
        }

        private static int WaitTicks()
        {
            return (int)(1.0f / sBaseSpeed);
        }

        public LogisticsSystem GetLogisticsSystem()
        {
            return mLogisticsSystem;
        }

        public void UpdateIslands()
        {
            IslandMapper = new IslandMapper(this);
        }
        public HashSet<int> GetLodges()
        {
            var res = new HashSet<int>();
            foreach (var ob in mImmovables)
            {
                if (GetObject(ob.Key) is ForestersLodge)
                {
                    res.Add(ob.Key);
                }
            }
            return res;
        }

        private void OnAdd(ObjectImmovable forestersLodge)
        {
            Added ??= new List<int>();
            Added.Add(forestersLodge.Id);
            foreach (var tree in GetAround<Tree>(forestersLodge.Location.X, forestersLodge.Location.Y))
            {
                if (tree.ForestersLodgeId == -1)
                {
                    tree.ForestersLodgeId = forestersLodge.Id;
                    ChangedTree ??= new List<Tree>();
                    ChangedTree.Add(tree);
                }else if (tree.ForestersLodgeId == forestersLodge.Id)
                {
                    ChangedTree ??= new List<Tree>();
                    ChangedTree.Add(tree);
                }
            }
        }

        private void OnRemove(ObjectImmovable forestersLodge)
        {
            Removed ??= new List<int>();
            Removed.Add(forestersLodge.Id);
            foreach (var tree in GetAround<Tree>(forestersLodge.Location.X, forestersLodge.Location.Y))
            {
                if (tree.ForestersLodgeId == forestersLodge.Id)
                {
                    var lodges = GetAround<ForestersLodge>(tree.Location.X, tree.Location.Y);
                    if (lodges.Count >= 1)
                    {
                        tree.ForestersLodgeId = lodges[0].Id;
                    }
                    else
                    {
                        tree.ForestersLodgeId = -1;
                    }
                    ChangedTree ??= new List<Tree>();
                    ChangedTree.Add(tree);
                }
            }
        }

        private List<T> GetAround<T>(int xPos, int yPos)
        {
            var res = new List<T>();
            for (var x = -ForestersLodge.sRadius; x <= ForestersLodge.sRadius; x++)
            {
                for (var y = -ForestersLodge.sRadius; y <= ForestersLodge.sRadius; y++)
                {
                    if (GetObject(xPos + x, yPos + y) is T t)
                    {
                        res.Add(t);
                    }
                }
            }
            return res;
        }
        public bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < mTerrains.GetLength(0) && y < mTerrains[0].GetLength(0);
        }
        public Size GetSize()
        {
            return new Size(mTerrains.GetLength(0), mTerrains[0].GetLength(0));
        }

        private bool ResyncRoads(ResyncMessage resyncMessage)
        {
            bool sync=true;
            foreach (var roadPoint in resyncMessage.Roads.ToArray())
            {
                Terrain terrain = GetTerrainAt((uint) roadPoint.X, (uint) roadPoint.Y);
                if (terrain!=Terrain.Bridge&&terrain!=Terrain.Road)
                {
                    sync = false;
                    Console.WriteLine(roadPoint+" async road");
                    SetTerrain((uint) roadPoint.X, (uint) roadPoint.Y,terrain==Terrain.Water?Terrain.Bridge:Terrain.Road);
                }
            }
            foreach (var roadPoint in sRoads.ToArray())
            {
                if (!resyncMessage.Roads.Contains(roadPoint))
                {
                    sync = false;
                    Console.WriteLine(roadPoint+" async road2");
                    SetTerrain((uint) roadPoint.X, (uint) roadPoint.Y,sMountains.Contains(roadPoint)?Terrain.Mountains:
                        (GetTerrainAt((uint) roadPoint.X, (uint) roadPoint.Y)==Terrain.Bridge?Terrain.Water:Terrain.Grass));
                }
            }

            return sync;
        }

        private void ReAddObject(GameObject ob)
        {
            var currentObject = GetObject(ob.Id);
            if (currentObject != null)
            {
                RemoveObject(currentObject, true, true);
            }

            AddObject(ob,true,true);
            FogOfWarListener.AddCurrentVision(ob,this);
        }
        private bool ResyncObjects(ResyncMessage resyncMessage)
        {
            var sync = true;
            foreach (var ob in resyncMessage.ObjectsMoving)
            {
                var currentObject = GetObject(ob.Id);
                if (currentObject==null||ob.GetBounds().Location != GetObject(ob.Id).GetBounds().Location)
                {
                    Console.WriteLine(GetObject(ob.Id)+" "+ob.Id+" is asynchronous");
                    
                    sync = false;
                }
            }

            sync = sync&&resyncMessage.ObjectsMoving.Count() == mMovings.Count&&resyncMessage.ObjectsImmovable.Count() == mImmovables.Count;
            

            if (!sync)
            {
                Console.WriteLine("syncing...");
                var immovable = mImmovables.Keys.ToHashSet();
                var moving = mIdToGameObject.Keys.ToHashSet();
                foreach (var ob in resyncMessage.ObjectsImmovable)
                {
                    if (GetObject(ob.Id) != null)
                    {
                        immovable.Remove(ob.Id);
                    }
                    ReAddObject(ob);
                }
                foreach (var ob in resyncMessage.ObjectsMoving)
                {
                    if (GetObject(ob.Id) != null)
                    {
                        moving.Remove(ob.Id);
                    }
                    ReAddObject(ob);
                }

                foreach (var imm in immovable)
                {
                    RemoveObject(imm,true,true);
                }
                foreach (var imm in moving)
                {
                    RemoveObject(imm,true,true);
                }

            }

            return sync;
        }

        // ReSharper disable once UnusedMember.Global
        // ReSharper think it is not used but is it used more then once 
        public bool Resync(ResyncMessage resyncMessage)
        {
            return ResyncObjects(resyncMessage) && ResyncRoads(resyncMessage);
        }
        public ResyncMessage GetSyncMessage(int tick)
        {
            ResyncMessage resyncMessage = new ResyncMessage
            {
                ObjectsMoving = mMovings,
                ObjectsImmovable = mImmovables.Values,
                Roads = sRoads,
                Tick=tick
            };
            return resyncMessage;
        }
        public GameMap Clone()
        {
            var res = new HashSet<GameObject>();
            foreach (var value in mIdToGameObject.Values)
            {
                if (value is ObjectMoving objectMoving)
                {
                    var newValue = objectMoving.Clone();
                    res.Add(newValue);
                }
            }
            return GetMap(new MapMessage(null, res, 0, 0, null, false, 0, null, null), false);
        }
        public MapMessage ToMapMessage(int tick, int playerNumber, Vector2[] cameraStarts, bool isPaused, int maxPlayers, bool cloneTiles,bool resyncMessage=false)
        {
            HashSet<GameObject> res;
            if (cloneTiles||resyncMessage)
            {

                res = mIdToGameObject.Values.ToHashSet();
                res.UnionWith(mImmovables.Values);
            }
            else
            {
                res = mMovings.ToHashSet();
            }
            return new MapMessage(cloneTiles?mTerrains:null, res, tick, playerNumber, cameraStarts, isPaused, maxPlayers, cloneTiles?FogOfWarListener:null,cloneTiles||resyncMessage?StatisticsManager:null);
        }

        public void UseLogisticsSystem(GameMap gameMap)
        {
            mLogisticsSystem = gameMap.mLogisticsSystem;
        }
        public static GameMap GetMap(MapMessage mapMessage, bool newMap)
        {
            
            if (mapMessage.StatisticsManager != null)
            {
                StatisticsManager = mapMessage.StatisticsManager;
                StatisticsManager.OnNewLoad();
            }

            var map = mapMessage.Terrains != null 
                ? new GameMap(mapMessage.Terrains,true) : new GameMap(LastTerrains,false);
            map.mImmovables = mapMessage.Terrains == null ? LastImmovable : new SortedDictionary<int, GameObject>();
            map.mObjectsImmovable = mapMessage.Terrains == null ? LastImmovablePos : new GameObject[map.GetSize().Width, map.GetSize().Height];
            
            if (mapMessage.FogOfWar != null)
            {
                FogOfWarListener = mapMessage.FogOfWar;
            }
            else
            {
                if (newMap)
                {
                    FogOfWarListener = FogOfWar.CreateNewFogOfWar(map.GetSize());
                }
            }

            foreach (var gameObject in mapMessage.Objects)
            {
                if (gameObject is Scaffolding scaffolding)
                {
                    scaffolding.EmptyResources(true);
                }
                map.AddObject(gameObject, newMap, true);
            }
            return map;
        }

        public void UpdateLasts()
        {
            LastImmovablePos = mObjectsImmovable;
            LastImmovable = mImmovables;
            LastTerrains = mTerrains;
        }

        public Terrain GetTerrainAt(uint x, uint y)
        {
            return InBounds((int)x, (int)y) ? mTerrains[x][y] : Terrain.OutOfBounds;
        }

        public void SetTerrain(uint x,uint y,Terrain terrain)
        {
            if (terrain==Terrain.Bridge||terrain==Terrain.Road)
            {
                sRoads.Add(new Point((int)x,(int)y));
            }
            else
            {
                sRoads.Remove(new Point((int)x,(int)y));
            }
            mTerrains[x][y] = terrain;
        }

        public GameObject GetObject(int id)
        {
            if (mImmovables.ContainsKey(id))
            {
                return mImmovables[id];
            }
            if (mIdToGameObject.ContainsKey(id))
            {
                return mIdToGameObject[id];
            }
            return null;
        }
        private void ChangePosition(ObjectMoving gameObject, float newX, float newY)
        {
            if (!newX.ToString(CultureInfo.InvariantCulture).Contains("."))
            {
                newX -= (float)NumberManager.ZeroPointZeroZeroOne;
            }
            if (!newY.ToString(CultureInfo.InvariantCulture).Contains("."))
            {
                newY -= (float)NumberManager.ZeroPointZeroZeroOne;
            }
            var newXRounded = (int) newX;
            var newYRounded = (int) newY;
            if (!InBounds(newXRounded, newYRounded))
            {
                return;
            }

            var ob = GetObject(newXRounded, newYRounded);
            var ob2 = GetSecondaryObject(newXRounded, newYRounded);
            if (!Equals(ob2, gameObject))
            {
                if (ob != null && !(ob is INonCollisional)||ob2!=null||GetSpeed(newXRounded,newYRounded,!gameObject.OnLand)==0)
                {
                    return;
                }
                FogOfWarListener.RemoveCurrentVision(gameObject, this);
                mObjects[(int) gameObject.X, (int) gameObject.Y] = default;
                gameObject.X = newX;
                gameObject.Y = newY;
                mObjects[newXRounded, newYRounded] = gameObject;
                gameObject.CurrentPath.RemoveFirst();
                OnNewTile(gameObject);
            }
            else
            {
                gameObject.X = newX;
                gameObject.Y = newY;
            }
        }
        
        private void OnNewTile(ObjectMoving objectMoving)
        {
            if (objectMoving is Worker worker)
            {
                mLogisticsSystem.OnNewPosition(this, worker);
            }
            else if ((objectMoving is Swordsman || // maybe redundant
                     objectMoving is Spearman ||
                     objectMoving is Shieldman ||
                     objectMoving is Spy))
            {
                var fighting = GetObject(objectMoving.CurrentAction.GoingToFightWithId);
                if (fighting is Building)
                {
                    if (objectMoving.Adjacent(mImmovables[objectMoving.CurrentAction.GoingToFightWithId]))
                    {
                        objectMoving.CurrentAction.FightingWithId = objectMoving.CurrentAction.GoingToFightWithId;
                        objectMoving.CurrentAction.GoingToFightWithId = -1;
                        Paths.Remove(objectMoving.CurrentPath);
                        objectMoving.CurrentPath = new Path(new LinkedList<Point>(new[]{new Point((int)objectMoving.X,(int)objectMoving.Y)}),false);
                    }
                }
                else if(fighting is ObjectMoving)
                {
                    objectMoving.RedoPath(this);
                }
                

                foreach (var enemy in mIdToGameObject.Values.OfType<ObjectMoving>()
                    .Where(enemy => enemy.PlayerNumber != objectMoving.PlayerNumber))
                {
                    if (objectMoving.Adjacent(enemy)&&(enemy.CurrentAction.IsAggressive||objectMoving.CurrentAction.IsAggressive||
                                                       objectMoving.CurrentAction.GoingToFightWithId==enemy.Id||
                                                       enemy.CurrentAction.GoingToFightWithId==objectMoving.Id))
                    {
                        if (objectMoving is Spy && !(enemy is Spy) || !(objectMoving is Spy) && enemy is Spy)
                        {
                            continue;
                        }

                        if (objectMoving is Spy spy && enemy is Spy enemySpy &&
                            spy.CurrentAction.GoingToMurderId != enemySpy.Id &&
                            enemySpy.CurrentAction.GoingToMurderId != spy.Id)
                        {
                            continue;
                        }
                        objectMoving.CurrentAction.FightingWithId = enemy.Id;
                        enemy.CurrentAction.FightingWithId = objectMoving.Id;
                        Paths.Remove(objectMoving.CurrentPath);
                        Paths.Remove(enemy.CurrentPath);
                        objectMoving.CurrentPath = new Path(new LinkedList<Point>(new[] { new Point((int)objectMoving.X, (int)objectMoving.Y) }), false);
                        enemy.CurrentPath = new Path(new LinkedList<Point>(new[] { new Point((int)enemy.X, (int)enemy.Y) }), false);
                    }
                }
            }
            if (objectMoving.CurrentAction.GoingIntoId != -1 && GetObject(objectMoving.CurrentAction.GoingIntoId) is Tower &&
                     (objectMoving is Shieldman || objectMoving is Spearman || objectMoving is Swordsman))
            {
                if (objectMoving.Adjacent(mImmovables[objectMoving.CurrentAction.GoingIntoId]))
                {
                    var tower = (Tower)GetObject(objectMoving.CurrentAction.GoingIntoId);
                    if (tower.UnitsInside.Count < NumberManager.Ten)
                    {
                        Changes.Add(new ObjectAddChange(objectMoving, tower));
                        Changes.Add(new ObjectRemoveChange(objectMoving));
                    }
                    else
                    {
                        objectMoving.CurrentAction.GoingIntoId = -1;
                    }
                }
            }
            if (objectMoving.CurrentAction.GoingIntoId != -1 &&
                     GetObject(objectMoving.CurrentAction.GoingIntoId) is TransportShip &&
                     !(objectMoving is TransportShip || objectMoving is ScoutShip))
            {
                if (objectMoving.Adjacent(GetObject(objectMoving.CurrentAction.GoingIntoId)))
                {
                    var transportShip = (TransportShip)GetObject(objectMoving.CurrentAction.GoingIntoId);
                    if (transportShip.UnitsInside.Count < NumberManager.Ten)
                    {
                        Changes.Add(new ObjectAddChange(objectMoving, transportShip));
                        Changes.Add(new ObjectRemoveChange(objectMoving));
                    }
                    else
                    {
                        objectMoving.CurrentAction.GoingIntoId = -1;
                    }
                }
            }

            if (objectMoving.CurrentAction.GoingIntoId != -1 &&
                GetObject(objectMoving.CurrentAction.GoingIntoId) is Tower && objectMoving is Spy)
            {
                if (objectMoving.Adjacent(GetObject(objectMoving.CurrentAction.GoingIntoId)))
                {
                    var tower = (Tower) GetObject(objectMoving.CurrentAction.GoingIntoId);
                    if (tower.SpyInside.Count == 1)
                    {
                        tower.SpyInside.Clear();
                        objectMoving.CurrentAction.GoingIntoId = -1;
                    }
                    else
                    {
                        objectMoving.CurrentAction.GoingIntoId = -1;
                    }
                }
            }

            if (objectMoving.CurrentAction.InfiltrateId != -1 &&
                GetObject(objectMoving.CurrentAction.InfiltrateId) is Tower && objectMoving is Spy)
            {
                if (objectMoving.Adjacent(GetObject(objectMoving.CurrentAction.InfiltrateId)))
                {
                    var tower = (Tower) GetObject(objectMoving.CurrentAction.InfiltrateId);
                    if (tower.SpyInside.Count == 0)
                    {
                        Changes.Add(new ObjectAddChange(objectMoving, tower));
                        Changes.Add(new ObjectRemoveChange(objectMoving));
                    }
                    else
                    {
                        objectMoving.CurrentAction.InfiltrateId = -1;
                    }
                }
            }
            DoTowerMovingCheck(objectMoving);
            FogOfWarListener.AddCurrentVision(objectMoving, this);
        }

        private int TrueSpeed(int x, int y, bool ship)
        {
            if (GetObject(x,y) is ObjectImmovable objectImmovable && !(objectImmovable is INonCollisional))
            {
                return 0;
            }
            if (ship)
            {
                return mTerrains[x][y] switch
                {
                    Terrain.Bridge => NumberManager.Two,
                    Terrain.Water => NumberManager.Two,
                    Terrain.Mountains => 0,
                    Terrain.Road => 0,
                    Terrain.Grass => 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(ship), null, null)
                };
            }

            return mTerrains[x][y] switch
            {
                Terrain.Grass => NumberManager.Two,
                Terrain.Water => 0,
                Terrain.Mountains => 1,
                Terrain.Road => NumberManager.Six,
                Terrain.Bridge => NumberManager.Six,
                _ => throw new ArgumentOutOfRangeException(nameof(ship),null,null)
            };
        }

        private float TilesPerTick(ObjectMoving gameObject)
        {
            return sBaseSpeed * gameObject.BaseSpeed * TrueSpeed((int)gameObject.Position.X, (int)gameObject.Position.Y, !gameObject.OnLand);
        }

        public void DoMovementTicks()
        {
            foreach (var gameObject in mMovings)
            {
                if (gameObject is ObjectMoving {CurrentPath: { }} moving && DoTick(moving)) 
                {
                        ChangeObjectPath(moving, null, true);
                }
            }
        }

        private void DoTowerCheck(ObjectImmovable tower)
        {
            var vision = 5;
            for (var x = -vision; x <= vision; x++)
            {
                for (var y = -vision; y <= vision; y++)
                {
                    var movingPositionX = tower.Location.X + 1 + x;
                    var movingPositionY = tower.Location.Y + 1 + y;
                    if (Math.Abs(x) + Math.Abs(y) <= vision && InBounds(movingPositionX, movingPositionY))
                    {
                        if (GetObject(movingPositionX, movingPositionY) is ObjectMoving moving && moving.PlayerNumber != tower.PlayerNumber)
                        {
                            if (!(mMovingsInTowerRange.ContainsKey(moving.Id)))
                            {
                                mMovingsInTowerRange.Add(moving.Id, tower.Id);
                            }
                            Debug.WriteLine(mMovingsInTowerRange.Count);
                            return;
                        }
                    }
                }
            }
        }

        private void DoTowerMovingCheck(ObjectMoving moving)
        {
            var vision = 5;
            for (var x = -vision; x <= vision; x++)
            {
                for (var y = -vision; y <= vision; y++)
                {
                    var movingPositionX = (int) (moving.Position.X) + x;
                    var movingPositionY = (int) (moving.Position.Y) + y;
                    if (Math.Abs(x) + Math.Abs(y) <= vision && InBounds(movingPositionX, movingPositionY))
                    {
                        if (GetObject(movingPositionX, movingPositionY) is Tower)
                        {
                            return;
                        }
                    }
                }
            }
            if (mMovingsInTowerRange.ContainsKey(moving.Id))
            {
                mMovingsInTowerRange.Remove(moving.Id);
            }
        }

        private void DoTowerDamage()
        {
            foreach (var newTowerId in mImmovables)
            {
                var gameObject = newTowerId.Value;
                if (gameObject is Tower tower)
                {
                    DoTowerCheck(tower);
                }
            }

            var shotAlready = new List<Tower>();
            foreach (var pair in mMovingsInTowerRange)
            {
                var unit = (ObjectMoving) GetObject(pair.Key);
                var tower = (Tower) GetObject(pair.Value);
                if (!(shotAlready.Contains(tower)))
                {
                    if (unit is Swordsman)
                    {
                        unit.CurrentHp -= tower.PikemenInsideInt() * NumberManager.ZeroPointFive;
                        unit.CurrentHp -= tower.SpearmenInsideInt() * 1f;
                        unit.CurrentHp -= tower.SwordsmenInsideInt() * NumberManager.ZeroPointFive;
                    }
                    else if (unit is Shieldman)
                    {
                        unit.CurrentHp -= tower.PikemenInsideInt() * NumberManager.ZeroPointFive;
                        unit.CurrentHp -= tower.SpearmenInsideInt() * NumberManager.ZeroPointFive;
                        unit.CurrentHp -= tower.SwordsmenInsideInt() * 1f;
                    }
                    else if (unit is Spearman)
                    {
                        unit.CurrentHp -= tower.PikemenInsideInt() * 1f;
                        unit.CurrentHp -= tower.SpearmenInsideInt() * NumberManager.ZeroPointFive;
                        unit.CurrentHp -= tower.SwordsmenInsideInt() * NumberManager.ZeroPointFive;
                    }
                    else if (unit is Worker)
                    {
                        unit.CurrentHp -= tower.PikemenInsideInt() * NumberManager.ZeroPointFive;
                        unit.CurrentHp -= tower.SpearmenInsideInt() * NumberManager.ZeroPointFive;
                        unit.CurrentHp -= tower.SwordsmenInsideInt() * NumberManager.ZeroPointFive;
                    }

                    if (unit.CurrentHp <= 0)
                    {
                        RemoveObject(unit);
                    }

                    if (!(unit is Spy))
                    {
                        shotAlready.Add(tower);
                    }
                }
            }
        }

        public void DoDamageTicks(int tickNr)
        {
            if (tickNr % NumberManager.Forty == 0)
            {
                DoTowerDamage();
            }
            foreach (var unit in mIdToGameObject.Values.OfType<ObjectMoving>().Where(unit => unit.CurrentAction.FightingWithId >= 0).ToArray())
            {
                if (GetObject(unit.CurrentAction.FightingWithId) == null)
                {
                    unit.CurrentAction.FightingWithId = -1;
                }
                else if (unit.CurrentHp <= 0)
                {
                    if (GetObject(unit.CurrentAction.FightingWithId) is ObjectMoving moving)
                    {
                        moving.CurrentAction.FightingWithId = -1;
                    }
                    Changes.Add(new StatisticChange(NumberManager.Three -unit.PlayerNumber,Statistic.UnitsKilled));
                    RemoveObject(unit);
                }
                else if (GetObject(unit.CurrentAction.FightingWithId) is Building building)
                {
                    var newRemove = new BuildingDeathChange(building,unit.AttackDamage);
                    Changes.Add(newRemove);
                } 
                else
                {
                    if (unit.CurrentAction.FightingWithId >= 0 &&
                        GetObject(unit.CurrentAction.FightingWithId) is ObjectMoving objectMoving)
                    {
                        if (unit is Spy spy && GetObject(unit.CurrentAction.GoingToMurderId) is Spy enemySpy)
                        {
                            if (spy.CurrentAction.GoingToMurderId == enemySpy.Id &&
                                enemySpy.CurrentAction.GoingToMurderId == spy.Id)
                            {
                                spy.CurrentHp = 0;
                                enemySpy.CurrentHp = 0;
                                RemoveObject(spy);
                                RemoveObject(enemySpy);
                                Changes.Add(new StatisticChange(spy.PlayerNumber, Statistic.SpiesKilled));
                                Changes.Add(new StatisticChange(enemySpy.PlayerNumber, Statistic.SpiesKilled));
                            }
                            else if (spy.CurrentAction.GoingToMurderId == enemySpy.Id &&
                                     enemySpy.CurrentAction.GoingToMurderId != spy.Id)
                            {
                                enemySpy.CurrentHp = 0;
                                RemoveObject(enemySpy);
                                spy.CurrentAction.GoingToMurderId = -1;
                                Changes.Add(new StatisticChange(spy.PlayerNumber, Statistic.SpiesKilled));
                            }
                        }

                        if (!(unit is Spy))
                        {
                            var winChance =
                                (int) (unit.FightWinChance(objectMoving) * NumberManager.OneHundred);
                            var random = new Random((int) unit.X * (unit.Id + 1) +
                                                    (int) unit.Y * (unit.CurrentAction.FightingWithId + 1) + tickNr);
                            var chance = random.Next(NumberManager.OneHundred);

                            if (chance < winChance)
                            {
                                var opponent = ((ObjectMoving) GetObject(unit.CurrentAction.FightingWithId));
                                if (opponent.CurrentAction.FightingWithId == -1)
                                {
                                    opponent.CurrentAction.FightingWithId = unit.Id;
                                }

                                if (unit is Spearman && opponent is Swordsman)
                                {
                                    opponent.CurrentHp -= unit.AttackDamage * 1;
                                }
                                else if (unit is Shieldman && opponent is Spearman)
                                {
                                    opponent.CurrentHp -= unit.AttackDamage * 1;
                                }
                                else if (unit is Swordsman && opponent is Shieldman)
                                {
                                    opponent.CurrentHp -= unit.AttackDamage * 1;
                                }
                                else
                                {
                                    ((ObjectMoving) mIdToGameObject[unit.CurrentAction.FightingWithId]).CurrentHp -=
                                        unit.AttackDamage * NumberManager.ZeroPointFive;
                                }

                                if (opponent.CurrentHp <= 0)
                                {
                                    RemoveObject(opponent);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void DoProductionTicks(int tickNr)
        {
            foreach (var production in mImmovables.Values.OfType<ResourceBuilding>())
            {
                if (production is MainBuilding && tickNr % NumberManager.Thousand == 0)
                {
                    Changes.Add(new ProductionChange(Item.Stone,production.Id,1));
                    Changes.Add(new ProductionChange(Item.Plank,production.Id,1));
                } 
                if (production is StoneMine && tickNr % NumberManager.FourHundred == 0 
                    && production.CurrentResourcesStored.ItemAmounts[Item.RawStone] < production.MaxResourcesStorable.ItemAmounts[Item.RawStone])
                {
                    Changes.Add(new ProductionChange(Item.RawStone,production.Id,1));
                }
                if (production is IronMine && production.IronDepositsConsumed.Any() && tickNr % NumberManager.FourHundred == 0 
                    && production.CurrentResourcesStored.ItemAmounts[Item.IronOre] < production.MaxResourcesStorable.ItemAmounts[Item.IronOre])
                {
                    Changes.Add(new ProductionChange(Item.IronOre,production.Id,1));
                }
                if (production is Sawmill && tickNr % NumberManager.TwoHundred == 0 
                    && production.CurrentResourcesStored.ItemAmounts[Item.Wood] > 0 
                    && production.CurrentResourcesStored.ItemAmounts[Item.Plank] < production.MaxResourcesStorable.ItemAmounts[Item.Plank])
                {
                    Changes.Add(new ProductionChange(Item.Plank,Item.Wood,production.Id,1,-1));
                }
                if (production is StoneProcessing && tickNr % NumberManager.FourHundred == 0 
                    && production.CurrentResourcesStored.ItemAmounts[Item.RawStone] > 0 
                    && production.CurrentResourcesStored.ItemAmounts[Item.Stone] < production.MaxResourcesStorable.ItemAmounts[Item.Stone])
                {
                    
                    Changes.Add(new ProductionChange(Item.Stone,Item.RawStone,production.Id,1,-1));
                }
                if (production is IronForge && tickNr % NumberManager.OneHundred == 0 
                    && production.CurrentResourcesStored.ItemAmounts[Item.IronOre] > 0 
                    && production.CurrentResourcesStored.ItemAmounts[Item.Iron] < production.MaxResourcesStorable.ItemAmounts[Item.Iron])
                {

                    Changes.Add(new ProductionChange(Item.Iron,Item.IronOre,production.Id,1,-1));
                }
            }
        }

        public int AmountOfAllStoredResourcesOfKind(Item item, int playerNumber)
        {
            var amount = 0;
            var x = mImmovables.Values.OfType<ResourceBuilding>();
            foreach (var building in x.Where(building => building.PlayerNumber == playerNumber))
            {
                if (building.CurrentResourcesStored.ItemAmounts.ContainsKey(item))
                {
                    amount += building.CurrentResourcesStored.ItemAmounts[item];
                }
            }
            return amount;
        }

        public List<int> AmountOfAllStoredResources(int playerNumber)
        {
            var i = 0;
            var amounts = new List<int>();
            var items = new List<Item>()
            {
                Item.Wood,
                Item.Plank,
                Item.IronOre,
                Item.Iron,
                Item.RawStone,
                Item.Stone
            };
            var j = GetNumber(items);
            while (i != j)
            {
                amounts.Add(AmountOfAllStoredResourcesOfKind(items[i], playerNumber));
                i++;
            }
            return amounts;
        }

        private static int GetNumber(ICollection list)
        {
            return list.Count;
        }
        public Dictionary<string, int> CountPeopleByProfession(int playerNumber)
        {
            var professions = new Dictionary<string, int>()
            {
                { "Worker", 0 },
                { "Shieldman", 0 },
                { "Spearman", 0 },
                { "Swordsman", 0 },
                { "Spy", 0 },
                {"TransportShip", 0},
                {"ScoutShip", 0}
            };
            foreach (var man in mIdToGameObject.Values.OfType<ObjectMoving>().Where(man => man.PlayerNumber == playerNumber))
            {
                if (man is Worker)
                {
                    professions["Worker"] += 1;
                }
                else if (man is Shieldman)
                {
                    professions["Shieldman"] += 1;
                }
                else if (man is Spearman)
                {
                    professions["Spearman"] += 1;
                }
                else if (man is Swordsman)
                {
                    professions["Swordsman"] += 1;
                } 
                else if (man is Spy)
                {
                    professions["Spy"] += 1;
                }
                else if (man is TransportShip)
                {
                    professions["TransportShip"] += 1;
                }
                else if (man is ScoutShip)
                {
                    professions["ScoutShip"] += 1;
                }
            }
            return professions;
        }

        private bool DoTick(ObjectMoving gameObjectMoving)
        {
            
            if (gameObjectMoving.CurrentPath == null || !(gameObjectMoving.CurrentPath is {WayPoints: {First: { }}}))
            {
                return false;
            }
            if (gameObjectMoving.CurrentPath.WayPoints.First.Next != null && gameObjectMoving.CurrentPath.WayPoints.First.Next.Value == gameObjectMoving.CurrentPath.WayPoints.First.Value)
            {
                DoWaitTick(gameObjectMoving);
            }
            else
            {
                UpdatePathOnCollision(gameObjectMoving);
                if (!(gameObjectMoving.CurrentPath is {WayPoints: {First: { }}})) //extremely rare case no idea why it happens
                {
                    return false;
                }
                DoMovementTick(gameObjectMoving);
            }

            if (gameObjectMoving.CurrentPath != null)
            {
                var end = new Vector2(gameObjectMoving.CurrentPath.End.X + NumberManager.ZeroPointFiveF,
                    gameObjectMoving.CurrentPath.End.Y + NumberManager.ZeroPointFiveF);
                return Vector2.Subtract(gameObjectMoving.Position, end).LengthSquared() < NumberManager.ZeroPointZeroOne;
            }

            return true;
        }

        private void UpdatePathOnCollision(ObjectMoving objectMoving)
        {
            var nextPoint = objectMoving.CurrentPath.WayPoints.First?.Next;
            if (nextPoint != null)
            {
                var gameObject = GetObject(nextPoint.Value.X, nextPoint.Value.Y);
                if (gameObject != null && (!(gameObject is INonCollisional)||GetSecondaryObject(nextPoint.Value.X, nextPoint.Value.Y)!=null))
                {
                    objectMoving.RedoPath(this);
                }
            }
        }
        private static void DoWaitTick(ObjectMoving gameObject)
        {
            if (gameObject.StartedWaitingAtTick++ < WaitTicks() * PathAbstract.mTimeToWait / PathAbstract.sMaxSpeed)
            {
                return;
            }
            gameObject.StartedWaitingAtTick = 0;
            gameObject.CurrentPath.RemoveFirst();
        }
        private void DoMovementTick(ObjectMoving gameObject)
        {
            float resultX;
            float resultY;
            float tilesPerTick = TilesPerTick(gameObject);
            var first = gameObject.CurrentPath.WayPoints.First;
            if (first == null)
            {
                return;
            }
            var xToGoTile = 0;
            var yToGoTile = 0;
            if (first.Next != null)
            {
                xToGoTile=first.Value.X - first.Next.Value.X;
                yToGoTile=first.Value.Y - first.Next.Value.Y;
                
            }
            float xSubPos = gameObject.Position.X%1;
            float ySubPos = gameObject.Position.Y%1;
            if (xToGoTile != 0)
            {
                //If The Tile to go to is either to the left or to the right of
                //the gameObject go towards the middle of the tile first
                var yToGo = NumberManager.ZeroPointFive - ySubPos;
                resultY = (float)Math.Min(Math.Abs(yToGo), tilesPerTick) * Math.Sign(yToGo);
                tilesPerTick -= Math.Abs(resultY);
                //If theres any movement left afterwards go in the x direction
                resultX = -xToGoTile * tilesPerTick;
            }
            else if(yToGoTile!=0)
            {
                //If The Tile to go to is either above or below the gameObject go towards the middle of the tile first
                var xToGo = NumberManager.ZeroPointFive - xSubPos;
                resultX = (float)Math.Min(Math.Abs(xToGo), tilesPerTick) * Math.Sign(xToGo);
                tilesPerTick -= Math.Abs(resultX);
                //If theres any movement left afterwards go in the y direction
                resultY = -yToGoTile * tilesPerTick;
            }
            else
            {
                //If the gameObject has reached its final tile go towards the middle
                var xToGo = NumberManager.ZeroPointFive - xSubPos;
                resultX = (float)Math.Min(Math.Abs(xToGo), tilesPerTick) * Math.Sign(xToGo);
                tilesPerTick -= Math.Abs(resultX);
                var yToGo = NumberManager.ZeroPointFive - ySubPos;
                resultY = (float)Math.Min(Math.Abs(yToGo), tilesPerTick) * Math.Sign(yToGo);
            }
            //Apply the calculated movement changes
            ChangePosition(gameObject, gameObject.X + resultX, gameObject.Y + resultY);
            //If the gameObject has reached is within 0.1 tiles of the middle of the destination tile return true
        }

        public void RedoTask(Worker worker)
        {
            worker = (Worker)GetObject(worker.Id);
            mLogisticsSystem.RemoveTask(worker, this, false);
            mLogisticsSystem.NewTask(this,worker);
        }
        public void ChangeObjectAction(ObjectMoving moving, ObjectAction objectAction)
        {
            objectAction.IsOccupied = true;

                if (objectAction.CurrentPath != moving.CurrentPath)
                {
                    Paths.Remove(moving.CurrentPath);
                    if (objectAction.CurrentPath != null)
                    {
                        Paths.Add(objectAction.CurrentPath);
                    }
                }
                if (moving is Worker worker)
                {
                    mLogisticsSystem.OnNewObjectAction(this, worker, objectAction);
                }
                else
                {
                    moving.CurrentAction = objectAction;
                }
                if (objectAction.GoingToFightWithId != -1)
                {
                    moving.PathTo(this, objectAction.GoingToFightWithId, !moving.OnLand);
                    OnNewTile(moving);
                }
                if (objectAction.GoingIntoId != -1)
                {
                    Console.WriteLine(moving.CurrentAction.CurrentPath);
                    moving.PathTo(this, objectAction.GoingIntoId, !moving.OnLand);
                    OnNewTile(moving);
                }
                if (objectAction.InfiltrateId != -1)
                {
                    moving.PathTo(this, objectAction.InfiltrateId, !moving.OnLand);
                    OnNewTile(moving);
                }

                if (objectAction.GoingToMurderId != -1)
                {
                    moving.PathTo(this, objectAction.GoingToMurderId, !moving.OnLand);
                    OnNewTile(moving);
                }
        }

        public bool CanBePut(ObjectImmovable objectImmovable,int playerNum, List<Terrain> required = null)
        {
            if (required == null)
            {
                required = new List<Terrain>();
                if (objectImmovable is Building building)
                {
                    foreach (var terrain in building.TerrainRequirement)
                    {
                        required.Add(terrain);
                    }
                }
            }

            if (objectImmovable is Tower tower)
            {
                if (!IslandMapper.CanAddTower(this, tower))
                {
                    return false;
                }
            }

            if (playerNum!=0&&Scaffolding.MaxBlueprintSpaces-StatisticsManager.GetStatistic(Statistic.BlueprintSpaces,playerNum)<objectImmovable.GetBounds().Width*objectImmovable.GetBounds().Height)
            {
                return false;
            }

            var shipyardRequirements = new List<Terrain>();
            var x = objectImmovable.Location.X;
            var y = objectImmovable.Location.Y;
            var hasIron=false;
            for (var i = 0; i < objectImmovable.TileSize.Width; i++)
            {
                for (var j = 0; j < objectImmovable.TileSize.Height; j++)
                {
                    shipyardRequirements.Add(GetTerrainAt((uint)(x + i), (uint)(y + j)));
                    if (!InBounds(x + i, y + j))
                    {
                        return false;
                    }

                    if (GetObject(x + i, y + j) is IronDeposit)
                    {
                        hasIron = true;
                    }
                    if (GetObject(x + i, y + j) != null && (!(GetObject(x + i, y + j) is IronDeposit) || objectImmovable is IronDeposit) ||
                            (!required.Contains(GetTerrainAt((uint) (x + i), (uint) (y + j)))))
                    {
                        return false;
                    }
                }
            }

            if (objectImmovable is Shipyard && (!(shipyardRequirements.Contains(Terrain.Grass)) || !(shipyardRequirements.Contains(Terrain.Water))))
            {
                return false;
            }
            return !(objectImmovable is IronMine)||hasIron;
        }

        public void ChangeObjectPath(ObjectMoving moving, Path newPath, bool logisticIgnore,bool changeAggressiveness=false,bool aggressive=false)
        {
            if (newPath == null)
            {
                return;
            }


            Paths.Remove(moving.CurrentPath);

            if (moving is Worker worker && !logisticIgnore)
            {
                var newAction = new ObjectAction
                {
                    IsOccupied = true,
                    FightingWithId = worker.CurrentAction.FightingWithId,
                    CurrentPath = newPath,
                    TransportingFromId = -1,
                    TransportingToId = -1,
                    GoingToFightWithId = worker.CurrentAction.GoingToFightWithId,
                    IsAggressive = worker.CurrentAction.IsAggressive,
                    ItemTransportIntent = Item.Nothing
                };
                mLogisticsSystem.OnNewObjectAction(this,worker,newAction);
            }
            else
            {

                moving.CurrentPath = newPath;
                moving.CurrentAction.FightingWithId = -1;
                

                if (changeAggressiveness)
                {
                    moving.CurrentAction.IsAggressive = aggressive;
                }
            }
            Paths.Add(newPath);
        }

        public void OnNewPosition(Worker worker)
        {
            mLogisticsSystem.OnNewPosition(this,worker);
        }
        public void RemoveObject(GameObject gameObject,bool removeInternally=true,bool reverting=false)
        {
            RemoveObject(gameObject.Id,removeInternally,reverting);
        }

        public List<Worker> GetAllWorkers()
        {
            return mLogisticsSystem.GetAllWorkers(this);
        }

        public static void EndGame(int playerLostId)
        {
            foreach (var gameEndListener in mGameEndListeners)
            {
                gameEndListener.OnGameEnd(NumberManager.Three -playerLostId);
            }
        }
        private void RemoveObject(int id,bool removeInternally=true,bool reverting=false)
        {
            var gameObject = GetObject(id);
            if (gameObject is Scaffolding scaffolding)
            {
                StatisticsManager.RemoveStatistic(Statistic.BlueprintSpaces,scaffolding.PlayerNumber,scaffolding.GetBounds().Width*scaffolding.GetBounds().Height);
            }
            FogOfWarListener.RemoveCurrentVision(gameObject, this);
            if (gameObject == null)
            {
                Console.WriteLine("could not remove non existent object " + id);
                return;
            }
            if (gameObject is MainBuilding&&!reverting)
            {
                Console.WriteLine("Game ended");
                EndGame(gameObject.PlayerNumber);
                return;
            }
            if (gameObject is ObjectImmovable objectImmovable)
            {
                if (objectImmovable is Tower)
                {
                    StatisticsManager.RemoveStatistic(Statistic.TowersStanding,objectImmovable.PlayerNumber);
                }
                if (objectImmovable is House)
                {
                    AddCap(-House.HousingSpace,objectImmovable.PlayerNumber);
                }
                mImmovables.Remove(objectImmovable.Id);
            }

            
            mLogisticsSystem.OnRemove(this, gameObject,reverting);
            mIdToGameObject.Remove(gameObject.Id); //?
            var bounds = gameObject.GetBounds();
            for (var x = bounds.X; x < bounds.Right; x++)
            {
                for (var y = bounds.Y; y < bounds.Bottom; y++)
                {
                    if (mObjects[x, y] != null)
                    {
                        if (mObjects[x, y].Id == gameObject.Id)
                        {
                            mObjects[x, y] = null;
                        }
                    }

                    if (mObjectsImmovable[x, y] != null)
                    {
                        if (mObjectsImmovable[x, y].Id == gameObject.Id)
                        {
                            mObjectsImmovable[x, y] = null;
                        }
                    }
                }
            }

            if ((gameObject as ObjectImmovable)?.IronDepositsConsumed != null && ((ObjectImmovable)gameObject).IronDepositsConsumed.Any())
            {
                foreach (var deposit in ((ObjectImmovable)gameObject).IronDepositsConsumed)
                {
                    AddObject(deposit);
                    
                }
            }
            
            if (gameObject is ObjectMoving objectMoving)
            {
                mFilledCap[gameObject.PlayerNumber-1] -= objectMoving.RequiredCap;
                Paths.Remove(objectMoving.CurrentPath);
                if (removeInternally)
                {
                    mMovings.Remove(objectMoving);
                }
            }
            if (gameObject is ForestersLodge lodge)
            {
                OnRemove(lodge);
            }
        }

        public void AddObject(GameObject gameObject,bool newObject = true, bool initialization = false,bool addInternally=true)
        {

            if (gameObject is House)
            {
                AddCap(House.HousingSpace,gameObject.PlayerNumber);
            }
            if (gameObject is Scaffolding scaffolding)
            {
                StatisticsManager.AddStatistic(Statistic.BlueprintSpaces,scaffolding.PlayerNumber,scaffolding.GetBounds().Width*scaffolding.GetBounds().Height);
            }
            if (!initialization&&newObject && gameObject.PlayerNumber != 0)
            {
                FogOfWarListener.AddCurrentVision(gameObject, this);
            }
            if (gameObject is ForestersLodge lodge && newObject)
            {
                OnAdd(lodge);
            }

            if (gameObject is ObjectMoving objectMoving)
            {
                mFilledCap[gameObject.PlayerNumber-1] += objectMoving.RequiredCap;
                if (addInternally)
                {
                    mMovings.Add(objectMoving);
                }
            }

            if (gameObject is ObjectImmovable objectImmovable)
            {
                if (objectImmovable is Tower || objectImmovable is MainBuilding)
                {
                    IslandMapper ??= new IslandMapper(this);
                    if (!IslandMapper.OnAddTower(this, objectImmovable))
                    {
                        return;
                    }

                    if (objectImmovable is Tower)
                    {
                        StatisticsManager.AddStatistic(Statistic.TowersStanding,objectImmovable.PlayerNumber);
                    }
                } 
                mImmovables.Add(objectImmovable.Id, objectImmovable);
            }
            else
            {
                mIdToGameObject[gameObject.Id] = gameObject;
            }

            var bounds = gameObject.GetBounds();
            for (var x = bounds.X; x < bounds.Right; x++)
            {
                for (var y = bounds.Y; y < bounds.Bottom; y++)
                {
                    if (gameObject is ObjectImmovable immovable)
                    {
                        if (mObjectsImmovable[x, y] is IronDeposit deposit)
                        {
                            if (!(immovable is IronDeposit))
                            {
                                immovable.AddIronDeposit(deposit);
                                var boundsDeposit = deposit.GetBounds();
                                for (var i = boundsDeposit.X; i < boundsDeposit.Right; i++)
                                {
                                    for (var j = boundsDeposit.Y; j < boundsDeposit.Bottom; j++)
                                    {
                                        mObjectsImmovable[i, j] = null;
                                    }
                                }
                                mImmovables.Remove(deposit.Id);
                            }
                        }
                        mObjectsImmovable[x, y] = immovable;
                    }
                    else
                    {
                        mObjects[x, y] = gameObject;
                    }
                }
            }
            mLogisticsSystem.OnAdd(this, gameObject,newObject,initialization);
            if(gameObject is ObjectMoving {CurrentPath: { }} moving)
            {
                Paths.Add(moving.CurrentPath);
            }
        }

        public void RedoLogisticSystem()
        {
            mLogisticsSystem.RetryAllWorkerTasks(this);
        }

        public Point ClosestFreeTile(ObjectImmovable immovable,bool ship)
        {
            var bounds = immovable.GetBounds();
            for (var x = bounds.Left - 1; x < (bounds.Right + 1); x++)
            {
                for (var y = bounds.Top - 1; y < (bounds.Bottom + 1); y++)
                {
                    var ob = GetSecondaryObject(x, y);
                    if (GetSpeed(x,y,ship)>0&&ob==null)
                    {
                        return new Point(x, y);
                    }
                }
            }
            return immovable.Location;
        }

        public GameObject GetSecondaryObject(int x, int y)
        {
            if (!InBounds(x, y))
            {
                return null;
            }
            return mObjects[x, y];
        }
        public GameObject GetObject(int x, int y)
        {
            if (!InBounds(x, y))
            {
                return null;
            }
            if (mObjectsImmovable[x,y] != null)
            {
                return mObjectsImmovable[x, y];
            }
            return mObjects[x, y];
        }
    }
}