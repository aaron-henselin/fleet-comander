using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetCommander.Simulation.Simulation.Ships
{
    public static class DamageAllocationChart
    {
        static DamageAllocationTrack1 track1 = new DamageAllocationTrack1();
        public static  DamageAllocationTrack GetTrack(int trackNumber)
        {
            return track1;
        }

        public static DamageAllocationTrack Power => track1;
        public static DamageAllocationTrack Weapons => track1;
    }

    public class DamageAllocationTrack1 :DamageAllocationTrack
    {
        public DamageAllocationTrack1()
        {
            Add(DamageAllocationType.RWarp,DamageAllocationType.LWarp);
            Add(DamageAllocationType.Impulse, DamageAllocationType.Reactor);
            Add(DamageAllocationType.LWarp, DamageAllocationType.RWarp);
            Add(DamageAllocationType.FHull, DamageAllocationType.RHull);
            Add(DamageAllocationType.Lab, DamageAllocationType.Tractor);
            Add(DamageAllocationType.Trans, DamageAllocationType.Battery);
            Add(DamageAllocationType.Battery, DamageAllocationType.CWarp);
            Add(DamageAllocationType.RHull, DamageAllocationType.Lab);
            Add(DamageAllocationType.Reactor, DamageAllocationType.Impulse);
            Add(DamageAllocationType.AnyWarp, DamageAllocationType.Frame);
        }
    }

    public abstract class DamageAllocationTrack
    {
        public SystemTargeting SystemTargeting { get; protected set; }
        
        private List<DamageAllocationTrackItem> _items = new List<DamageAllocationTrackItem>();

        protected void Add(DamageAllocationType damageType, DamageAllocationType altDamageType)
        {
            _items.Add(new DamageAllocationTrackItem {DamageType = damageType,AltDamageType = altDamageType});
        }

        public DamageAllocationTrackItem Get(int index)
        {
            return _items[index];
        }
    }

    public class DamageAllocationTrackItem
    {
        public DamageAllocationType DamageType { get; set; }
        public DamageAllocationType AltDamageType { get; set; }
    }

    public enum DamageAllocationType
    {
        RWarp,LWarp,Impulse,Reactor,FHull,RHull,Lab,Tractor,Trans,Battery,CWarp,AnyWarp,Frame,
        Phaser,Drone,Bridge,Flag,Shuttle,Aux,Emer,Torpedo
    }
}
