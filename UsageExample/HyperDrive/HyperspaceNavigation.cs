namespace Hyperspace
{
    public class HyperspaceNavigation
    {
        public HyperspaceNavigation()
        {
            CurrentLocation = Tuple.Create(0.0, 0.0, 0.0);
        }

        public Tuple<double, double, double> CurrentLocation { get; set; }

        public HyperspaceRoute CalculateHyperspaceRoute(Tuple<double, double, double> destination)
        {
            return new HyperspaceRoute
            {
                Start = CurrentLocation,
                Destination = destination
            };
        }
    }
}
