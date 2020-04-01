using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace FleetCommander.Simulation
{
    public class DirectFireProjectile
    {
        public int Target { get; internal set; }
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

        public void ApplyDamage()
        {

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
                photonTorp.State = PhotonTorpedoState.Unloaded;
                return new DirectFireProjectile();
            }

            var phaser = ShipState.AvailablePhasers.SingleOrDefault(x => x.SsdCode == ssdCode);
            phaser.FiringState = PhaserFiringState.Expended;
            ExpendEnergyAdHoc(1);
            return new DirectFireProjectile();
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

                var routed = allocation.RoutedToWeapons[photonTorpedo.SsdCode];
                
                if (routed == 0)
                    photonTorpedo.State = PhotonTorpedoState.Unloaded;
                else
                {
                    ExpendEnergyAdHoc(routed);

                    switch (photonTorpedo.State)
                    {
                        case PhotonTorpedoState.Preloaded:
                            photonTorpedo.State = PhotonTorpedoState.Loaded;
                            break;
                        case PhotonTorpedoState.Loaded:
                            photonTorpedo.State = PhotonTorpedoState.Held;
                            break;
                        case PhotonTorpedoState.Unloaded:
                            photonTorpedo.State = PhotonTorpedoState.Preloaded;
                            break;
                    }
                }
            }

            foreach (var shieldAllocation in allocation.RoutedToShields)
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
        
        public Dictionary<string,int> RoutedToWeapons { get; set; } = new Dictionary<string, int>();

        public Dictionary<char, int> RoutedToShields { get; set; } = new Dictionary<char, int>();
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
                        State = PhotonTorpedoState.Unloaded
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
        public PhotonTorpedoState State { get; set; } = PhotonTorpedoState.Unloaded;

    }
    
    public class Phaser : WeaponComponent
    {
        public PhaserFiringState FiringState { get; set; } = PhaserFiringState.Ready;
    }

    public enum PhaserFiringState
    {
        Ready, Expended
    }

    public enum PhotonTorpedoState
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