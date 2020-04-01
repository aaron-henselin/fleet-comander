using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using FleetCommander.Simulation.Framework.GridSystem;

namespace FleetCommander.Simulation
{
    public class PhotonTorpedoProjectile :DirectFireProjectile
    {
        public PhotonTorpedoOverloadState OverloadState { get; set; } = PhotonTorpedoOverloadState.NotOverloaded;

        readonly StandardPhotonTorpedoDamageOutputTable _standardPhotonTorpedoDamageOutputTable = new StandardPhotonTorpedoDamageOutputTable();
        readonly Overload4PhotonTorpedoDamageOutputTable _overload4PhotonTorpedoDamageOutputTable = new Overload4PhotonTorpedoDamageOutputTable();
        readonly Overload8PhotonTorpedoDamageOutputTable _overload8PhotonTorpedoDamageOutputTable = new Overload8PhotonTorpedoDamageOutputTable();

        public override int CalculateDamage()
        {
            if (OverloadState == PhotonTorpedoOverloadState.NotOverloaded)
            {
                return _standardPhotonTorpedoDamageOutputTable.GetDamageOutput(Distance, HitTrack).DamageOutputed;
            }

            if (OverloadState == PhotonTorpedoOverloadState.Overloaded4)
            {
                return _overload4PhotonTorpedoDamageOutputTable.GetDamageOutput(Distance, HitTrack).DamageOutputed;
            }

            if (OverloadState == PhotonTorpedoOverloadState.Overloaded8)
            {
                return _overload8PhotonTorpedoDamageOutputTable.GetDamageOutput(Distance, HitTrack).DamageOutputed;
            }

            throw new ArgumentException();
        }


        class StandardPhotonTorpedoDamageOutputTable : DamageOutputTable
        {
            public StandardPhotonTorpedoDamageOutputTable()
            {
                Add(0, 0, 1, 6, 8);
                Add(1, 1, 1, 6, 8);
                Add(2, 2, 1, 5, 8);
                Add(3, 4, 1, 4, 8);
                Add(5, 8, 1, 3, 8);
                Add(9, 12, 1, 2, 8);
                Add(13, 25, 1, 1, 8);
            }
        }

        class Overload4PhotonTorpedoDamageOutputTable : DamageOutputTable
        {
            public Overload4PhotonTorpedoDamageOutputTable()
            {
                Add(0, 0, 1, 6, 12);
                Add(1, 1, 1, 6, 12);
                Add(2, 2, 1, 5, 12);
                Add(3, 4, 1, 4, 12);
                Add(5, 8, 1, 3, 12);
            }
        }
        class Overload8PhotonTorpedoDamageOutputTable : DamageOutputTable
        {
            public Overload8PhotonTorpedoDamageOutputTable()
            {
                Add(0, 0, 1, 6, 16);
                Add(1, 1, 1, 6, 16);
                Add(2, 2, 1, 5, 16);
                Add(3, 4, 1, 4, 16);
                Add(5, 8, 1, 3, 16);
            }
        }
    }

    public class Phaser1Projectile : DirectFireProjectile
    {
        public override int CalculateDamage()
        {
            throw new NotImplementedException();
        }

        class Phaser1DamageOutputTable : DamageOutputTable
        {
            public Phaser1DamageOutputTable()
            {
                Add(0, 0, 1, 1, 9);
                Add(1, 1, 1, 1, 8);
                Add(2, 2, 1, 1, 7);
                Add(3, 3, 1, 1, 6);
                Add(4, 4, 1, 1, 5);
                Add(5, 5, 1, 1, 5);
                Add(6, 8, 1, 1, 4);
                Add(9, 15, 1, 1, 3);
                Add(16, 25, 1, 1, 2);

                Add(0, 0, 2, 2, 8);
                Add(1, 1, 2, 2, 7);
                Add(2, 2, 2, 2, 6);
                Add(3, 3, 2, 2, 5);
                Add(4, 4, 2, 2, 5);
                Add(5, 5, 2, 2, 4);
                Add(6, 8, 2, 2, 3);
                Add(9, 15, 2, 2, 2);
                Add(16, 25, 2, 2, 1);

                //todo
            }
        }
    }

    public class Phaser2Projectile : DirectFireProjectile
    {
        public override int CalculateDamage()
        {
            throw new NotImplementedException();
        }
    }

    public class Phaser3Projectile : DirectFireProjectile
    {
        public override int CalculateDamage()
        {
            throw new NotImplementedException();
        }
    }

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
    
    public abstract class DirectFireProjectile
    {
        public int Target { get; set; }
        public int Distance { get; set; }
        public SystemTargeting Targeting { get; set; }
        public int HitTrack { get; internal set; }
        public abstract int CalculateDamage();
    }
    public class Ship
    {
       
        public int ShipId { get; set; }
        public int CurrentSpeed { get; set; }
        public decimal UndeclaredEnginePool { get; set; }
        public ShipState ShipState { get; set; }
        public Position Position { get; set; }

        /* (During the first turn of a scenario, the ship has additional energy tokens
         *  equal to the number of batteries on the ship, representing power stored in the batteries. )
         */
        public void Initialize()
        {
            foreach (var shipStateAvailableBattery in ShipState.AvailableBatteries)
                shipStateAvailableBattery.IsCharged = true;
            
        }

        /*
         *  (1E3a) Power Phase: At the end of the turn, ships
         *       may transfer any unexpended power to their batteries (up to the limits of the battery capacity); 
         *       any excess unused power is lost.
         */
        internal void RolloverExcessEnergyIntoBatteries()
        {
            foreach (var battery in ShipState.AvailableBatteries)
                if (!battery.IsCharged && UndeclaredEnginePool >= 1)
                {
                    battery.IsCharged = true;
                    UndeclaredEnginePool -=1;
                }
            

        }

        public void ApplyDamage(DirectFireProjectile df, Dice dice)
        {

            //(3D3a) Step 1: Players roll a single six - sided die
            //to determine which one they will use(first), with the
            //die roll corresponding to the chart number selected

            int damageTrack = 0;
            if (df.Targeting == SystemTargeting.Indiscriminant)
                damageTrack = dice.RollD6();

            if (df.Targeting == SystemTargeting.Power)
                damageTrack = 1;

            if (df.Targeting == SystemTargeting.Weapons)
                damageTrack = 6;

        }

        public void EnactRepairs(int componentId)
        {
            var component = ShipState.GetComponent(componentId);
            component.Damaged = false;
            if (component is WeaponComponent weaponComponent)
            {
                ShipState.DamageControl.RepairPointsPool -= 4;
            }
            else if (component is EnergyComponent energyComponent)
            {
                ShipState.DamageControl.RepairPointsPool -= 3;
            }
            else if (component is SystemComponent controlSystemComponent)
            {
                ShipState.DamageControl.RepairPointsPool -= 2;
            }
            else
                ShipState.DamageControl.RepairPointsPool -= 1;

        }


        public DirectFireProjectile ExpendDirectFireProjectile(string ssdCode)
        {
            var photonTorp = ShipState.AvailablePhotonTorpedoes.SingleOrDefault(x => x.SsdCode == ssdCode);
            if (photonTorp != null)
            {
                var projectile = new PhotonTorpedoProjectile();
                projectile.OverloadState = photonTorp.OverloadState;

                photonTorp.LoadingState = PhotonTorpedoLoadingState.Unloaded;
                photonTorp.OverloadState = PhotonTorpedoOverloadState.NotOverloaded;
            }

            var phaser = ShipState.AvailablePhasers.SingleOrDefault(x => x.SsdCode == ssdCode);
            phaser.FiringState = PhaserFiringState.Expended;
            ExpendEnergyAdHoc(1);

            if (phaser.PhaserClass == 1)
                return new Phaser1Projectile();

            if (phaser.PhaserClass == 2)
                return new Phaser2Projectile();

            if (phaser.PhaserClass == 3)
                return new Phaser3Projectile();

            throw new ArgumentException();
        }

        public void AdvanceOverloadTrack(string ssdCode)
        {
            var torp = this.ShipState.AvailablePhotonTorpedoes.Single(x => x.SsdCode == ssdCode);
            if (torp.LoadingState == PhotonTorpedoLoadingState.Unloaded)
                throw new ArgumentException("Ssd Code " + ssdCode + " is unloaded.");

            if (torp.OverloadState == PhotonTorpedoOverloadState.NotOverloaded)
            {
                ExpendEnergyAdHoc(2);
                torp.OverloadState = PhotonTorpedoOverloadState.Overloaded4;
            }
            if (torp.OverloadState == PhotonTorpedoOverloadState.Overloaded4)
            {
                ExpendEnergyAdHoc(2);
                torp.OverloadState = PhotonTorpedoOverloadState.Overloaded8;
            }
            if (torp.OverloadState == PhotonTorpedoOverloadState.Overloaded8)
                throw new ArgumentException("Ssd Code " + ssdCode + " is at max overload.");
        }

        public void ExpendEnergyAdHoc(int energyRequired)
        {
            var capturedEnergy = 0m;
            if (UndeclaredEnginePool >= 0)
                capturedEnergy = Math.Min(UndeclaredEnginePool, energyRequired);

            //cut into the batteries to make up the difference
            foreach (var shipStateAvailableBattery in ShipState.AvailableBatteries)
            {
                if (energyRequired < capturedEnergy && shipStateAvailableBattery.IsCharged)
                {
                    capturedEnergy += 1;
                    shipStateAvailableBattery.IsCharged = false;
                }
            }
        }

        public void ExecuteDeclaredEnergyAllocation(DeclaredEnergyAllocation allocation)
        {
            UndeclaredEnginePool = ShipState.AvailableEnergyOutput;
            
            CurrentSpeed = allocation.RoutedToEngines * 2;
            ExpendEnergyAdHoc(allocation.RoutedToEngines);

            foreach (var photonTorpedo in ShipState.AvailablePhotonTorpedoes)
            {
                if (photonTorpedo.Damaged)
                    continue;

                var routed = allocation.PhotonTorpedoHolding[photonTorpedo.SsdCode];
                
                if (routed == 0)
                    photonTorpedo.LoadingState = PhotonTorpedoLoadingState.Unloaded;
                else
                {
                    ExpendEnergyAdHoc(routed);

                    switch (photonTorpedo.LoadingState)
                    {
                        case PhotonTorpedoLoadingState.Preloaded:
                            photonTorpedo.LoadingState = PhotonTorpedoLoadingState.Loaded;
                            break;
                        case PhotonTorpedoLoadingState.Loaded:
                            photonTorpedo.LoadingState = PhotonTorpedoLoadingState.Held;
                            break;
                        case PhotonTorpedoLoadingState.Unloaded:
                            photonTorpedo.LoadingState = PhotonTorpedoLoadingState.Preloaded;
                            break;
                    }
                }
            }

            foreach (var shieldAllocation in allocation.ShieldReinforcement.ShieldsToReinforce)
            {
                var shieldsToRestore = shieldAllocation.Value / 2;
                ExpendEnergyAdHoc(shieldAllocation.Value);
                var shield = this.ShipState.GetShield(shieldAllocation.Key);
                shield.Remaining = Math.Min(shield.Remaining + shieldsToRestore, shield.Capacity);
            }


            
        }
    }

    public class DeclaredEnergyAllocation
    {
        public int RoutedToEngines { get; set; }
        
        public Dictionary<string,int> PhotonTorpedoHolding { get; set; } = new Dictionary<string, int>();

        public ShieldReinforcement ShieldReinforcement { get; set; }= new ShieldReinforcement();
    }

    public class ShieldReinforcement
    {
        public Dictionary<char, int> ShieldsToReinforce { get; set; } = new Dictionary<char, int>();
    }

    public class DamageControl
    {
        public int RepairPointsProduction { get; set; }
        public int RepairPointsPool { get; set; }
    }

    public class ShipState
    {
        private readonly ShipSpecification _spec;
        private readonly List<ShipComponent> _allComponents = new List<ShipComponent>();
        private readonly Dictionary<char, Shield> _shields = new Dictionary<char, Shield>();
        public ShipState(ShipSpecification spec)
        {
            _spec = spec;

            int componentId = 100;

            for (int i = 0; i < _spec.Loadout.EnergyLoadout.LeftWarp; i++)
            {
                _allComponents.Add(new WarpEngineComponent
                {
                    Position = WarpEnginePosition.Left,
                    ComponentId = componentId++
                });
            }

            for (int i = 0; i < _spec.Loadout.EnergyLoadout.RightWarp; i++)
            {
                _allComponents.Add(new WarpEngineComponent
                {
                    Position = WarpEnginePosition.Right,
                    ComponentId = componentId++
                });
            }

            for (int i = 0; i < _spec.Loadout.EnergyLoadout.BatteryCount; i++)
            {
                _allComponents.Add(new BatteryComponent
                {
                    ComponentId = componentId++
                });
            }


            foreach (var weaponComplement in _spec.Loadout.Weapons)
            {
                if (weaponComplement.WeaponType == WeaponType.PHOT)
                    _allComponents.Add(new PhotonTorpedo
                    {
                        SsdCode = weaponComplement.SsdCode,
                        ComponentId = componentId++,
                        LoadingState = PhotonTorpedoLoadingState.Unloaded
                    });

                if (weaponComplement.WeaponType == WeaponType.PH1)
                    _allComponents.Add(new Phaser
                    {
                        SsdCode = weaponComplement.SsdCode,
                        ComponentId = componentId++,
                        PhaserClass = 1
                    });
            }

            _shields[Facing.A] = new Shield(spec.ShieldStrength[Facing.A]);
            _shields[Facing.B] = new Shield(spec.ShieldStrength[Facing.B]);
            _shields[Facing.C] = new Shield(spec.ShieldStrength[Facing.C]);
            _shields[Facing.D] = new Shield(spec.ShieldStrength[Facing.D]);
            _shields[Facing.E] = new Shield(spec.ShieldStrength[Facing.E]);
            _shields[Facing.F] = new Shield(spec.ShieldStrength[Facing.F]);

            DamageControl = new DamageControl
            {
                RepairPointsPool = 0,
                RepairPointsProduction = _spec.DamageControlRating
            };
        }

        private IEnumerable<ShipComponent> UndamagedComponents => _allComponents.Where(x => !x.Damaged);

        public int AvailableEnergyStorage => UndamagedComponents.OfType<BatteryComponent>().Count();
        public int AvailableEnergyOutput => UndamagedComponents.OfType<IProducesEnergy>().Count();
        public IReadOnlyCollection<PhotonTorpedo> AvailablePhotonTorpedoes => UndamagedComponents.OfType<PhotonTorpedo>().ToList();
        public IReadOnlyCollection<BatteryComponent> AvailableBatteries => UndamagedComponents.OfType<BatteryComponent>().ToList();
        public IReadOnlyCollection<Phaser> AvailablePhasers => UndamagedComponents.OfType<Phaser>().ToList();

        public Shield GetShield(char facing)
        {
            return _shields[facing];
        }

        public ShipComponent GetComponent(int id)
        {
            return _allComponents.Single(x => x.ComponentId == id);
        }



        public DamageControl DamageControl { get; set; }
    }

    public interface IProducesEnergy
    {

    }
    public class SystemComponent : ShipComponent
    {

    }
    public class EnergyComponent : ShipComponent
    {

    }
    public class ReactorComponent : EnergyComponent, IProducesEnergy
    {

    }

    public class ImpulseEngineComponent : EnergyComponent, IProducesEnergy
    {

    }

    public class WarpEngineComponent : EnergyComponent, IProducesEnergy
    {
        public WarpEnginePosition Position { get; set; }
    }



    public enum WarpEnginePosition
    {
        Unknown,Left,Right
    }

    public class BatteryComponent : EnergyComponent
    {
        public bool IsCharged { get; set; }
    }
    public class WeaponComponent : ShipComponent
    {
        public string SsdCode { get; set; }


    }

    //(4C5a) Pre-Load: Carried out during Energy Allocation, costs two energy tokens per photon, does
    //not result in a photon able to be fired.
    //
    //(4C5b) Loading: Carried out during Energy Allocation, and can only be done on the turn after PreLoading.
    //If not done, the pre-load energy is lost.This
    //costs two energy tokens per photon and results in a
    //torpedo which can be fired during any impulse of the
    //Loading Turn.
    //
    //(4C5c) Holding: If the torpedo was not fired on
    //the loading turn, the player must pay one Energy Token per photon to hold them during the next turn, during which it could be fired on any impulse.An armed
    //photon can be held for any number of turns if energy
    //is paid to hold it.If the holding energy is not paid, the
    //torpedo is ejected into space and lost.
    public class PhotonTorpedo : WeaponComponent
    {
        public PhotonTorpedoLoadingState LoadingState { get; set; } = PhotonTorpedoLoadingState.Unloaded;

        public PhotonTorpedoOverloadState OverloadState { get; set; } = PhotonTorpedoOverloadState.NotOverloaded;

    }
    
    public class Phaser : WeaponComponent
    {
        public PhaserFiringState FiringState { get; set; } = PhaserFiringState.Ready;
        public int PhaserClass { get; set; }
    }

    public enum PhaserFiringState
    {
        Ready, Expended
    }

    public enum PhotonTorpedoOverloadState
    {
        NotOverloaded,Overloaded4,Overloaded8
    }

    public enum PhotonTorpedoLoadingState
    {
        Unloaded, Preloaded, Loaded, Held
    }

    public class Shield
    {
        public int Capacity { get; set; }
        public int Remaining { get; set; }
        public Shield(int strength)
        {
            Remaining = strength;
            Capacity = strength;
        }

        public Shield() { }
    }

}