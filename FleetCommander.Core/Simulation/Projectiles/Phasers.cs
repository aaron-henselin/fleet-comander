using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetCommander.Simulation.Simulation.Projectiles
{
    public class Phaser1Projectile : DirectFireProjectile
    {
        Phaser1DamageOutputTable _phaser1DamageOutputTable = new Phaser1DamageOutputTable();

        public override int CalculateDamage()
        {
            var output = _phaser1DamageOutputTable.GetDamageOutput(this.Distance, this.HitTrack);
            return output.DamageOutputed;
        }

        class Phaser1DamageOutputTable : DamageOutputTable
        {
            public Phaser1DamageOutputTable()
            {
                Add(0, 0, 1, 1, 9);
                Add(1, 1, 1, 1, 8);
                Add(2, 2, 1, 1, 7);
                Add(3, 3, 1, 1, 6);
                Add(4, 4, 1, 1, 5);
                Add(5, 5, 1, 1, 5);
                Add(6, 8, 1, 1, 4);
                Add(9, 15, 1, 1, 3);
                Add(16, 25, 1, 1, 2);

                Add(0, 0, 2, 2, 8);
                Add(1, 1, 2, 2, 7);
                Add(2, 2, 2, 2, 6);
                Add(3, 3, 2, 2, 5);
                Add(4, 4, 2, 2, 5);
                Add(5, 5, 2, 2, 4);
                Add(6, 8, 2, 2, 3);
                Add(9, 15, 2, 2, 2);
                Add(16, 25, 2, 2, 1);

                //todo
            }
        }
    }

    public class Phaser2Projectile : DirectFireProjectile
    {
        public override int CalculateDamage()
        {
            throw new NotImplementedException();
        }
    }

    public class Phaser3Projectile : DirectFireProjectile
    {
        public override int CalculateDamage()
        {
            throw new NotImplementedException();
        }
    }
}
