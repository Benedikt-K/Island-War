using Common.com.objects;
using Common.com.objects.entities;
using System.Collections.Generic;
using System.Linq;
using Common.com.game.settings;
using Common.com.objects.immovables.Buildings;
using Common.com.rollbacks;
using Common.com.rollbacks.changes;

namespace Common.com.game
{
    public sealed class LogisticsSystem
    {
        private readonly SortedSet<int> mWorkers=new SortedSet<int>();
        private SortedSet<int> StorageBuildings { get; }
        private SortedSet<int> ActiveProvider { get; }
        private SortedSet<int> ActiveRequester { get; }

        private TaskPossibilitySystem mSystem;
        public int TickNr { private get; set; }


        private readonly Dictionary<int, Items> mTaskItems=new Dictionary<int, Items>();

        public LogisticsSystem()
        {
            StorageBuildings = new SortedSet<int>();
            ActiveProvider = new SortedSet<int>();
            ActiveRequester = new SortedSet<int>();
        }

        private void OnAdd(GameMap gameMap, Worker worker, bool newObject,bool noNewTask=false)
        {
            
            mWorkers.Add(worker.Id);
            if (newObject)
            {
                
                if (!worker.CurrentAction.IsOccupied&&!noNewTask)
                {
                    NewTask(gameMap, worker);
                }
                else
                {
                    AddWorkerTask(gameMap,worker);
                }
            }
        }

        public void RetryAllWorkerTasks(GameMap gameMap)
        {
            foreach (var workerId in mWorkers.ToArray())
            {
                var worker = (Worker)gameMap.GetObject(workerId);
                if (!worker.CurrentAction.IsOccupied && worker.CurrentAction.TransportingFromId == -1)
                {
                    NewTask(gameMap,worker);
                }
                if (worker.CurrentAction.IsOccupied && worker.CurrentAction.TransportingFromId != -1&&worker.PathDone())
                {
                    worker.RedoPath(gameMap);
                    OnNewPosition(gameMap,worker);
                }

                if (!worker.CurrentAction.IsOccupied && worker.PathDone())
                {
                    RemoveTask(worker, gameMap, false);
                    NewTask(gameMap, worker);
                }
            }
        }
        private void UpdateAllWorkerTasks(GameMap gameMap)
        {
            
            foreach (var workerId in mWorkers)
            {
                var worker = (Worker)gameMap.GetObject(workerId);
                if (!worker.CurrentAction.IsOccupied && worker.CurrentAction.TransportingFromId == -1)
                {
                    NewTask(gameMap,worker);
                }
            }
        }
        public void OnNewObjectAction(GameMap gameMap,Worker worker,ObjectAction objectAction)
        {
            OnRemove(gameMap,worker);
            worker.CurrentAction = objectAction;
            OnAdd(gameMap,worker,true,true);
            
        }
        private void OnRemove(GameMap gameMap, Worker worker)
        {
            mWorkers.Remove(worker.Id);
            if (worker.CurrentAction.IsOccupied)
            {
                RemoveTask(worker,gameMap,false);
            }
        }

        public void AddTaskItemAmount(int buildingId,Item item,int amount)
        {
            if (!mTaskItems.ContainsKey(buildingId))
            {
                var items = new Items
                {
                    ItemAmounts = new Dictionary<Item, int>()
                };
                mTaskItems[buildingId] = items;
            }

            if (!mTaskItems[buildingId].ItemAmounts.ContainsKey(item))
            {
                mTaskItems[buildingId].ItemAmounts[item] = 0;
            }

            mTaskItems[buildingId].ItemAmounts[item] += amount;
        }

        private int GetTaskItemAmount(int buildingId,Item item)
        {
            if (!mTaskItems.ContainsKey(buildingId)||!mTaskItems[buildingId].ItemAmounts.ContainsKey(item))
            {
                return 0;
            }

            return mTaskItems[buildingId].ItemAmounts[item];
        }

        public int PlannedItems(Item item,ResourceBuilding resourceBuilding)
        {
            return GetTaskItemAmount(resourceBuilding.Id,item) + resourceBuilding.CurrentResourcesStored.Get(item);
        }
        private bool IsPlannedFull(Item item, ResourceBuilding resourceBuilding)
        {
            if (!resourceBuilding.MaxResourcesStorable.ItemAmounts.ContainsKey(item))
            {
                return true;
            }
            return PlannedItems(item,resourceBuilding)>=resourceBuilding.MaxResourcesStorable.ItemAmounts[item];
        }
        private bool IsPlannedEmpty(Item item, ResourceBuilding resourceBuilding)
        {
            if (!resourceBuilding.MaxResourcesStorable.ItemAmounts.ContainsKey(item))
            {
                return true;
            }
            return PlannedItems(item,resourceBuilding)<=0;
        }
        
        private int GetTaskPriority(GameMap gameMap, GameObject worker, ObjectAction objectAction)
        {
            
            if (objectAction == null)
            {
                return -1;
            }

            var res = 0;
            var building1 = (ResourceBuilding)gameMap.GetObject(objectAction.TransportingFromId);
            var building2 = (ResourceBuilding)gameMap.GetObject(objectAction.TransportingToId);
            var distanceFactor = (int)building1.EuclideanDistance(building2);
            if (building2 is Scaffolding {TurnsInto: Tower _})
            {
                res += NumberManager.Thirty;
            }

            switch (building2)
            {
                case Scaffolding _:res += NumberManager.Twenty;
                    break;
            }

            if (building2.ActiveRequester!=null&&building2.ActiveRequester.Contains(objectAction.ItemTransportIntent))
            {
                res += NumberManager.OneHundred;
            } 
            res += PlannedItems(objectAction.ItemTransportIntent, building1);
            
            if (!(building2 is Scaffolding))
            {
                res -= PlannedItems(objectAction.ItemTransportIntent, building2)*NumberManager.Five;
            }

            res += building1.Priority* NumberManager.Twenty;
            res += NumberManager.TenThousand - distanceFactor;
            res -= (int) worker.EuclideanDistance(building1);
            return res;
        }

        private bool Tiebreaker(ObjectAction action1, ObjectAction action2)
        {
            if (action1 == null)
            {
                return false;
            }
            if (action2 == null)
            {
                return true;
            }
            return action1.TransportingFromId > action2.TransportingFromId ||
                   (action1.TransportingFromId == action2.TransportingFromId &&
                    action1.TransportingToId > action2.TransportingToId)||(
                   action1.TransportingFromId == action2.TransportingFromId&&
                   action1.TransportingToId == action2.TransportingToId&&
                   action2.ItemTransportIntent.CompareTo(action1.ItemTransportIntent)<0);
        }
        private ObjectAction NewRequesterTask(GameMap gameMap, Worker worker)
        {
            ObjectAction bestTask=null;
            var bestPriority = -1;
            foreach (var activeRequester in ActiveRequester)
            {
                if (worker.PlayerNumber != gameMap.GetObject(activeRequester).PlayerNumber)
                {
                    continue;
                }
                if (!IsTaskDoable(worker,
                    gameMap,
                    gameMap.GetObject(activeRequester)))
                {
                    continue;
                }
                
                foreach (var item in ((ResourceBuilding)gameMap.GetObject(activeRequester)).ActiveRequester)
                {
                    
                    foreach (var storage in StorageBuildings)
                    {

                        if (!IsTaskDoable(worker,
                            gameMap,
                            gameMap.GetObject(storage)))
                        {
                            continue;
                        }
                        
                        if (worker.PlayerNumber != gameMap.GetObject(storage).PlayerNumber)
                        {
                            continue;
                        }
                        var currentTask = new ObjectAction();
                        if (!IsPlannedEmpty(item, (ResourceBuilding)gameMap.GetObject(storage))&&
                            !IsPlannedFull(item, (ResourceBuilding)gameMap.GetObject(activeRequester)))
                        {
                            currentTask.ItemTransportIntent = item;
                            currentTask.TransportingFromId = storage;
                            currentTask.TransportingToId = activeRequester;
                            var currentPriority = GetTaskPriority(gameMap,worker, currentTask);
                            if (bestPriority == -1 || (bestPriority<=currentPriority&&(bestPriority < currentPriority||Tiebreaker(currentTask,bestTask))))
                            {
                                bestPriority = currentPriority;
                                bestTask = currentTask;
                            }
                        }


                    }
                }
                
            }
            
            return bestTask;
        }
        private ObjectAction NewProviderTask(GameMap gameMap, Worker worker)
        {
            ObjectAction bestTask = null;
            var bestPriority = -1;
            foreach (var activeProviderId in ActiveProvider)
            {
                var activeProvider = ((ResourceBuilding)gameMap.GetObject(activeProviderId));
                if (!IsTaskDoable(worker,
                    gameMap,
                   activeProvider))
                {
                    continue;
                }
                
                foreach (var storage in StorageBuildings)
                {
                    var currentTask = new ObjectAction();
                    if (!IsTaskDoable(worker,
                       gameMap,
                      gameMap.GetObject(storage)))
                    {
                        continue;
                    }
                    if (!IsPlannedFull(activeProvider.ActiveProvider,
                            ((ResourceBuilding)gameMap.GetObject(storage))) &&
                        !IsPlannedEmpty(activeProvider.ActiveProvider, activeProvider))
                    {
                        currentTask.ItemTransportIntent = activeProvider.ActiveProvider;
                        currentTask.TransportingFromId = activeProviderId;
                        currentTask.TransportingToId = storage;

                        var currentPriority = GetTaskPriority(gameMap,worker, currentTask);
                        if (bestPriority == -1 || (bestPriority <= currentPriority &&
                                                   (bestPriority < currentPriority ||
                                                    Tiebreaker(currentTask, bestTask))))
                        {
                            bestPriority = currentPriority;
                            bestTask = currentTask;
                        }
                    }


                }

                foreach (var storage in ActiveRequester)
                {
                    if (worker.PlayerNumber != gameMap.GetObject(storage).PlayerNumber)
                    {
                        continue;
                    }
                    if (!IsTaskDoable(worker,
                        gameMap,
                        gameMap.GetObject(storage)))
                    {
                        continue;
                    }

                    if (activeProvider.Id == storage)
                    {
                        continue;
                    }
                    var currentTask = new ObjectAction();
                    if (((ResourceBuilding)gameMap.GetObject(storage)).ActiveRequester.Contains(activeProvider.ActiveProvider)&&!IsPlannedFull(activeProvider.ActiveProvider, ((ResourceBuilding) gameMap.GetObject(storage)))&&
                        !IsPlannedEmpty(activeProvider.ActiveProvider, activeProvider))
                    {
                        currentTask.ItemTransportIntent = activeProvider.ActiveProvider;
                        currentTask.TransportingFromId = activeProvider.Id;
                        currentTask.TransportingToId = storage;
                        var currentPriority = GetTaskPriority(gameMap,worker, currentTask);
                        if (bestPriority == -1 || (bestPriority<=currentPriority&&(bestPriority < currentPriority||Tiebreaker(currentTask,bestTask))))
                        {
                            bestPriority = currentPriority;
                            bestTask = currentTask;
                        }
                    }

                }
            }
            
            return bestTask;
        }

        public void OnNewPosition(GameMap gameMap,Worker worker)
        {
            if (worker.CurrentAction.TransportingFromId == -1)
            {
                return;
            }
            if (!worker.HasItemRequested())
            {
                var transportingFrom = gameMap.GetObject(worker.CurrentAction.TransportingFromId);
                if (transportingFrom.Adjacent(worker) && transportingFrom is ResourceBuilding resourceBuilding)
                {
                    
                    if (resourceBuilding.HasResources(new Items(new[] {worker.CurrentAction.ItemTransportIntent},new[]{1})))
                    {
                        gameMap.Changes ??= new List<IChange>();
                        gameMap.Changes.Add(new ItemChange(worker, resourceBuilding, false,
                            worker.CurrentAction.ItemTransportIntent, this));
                        gameMap.Changes[^1].DoChange(new GameState(gameMap,-1,false, NumberManager.Two));
                    }
                }
            }
            else
            {
                var transportingTo = gameMap.GetObject(worker.CurrentAction.TransportingToId);
                if (transportingTo!=null&&transportingTo.Adjacent(worker)&&transportingTo is ResourceBuilding resourceBuilding)
                {
                    gameMap.Changes ??= new List<IChange>();
                    gameMap.Changes.Add(new ItemChange(worker,resourceBuilding,true,worker.CurrentAction.ItemTransportIntent,this));
                    gameMap.Changes[^1].DoChange(new GameState(gameMap,-1,false, NumberManager.Two));
                }
            }
        }

        public void NewTask(GameMap gameMap, Worker worker)
        {
            if (worker.CurrentAction.IsOccupied)
            {
                return;
            }

            if (worker.HoldingItem != Item.Nothing)
            {
                 BringItemToStorage(gameMap,worker);
                return;
            }
            var action1 = NewProviderTask(gameMap, worker);
            var action2 = NewRequesterTask(gameMap, worker);
            var priority1 = GetTaskPriority(gameMap,worker, action1);
            var priority2 = GetTaskPriority(gameMap,worker, action2);
            var best = priority1>=priority2&&(priority1>priority2||Tiebreaker(action1,action2))?action1:action2;
            if (best != null)
            {
                best.UserMade = false;
                AddTask(gameMap,worker, best.TransportingFromId, best.TransportingToId, best.ItemTransportIntent);
                OnNewPosition(gameMap,worker);
                worker.RedoPath(gameMap);
            }
        }
        private void AddWorkerTask(GameMap gameMap,Worker worker)
        {
            if (worker.CurrentAction.TransportingFromId == -1)
            {
                return;
            }
            AddTask(gameMap,worker,worker.CurrentAction.TransportingFromId,worker.CurrentAction.TransportingToId,worker.CurrentAction.ItemTransportIntent);
        }
        private void AddTask(GameMap gameMap,Worker worker, int building1Id, int building2Id, Item item)
        {
            worker.CurrentAction.ItemTransportIntent = item;
            worker.CurrentAction.IsOccupied = true;
            worker.CurrentAction.TransportingFromId = building1Id;
            worker.CurrentAction.TransportingToId = building2Id;
            AddTaskItem(gameMap,worker);
            
        }

        public void RemoveTask(Worker worker, GameMap gameMap,bool redoPath)
        {
            var doChanges=false;
            var b = false;
            if (worker.CurrentAction.ItemTransportIntent != Item.Nothing)
            {
                b = worker.CurrentAction.ItemTransportIntent != worker.HoldingItem&&!redoPath;
                RemoveTaskItem(gameMap, worker, redoPath);
                doChanges = true;
            }


            worker.CurrentAction.ItemTransportIntent = Item.Nothing;
            worker.CurrentAction.IsOccupied = false;
            worker.CurrentAction.TransportingFromId = -1;
            worker.CurrentAction.TransportingToId = -1;
            if (doChanges)
            {
                if (b)
                {
                    gameMap.Changes[^NumberManager.Two].DoChange(new GameState(gameMap, -1, false, NumberManager.Two));
                    gameMap.Changes[^1].DoChange(new GameState(gameMap, -1, false, NumberManager.Two));
                }
                else
                {
                    gameMap.Changes[^1].DoChange(new GameState(gameMap, -1, false, NumberManager.Two));
                }
            }

        }

        private void AddTaskItem(GameMap gameMap, Worker taskWorker)
        {
            gameMap.Changes ??= new List<IChange>();
            gameMap.Changes.Add(new ExpectedItemChange(taskWorker.CurrentAction.ItemTransportIntent,
                taskWorker.CurrentAction.TransportingToId, 1));
            gameMap.Changes[^1].DoChange(new GameState(gameMap,-1,false, NumberManager.Two));
            if (!taskWorker.HasItemRequested())
            {
                gameMap.Changes.Add(new ExpectedItemChange(taskWorker.CurrentAction.ItemTransportIntent,
                    taskWorker.CurrentAction.TransportingFromId, -1));
                gameMap.Changes[^1].DoChange(new GameState(gameMap,-1,false, NumberManager.Two));
            }
        }

        public List<Worker> GetAllWorkers(GameMap gameMap)
        {
            var res = new List<Worker>();
            foreach (var worker in mWorkers)
            {
                res.Add((Worker)gameMap.GetObject(worker));
            }
            return res;
        }
        public void AddSourceTaskItem(GameMap gameMap,Worker taskWorker)
        {
            gameMap.Changes.Add(new ExpectedItemChange(taskWorker.CurrentAction.ItemTransportIntent,
                taskWorker.CurrentAction.TransportingFromId, 1));
            gameMap.Changes[^1].DoChange(new GameState(gameMap,-1,false, NumberManager.Two));
        }
        private void RemoveTaskItem(GameMap gameMap,Worker taskWorker,bool redoPath)
        {
            gameMap.Changes ??= new List<IChange>();
            gameMap.Changes.Add(new ExpectedItemChange(taskWorker.CurrentAction.ItemTransportIntent,
                    taskWorker.CurrentAction.TransportingToId,
                    -1));

             
            if (taskWorker.CurrentAction.ItemTransportIntent!=taskWorker.HoldingItem&&!redoPath)
            {
                gameMap.Changes.Add(new ExpectedItemChange(taskWorker.CurrentAction.ItemTransportIntent,
                    taskWorker.CurrentAction.TransportingFromId, 1));
            }
        }
        public void OnRemove(GameMap gameMap,GameObject gameObject,bool reverting)
        {
            switch (gameObject)
            {
                case ResourceBuilding resourceBuilding: OnRemove(gameMap,resourceBuilding,reverting);
                    break;
                case Worker worker: OnRemove(gameMap,worker);
                    break;
            }
        }
        public void OnAdd(GameMap gameMap,GameObject gameObject,bool newObject,bool initialization)
        {
            switch (gameObject)
            {
                case ResourceBuilding resourceBuilding: OnAdd(gameMap,resourceBuilding,newObject,initialization);
                    break;
                case Worker worker: OnAdd(gameMap,worker,newObject,initialization);
                    break;
            }
        }
        private void OnAdd(GameMap gameMap,ResourceBuilding building,bool newObject,bool initialization=false)
        {
            
            if (building.ActiveProvider!=Item.Nothing)
            {
                ActiveProvider.Add(building.Id);
            }
            if (building.ActiveRequester!=null)
            {
                ActiveRequester.Add(building.Id);
            }
            else
            {
                if (building.ActiveProvider==Item.Nothing)
                {
                    StorageBuildings.Add(building.Id);
                }
            }
            if (newObject&&!initialization)
            {
                UpdateAllWorkerTasks(gameMap);
            }
            
        }

        private void OnRemove(GameMap gameMap,ResourceBuilding building,bool reverting)
        {
            if (building.ActiveProvider!=Item.Nothing)
            {
                ActiveProvider.Remove(building.Id);
            }
            if (building.ActiveRequester!=null)
            {
                ActiveRequester.Remove(building.Id);
            }
            else
            {
                if (building.ActiveProvider==Item.Nothing)
                {
                    StorageBuildings.Remove(building.Id);
                }
            }

            if (!reverting)
            {
                foreach (var workerId in mWorkers)
                {
                    var worker = ((Worker) gameMap.GetObject(workerId));
                    if (worker.CurrentAction.TransportingFromId == building.Id)
                    {
                        RemoveTask(worker, gameMap, false);
                        NewTask(gameMap, worker);
                    }

                    if (worker.CurrentAction.TransportingToId == building.Id)
                    {
                        RemoveTask(worker, gameMap, false);
                        NewTask(gameMap, worker);
                    }
                }
            }

        }

        public void OnNewBridge(GameMap gameMap,int x,int y)
        {
            TaskPossibilitySystem.IsTaskDoable(gameMap,new Worker(){X=x,Y=y},null,TickNr);
        }
        private bool IsTaskDoable(Worker worker, GameMap gameMap, GameObject resourceBuilding)
        {
            if (worker.PlayerNumber!=resourceBuilding.PlayerNumber)
            {
                return false;
            }
            if (resourceBuilding is Scaffolding {IsBridge: true} scaffolding)
            {
                foreach (var resource in scaffolding.ActiveRequester)
                {
                    if (PlannedItems(resource,scaffolding)>0&&scaffolding.StorageAvailable(resource))
                    {
                        return false;
                    }
                }
            }
            if (mSystem == null || !mSystem.IsTick(TickNr))
            {
                mSystem = new TaskPossibilitySystem(TickNr);
            }

            
            return mSystem.IsTaskDoable(gameMap,worker,resourceBuilding);
        }
        
        private void BringItemToStorage(GameMap gameMap, Worker worker)
        {
            ObjectAction bestYet=null;
            var bestPriority = -1;

            void UpdatePriorityWith(int storage)
            {
                if (!IsTaskDoable(worker,
                    gameMap,
                    gameMap.GetObject(storage)))
                {
                    return;
                }

                if (worker.PlayerNumber != gameMap.GetObject(storage).PlayerNumber)
                {
                    return ;
                }
                var currentTask = new ObjectAction();
                if (!IsPlannedFull(worker.HoldingItem, (ResourceBuilding)gameMap.GetObject(storage)))
                {
                    currentTask.ItemTransportIntent = worker.HoldingItem;
                    currentTask.TransportingFromId = storage;
                    currentTask.TransportingToId = storage;
                    currentTask.UserMade = false;
                    var currentPriority = GetTaskPriority(gameMap,worker, currentTask);
                    if (bestPriority == -1 || (bestPriority<=currentPriority&&(bestPriority < currentPriority||Tiebreaker(currentTask,bestYet))))
                    {
                        bestPriority = currentPriority;
                        bestYet = currentTask;
                    }
                }
            }
            foreach (var storage in StorageBuildings)
            {
                UpdatePriorityWith(storage);
            }
            foreach (var storage in ActiveRequester)
            {
                if (gameMap.GetObject(storage) is ResourceBuilding {ActiveRequester: { }} resourceBuilding&&resourceBuilding.ActiveRequester.Contains(worker.HoldingItem))
                {
                    UpdatePriorityWith(storage);
                }
            }

            if (bestYet != null)
            {
                AddTask(gameMap, worker, bestYet.TransportingFromId, bestYet.TransportingToId, bestYet.ItemTransportIntent);
                OnNewPosition(gameMap, worker);
                worker.RedoPath(gameMap);
            }
            
        }
        
    }
}

