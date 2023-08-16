namespace Hyperspace
{
    public class HyperspaceRoute
    {
        public HyperspaceRoute()
        {
            Start = Tuple.Create(0.0, 0.0, 0.0);
            Destination = Tuple.Create(0.0, 0.0, 0.0);
        }

        public Tuple<double, double, double> Start { get; set; }

        public Tuple<double, double, double> Destination { get; set; }
    }
}
