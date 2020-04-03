using System;
using System.Collections.Generic;
using System.Dynamic;
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






    public struct Position
    {
        public Hex Hex { get; set; }
        public int Rotation { get; set; }
        
        public void SetFacing(int facing)
        {
            this.Rotation = facing;
        }

        public void SetPosition(Hex coordinate)
        {
            this.Hex = coordinate;
        }
    }

    //    0
    // -1   1
    // -2   2
    //    3
    //
    public static class HexRotation{
        public static Hex NewFacing(Hex originalFacing,int rotation)
        {
            var sideNumber = Hex.Direction(originalFacing);

            var newFacing = sideNumber + rotation;
            if (newFacing < 0)
                newFacing += 6;
            if (newFacing > 5)
                newFacing -= 6;

            return Hex.Direction(newFacing);
        }
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




    public struct SimulationTimeStamp
    {
        public string Id => $"{TurnNumber}-{TurnStep}-{ImpulseStep}-{Impulse}-{SubPulse}";
        public int TurnNumber { get; set; }
        public TurnStep TurnStep { get; set; }
        public ImpulseStep? ImpulseStep { get; set; }
        public int? Impulse { get; set; }
        public int? SubPulse { get; set; }

        public void Increment()
        {
            if (TurnStep == TurnStep.EnergyAllocation)
            {
                Impulse = 0;
                SubPulse = 0;
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
                    SubPulse++;

                    if (SubPulse == 4)
                    {
                        SubPulse = 0;
                        ImpulseStep = FleetCommander.Simulation.ImpulseStep.OffensiveFire;
                    }
                }

                if (ImpulseStep == FleetCommander.Simulation.ImpulseStep.OffensiveFire)
                {
                    Impulse++;
                    if (Impulse == 8)
                    {
                        Impulse = 0;
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

        internal SimulationTimeStamp GetNextFiringOppporunity()
        {
           
            if (TurnStep == TurnStep.RepairPhase) //too late for this turn.
            {
                return new SimulationTimeStamp
                {
                    TurnNumber = TurnNumber+1,
                    TurnStep = TurnStep.ImpulseProcess, 
                    Impulse = 0, 
                    SubPulse = null,
                    ImpulseStep = FleetCommander.Simulation.ImpulseStep.OffensiveFire
                };
            }
            else
            {
                return new SimulationTimeStamp
                {
                    TurnNumber = TurnNumber,
                    TurnStep = TurnStep.ImpulseProcess,
                    Impulse = this.Impulse ?? 0,
                    SubPulse = null,
                    ImpulseStep = FleetCommander.Simulation.ImpulseStep.OffensiveFire
                };
            }
        }

        public bool CanScheduleEnergyAllocationOnThisTurn()
        {
            return TurnStep == TurnStep.EnergyAllocation;
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
