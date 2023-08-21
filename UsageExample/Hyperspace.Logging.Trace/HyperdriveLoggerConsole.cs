using SharpSenz;

namespace Hyperspace.Logging.ConsoleOut
{
    public class HyperdriveLoggerConsole : Hyperdrive.ISignalsReceptor
    {
        void Hyperdrive.ISignalsReceptor.JumpThroughHyperspace_StartingHyperjump(SignalContext context, Tuple<double, double, double> destination)
        {
            Log(context, $"Starting a hyperjump to ({destination.Item1:0.0}, {destination.Item2:0.0}, {destination.Item3:0.0})");
        }

        void Hyperdrive.ISignalsReceptor.JumpThroughHyperspace_PreparingHyperdrive(SignalContext context, HyperspaceRoute route)
        {
            Log(context, context.Signal);
        }

        void Hyperdrive.ISignalsReceptor.JumpThroughHyperspace_Jumping(SignalContext context, HyperspaceRoute route)
        {
            Log(context, context.Signal);
        }

        void Hyperdrive.ISignalsReceptor.JumpThroughHyperspace_HyperjumpIsCompleted(SignalContext context, HyperspaceRoute route, HyperspaceNavigation _hyperspaceNavigation)
        {
            Tuple<double, double, double> location = _hyperspaceNavigation.CurrentLocation;
            Log(context, $"Hyperjump is completed. Current location is ({location.Item1:0.0}, {location.Item2:0.0}, {location.Item3:0.0})");
        }

        private void Log(SignalContext context, string message)
        {
            Console.WriteLine($"{DateTime.Now:yyyy:MM:dd:HH:mm:ss} - {context.ClassName}.{context.MethodName} - {message}");
        }
    }
}
