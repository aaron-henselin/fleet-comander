using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetCommander.Simulation
{
    public class Dice
    {
        private Random _random;

        public Dice(int seed)
        {
            _random = new Random(seed);
        }

        public int RollD6()
        {
            return _random.Next(1, 7);
        }
    }
}
