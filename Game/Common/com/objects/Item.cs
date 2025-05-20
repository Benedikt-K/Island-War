using System.Collections.Generic;

namespace Common.com.objects
{
    public enum Item
    {
        Nothing=0, Wood=6, Plank=1,
        RawStone=2, Stone=3,
        IronOre=4, Iron=5,
        
    }
    public struct Items
    {
        public Dictionary<Item, int> ItemAmounts { get; set; }
       
        public Items(IReadOnlyList<Item> items, IReadOnlyList<int> amounts)
        {
            ItemAmounts = new Dictionary<Item, int>();
            for (var i = 0; i < items.Count; i++)
            {
                ItemAmounts[items[i]] = amounts[i];
            }
        }

        public int Get(Item item)
        {
            return ItemAmounts.ContainsKey(item) ? ItemAmounts[item] : 0;
        }
        public bool Same(Items items,bool run1=true)
        {
            foreach (var resource in ItemAmounts.Keys)
            {
                if (ItemAmounts[resource] == 0)
                {
                    if (items.ItemAmounts.ContainsKey(resource) && items.ItemAmounts[resource] != 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if (!items.ItemAmounts.ContainsKey(resource) || items.ItemAmounts[resource] != ItemAmounts[resource])
                    {
                        return false;
                    }
                }
            }

            if (run1)
            {
                return items.Same(this,false);
            }

            return true;
        }
    }
}