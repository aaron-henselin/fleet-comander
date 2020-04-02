using System.Collections.Generic;
using System.Linq;

namespace FleetCommander.Simulation
{
    public class DamageOutputTable
    {
        protected List<DamageOutput> DamageOutputs { get; set; } = new List<DamageOutput>();

        protected void Add(int distanceMin,int distanceMax, int hitMin, int hitMax, int damage)
        {
            this.DamageOutputs.Add(new DamageOutput
            {
                RangeRequirement = new RangeRequirement
                {
                    MinDistance = distanceMin,
                    MaxDistance=distanceMax
                },
                HitRequirement = new HitRequirement
                {
                    MaxDieRoll = hitMin,
                    MinDieRoll = hitMax
                },
                DamageOutputed = damage
                
            });
        }

        public DamageOutput GetDamageOutput(int distance, int hit)
        {
            var byRange = DamageOutputs.Where(x => distance <= x.RangeRequirement.MaxDistance && distance >= x.RangeRequirement.MinDistance);
            var byHit = byRange.Where(x => hit <= x.HitRequirement.MaxDieRoll && hit >= x.HitRequirement.MinDieRoll);

            return byHit.SingleOrDefault();
        }
    }

    public struct DamageOutput
    {
        public RangeRequirement RangeRequirement { get; set; }
        public HitRequirement HitRequirement { get; set; }
        public int DamageOutputed { get; set; }
    }
    public struct RangeRequirement
    {
        public int MinDistance { get; set; }
        public int MaxDistance { get; set; }
    }
    public struct HitRequirement
    {
        public int MinDieRoll { get; set; }
        public int MaxDieRoll { get; set; }
    }

}