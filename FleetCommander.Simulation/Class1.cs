using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FleetCommander.Simulation.Framework.GridSystem;

namespace FleetCommander.Simulation
{
    public interface IToken
    {
        Position Position { get; set; }
    }

    public class ShipComponent
    {
        public bool Damaged { get; set; }
        public int ComponentId { get; set; }
    }

    public class MovementMarker : IToken
    {
        public int ForShipId { get; set; }
        public int Remaining { get; set; }
        public Position Position { get; set; }
        public bool IsExpired => Remaining == 0;
        public void Decrement()
        {
            Remaining--;
        }
    }

    public class SideSlipMarker : MovementMarker
    {

    }
    public class TurnMarker : MovementMarker
    {

    }
    /*
        Starships move by expending power and will always have a Baseline Speed. Starships can vary their
        speed by expending different amounts of power each
        turn. The cruisers in First missions can spend either
        0, 4, 8, or 12 Energy Tokens during Energy Allocation
        to produce a baseline speed of 0, 8, 16, or 24 respectively. Ships can then expend additional energy (half
        a point) during each Impulse to increase their speed
        by one hex for that Impulse only (2B2). Ships which
        have taken combat damage may not be able to move
        at their full speed in later turns. See (2C7) for
        “stopped”.
     */

    public struct OffensiveFireDeclaration
    {
        public List<Volley> Volleys { get; set; }
    }

    public struct Volley
    {
        public List<string> SsdCode { get; set; }
        public int ShipFrom { get; set; }
        public int ShipTo { get; set; }
        public SystemTargeting Targeting { get; set; }
    }

    public enum SystemTargeting
    {
        Indiscriminant, Power, Weapons
    }

    public struct DeclaredSpeedChange {
        public SpeedChange? SpeedChange { get; set; }
    }
    public struct DeclaredNavigation
    {
        public int ShipId { get; set; }
        public int? SideSlipDirection { get; set; }
        public int? NewFacing { get; set; }
    } 

    public enum SpeedChange { Unknown, Accelerate, Decelerate }

    public struct Position
    {
        public Hex Hex { get; set; }
        public int Facing { get; private set; }
        
        public void SetFacing(int facing)
        {
            this.Facing = facing;
        }

        public void SetPosition(Hex coordinate)
        {
            this.Hex = coordinate;
        }
    }

    public class SimulationTurn
    {
        public int TurnNumber { get; set; }

        public Dictionary<int,DeclaredEnergyAllocation> EnergyAllocations { get; set; } = new Dictionary<int, DeclaredEnergyAllocation>();


    }

    public class Impulse
    {

    }

    public class SubPulse
    {

    }

    public static class SimulationSeed
    {
        public static int Generate()
        {
            return Guid.NewGuid().GetHashCode();
        }
    }

    /*
        Each unit will move one hex, and only one hex,
        during each Movement Sub-Pulse in which movement
        is called for by the IMPULSE CHART below:
        Baseline Pulse  Moved
        Speed    Sp   1 2 3 4
        0           0 — — — —
        8           1 — — — X
        16          2 — X — X
        24          3 — X X X
        —           4 X X X X
        If a player allocates energy for 16 hexes of movement, his ship will move one hex in each of 16 Movement Sub-Pulses(two in each Impulse, as per the
        Chart above, #2 and #4) during that turn. The movement cost of all ships is listed on their Ship Diagram.
        If no units are scheduled to move, that Movement
        Sub-Pulse can be skipped. (In most Impulses on the
        Klingon Border, nobody will move in the first Sub-Pulse
        and it can simply be ignored.)
    */
    public class Simulation
    {
        public int Seed { get; }
        public Dice Dice { get; }
        public SimulationTimeStamp SimulationTimeStamp { get; set; }

        public List<Ship> AllShips { get; set; } = new List<Ship>();
        public List<MovementMarker> MovementMarkers { get; set; } = new List<MovementMarker>();

        public Simulation(int seed)
        {
            this.Seed = seed;
            this.Dice = new Dice(seed);
        }
        
        //public void AdvanceBySubPulse()
        //{
        //    /*(2B2a) Acceleration: Ships can increase their
        //    speed during any impulse by paying extra Energy
        //    Tokens.At the start of every Impulse, each ship has
        //    the option to pay Energy Tokens equal to one movement point to increase its speed for that one Impulse
        //    by one movement point.It could do this during any or
        //    all impulses.*/
        //    AllShips.ForEach(ExecutePlottedAction);

        //    if (SimulationTimeStamp.IsTurnEnd)
        //        ExecuteEndOfTurn();
            
        //    SimulationTimeStamp.AdvanceBySubPulse();
        //    if (SimulationTimeStamp.IsTurnEnd)
        //        AllShips.ForEach(x => x.ResetEnergyAllocation());
            
        //}

        /*       
            (1E3) END OF TURN PROCEDURE


            (1E3b) Weapons Records: Erase any marked
            letters on the “weapons used” track so those weapons can be used again on the next turn. This procedure is used because each weapon can only be used
            once per turn.

            (1E3c) Marine Combat Phase: See Federation
            Commander: Klingon Border.

            (1E3d) Repair Phase: Determine the number of
            available repair points, and use them to repair damaged systems as per the rules (5G2). You may also
            transfer five boxes (3C3) from any one shield to any
            adjacent shield (but this can only replace disabled
            boxes, not increase the original strength of the shield).

            (1E3e) Undocking: See Federation Commander: Klingon Border.
        */
        private void ExecuteEndOfTurn()
        {

            AllShips.ForEach(x => x.RolloverExcessEnergyIntoBatteries());
            //AllShips.ForEach(x => x.ResetWeaponsUsed());
        }

        private void ExecuteSpeedChange(Ship ship, DeclaredSpeedChange declaredSpeedChange)
        {
            if (declaredSpeedChange.SpeedChange.HasValue)
            {

            }
        }

        private void ExecutePlottedNavigation(DeclaredNavigation declaredNavigation)
        {
            var ship = AllShips.Single(x => x.ShipId == declaredNavigation.ShipId);
            if (declaredNavigation.NewFacing.HasValue)
            {
                 ship.Position.SetFacing(declaredNavigation.NewFacing.Value);
                 MovementMarkers.Add(new TurnMarker
                 {
                     ForShipId = ship.ShipId,
                     Remaining = 4,
                     Position = ship.Position
                 });
            }

            if (declaredNavigation.SideSlipDirection.HasValue)
            {
                ExecuteSideSlip(ship,declaredNavigation.SideSlipDirection.Value);
            }
            else
            {
                ExecuteStandardMovement(ship);
            }

            SimulationTimeStamp.AdvanceSimulationTime();
        }

        public IReadOnlyCollection<DirectFireProjectile> ExecuteFireDeclarations(IReadOnlyCollection<OffensiveFireDeclaration> fireDeclarations)
        {
            List<DirectFireProjectile> projectiles = new List<DirectFireProjectile>();
            foreach (var volley in fireDeclarations.SelectMany(x => x.Volleys))
            {
                var shipFrom = this.AllShips.Single(x => x.ShipId == volley.ShipFrom);
                foreach (var ssdCode in volley.SsdCode)
                {
                    var projectile = shipFrom.ExpendDirectFireProjectile(ssdCode);
                    projectile.Target = volley.ShipTo;
                    var targetPosition = this.AllShips.Single(x => x.ShipId == projectile.Target).Position;
                    projectile.Distance = shipFrom.Position.Hex.Distance(targetPosition.Hex);
                    projectile.HitTrack = this.Dice.RollD6();
                    var damage = projectile.CalculateDamage();


                    projectiles.Add(projectile);
                }
            }

            return projectiles;
        }

        public void ApplyDirectFireProjectiles(IReadOnlyCollection<DirectFireProjectile> projectiles)
        {
            foreach (var projectile in projectiles)
            {
                if (projectile is PhotonTorpedoProjectile photonTorpedoProjectile)
                {
                    if (photonTorpedoProjectile.OverloadState == PhotonTorpedoOverloadState.Overloaded4 || photonTorpedoProjectile.OverloadState == PhotonTorpedoOverloadState.Overloaded8)
                    {
                    }
                }
            }
        }

        //(1E1) ENERGY ALLOCATION
        //See the rules on this subject (1D). In summary,
        //count the amount of energy your ship has, and obtain
        //energy tokens for each point. (During the first turn of
        //a scenario, the ship has additional energy tokens
        //equal to the number of batteries on the ship, representing power stored in the batteries.)
        //Pick and pay for your baseline speed (2B1b) secretly and simultaneously with other players.
        //Pay for any weapon pre-loading, such as Photon
        //Torpedoes (4C2).
        //Pay for any Shield Regeneration (3C7) at the rate
        //of two energy tokens for each shield box repaired
        private void ExecuteEnergyAllocation(Ship ship, DeclaredEnergyAllocation allocation)
        {
            ship.ExecuteDeclaredEnergyAllocation(allocation);

        }

        private void ExecuteStandardMovement(Ship ship)
        {

            var newCoodinate = ship.Position.Hex.Neighbor(ship.Position.Facing);
            ship.Position.SetPosition(newCoodinate);

            var turnMarkersToDecrement = MovementMarkers.Where(x => x.ForShipId == ship.ShipId).ToList();
            foreach (var turnMarker in turnMarkersToDecrement)
                turnMarker.Decrement();
        }

        private void ExecuteSideSlip(Ship ship, int direction)
        {
            MovementMarkers.Add(new SideSlipMarker
            {
                Position = ship.Position,
                Remaining = 1,
                ForShipId = ship.ShipId
            });

            var newCoodinate = ship.Position.Hex.Neighbor(direction);
            ship.Position.SetPosition(newCoodinate);
        }
    }



    public struct ShipId
    {

    }





    public struct SimulationTimeStamp
    {
        public string Id => $"{TurnNumber}-{TurnStep}-{ImpulseStep}-{CurrentImpulse}-{CurrentSubPulse}";
        public int TurnNumber { get; set; }
        public TurnStep TurnStep { get; set; }
        public ImpulseStep? ImpulseStep { get; set; }
        public int? CurrentImpulse { get; set; }
        public int? CurrentSubPulse { get; set; }

        public void AdvanceSimulationTime()
        {
            if (TurnStep == TurnStep.EnergyAllocation)
            {
                CurrentImpulse = 0;
                CurrentSubPulse = 0;
                TurnStep = TurnStep.ImpulseProcess;
                ImpulseStep = FleetCommander.Simulation.ImpulseStep.SpeedChange;
                return;
            }

            if (TurnStep == TurnStep.ImpulseProcess)
            {
                if (ImpulseStep == FleetCommander.Simulation.ImpulseStep.SpeedChange)
                {
                    ImpulseStep = FleetCommander.Simulation.ImpulseStep.Movement;
                    return;
                }

                if (ImpulseStep == FleetCommander.Simulation.ImpulseStep.Movement)
                {
                    CurrentSubPulse++;

                    if (CurrentSubPulse == 4)
                    {
                        CurrentSubPulse = 0;
                        ImpulseStep = FleetCommander.Simulation.ImpulseStep.OffensiveFire;
                    }
                }

                if (ImpulseStep == FleetCommander.Simulation.ImpulseStep.OffensiveFire)
                {
                    CurrentImpulse++;
                    if (CurrentImpulse == 8)
                    {
                        CurrentImpulse = 0;
                        TurnStep = TurnStep.RepairPhase;
                    }
                    else
                    {
                        ImpulseStep = FleetCommander.Simulation.ImpulseStep.SpeedChange;
                    }
                    return;
                }
            }

            if (TurnStep == TurnStep.RepairPhase)
            {
                TurnNumber++;
                TurnStep = TurnStep.EnergyAllocation;
            }
        }


    }

    public enum TurnStep
    {
        EnergyAllocation, ImpulseProcess, RepairPhase
    }

    public enum ImpulseStep
    {
        SpeedChange, Movement, OffensiveFire
    }
}
