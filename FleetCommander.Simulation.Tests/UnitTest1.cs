using System;
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
                Hex=new Hex(0,-1,1),
                Rotation = 0
            });
            var ship2 = game.Spawn(new FederationHeavyCruiserSpecification(), new Position
            {
                Hex = new Hex(1, -1, 0),
                Rotation = 0
            });
            game.ScheduleNextEnergyAllocation(ship1.ShipId,new EnergyAllocationDeclaration {RoutedToEngines = 8});
            game.ScheduleNextEnergyAllocation(ship2.ShipId, new EnergyAllocationDeclaration { RoutedToEngines = 8 });

            var originalPosition = ship1.Position.Hex;

            for (int i = 0; i < 200; i++)
            {
                game.AdvanceGame();
            }
            
            Console.WriteLine(game.SimulationTimeStamp);

            var eventualPosition = ship1.Position.Hex;
            Assert.AreNotEqual(originalPosition,eventualPosition);
        }
    }
}
