using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetCommander.Simulation.Simulation.Ships
{
    public class ShipComponent
    {
        public bool Damaged { get; set; }
        public int ComponentId { get; set; }
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

    public class LabComponent : ShipComponent { }

    public class ShuttleComponent : ShipComponent { }

    public class BridgeComponent : ShipComponent { }

    public class AuxComponent : ShipComponent { }
    public class FlagBridgeComponent : ShipComponent { }
    public class EmergencyBridgeComponent : ShipComponent { }

    public class HullComponent :ShipComponent
    {
        public HullPosition Position { get; set; }
    }

    public class TransComponent : ShipComponent { }

    public class TractorComponent : ShipComponent { }

    public class FrameComponent : ShipComponent { }

    public abstract class DroneBasedComponent : WeaponComponent { }

    public class DoneLauncherComponent : DroneBasedComponent { }
    public class AntiDroneLauncherComponent : DroneBasedComponent { }

    public enum HullPosition
    {
        F,R
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
        Unknown, Center, Right,
        Left
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
    public class PhotonTorpedoComponent : WeaponComponent
    {
        public PhotonTorpedoLoadingState LoadingState { get; set; } = PhotonTorpedoLoadingState.Unloaded;

        public PhotonTorpedoOverloadState OverloadState { get; set; } = PhotonTorpedoOverloadState.NotOverloaded;

    }

    public class PhaserComponent : WeaponComponent
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
        NotOverloaded, Overloaded4, Overloaded8
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
