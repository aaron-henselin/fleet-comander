using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FleetCommander.Simulation.Simulation.Ships;

namespace FleetCommander.Simulation
{
  
    public class SimulationTurn
    {
        public int TurnNumber { get; set; }

        public Dictionary<int, EnergyAllocationDeclaration> EnergyAllocations { get; set; } = new Dictionary<int, EnergyAllocationDeclaration>();
        public ImpulseProcessActions ImpulseProcessActions { get; set; } = new ImpulseProcessActions();
    }


    public class ImpulseProcessActions
    {
        private Dictionary<Tuple<SimulationTimeStamp, int>, DeclaredNavigation> NavigationSchedule { get; }
            = new Dictionary<Tuple<SimulationTimeStamp, int>, DeclaredNavigation>();

        private Dictionary<Tuple<SimulationTimeStamp, int>, OffensiveFireDeclaration> OffensiveFireSchedule { get; }
            = new Dictionary<Tuple<SimulationTimeStamp, int>, OffensiveFireDeclaration>();

        public void SetDeclaredNavigation(int shipId, SimulationTimeStamp ts,DeclaredNavigation navigation)
        {
            var key = new Tuple<SimulationTimeStamp, int>(ts, shipId);
            NavigationSchedule[key] = navigation;
        }

        public DeclaredNavigation GetDeclaredNavigation(int shipId, SimulationTimeStamp ts)
        {
            var key = new Tuple<SimulationTimeStamp, int>(ts, shipId);
            if (NavigationSchedule.ContainsKey(key))
                return NavigationSchedule[key];

            return default(DeclaredNavigation);
        }

        public void SetOffensiveFire(int shipId, SimulationTimeStamp ts, OffensiveFireDeclaration offensiveFireDeclaration)
        {
            var key = new Tuple<SimulationTimeStamp, int>(ts, shipId);
            OffensiveFireSchedule[key] = offensiveFireDeclaration;
        }

        public OffensiveFireDeclaration GetOffensiveFire(int shipId, SimulationTimeStamp ts)
        {
            var key = new Tuple<SimulationTimeStamp, int>(ts, shipId);
            if (OffensiveFireSchedule.ContainsKey(key))
                return OffensiveFireSchedule[key];

            return null;
        }
    }



    public class SubPulseActions
    {
        internal Dictionary<int, DeclaredNavigation> DeclaredNavigation { get; set; } = new Dictionary<int, DeclaredNavigation>();
    }

    public class EndOfTurnActions
    {
    }

    public class RepairPrioritization
    {

    }
    public enum SystemTargeting
    {
        Indiscriminant, Power, Weapons
    }
    public class OffensiveFireDeclaration
    {
        public List<Volley> Volleys { get; set; }
    }

    public class Volley
    {
        public List<string> SsdCode { get; set; }
        public int ShipFrom { get; set; }
        public int ShipTo { get; set; }
        public SystemTargeting Targeting { get; set; }
        public int Distance { get; internal set; }
    }
    public struct DeclaredSpeedChange
    {
        public SpeedChange? SpeedChange { get; set; }
    }
    public struct DeclaredNavigation
    {
        public int? SideSlipDirection { get; set; }
        public int? NewFacing { get; set; }
    }

    public enum SpeedChange { Unknown, Accelerate, Decelerate }

}
