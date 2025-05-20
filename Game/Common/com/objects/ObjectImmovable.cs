using Common.com.objects.immovables.Resources;
using System.Collections.Generic;
using System.Drawing;

namespace Common.com.objects
{
    public abstract class ObjectImmovable:GameObject
    {
        public Point Location { get; set; }
        public abstract Size TileSize { get; }
        public HashSet<IronDeposit> IronDepositsConsumed { get; set; }

        
        public override Rectangle GetBounds()
        {
            
            return new Rectangle(Location,TileSize);
        }

        public void AddIronDeposit(IronDeposit deposit)
        {
            IronDepositsConsumed ??= new HashSet<IronDeposit>();
            IronDepositsConsumed.Add(deposit);
        }
    }
}