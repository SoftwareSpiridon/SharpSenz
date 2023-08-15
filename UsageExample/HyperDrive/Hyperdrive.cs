namespace Hyperspace
{
    public class Hyperdrive
    {
        private readonly HyperspaceNavigation _hyperspaceNavigation;

        public Hyperdrive(HyperspaceNavigation hyperspaceNavigation)
        {
            _hyperspaceNavigation = hyperspaceNavigation;
        }

        public void JumpThroughHyperspace(Tuple<double, double, double> start, Tuple<double, double, double> destination)
        {
            HyperspaceRoute route = _hyperspaceNavigation.CalculateHyperspaceRoute(start, destination);

            PrepareHyperspaceJump(route);

            PerformHyperspaceJump();
        }

        private void PrepareHyperspaceJump(HyperspaceRoute route)
        {
        }

        private void PerformHyperspaceJump()
        {
        }
    }
}