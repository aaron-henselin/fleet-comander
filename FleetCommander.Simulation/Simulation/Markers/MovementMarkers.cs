using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetCommander.Simulation.Simulation.Markers
{
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
}
