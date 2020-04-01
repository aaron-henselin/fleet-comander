namespace FleetCommander.Simulation
{
    public struct Coordinate
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public Coordinate Project(char facing,int hexes)
        {
            int x=0;
            int y=0;
            int z=0;

            if (facing == Facing.A)
            {
                x = 1;
                z = 0;
                y = -1;
            }
            if (facing == Facing.B)
            {
                x = 0;
                z = 1;
                y = -1;
            }
            if (facing == Facing.C)
            {
                x = -1;
                z = 1;
                y = 0;
            }
            if (facing == Facing.D)
            { 
                x = -1;
                z = 0;
                y = 1;
            }
            if (facing == Facing.E)
            {
                x = 0;
                z = -1;
                y = 1;
            }
            if (facing == Facing.F)
            {
                x = 1;
                z = -1;
                y = 0;
            }

            return new Coordinate
            {
                X = x * hexes,
                Y = y * hexes,
                Z = z * hexes
            };
        }
    }
}