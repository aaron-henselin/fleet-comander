using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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

        [Pure]
        public Position WithFacing(int facing)
        {
            return new Position {Hex = Hex, Rotation = facing};
        }

        [Pure]
        public Position WithHex(Hex coordinate)
        {
            return new Position { Hex = coordinate, Rotation = Rotation };
        }
    }

    //    0
    // -1   1
    // -2   2
    //    3
    //
    //public static class HexRotation{
    //    public static Hex NewFacing(Hex originalFacing,int rotation)
    //    {
    //        var sideNumber = Hex.Direction(originalFacing);

    //        var newFacing = sideNumber + rotation;
    //        if (newFacing < 0)
    //            newFacing += 6;
    //        if (newFacing > 5)
    //            newFacing -= 6;

    //        return Hex.Direction(newFacing);
    //    }
    //}

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


    internal class SimulationTimeStampBuilder
    {
      
        //public static SimulationTimeStampBuilder Unpack(string id)
        //{
        //    var tsBuilder = new SimulationTimeStampBuilder();
        //    var parts = id.Split(',');
        //    tsBuilder.TurnNumber = Convert.ToInt32(parts[0]);
        //    tsBuilder.TurnStep = (TurnStep)Enum.Parse(typeof(TurnStep), parts[1]);

        //    if (!string.IsNullOrEmpty(parts[2]))
        //        tsBuilder.ImpulseStep = (ImpulseStep)Enum.Parse(typeof(ImpulseStep), parts[2]);

        //    if (!string.IsNullOrEmpty(parts[3]))
        //        tsBuilder.Impulse = Convert.ToInt32(parts[3]);

        //    if (!string.IsNullOrEmpty(parts[4]))
        //        tsBuilder.SubPulse = Convert.ToInt32(parts[4]);
        //    return tsBuilder;
        //}

        public SimulationTimeStampBuilder()
        {
        }

        public SimulationTimeStampBuilder(SimulationTimeStamp ts)
        {
            TurnNumber = ts.TurnNumber;
            TurnStep = ts.TurnStep;
            ImpulseStep = ts.ImpulseStep;
            Impulse = ts.Impulse;
            SubPulse = ts.SubPulse;
        }

        private int TurnNumber { get; set; }
        private TurnStep TurnStep { get; set; }
        private ImpulseStep? ImpulseStep { get; set; }
        private int? Impulse { get; set; }
        private int? SubPulse { get; set; }



        public SimulationTimeStamp ToTimeStamp()
        {
            return new SimulationTimeStamp
            {
                TurnNumber = TurnNumber,
                TurnStep = TurnStep,
                ImpulseStep = ImpulseStep,
                SubPulse = SubPulse,
                Impulse = Impulse,
            };
        }

        public void Increment()
        {

            if (TurnStep == TurnStep.EnergyAllocation)
            {
                Impulse = 0;
                SubPulse = null;
                ImpulseStep = FleetCommander.Simulation.ImpulseStep.SpeedChange;
                TurnNumber = this.TurnNumber;
                TurnStep = TurnStep.ImpulseProcess;
                return;
            };

            if (TurnStep == TurnStep.ImpulseProcess)
            {
                if (ImpulseStep == FleetCommander.Simulation.ImpulseStep.SpeedChange)
                {
                    SubPulse = 0;
                    ImpulseStep = FleetCommander.Simulation.ImpulseStep.Movement;
                    return;
                }

                if (ImpulseStep == FleetCommander.Simulation.ImpulseStep.Movement)
                {
                    SubPulse++;
                    if (SubPulse == 4)
                    {
                        SubPulse = null;
                        ImpulseStep = FleetCommander.Simulation.ImpulseStep.OffensiveFire;
                    }

                    return;
                }

                if (ImpulseStep == FleetCommander.Simulation.ImpulseStep.OffensiveFire)
                {
                    Impulse++;
                    if (Impulse == 8)
                    {
                        Impulse = null;
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

    public struct SimulationTimeStamp
    {
        //public string Id { get; set; }

        public int TurnNumber { get; set; }
        public TurnStep TurnStep { get; set; }
        public ImpulseStep? ImpulseStep { get; set; }
        public int? Impulse { get; set; }
        public int? SubPulse { get; set; }

        public override string ToString()
        {
            return GetId();
        }

        [Pure]
        public string GetId()
        {
            return $"{TurnNumber},{TurnStep},{ImpulseStep},{Impulse},{SubPulse}";
        }

        [Pure]
        public SimulationTimeStamp Increment()
        {
            var builder = new SimulationTimeStampBuilder(this);
            builder.Increment();
            return builder.ToTimeStamp();
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
