using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using FleetCommander.Simulation.Framework.GridSystem;
using FleetCommander.Simulation.Simulation.Markers;
using FleetCommander.Simulation.Simulation.Projectiles;
using FleetCommander.Simulation.Simulation.Ships;

namespace FleetCommander.Simulation
{
    //public class SimulationPaused : IDisposable
    //{
    //    private readonly Action _locker;
    //    private readonly Action _releaser;

    //    public SimulationPaused(Action locker, Action releaser)
    //    {
    //        _locker = locker;
    //        _releaser = releaser;
    //    }

    //    public void Dispose()
    //    {

    //    }
    //}



    public class Game
    {
        public int Seed { get; }
        public Dice Dice { get; }
        public SimulationTimeStamp SimulationTimeStamp { get; set; }
     

        public List<Ship> AllShips { get; set; } = new List<Ship>();
        public List<MovementMarker> MovementMarkers { get; set; } = new List<MovementMarker>();

        public Game(int seed)
        {
            this.Seed = seed;
            this.Dice = new Dice(seed);
            GetOrCreateTurn(0);
        }

        public Ship Spawn(ShipSpecification spec, Position p)
        {
            var ship = new Ship(spec);
            ship.Initialize();
            ship.Position = p;
            ship.ShipId = 1;
            if (AllShips.Any())
                ship.ShipId=AllShips.Max(x => x.ShipId) + 1;
            AllShips.Add(ship);
            return ship;
        }

        private SimulationTurn GetOrCreateTurn(int turnNumber)
        {
            if (_simulationTurns.ContainsKey(turnNumber))
                return _simulationTurns[turnNumber];

            var newTurn = new SimulationTurn();
            if (_simulationTurns.ContainsKey(turnNumber - 1))
            {
                var previousTurn = _simulationTurns[turnNumber - 1];

                //this isn't right, we need a 'best guess'
                newTurn.EnergyAllocations = previousTurn.EnergyAllocations;
            }

            _simulationTurns.TryAdd(turnNumber,newTurn);
            return newTurn;
        }

        ConcurrentDictionary<int,SimulationTurn> _simulationTurns { get; set; } = new ConcurrentDictionary<int, SimulationTurn>();

        #region user input
        public void ScheduleNextEnergyAllocation(int shipId, EnergyAllocationDeclaration energyAllocationDeclarationChange)
        {
            SimulationTurn turnToModify;
            if (SimulationTimeStamp.CanScheduleEnergyAllocationOnThisTurn())
                turnToModify = GetOrCreateTurn(SimulationTimeStamp.TurnNumber);
            else
                turnToModify = GetOrCreateTurn(SimulationTimeStamp.TurnNumber + 1);

            turnToModify.EnergyAllocations[shipId] = energyAllocationDeclarationChange;
        }

        public void ChangeCourseImmediate(int shipId, string timestampId, DeclaredNavigation navigation)
        {
            var currentTimeStamp = SimulationTimeStamp;
            if (currentTimeStamp.GetId() != timestampId)
                return;

            var turnToModify = GetOrCreateTurn(SimulationTimeStamp.TurnNumber);
            turnToModify.ImpulseProcessActions.SetDeclaredNavigation(shipId, currentTimeStamp, navigation);
        }

        public void ScheduleOffensiveFire(int shipId, OffensiveFireDeclaration offensiveFireDeclaration )
        {
            var nextFiringOpportunity = SimulationTimeStamp.GetNextFiringOppporunity();
            var turnToModify = GetOrCreateTurn(nextFiringOpportunity.TurnNumber);
            turnToModify.ImpulseProcessActions.SetOffensiveFire(0,nextFiringOpportunity,offensiveFireDeclaration);
        }
        #endregion



        private void ExecuteSpeedChange(Ship ship, DeclaredSpeedChange declaredSpeedChange)
        {
            if (declaredSpeedChange.SpeedChange.HasValue)
            {

            }
        }

        private void ExecutePlottedNavigation(Ship ship, DeclaredNavigation declaredNavigation)
        {
            if (declaredNavigation.SideSlipDirection.HasValue)
            {
                ExecuteSideSlip(ship, declaredNavigation.SideSlipDirection.Value);

            }
            else
            {
                if (declaredNavigation.NewFacing.HasValue)
                {
                    ship.Position = ship.Position.WithFacing(declaredNavigation.NewFacing.Value);
                    MovementMarkers.Add(new TurnMarker
                    {
                        ForShipId = ship.ShipId,
                        Remaining = ship.CurrentSpeed,
                        Position = ship.Position
                    });
                }
                ExecuteStandardMovement(ship);
            }

        }



        public void ExecuteSimultaneousFireDeclarations(IReadOnlyCollection<OffensiveFireDeclaration> fireDeclarations)
        {
            var salvos = GenerateSalvos(fireDeclarations);

            foreach (var salvo in salvos)
            {
                var ship = AllShips.Single(x => x.ShipId == salvo.TargetShipId);

                var useTargeting = salvo.Targeting;
                var damageRemainingToApply = salvo.TotalDamage;

                var applyDamageToFace = salvo.IncomingDamageDirection;
                for (int i = 0; i < ship.Position.Rotation; i++)
                    applyDamageToFace = applyDamageToFace.RotateLeft();
  
                ApplyDamage(damageRemainingToApply, applyDamageToFace, useTargeting, ship, DamageType.WeaponFire);

            }
        }

        private void ApplyDamage(int salvoDamage, Hex damageDirection, SystemTargeting salvoTargeting, Ship ship,DamageType damageType)
        {
            var damageRemainingToApply = salvoDamage;
            var useTargeting = salvoTargeting;

            var amtPenetrated = 0;

           

            while (damageRemainingToApply > 0 && ship.ShipInternals.IsFrameIntact)
            {
                var damageInThisBatch = Math.Min(damageRemainingToApply, 10);
                var dacTrack = GenerateDamageAllocationTrack(useTargeting);

                var applyDamageResult = ship.ApplyDamage(
                    damageInThisBatch,
                    damageDirection, 
                    dacTrack, 
                    damageType,
                    useTargeting);

                damageRemainingToApply -= damageInThisBatch;
                useTargeting = SystemTargeting.Indiscriminant;
                damageRemainingToApply += applyDamageResult.DamageToMoveToNextbatch;

                amtPenetrated += applyDamageResult.PenetratedDamage;
            }

            var qualifiesForBurnThrough = salvoDamage >= 10 && amtPenetrated == 0;
            if (qualifiesForBurnThrough)
                ApplyDamage(1,damageDirection,salvoTargeting,ship,DamageType.BurnThrough);
        }

        private DamageAllocationTrack GenerateDamageAllocationTrack(SystemTargeting useTargeting)
        {
            var d6 = Dice.RollD6();
            DamageAllocationTrack dacTrack = null;
            if (useTargeting == SystemTargeting.Indiscriminant)
                dacTrack = DamageAllocationChart.GetTrack(d6);

            if (useTargeting == SystemTargeting.Weapons)
            {
                if (d6 == 1 || d6 == 2)
                    dacTrack = DamageAllocationChart.Weapons;
            }

            if (useTargeting == SystemTargeting.Power)
            {
                if (d6 == 5 || d6 == 6)
                    dacTrack = DamageAllocationChart.Power;
            }

            return dacTrack;
        }

        private class Salvo
        {
            public List<DirectFireProjectile> Projectiles { get; set; } = new List<DirectFireProjectile>();
            public SystemTargeting Targeting { get; internal set; }
            public int TargetShipId { get; internal set; }
            public int TotalDamage => Projectiles.Sum(x => x.CalculateDamage());

            public Ship ShipFrom { get; internal set; }
            public Ship ShipTo { get; internal set; }
            public Hex IncomingDamageDirection { get; internal set; }
        }

        private IReadOnlyCollection<Salvo> GenerateSalvos(IReadOnlyCollection<OffensiveFireDeclaration> fireDeclarations)
        {
            List<Salvo> salvoes = new List<Salvo>();
            foreach (var volley in fireDeclarations.SelectMany(x => x.Volleys))
            {
                List<DirectFireProjectile> projectiles = new List<DirectFireProjectile>();

                var shipFrom = this.AllShips.Single(x => x.ShipId == volley.ShipFrom);
                var shipTo = this.AllShips.Single(x => x.ShipId == volley.ShipTo);
                volley.Distance = shipFrom.Position.Hex.Distance(shipTo.Position.Hex);

                Hex damageDirection;
                if (volley.Distance > 0)
                {
                    var hexes = FractionalHex.HexLinedraw(shipTo.Position.Hex, shipFrom.Position.Hex);

                    var h0 = hexes[0];
                    var h1 = hexes[1];
                    damageDirection = h0.NeighborDirection(h1);
                    
                }
                else
                {
                    damageDirection = Hex.Direction(Dice.RollD6());
                }

                foreach (var ssdCode in volley.SsdCode)
                {
                    var projectile = shipFrom.ExpendDirectFireProjectile(ssdCode);
                    projectile.VolleyId = new VolleyId {
                        Origin = volley.ShipFrom,
                        Target = volley.ShipTo

                    };
                    projectile.Origin = volley.ShipFrom;
                    projectile.Target = volley.ShipTo;
                    
                    projectile.HitTrack = this.Dice.RollD6();
                    
                    projectiles.Add(projectile);

                    if (projectile is ICanBeOverloaded overloading)
                        if (overloading.IsOverloaded)
                            volley.Targeting = SystemTargeting.Indiscriminant;
                        
                }
                salvoes.Add(new Salvo
                {
                    ShipFrom = shipFrom,
                    ShipTo = shipTo,
                    TargetShipId = volley.ShipTo,
                    Targeting = volley.Targeting,
                    Projectiles = projectiles,
                    IncomingDamageDirection = damageDirection
                });
              
            }

            return salvoes;
        }

        public void ApplyDirectFireProjectiles(IReadOnlyCollection<DirectFireProjectile> projectiles)
        {
            foreach (var projectile in projectiles)
            {
                if (projectile is PhotonTorpedoProjectile photonTorpedoProjectile)
                {
                    if (photonTorpedoProjectile.OverloadState == PhotonTorpedoOverloadState.Overloaded4 || photonTorpedoProjectile.OverloadState == PhotonTorpedoOverloadState.Overloaded8)
                    {
                    }
                }
            }
        }

        private void AdvanceSimulationTimestamp()
        {
        }

        public void AdvanceGame()
        {
            var turn = this.GetOrCreateTurn(this.SimulationTimeStamp.TurnNumber);
            if (this.SimulationTimeStamp.TurnStep == TurnStep.EnergyAllocation)
            {
                AllShips.ForEach(x =>
                {
                    if (turn.EnergyAllocations.ContainsKey(x.ShipId))
                    {
                        var energyAllocation = turn.EnergyAllocations[x.ShipId];
                        x.ExecuteDeclaredEnergyAllocation(energyAllocation);
                    }
                    else
                    {
                        var energyAllocation = x.CreateDefaultEnergyAllocation();
                        x.ExecuteDeclaredEnergyAllocation(energyAllocation);
                    }
                });

                this.SimulationTimeStamp = SimulationTimeStamp.Increment();

                return;
            }

            if (this.SimulationTimeStamp.TurnStep == TurnStep.ImpulseProcess)
            {
                if (this.SimulationTimeStamp.ImpulseStep == ImpulseStep.SpeedChange)
                {
                    this.SimulationTimeStamp = SimulationTimeStamp.Increment();
                    return;
                }

                if (this.SimulationTimeStamp.ImpulseStep == ImpulseStep.Movement)
                {
                    AllShips.ForEach(x =>
                    {
                        var declaredNavigation =
                            turn.ImpulseProcessActions.GetDeclaredNavigation(x.ShipId, this.SimulationTimeStamp);
                        ExecutePlottedNavigation(x, declaredNavigation);
                    });

                    this.SimulationTimeStamp = SimulationTimeStamp.Increment();
                    return;
                }

                if (this.SimulationTimeStamp.ImpulseStep == ImpulseStep.OffensiveFire)
                {
                    var fireDeclarations 
                        = AllShips.Select(x => turn.ImpulseProcessActions.GetOffensiveFire(x.ShipId, this.SimulationTimeStamp))
                            .Where(x => x != null).ToList();

                    GenerateSalvos(fireDeclarations);

                    this.SimulationTimeStamp = SimulationTimeStamp.Increment();
                    return;
                }
            }

            if (this.SimulationTimeStamp.TurnStep == TurnStep.RepairPhase)
            {
                AllShips.ForEach(x => x.RolloverExcessEnergyIntoBatteries());
                this.SimulationTimeStamp = SimulationTimeStamp.Increment();
                return;
            }
        }

        //(1E1) ENERGY ALLOCATION
        //See the rules on this subject (1D). In summary,
        //count the amount of energy your ship has, and obtain
        //energy tokens for each point. (During the first turn of
        //a scenario, the ship has additional energy tokens
        //equal to the number of batteries on the ship, representing power stored in the batteries.)
        //Pick and pay for your baseline speed (2B1b) secretly and simultaneously with other players.
        //Pay for any weapon pre-loading, such as Photon
        //Torpedoes (4C2).
        //Pay for any Shield Regeneration (3C7) at the rate
        //of two energy tokens for each shield box repaired
        private void ExecuteEnergyAllocation(Ship ship, EnergyAllocationDeclaration allocationDeclaration)
        {
            ship.ExecuteDeclaredEnergyAllocation(allocationDeclaration);

        }

        private void ExecuteStandardMovement(Ship ship)
        {

            var newCoodinate = ship.Position.Hex.Neighbor(ship.Position.Rotation);
            ship.Position = ship.Position.WithHex(newCoodinate);

            var turnMarkersToDecrement = MovementMarkers.Where(x => x.ForShipId == ship.ShipId).ToList();
            foreach (var turnMarker in turnMarkersToDecrement)
                turnMarker.Decrement();
        }

        private void ExecuteSideSlip(Ship ship, int direction)
        {
            MovementMarkers.Add(new SideSlipMarker
            {
                Position = ship.Position,
                Remaining = 1,
                ForShipId = ship.ShipId
            });

            var newCoodinate = ship.Position.Hex.Neighbor(direction);
            ship.Position = ship.Position.WithHex(newCoodinate);
        }
    }
}