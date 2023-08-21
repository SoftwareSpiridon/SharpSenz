using SharpSenz;

namespace Hyperspace.PerformanceCounters
{
    public class HyperdrivePerformanceCounters : Hyperdrive.ISignalsReceptor
    {
        void Hyperdrive.ISignalsReceptor.JumpThroughHyperspace_HyperjumpIsCompleted(SignalContext context, HyperspaceRoute route, HyperspaceNavigation _hyperspaceNavigation)
        {
        }

        void Hyperdrive.ISignalsReceptor.JumpThroughHyperspace_Jumping(SignalContext context, HyperspaceRoute route)
        {
        }

        void Hyperdrive.ISignalsReceptor.JumpThroughHyperspace_PreparingHyperdrive(SignalContext context, HyperspaceRoute route)
        {
        }

        void Hyperdrive.ISignalsReceptor.JumpThroughHyperspace_StartingHyperjump(SignalContext context, Tuple<double, double, double> destination)
        {
        }
    }
}