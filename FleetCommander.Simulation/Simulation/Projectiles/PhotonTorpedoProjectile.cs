using System;
using FleetCommander.Simulation.Simulation.Ships;

namespace FleetCommander.Simulation.Simulation.Projectiles
{
    public class PhotonTorpedoProjectile : DirectFireProjectile, ICanBeOverloaded
    {
        public PhotonTorpedoOverloadState OverloadState { get; set; } = PhotonTorpedoOverloadState.NotOverloaded;

        public bool IsOverloaded => OverloadState != PhotonTorpedoOverloadState.NotOverloaded;

        readonly StandardPhotonTorpedoDamageOutputTable _standardPhotonTorpedoDamageOutputTable = new StandardPhotonTorpedoDamageOutputTable();
        readonly Overload4PhotonTorpedoDamageOutputTable _overload4PhotonTorpedoDamageOutputTable = new Overload4PhotonTorpedoDamageOutputTable();
        readonly Overload8PhotonTorpedoDamageOutputTable _overload8PhotonTorpedoDamageOutputTable = new Overload8PhotonTorpedoDamageOutputTable();

        public override int CalculateDamage()
        {
            if (OverloadState == PhotonTorpedoOverloadState.NotOverloaded)
            {
                return _standardPhotonTorpedoDamageOutputTable.GetDamageOutput(Distance, HitTrack).DamageOutputed;
            }

            if (OverloadState == PhotonTorpedoOverloadState.Overloaded4)
            {
                return _overload4PhotonTorpedoDamageOutputTable.GetDamageOutput(Distance, HitTrack).DamageOutputed;
            }

            if (OverloadState == PhotonTorpedoOverloadState.Overloaded8)
            {
                return _overload8PhotonTorpedoDamageOutputTable.GetDamageOutput(Distance, HitTrack).DamageOutputed;
            }

            throw new ArgumentException();
        }


        class StandardPhotonTorpedoDamageOutputTable : DamageOutputTable
        {
            public StandardPhotonTorpedoDamageOutputTable()
            {
                Add(0, 0, 1, 6, 8);
                Add(1, 1, 1, 6, 8);
                Add(2, 2, 1, 5, 8);
                Add(3, 4, 1, 4, 8);
                Add(5, 8, 1, 3, 8);
                Add(9, 12, 1, 2, 8);
                Add(13, 25, 1, 1, 8);
            }
        }

        class Overload4PhotonTorpedoDamageOutputTable : DamageOutputTable
        {
            public Overload4PhotonTorpedoDamageOutputTable()
            {
                Add(0, 0, 1, 6, 12);
                Add(1, 1, 1, 6, 12);
                Add(2, 2, 1, 5, 12);
                Add(3, 4, 1, 4, 12);
                Add(5, 8, 1, 3, 12);
            }
        }
        class Overload8PhotonTorpedoDamageOutputTable : DamageOutputTable
        {
            public Overload8PhotonTorpedoDamageOutputTable()
            {
                Add(0, 0, 1, 6, 16);
                Add(1, 1, 1, 6, 16);
                Add(2, 2, 1, 5, 16);
                Add(3, 4, 1, 4, 16);
                Add(5, 8, 1, 3, 16);
            }
        }
    }
}