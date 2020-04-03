using FleetCommander.Simulation.Framework.GridSystem;
using FleetCommander.Simulation.Simulation.Ships;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FleetCommander.Simulation.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var game = new Game(0);
            var ship1 = game.Spawn(new FederationHeavyCruiserSpecification(), new Position
            {
                Hex=new Hex(0,0,0),
                Rotation = 0
            });
            var ship2 = game.Spawn(new FederationHeavyCruiserSpecification(), new Position
            {
                Hex = new Hex(10, 10, 10),
                Rotation = 0
            });
 
            game.AdvanceGame();
        }
    }
}
