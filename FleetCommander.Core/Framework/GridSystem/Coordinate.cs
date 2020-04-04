using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace FleetCommander.Simulation.Framework.GridSystem
{
    //public struct Coordinate
    //{
    //    public int X { get; set; }
    //    public int Y { get; set; }
    //    public int Z { get; set; }
    //    //public Coordinate Project(char facing,int hexes)
    //    //{
    //    //    int x=0;
    //    //    int y=0;
    //    //    int z=0;

    //    //    if (facing == Facing.A)
    //    //    {
    //    //        x = 1;
    //    //        z = 0;
    //    //        y = -1;
    //    //    }
    //    //    if (facing == Facing.B)
    //    //    {
    //    //        x = 0;
    //    //        z = 1;
    //    //        y = -1;
    //    //    }
    //    //    if (facing == Facing.C)
    //    //    {
    //    //        x = -1;
    //    //        z = 1;
    //    //        y = 0;
    //    //    }
    //    //    if (facing == Facing.D)
    //    //    { 
    //    //        x = -1;
    //    //        z = 0;
    //    //        y = 1;
    //    //    }
    //    //    if (facing == Facing.E)
    //    //    {
    //    //        x = 0;
    //    //        z = -1;
    //    //        y = 1;
    //    //    }
    //    //    if (facing == Facing.F)
    //    //    {
    //    //        x = 1;
    //    //        z = -1;
    //    //        y = 0;
    //    //    }

    //    //    return new Coordinate
    //    //    {
    //    //        X = x * hexes,
    //    //        Y = y * hexes,
    //    //        Z = z * hexes
    //    //    };
    //    //}


    //    //public int Distance(Coordinate otherCoordinate)
    //    //{
    //    //    return (Math.Abs(X - otherCoordinate.X) + Math.Abs(Y - otherCoordinate.Y) + Math.Abs(Z - otherCoordinate.Z)) / 2;
    //    //}
    //}


    struct DoubledCoord
    {
        public DoubledCoord(int col, int row)
        {
            this.col = col;
            this.row = row;
        }
        public readonly int col;
        public readonly int row;

        static public DoubledCoord QdoubledFromCube(Hex h)
        {
            int col = h.q;
            int row = 2 * h.r + h.q;
            return new DoubledCoord(col, row);
        }


        public Hex QdoubledToCube()
        {
            int q = col;
            int r = (int)((row - col) / 2);
            int s = -q - r;
            return new Hex(q, r, s);
        }


        static public DoubledCoord RdoubledFromCube(Hex h)
        {
            int col = 2 * h.q + h.r;
            int row = h.r;
            return new DoubledCoord(col, row);
        }


        public Hex RdoubledToCube()
        {
            int q = (int)((col - row) / 2);
            int r = row;
            int s = -q - r;
            return new Hex(q, r, s);
        }

    }

    public struct OffsetCoord
    {
        public OffsetCoord(int col, int row)
        {
            this.col = col;
            this.row = row;
        }
        public readonly int col;
        public readonly int row;
        static public int EVEN = 1;
        static public int ODD = -1;

        static public OffsetCoord QoffsetFromCube(int offset, Hex h)
        {
            int col = h.q;
            int row = h.r + (int)((h.q + offset * (h.q & 1)) / 2);
            if (offset != OffsetCoord.EVEN && offset != OffsetCoord.ODD)
            {
                throw new ArgumentException("offset must be EVEN (+1) or ODD (-1)");
            }
            return new OffsetCoord(col, row);
        }


        static public Hex QoffsetToCube(int offset, OffsetCoord h)
        {
            int q = h.col;
            int r = h.row - (int)((h.col + offset * (h.col & 1)) / 2);
            int s = -q - r;
            if (offset != OffsetCoord.EVEN && offset != OffsetCoord.ODD)
            {
                throw new ArgumentException("offset must be EVEN (+1) or ODD (-1)");
            }
            return new Hex(q, r, s);
        }


        static public OffsetCoord RoffsetFromCube(int offset, Hex h)
        {
            int col = h.q + (int)((h.r + offset * (h.r & 1)) / 2);
            int row = h.r;
            if (offset != OffsetCoord.EVEN && offset != OffsetCoord.ODD)
            {
                throw new ArgumentException("offset must be EVEN (+1) or ODD (-1)");
            }
            return new OffsetCoord(col, row);
        }


        static public Hex RoffsetToCube(int offset, OffsetCoord h)
        {
            int q = h.col - (int)((h.row + offset * (h.row & 1)) / 2);
            int r = h.row;
            int s = -q - r;
            if (offset != OffsetCoord.EVEN && offset != OffsetCoord.ODD)
            {
                throw new ArgumentException("offset must be EVEN (+1) or ODD (-1)");
            }
            return new Hex(q, r, s);
        }

    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct Hex
    {
        public OffsetCoord ToOffsetCoord()
        {
            return OffsetCoord.QoffsetFromCube(OffsetCoord.ODD, this);
        }

        private string DebuggerDisplay
        {
            get
            {
                var coord = ToOffsetCoord();
                return string.Format($"Coordinate=[{coord.col},{coord.row}], QRS=[{q},{r},{s}]");
            }
        }

        public Hex(int q, int r, int s)
        {
            this.q = q;
            this.r = r;
            this.s = s;
            if (q + r + s != 0) throw new ArgumentException("q + r + s must be 0");
        }
        public readonly int q;
        public readonly int r;
        public readonly int s;

        public Hex Add(Hex b)
        {
            return new Hex(q + b.q, r + b.r, s + b.s);
        }


        public Hex Subtract(Hex b)
        {
            return new Hex(q - b.q, r - b.r, s - b.s);
        }


        public Hex Scale(int k)
        {
            return new Hex(q * k, r * k, s * k);
        }


        public Hex RotateLeft()
        {
            return new Hex(-s, -q, -r);
        }


        public Hex RotateRight()
        {
            return new Hex(-r, -s, -q);
        }

        static public List<Hex> directions = new List<Hex> { new Hex(1, 0, -1), new Hex(1, -1, 0), new Hex(0, -1, 1), new Hex(-1, 0, 1), new Hex(-1, 1, 0), new Hex(0, 1, -1) };

        static public Hex Direction(int direction)
        {
            return Hex.directions[direction];
        }
        public static int Direction(Hex hexDirection)
        {
            var directionLookup = new Dictionary<Hex, int>();
            directionLookup.Add(directions[0], 0);
            directionLookup.Add(directions[1], 1);
            directionLookup.Add(directions[2], 2);
            directionLookup.Add(directions[3], 3);
            directionLookup.Add(directions[4], 4);
            directionLookup.Add(directions[5], 5);
            return directionLookup[hexDirection];
        }


        public Hex Neighbor(int direction)
        {
            return Add(Hex.Direction(direction));
        }

        public Hex NeighborDirection(Hex neighbor)
        {
            return this.Subtract(neighbor);
        }

        static public List<Hex> diagonals = new List<Hex> { new Hex(2, -1, -1), new Hex(1, -2, 1), new Hex(-1, -1, 2), new Hex(-2, 1, 1), new Hex(-1, 2, -1), new Hex(1, 1, -2) };

        public Hex DiagonalNeighbor(int direction)
        {
            return Add(Hex.diagonals[direction]);
        }


        public int Length()
        {
            return (int)((Math.Abs(q) + Math.Abs(r) + Math.Abs(s)) / 2);
        }


        public int Distance(Hex b)
        {
            return Subtract(b).Length();
        }


    }


    struct FractionalHex
    {
        public FractionalHex(double q, double r, double s)
        {
            this.q = q;
            this.r = r;
            this.s = s;
            if (Math.Round(q + r + s) != 0) throw new ArgumentException("q + r + s must be 0");
        }
        public readonly double q;
        public readonly double r;
        public readonly double s;

        public Hex HexRound()
        {
            int qi = (int)(Math.Round(q));
            int ri = (int)(Math.Round(r));
            int si = (int)(Math.Round(s));
            double q_diff = Math.Abs(qi - q);
            double r_diff = Math.Abs(ri - r);
            double s_diff = Math.Abs(si - s);
            if (q_diff > r_diff && q_diff > s_diff)
            {
                qi = -ri - si;
            }
            else
            if (r_diff > s_diff)
            {
                ri = -qi - si;
            }
            else
            {
                si = -qi - ri;
            }
            return new Hex(qi, ri, si);
        }


        public FractionalHex HexLerp(FractionalHex b, double t)
        {
            return new FractionalHex(q * (1.0 - t) + b.q * t, r * (1.0 - t) + b.r * t, s * (1.0 - t) + b.s * t);
        }


        static public List<Hex> HexLinedraw(Hex a, Hex b)
        {
            int N = a.Distance(b);
            FractionalHex a_nudge = new FractionalHex(a.q + 1e-06, a.r + 1e-06, a.s - 2e-06);
            FractionalHex b_nudge = new FractionalHex(b.q + 1e-06, b.r + 1e-06, b.s - 2e-06);
            List<Hex> results = new List<Hex> { };
            double step = 1.0 / Math.Max(N, 1);
            for (int i = 0; i <= N; i++)
            {
                results.Add(a_nudge.HexLerp(b_nudge, step * i).HexRound());
            }
            return results;
        }

    }

}