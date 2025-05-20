using System.Collections.Generic;

namespace Common.com.objects
{
    public abstract class Building : ObjectImmovable
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public bool IsBlueprint { get; set; }

        public int CurrentHp { get; set; }

        public abstract int MaxHp { get; }
        public abstract Items ResourceCost { get; }
        
        public abstract List<Terrain> TerrainRequirement { get; }
        public abstract int VisionRange { get; }
        protected Building()
        {
            ResetHp();
        }

        private void ResetHp()
        {
            CurrentHp = MaxHp;
        }
    }
    
}