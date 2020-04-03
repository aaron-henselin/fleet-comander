namespace FleetCommander.Simulation
{
    public interface ICanBeOverloaded
    {
        bool IsOverloaded { get; }
    }

    public abstract class DirectFireProjectile
    {
        public int Target { get; set; }
        public int Distance { get; set; }
        public SystemTargeting Targeting { get; set; }
        public int HitTrack { get; internal set; }
        public int Origin { get; internal set; }
        public VolleyId VolleyId { get; internal set; }

        public abstract int CalculateDamage();
    }

    public struct VolleyId
    {
        public int Target { get; set; }
        public int Origin { get; set; }
    }
}