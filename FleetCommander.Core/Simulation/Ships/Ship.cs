using System;
using System.Collections.Generic;
using System.Linq;
using FleetCommander.Simulation.Framework.GridSystem;
using FleetCommander.Simulation.Simulation.Projectiles;

namespace FleetCommander.Simulation.Simulation.Ships
{
    public class UnableToFireException :Exception
    {
    }

    public class ApplyDamageResult
    {
        public int ShieldDamageCount { get; set; }
        public int DamageToMoveToNextbatch { get; set; }
        public int PenetratedDamage { get; set; }
    }

    public enum DamageType
    {
        WeaponFire, BurnThrough
    }


    public class Ship
    {

        public int ShipId { get; set; }
        public int CurrentSpeed { get; set; }
        public decimal UndeclaredEnginePool { get; set; }
        public ShipInternals ShipInternals { get; set; }
        public Position Position { get; set; }

        /* (During the first turn of a scenario, the ship has additional energy tokens
         *  equal to the number of batteries on the ship, representing power stored in the batteries. )
         */
        public void Initialize(ShipSpecification spec)
        {
            ShipInternals = new ShipInternals(spec);

            foreach (var shipStateAvailableBattery in ShipInternals.AvailableBatteries)
                shipStateAvailableBattery.IsCharged = true;
            
        }

        /*
         *  (1E3a) Power Phase: At the end of the turn, ships
         *       may transfer any unexpended power to their batteries (up to the limits of the battery capacity); 
         *       any excess unused power is lost.
         */
        internal void RolloverExcessEnergyIntoBatteries()
        {
            foreach (var battery in ShipInternals.AvailableBatteries)
                if (!battery.IsCharged && UndeclaredEnginePool >= 1)
                {
                    battery.IsCharged = true;
                    UndeclaredEnginePool -=1;
                }
            

        }

        public void EnactRepairs(int componentId)
        {
            var component = ShipInternals.GetComponent(componentId);
            component.Damaged = false;
            if (component is WeaponComponent weaponComponent)
            {
                ShipInternals.DamageControl.RepairPointsPool -= 4;
            }
            else if (component is EnergyComponent energyComponent)
            {
                ShipInternals.DamageControl.RepairPointsPool -= 3;
            }
            else if (component is SystemComponent controlSystemComponent)
            {
                ShipInternals.DamageControl.RepairPointsPool -= 2;
            }
            else
                ShipInternals.DamageControl.RepairPointsPool -= 1;

        }


        public DirectFireProjectile ExpendDirectFireProjectile(string ssdCode)
        {
            var photonTorp = ShipInternals.AvailablePhotonTorpedoes.SingleOrDefault(x => x.SsdCode == ssdCode);
            if (photonTorp != null)
            {
                if (photonTorp.LoadingState == PhotonTorpedoLoadingState.Unloaded)
                    throw new UnableToFireException();

                var projectile = new PhotonTorpedoProjectile();
                projectile.OverloadState = photonTorp.OverloadState;

                photonTorp.LoadingState = PhotonTorpedoLoadingState.Unloaded;
                photonTorp.OverloadState = PhotonTorpedoOverloadState.NotOverloaded;
            }

            var phaser = ShipInternals.AvailablePhasers.SingleOrDefault(x => x.SsdCode == ssdCode);
            if (phaser == null)
                throw new Exception("Invalid ssd code "+ssdCode);

            if (phaser.ChargingState == PhaserFiringState.Expended)
                throw new UnableToFireException();

            phaser.ChargingState = PhaserFiringState.Expended;
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
            var torp = this.ShipInternals.AvailablePhotonTorpedoes.Single(x => x.SsdCode == ssdCode);
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

        

        internal ApplyDamageResult ApplyDamage(int dmg, Hex damageDirection, DamageAllocationTrack track, DamageType damageType, SystemTargeting targeting)
        {
            var result = new ApplyDamageResult();

            var remainingDamage = dmg;
            var shield = this.ShipInternals.GetShield(damageDirection);
            if (damageType == DamageType.WeaponFire)
            {
                var absorbed = Math.Min(shield.Remaining, remainingDamage);
                shield.Remaining -= absorbed;
                remainingDamage -= absorbed;
                result.ShieldDamageCount = absorbed;
            }

            //give back the damage to the shield, because the damage actually passed 'through' the shield.
            if (damageType == DamageType.BurnThrough)
                shield.Remaining+=dmg;
            
            
            result.PenetratedDamage = remainingDamage;

            for (int i = 0; i < 10; i++)
            {
                if (remainingDamage == 0)
                    break;
               
                var target = track.Get(i);
                var success = ShipInternals.TryApplyDamageToInternalComponent(target.DamageType);
                if (success)
                {
                    remainingDamage--;
                    continue;
                }

                success = ShipInternals.TryApplyDamageToInternalComponent(target.AltDamageType);
                if (success)
                {
                    remainingDamage--;
                    continue;
                }

                //can't move damage to next batch if you were targeting a specific system.
                if (targeting != SystemTargeting.Indiscriminant)
                    remainingDamage--;
            }

            result.DamageToMoveToNextbatch = remainingDamage;

            return result;
        }

        
        public void ExpendEnergyAdHoc(int energyRequired)
        {
            var capturedEnergy = 0m;
            if (UndeclaredEnginePool >= 0)
                capturedEnergy = Math.Min(UndeclaredEnginePool, energyRequired);

            //cut into the batteries to make up the difference
            foreach (var shipStateAvailableBattery in ShipInternals.AvailableBatteries)
            {
                if (energyRequired < capturedEnergy && shipStateAvailableBattery.IsCharged)
                {
                    capturedEnergy += 1;
                    shipStateAvailableBattery.IsCharged = false;
                }
            }
        }


        public EnergyAllocationDeclaration CreateDefaultEnergyAllocation()
        {
            
            return new EnergyAllocationDeclaration
            {
                RoutedToEngines = this.CurrentSpeed / 2
            };
        }

        public void ExecuteDeclaredEnergyAllocation(EnergyAllocationDeclaration allocationDeclaration)
        {
            UndeclaredEnginePool = ShipInternals.AvailableEnergyOutput;
            
            CurrentSpeed = allocationDeclaration.RoutedToEngines * 2;
            ExpendEnergyAdHoc(allocationDeclaration.RoutedToEngines);

            foreach (var photonTorpedo in ShipInternals.AvailablePhotonTorpedoes)
            {
                if (photonTorpedo.Damaged)
                    continue;

                var routed = allocationDeclaration.PhotonTorpedos[photonTorpedo.SsdCode];
                
                if (routed == PhotonTorpedoEnergyRouting.None)
                    photonTorpedo.LoadingState = PhotonTorpedoLoadingState.Unloaded;
                else
                {
                    switch (photonTorpedo.LoadingState)
                    {
                        case PhotonTorpedoLoadingState.Preloaded:
                            ExpendEnergyAdHoc(2);
                            photonTorpedo.LoadingState = PhotonTorpedoLoadingState.Loaded;
                            break;
                        case PhotonTorpedoLoadingState.Loaded:
                            ExpendEnergyAdHoc(2);
                            photonTorpedo.LoadingState = PhotonTorpedoLoadingState.Held;
                            break;
                        case PhotonTorpedoLoadingState.Unloaded:
                            ExpendEnergyAdHoc(2);
                            photonTorpedo.LoadingState = PhotonTorpedoLoadingState.Preloaded;
                            break;
                    }
                }
            }

            foreach (var shieldAllocation in allocationDeclaration.ShieldReinforcement.ShieldsToReinforce)
            {
                var shieldsToRestore = shieldAllocation.Value / 2;
                ExpendEnergyAdHoc(shieldAllocation.Value);
                var shield = this.ShipInternals.GetShield(Hex.Direction(shieldAllocation.Key));
                shield.Remaining = Math.Min(shield.Remaining + shieldsToRestore, shield.Capacity);
            }
        }

        internal void RechargeWeapons()
        {
            foreach (var shipInternalsAvailablePhaser in this.ShipInternals.AvailablePhasers)
                shipInternalsAvailablePhaser.ChargingState = PhaserFiringState.Charged;
        }
    }

    public class PhotonTorpedoRouting
    {
        public Dictionary<string, int> PhotonTorpedoPreLoading { get; set; } = new Dictionary<string, int>();
    }

    public enum PhotonTorpedoEnergyRouting
    {
        None, Preload, Load, Hold
    }

    public class EnergyAllocationDeclaration
    {
        public int RoutedToEngines { get; set; }

        public Dictionary<string, PhotonTorpedoEnergyRouting> PhotonTorpedos { get; set; } = new Dictionary<string, PhotonTorpedoEnergyRouting>();


        public ShieldReinforcement ShieldReinforcement { get; set; }= new ShieldReinforcement();
    }

    public class ShieldReinforcement
    {
        public Dictionary<int, int> ShieldsToReinforce { get; set; } = new Dictionary<int, int>();
    }

    public class DamageControl
    {
        public int RepairPointsProduction { get; set; }
        public int RepairPointsPool { get; set; }
    }


    public class ShipInternals
    {
        private readonly ShipSpecification _spec;
        private readonly List<ShipComponent> _allComponents = new List<ShipComponent>();
        private readonly Dictionary<Hex, Shield> _shields = new Dictionary<Hex, Shield>();
        public ShipInternals(ShipSpecification spec)
        {
            _spec = spec;

           
            InitializeEnergyLoadout(spec.Loadout.EnergyLoadout);
            InitializeStructureLoadout(spec.Loadout.StructureLoadout);
            InitializeSystemsLoadout(spec.Loadout.SystemsLoadout);
            InitializeWeaponsLoadout(spec.Loadout.Weapons);
            
            _shields[Hex.Direction(0)] = new Shield(spec.ShieldStrength[Hex.Direction(0)]);
            _shields[Hex.Direction(1)] = new Shield(spec.ShieldStrength[Hex.Direction(1)]);
            _shields[Hex.Direction(2)] = new Shield(spec.ShieldStrength[Hex.Direction(2)]);
            _shields[Hex.Direction(3)] = new Shield(spec.ShieldStrength[Hex.Direction(3)]);
            _shields[Hex.Direction(4)] = new Shield(spec.ShieldStrength[Hex.Direction(4)]);
            _shields[Hex.Direction(5)] = new Shield(spec.ShieldStrength[Hex.Direction(5)]);

            DamageControl = new DamageControl
            {
                RepairPointsPool = 0,
                RepairPointsProduction = _spec.DamageControlRating
            };
        }

        private int _componentId = 0;

        private void InitializeWeaponsLoadout(IReadOnlyCollection<WeaponHardmount> weapons)
        {
            
            foreach (var weaponComplement in weapons)
            {
                if (weaponComplement.WeaponType == WeaponType.PHOT)
                    InitializeComponents<PhotonTorpedoComponent>(1, x =>
                        {
                            x.LoadingState = PhotonTorpedoLoadingState.Unloaded;
                            x.SsdCode = weaponComplement.SsdCode;
                        });

                if (weaponComplement.WeaponType == WeaponType.PH1)
                    InitializeComponents<PhaserComponent>(1, x =>
                    {
                        x.PhaserClass = 1;
                        x.SsdCode = weaponComplement.SsdCode;
                    });

                if (weaponComplement.WeaponType == WeaponType.PH3)
                    InitializeComponents<PhaserComponent>(1, x =>
                    {
                        x.PhaserClass = 3;
                        x.SsdCode = weaponComplement.SsdCode;
                    });
            }

        }

        private void InitializeComponents<T>(int count, Action<T> prepAction=null) where T:ShipComponent, new()
        {
            for (int i = 0; i < count; i++)
            {
                var shipComponent = new T {ComponentId = _componentId++};
                prepAction?.Invoke(shipComponent);
                _allComponents.Add(shipComponent);
            }
        }

        private void InitializeEnergyLoadout(EnergyLoadout loadout)
        {
            InitializeComponents<WarpEngineComponent>(loadout.LeftWarp,x => x.Position = WarpEnginePosition.Center);
            InitializeComponents<WarpEngineComponent>(loadout.RightWarp, x => x.Position = WarpEnginePosition.Right);
            InitializeComponents<BatteryComponent>(loadout.BatteryCount);
            InitializeComponents<ImpulseEngineComponent>(loadout.Impulse);
            InitializeComponents<ReactorComponent>(loadout.Reactor);
        }

        private void InitializeStructureLoadout(StructureLoadout loadout)
        {
            InitializeComponents<FrameComponent>(loadout.Frame);
            InitializeComponents<HullComponent>(loadout.ForwardHull,x => x.Position = HullPosition.F);
            InitializeComponents<HullComponent>(loadout.RearHull, x => x.Position = HullPosition.R);
        }

        private void InitializeSystemsLoadout(ControlSystemsLoadout loadout)
        {
            InitializeComponents<LabComponent>(loadout.Lab);
            InitializeComponents<TransComponent>(loadout.Trans);
        }

        private IEnumerable<ShipComponent> UndamagedComponents => _allComponents.Where(x => !x.Damaged);

        public int AvailableEnergyStorage => UndamagedComponents.OfType<BatteryComponent>().Count();
        public int AvailableEnergyOutput => UndamagedComponents.OfType<IProducesEnergy>().Count();
        public IReadOnlyCollection<PhotonTorpedoComponent> AvailablePhotonTorpedoes => UndamagedComponents.OfType<PhotonTorpedoComponent>().ToList();
        public IReadOnlyCollection<BatteryComponent> AvailableBatteries => UndamagedComponents.OfType<BatteryComponent>().ToList();
        public IReadOnlyCollection<PhaserComponent> AvailablePhasers => UndamagedComponents.OfType<PhaserComponent>().ToList();
        public bool IsFrameIntact => UndamagedComponents.OfType<FrameComponent>().Any();
        public Shield GetShield(Hex facing)
        {
            return _shields[facing];
        }

        public ShipComponent GetComponent(int id)
        {
            return _allComponents.Single(x => x.ComponentId == id);
        }

        internal bool TryApplyDamageToInternalComponent(DamageAllocationType damageType)
        {
            ShipComponent shipComponent;
            switch (damageType)
            {
                case DamageAllocationType.CWarp:
                    shipComponent = this.UndamagedComponents.OfType<WarpEngineComponent>().FirstOrDefault(x => x.Position == WarpEnginePosition.Center);
                    break;
                case DamageAllocationType.AnyWarp:
                    shipComponent = this.UndamagedComponents.OfType<WarpEngineComponent>().FirstOrDefault();
                    break;
                case DamageAllocationType.RWarp:
                    shipComponent=this.UndamagedComponents.OfType<WarpEngineComponent>().FirstOrDefault(x => x.Position == WarpEnginePosition.Right);
                    break;
                case DamageAllocationType.LWarp:
                    shipComponent = this.UndamagedComponents.OfType<WarpEngineComponent>().FirstOrDefault(x => x.Position == WarpEnginePosition.Left);
                    break;
                case DamageAllocationType.Impulse:
                    shipComponent = this.UndamagedComponents.OfType<ImpulseEngineComponent>().FirstOrDefault();
                    break;
                case DamageAllocationType.Reactor:
                    shipComponent = this.UndamagedComponents.OfType<ReactorComponent>().FirstOrDefault();
                    break;
                case DamageAllocationType.FHull:
                    shipComponent = this.UndamagedComponents.OfType<HullComponent>().FirstOrDefault(x => x.Position == HullPosition.F);
                    break;
                case DamageAllocationType.RHull:
                    shipComponent = this.UndamagedComponents.OfType<HullComponent>().FirstOrDefault(x => x.Position == HullPosition.R);
                    break;
                case DamageAllocationType.Lab:
                    shipComponent = this.UndamagedComponents.OfType<LabComponent>().FirstOrDefault();
                    break;
                case DamageAllocationType.Tractor:
                    shipComponent = this.UndamagedComponents.OfType<TractorComponent>().FirstOrDefault();
                    break;
                case DamageAllocationType.Trans:
                    shipComponent = this.UndamagedComponents.OfType<TransComponent>().FirstOrDefault();
                    break;
                case DamageAllocationType.Battery:
                    shipComponent = this.UndamagedComponents.OfType<BatteryComponent>().FirstOrDefault();
                    break;

                case DamageAllocationType.Frame:
                    shipComponent = this.UndamagedComponents.OfType<FrameComponent>().FirstOrDefault();
                    break;
                case DamageAllocationType.Phaser:
                    shipComponent = this.UndamagedComponents.OfType<PhaserComponent>().FirstOrDefault();
                    break;
                case DamageAllocationType.Drone:
                    shipComponent = this.UndamagedComponents.OfType<DroneBasedComponent>().FirstOrDefault();
                    break;
                case DamageAllocationType.Bridge:
                    shipComponent = this.UndamagedComponents.OfType<BridgeComponent>().FirstOrDefault();
                    break;
                case DamageAllocationType.Flag:
                    shipComponent = this.UndamagedComponents.OfType<FlagBridgeComponent>().FirstOrDefault();
                    break;
                case DamageAllocationType.Shuttle:
                    shipComponent = this.UndamagedComponents.OfType<ShuttleComponent>().FirstOrDefault();
                    break;
                case DamageAllocationType.Aux:
                    shipComponent = this.UndamagedComponents.OfType<AuxComponent>().FirstOrDefault();
                    break;
                case DamageAllocationType.Emer:
                    shipComponent = this.UndamagedComponents.OfType<EmergencyBridgeComponent>().FirstOrDefault();
                    break;
                case DamageAllocationType.Torpedo:
                    shipComponent = this.UndamagedComponents.OfType<PhotonTorpedoComponent>().FirstOrDefault(); //todo: disrupters.
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(damageType), damageType, null);
            }

            if (shipComponent == null)
                return false;

            shipComponent.Damaged = true;
            return true;
        }

        public DamageControl DamageControl { get; set; }
    }




}