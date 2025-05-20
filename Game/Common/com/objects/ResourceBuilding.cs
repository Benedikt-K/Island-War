using System.Collections.Generic;
using System.Text.Json.Serialization;
using Common.com.game;
using Common.com.game.settings;
using Common.com.Menu;
using Common.com.objects.entities;
using Common.com.objects.immovables.Buildings;
using Common.com.rollbacks;

namespace Common.com.objects
{
    public abstract class ResourceBuilding : Building
    {
        public Items CurrentResourcesStored { get; set; }
        [JsonIgnore]
        public abstract Items MaxResourcesStorable { get; }
        [JsonIgnore]public abstract Item ActiveProvider { get; }
        [JsonIgnore]public abstract Item[] ActiveRequester { get;}
        
        public int Priority { get; set; }
        protected ResourceBuilding()
        {
            Priority = NumberManager.Three;
            CurrentResourcesStored = new Items
            {
                ItemAmounts = new Dictionary<Item, int>()
            };
            EmptyResources();
        }
        private sealed class ItemTracker:INumber
        {
            private readonly RollbackManager mRollbackManager;
            private readonly int mId;
            private readonly Item mItem;
            private readonly LogisticsSystem mLogisticsSystem;
            public ItemTracker(RollbackManager rollbackManager,int id,Item item)
            {
                mRollbackManager = rollbackManager;
                mLogisticsSystem = mRollbackManager.CurrentState.Map.GetLogisticsSystem();
                mId = id;
                mItem = item;
            }

            public string Number =>
                "" + ((ResourceBuilding) mRollbackManager.CurrentState.Map.GetObject(mId)).CurrentResourcesStored
                .Get(mItem) + " (" + mLogisticsSystem.PlannedItems(mItem,(ResourceBuilding) mRollbackManager.CurrentState.Map.GetObject(mId))+")";
        }
        private sealed class PriorityTracker:INumber
        {
            private readonly RollbackManager mRollbackManager;
            private readonly int mId;
            public PriorityTracker(RollbackManager rollbackManager,int id)
            {
                mRollbackManager = rollbackManager;
                mId = id;
            }

            public string Number => "Priority "+((ResourceBuilding) mRollbackManager.CurrentState.Map.GetObject(mId)).Priority;
        }
        public INumber GetTracker(RollbackManager rollbackManager, Item item)
        {
            return new ItemTracker(rollbackManager,Id,item);
        }

        public INumber GetPriorityTracker(RollbackManager rollbackManager)
        {
            return new PriorityTracker(rollbackManager,Id);
        }
        
        public bool WithdrawResource(Worker worker, Item item)
        {
            if (Adjacent(worker)&& worker.HoldingItem == Item.Nothing)
            {
                if (TakeResources(new Items(new[] {item}, new[] {1})))
                {
                    worker.HoldingItem = item;
                    return true;
                }

                
            }
            return false;
        }

        public void AddResource(Item item, int amount)
        {
            if (!CurrentResourcesStored.ItemAmounts.ContainsKey(item))
            {
                CurrentResourcesStored.ItemAmounts[item] = 0;
            }
            CurrentResourcesStored.ItemAmounts[item]+=amount;
        }
        public virtual bool DepositResource(Worker worker)
        {
            if (Adjacent(worker)&& worker.HoldingItem!=Item.Nothing&&StorageAvailable(worker.HoldingItem))
            {
                AddResource(worker.HoldingItem,1);
                worker.HoldingItem = Item.Nothing;
                return true;
            }

            return false;
        }

        public bool HasResources(Items items)
        {
            foreach (var item in items.ItemAmounts.Keys)
            {
                if (!CurrentResourcesStored.ItemAmounts.ContainsKey(item) ||
                    items.ItemAmounts[item] > CurrentResourcesStored.ItemAmounts[item])
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasSpaceForResources(Items items)
        {
            foreach (var item in items.ItemAmounts.Keys)
            {
                if (!CurrentResourcesStored.ItemAmounts.ContainsKey(item) ||
                    items.ItemAmounts[item] > MaxResourcesStorable.ItemAmounts[item]-CurrentResourcesStored.ItemAmounts[item])
                {
                    return false;
                }
            }

            return true;
        }
        public bool CanChangeItem(Item item, int amount)
        {
            if (item == Item.Nothing)
            {
                return true;
            }
            if (amount<0)
            {
                if (CurrentResourcesStored.ItemAmounts[item] > 0)
                {
                    return true;
                }

                
            }
            else
            {
                if (CurrentResourcesStored.ItemAmounts[item]<MaxResourcesStorable.ItemAmounts[item])
                {
                    return true;
                }
            }

            return false;
        }
        // ReSharper disable once UnusedMember.Global
        // ReSharper think it is not used but is it used more then once 
        public void ChangeItem(Item item, int amount)
        {
            if (item == Item.Nothing)
            {
                return;
            }
            if (amount<0)
            {
                if (CurrentResourcesStored.ItemAmounts[item] > 0)
                {
                    CurrentResourcesStored.ItemAmounts[item] += amount;
                }

                
            }
            else
            {
                if (CurrentResourcesStored.ItemAmounts[item]<MaxResourcesStorable.ItemAmounts[item])
                {
                    CurrentResourcesStored.ItemAmounts[item] += amount;
                }
            }
        }
        public void EmptyResources(bool ignoreScaffolding=false)
        {
            if (this is Scaffolding && !ignoreScaffolding)
            {
                return;
            }
            foreach (var resource in new List<Item>(MaxResourcesStorable.ItemAmounts.Keys))
            {
                if (!CurrentResourcesStored.ItemAmounts.ContainsKey(resource))
                {
                    CurrentResourcesStored.ItemAmounts.Add(resource,0);
                }
            }
        }
        public void GiveResources(Items items)
        {
            if (HasSpaceForResources(items))
            {
                foreach (var item in new List<Item>(items.ItemAmounts.Keys))
                {
                    AddResource(item,items.ItemAmounts[item]);
                }
                
            }
        }
        public bool TakeResources(Items items)
        {
            if (HasResources(items))
            {
                foreach (var item in new List<Item>(items.ItemAmounts.Keys))
                {
                    CurrentResourcesStored.ItemAmounts[item] -= items.ItemAmounts[item];
                }
                return true;
            }

            return false;
        }
        public bool StorageAvailable(Item item)
        {
            return MaxResourcesStorable.ItemAmounts.ContainsKey(item)&&(!CurrentResourcesStored.ItemAmounts.ContainsKey(item)||CurrentResourcesStored.ItemAmounts[item] < MaxResourcesStorable.ItemAmounts[item]);
        }
    }
}
