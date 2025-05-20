using Common.com.game;
using Common.com.game.achievments;
using Common.com.game.settings;
using Common.com.objects;
using Common.com.objects.entities;
using Common.com.objects.immovables.Buildings;

namespace Common.com.rollbacks.changes
{
    public sealed class ItemChange : IChange
    {
        private readonly ResourceBuilding mResourceBuilding;
        private readonly Worker mWorker;
        private readonly bool mDeposit;
        private bool mDone;
        private bool mSuccess;
        private readonly Item mItem;
        private readonly LogisticsSystem mLogisticsSystem;
        private Terrain mTerrainOld;

        public ItemChange(Worker worker,
            ResourceBuilding resourceBuilding,
            bool deposit,
            Item item,
            LogisticsSystem logisticsSystem)
        {

            mWorker = worker;
            mResourceBuilding = resourceBuilding;
            mDeposit = deposit;
            mItem = item;
            mLogisticsSystem = logisticsSystem;
        }
        private static void CheckIfNExtToIt(GameState gameState, ObjectImmovable resourceBuilding)
        {
            var bounds = resourceBuilding.GetBounds();
            for (var x = bounds.Left; x < bounds.Right; x++)
            {
                for (var y = bounds.Top; y < bounds.Bottom; y++)
                {
                    if (!(gameState.Map.GetSecondaryObject(x, y) is ObjectMoving objectMoving))
                    {
                        continue;
                    }

                    var closestFreeTile =
                        gameState.Map.ClosestFreeTile(resourceBuilding, !objectMoving.OnLand);
                    gameState.Map.RemoveObject(objectMoving, false);
                    objectMoving.X = closestFreeTile.X + NumberManager.ZeroPointFiveF;
                    objectMoving.Y = closestFreeTile.Y + NumberManager.ZeroPointFiveF;
                    objectMoving.RedoPath(gameState.Map);
                    gameState.Map.AddObject(objectMoving, false, false, false);
                }
            }
        }
        private static void MakeDepositChange(GameState gameState, ResourceBuilding resourceBuilding)
        {
            if (resourceBuilding.MaxResourcesStorable.Same(resourceBuilding.CurrentResourcesStored) &&
                resourceBuilding is Scaffolding scaffolding)
            {

                if (scaffolding.IsBridge)
                {
                    GameMap.StatisticsManager.AddStatistic(Statistic.BridgesBuilt, scaffolding.PlayerNumber);
                    var newBridge =
                        new TileChange(
                            gameState.Map.GetTerrainAt((uint) scaffolding.Location.X, (uint) scaffolding.Location.Y),
                            Terrain.Bridge,
                            (uint) scaffolding.Location.X,
                            (uint) scaffolding.Location.Y);
                    newBridge.DoChange(gameState);
                    gameState.Map.RemoveObject(scaffolding, true, true);
                    gameState.Map.GetLogisticsSystem()
                        .OnNewBridge(gameState.Map, scaffolding.Location.X, scaffolding.Location.Y);
                }
                else if (scaffolding.IsRoad)
                {
                    var newRoad =
                        new TileChange(
                            gameState.Map.GetTerrainAt((uint) scaffolding.Location.X, (uint) scaffolding.Location.Y),
                            Terrain.Road,
                            (uint) scaffolding.Location.X,
                            (uint) scaffolding.Location.Y);
                    newRoad.DoChange(gameState);
                    gameState.Map.RemoveObject(scaffolding, true, true);
                }
                else
                {
                    CheckIfNExtToIt(gameState, resourceBuilding);
                    GameMap.StatisticsManager.AddStatistic(Statistic.BuildingsBuilt, scaffolding.PlayerNumber);
                    scaffolding.TurnsInto.IsBlueprint = false;
                    scaffolding.TurnsInto.Location = scaffolding.Location;
                    scaffolding.TurnsInto.IronDepositsConsumed = null;
                    gameState.Map.RemoveObject(scaffolding, true, true);
                    gameState.Map.AddObject(scaffolding.TurnsInto);
                }
            }
        }
       
        private void MakeChange(GameState gameState)
        {
            var resourceBuilding = (ResourceBuilding) gameState.Map.GetObject(mResourceBuilding.Id);
            mTerrainOld =
                gameState.Map.GetTerrainAt((uint) resourceBuilding.Location.X, (uint) resourceBuilding.Location.Y);
            if (mDeposit)
            {
                mSuccess = resourceBuilding.DepositResource(mWorker);
                MakeDepositChange(gameState, resourceBuilding);
                if (mSuccess)
                {
                    if (mWorker.CurrentAction.UserMade)
                    {
                        mLogisticsSystem.RemoveTask(mWorker, gameState.Map, true);
                        mWorker.CurrentAction.UserMade = false;
                    }
                    else
                    {
                        mLogisticsSystem.RemoveTask(mWorker, gameState.Map, true);
                        mLogisticsSystem.NewTask(gameState.Map, mWorker);
                    }


                }
            }
            else
            {
                if (resourceBuilding.WithdrawResource(mWorker, mItem))
                {
                    mSuccess = true;
                    mLogisticsSystem.AddSourceTaskItem(gameState.Map, mWorker);
                    if (!mWorker.CurrentAction.UserMade)
                    {
                        mWorker.RedoPath(gameState.Map);
                        mLogisticsSystem.OnNewPosition(gameState.Map, mWorker);
                    }
                    else
                    {
                        mLogisticsSystem.RemoveTask(mWorker, gameState.Map, true);
                        mWorker.CurrentAction.IsOccupied = true;
                        mWorker.CurrentAction.UserMade = false;
                    }
                }


            }
        }

        public void DoChange(GameState gameState)
        {
            if (mDone)
            {
                return;
            }

            mDone = true;
            MakeChange(gameState);
        }

        public void RevertChange(GameState gameState)
        {
            var old = (ResourceBuilding) gameState.Map.GetObject(mResourceBuilding.Id);
            if (mResourceBuilding is Scaffolding scaffolding && old == null)
            {
                if (scaffolding.IsRoad)
                {
                    var newRoad =
                        new TileChange(
                            gameState.Map.GetTerrainAt((uint) scaffolding.Location.X, (uint) scaffolding.Location.Y),
                            Terrain.Road,
                            (uint) scaffolding.Location.X,
                            (uint) scaffolding.Location.Y);
                    newRoad.RevertChange(gameState);
                    mResourceBuilding.IronDepositsConsumed = null;
                    mResourceBuilding.CurrentResourcesStored = mResourceBuilding.MaxResourcesStorable;
                    old = mResourceBuilding;
                    gameState.Map.AddObject(old);
                }
                else if (scaffolding.IsBridge)
                {
                    GameMap.StatisticsManager.RemoveStatistic(Statistic.BridgesBuilt, scaffolding.PlayerNumber);
                    var newBridge = new TileChange(mTerrainOld,
                        Terrain.Bridge,
                        (uint) scaffolding.Location.X,
                        (uint) scaffolding.Location.Y);
                    newBridge.RevertChange(gameState);
                    mResourceBuilding.IronDepositsConsumed = null;
                    mResourceBuilding.CurrentResourcesStored = mResourceBuilding.MaxResourcesStorable;
                    old = mResourceBuilding;
                    gameState.Map.AddObject(old);
                }
                else
                {
                    GameMap.StatisticsManager.RemoveStatistic(Statistic.BuildingsBuilt, scaffolding.PlayerNumber);
                    mResourceBuilding.IronDepositsConsumed = null;
                    gameState.Map.RemoveObject(scaffolding.TurnsInto, true, true);
                    mResourceBuilding.CurrentResourcesStored = mResourceBuilding.MaxResourcesStorable;
                    old = mResourceBuilding;
                    gameState.Map.AddObject(old);
                }
            }

            if (mSuccess)
            {
                old.CurrentResourcesStored.ItemAmounts[mItem] += mDeposit ? -1 : 1;
            }
        }
    }
}