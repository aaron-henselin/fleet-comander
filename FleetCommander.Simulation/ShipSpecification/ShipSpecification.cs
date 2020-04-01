using System.Collections.Generic;
using System.Linq;

namespace FleetCommander.Simulation
{
    public class ShipSpecification
    {
        public int DamageControlRating { get; set; }
        public Dictionary<char, int> ShieldStrength { get; set; } = new Dictionary<char, int>();
        public Dictionary<int, int> Manuverability { get; set; } = new Dictionary<int, int>();

        public Loadout Loadout = new Loadout();
    }

    public class EnergyLoadout
    {
        public int LeftWarp { get; set; }
        public int RightWarp { get; set; }
        public int Impulse { get; set; }
        public int BatteryCount { get; set; }
        public int Reactor { get; internal set; }
    }

    public class Loadout
    {
        public WeaponHardmount GetWeaponBySsdCode(string ssdCode)
        {
            return Weapons.Single(x => x.SsdCode == ssdCode);
        }

        public int RearHull { get; set; }
        public int ForwardHull { get; set; }
        public EnergyLoadout EnergyLoadout { get; set; } = new EnergyLoadout();
        public List<WeaponHardmount> Weapons = new List<WeaponHardmount>();

        public void AddPhaser360(int ssdCode)
        {
            var ph3 = new WeaponHardmount
            {
                SsdCode = ssdCode.ToString(),
                FiringArcs = new List<Arc>{Arc.Unrestricted},
                WeaponType = WeaponType.PH3
            };

            Weapons.Add(ph3);
        }
        public void AddPhaser1(int ssdCode, params Arc[] arcs)
        {
            var ph1 = new WeaponHardmount
            {
                SsdCode = ssdCode.ToString(),
                FiringArcs = new List<Arc>(arcs),
                WeaponType = WeaponType.PH1
            };

            Weapons.Add(ph1);
        }
    }

    public class WeaponHardmount
    {

        public WeaponType WeaponType { get; set; }
        public List<Arc> FiringArcs { get; set; }
        public string SsdCode { get; internal set; }
    }

    public enum WeaponType
    {
        PH1, PHOT,PH3
    }

    public class FederationHeavyCruiserSpecification : ShipSpecification
    {
        public FederationHeavyCruiserSpecification()
        {
            ShieldStrength[Facing.A] = 15;
            ShieldStrength[Facing.B] = 12;
            ShieldStrength[Facing.C] = 12;
            ShieldStrength[Facing.D] = 12;
            ShieldStrength[Facing.E] = 12;
            ShieldStrength[Facing.F] = 12;

            Loadout.EnergyLoadout = new EnergyLoadout
            {
                BatteryCount = 2,
                Impulse = 2,
                LeftWarp = 8,
                RightWarp = 8,
                Reactor = 1,
            };
            Loadout.AddPhaser1(1, Arc.FH);
            Loadout.AddPhaser1(2,Arc.LF,Arc.L);
            Loadout.AddPhaser1(3, Arc.RF, Arc.R);
            Loadout.AddPhaser360(4);
            Loadout.AddPhaser1(5,Arc.RH);

        }

    }
    
}