using SharpSenz;

namespace Hyperspace
{
    [SignalsSource]
    public partial class Hyperdrive
    {
        public partial class SignalsMultiplex { }
        public readonly SignalsMultiplex signals = new SignalsMultiplex();

        private readonly HyperspaceNavigation _hyperspaceNavigation;

        public Hyperdrive(HyperspaceNavigation hyperspaceNavigation)
        {
            _hyperspaceNavigation = hyperspaceNavigation;
        }

        public void JumpThroughHyperspace(Tuple<double, double, double> destination)
        {
            //SIG: Starting Hyperjump
            signals.JumpThroughHyperspace_StartingHyperjump(destination);

            HyperspaceRoute route = _hyperspaceNavigation.CalculateHyperspaceRoute(destination);

            //SIG: Preparing Hyperdrive
            signals.JumpThroughHyperspace_PreparingHyperdrive(route);

            PrepareHyperspaceJump(route);

            //SIG: Jumping
            signals.JumpThroughHyperspace_Jumping(route);

            PerformHyperspaceJump(route);

            //SIG: Hyperjump is completed
            signals.JumpThroughHyperspace_HyperjumpIsCompleted(route, _hyperspaceNavigation);
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