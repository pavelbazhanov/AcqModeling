
namespace AcqModeling
{
    public class AnnihilationEvent
    {
        public double Time, Energy, Phi, Theta;
        public Vector3d Position;
        public AnnihilationEvent()
        {
        }
        public AnnihilationEvent(Vector3d p, double t, double e, double phi, double theta)
        {
            Position = p;
            this.Time = t;
            this.Energy = e;
            this.Phi = phi;
            this.Theta = theta;
        }

        public override string ToString()
        {
            string result = string.Empty;

            result += Position.ToString() + " Energy " + Energy.ToString() + " Time " + Time.ToString() + " Phi " + Phi.ToString() + " Theta " + Theta.ToString();

            return result;
        }
    };
}
