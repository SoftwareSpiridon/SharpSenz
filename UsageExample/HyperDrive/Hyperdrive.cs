namespace Hyperspace
{
    public class Hyperdrive
    {
        private readonly HyperspaceNavigation _hyperspaceNavigation;

        public Hyperdrive(HyperspaceNavigation hyperspaceNavigation)
        {
            _hyperspaceNavigation = hyperspaceNavigation;
        }

        public void JumpThroughHyperspace(Tuple<double, double, double> destination)
        {
            HyperspaceRoute route = _hyperspaceNavigation.CalculateHyperspaceRoute(destination);

            PrepareHyperspaceJump(route);

            PerformHyperspaceJump(route);
        }

        private void PrepareHyperspaceJump(HyperspaceRoute route)
        {
        }

        private void PerformHyperspaceJump(HyperspaceRoute route)
        {
            _hyperspaceNavigation.CurrentLocation = route.Destination;
        }
    }
}