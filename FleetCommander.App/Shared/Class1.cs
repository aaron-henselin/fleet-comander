using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FleetCommander.Simulation;

namespace FleetCommander.App.Shared
{
    public class GameBoard
    {
        public List<ShipToken> ShipTokens { get; set; } = new List<ShipToken>();
        public List<SlipToken> SlipTokens { get; set; } = new List<SlipToken>();
        public List<TurnToken> TurnTokens { get; set; } = new List<TurnToken>();

        public SimulationTimeStamp TimeStamp { get; set; }

        public ILookup<string, IToken> CreateTokenLookup()
        {
            return ShipTokens.Concat<IToken>(SlipTokens).Concat(TurnTokens).ToLookup(x => x.Row + "," + x.Col);
        }
        
    }

    public class TurnToken : IToken
    {
        public int Col { get; set; }
        public int Row { get; set; }
        public int Rot { get; set; }
    }

    

    public class SlipToken : IToken
    {
        public int Col { get; set; }
        public int Row { get; set; }
        public int Rot { get; set; }
    }

    public interface IToken
    {
        public int Col { get; }
        public int Row { get;}
        public int Rot { get; }
    }

    public class ShipToken : IToken
    {
        public int Col { get; set; }
        public int Row { get; set; }
        public int Rot { get; set; }
    }
}
