using System;
using System.Collections.Generic;
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
            //var ship2 = game.Spawn(new FederationHeavyCruiserSpecification(), new Position
            //{
            //    Hex = new Hex(1, -1, 0),
            //    Rotation = 0
            //});
            game.ScheduleNextEnergyAllocation(ship1.ShipId,new EnergyAllocationDeclaration {RoutedToEngines = 8});
            //game.ScheduleNextEnergyAllocation(ship2.ShipId, new EnergyAllocationDeclaration { RoutedToEngines = 8 });
            
            var originalPosition = ship1.Position.Hex;

            for (int i = 0; i < 1000; i++)
            {
                game.AdvanceGame();

                //game.ScheduleOffensiveFire(ship1.ShipId,new OffensiveFireDeclaration
                //{
                //    Volleys = new List<Volley>
                //    {
                //        new Volley
                //        {
                //            ShipFrom = ship1.ShipId,
                //            ShipTo = ship2.ShipId,
                //            SsdCode = new List<string>{"1","2"},
                //            Targeting = SystemTargeting.Weapons
                //        }
                //    }
                //});
            }
            
            

            Console.WriteLine(game.SimulationTimeStamp);

            var eventualPosition = ship1.Position.Hex;
            //Assert.IsFalse(ship2.ShipInternals.IsFrameIntact);
        }
    }
}
