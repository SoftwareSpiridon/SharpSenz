using NLog;
using NLog.Targets;
using SharpSenz;

namespace Hyperspace.Logging.NLogOut
{
    public class HyperdriveLoggerNLog : Hyperdrive.ISignalsReceptor
    {
        private readonly Logger _log = LogManager.GetLogger(typeof(Hyperdrive).ToString());

        static HyperdriveLoggerNLog()
        {
            var target = new ColoredConsoleTarget();
            target.Layout = "${date:format=yy-MM-dd HH\\:MM\\:ss} ${logger} ${message}";

            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);
        }

        private void Log(LogLevel level, SignalContext context, string message)
        {
            _log.Log(level, $"{context.MethodName} - {message}");
        }

        void Hyperdrive.ISignalsReceptor.JumpThroughHyperspace_StartingHyperjump(SignalContext context, Tuple<double, double, double> destination)
        {
            Log(LogLevel.Info, context, $"Starting a hyperjump to ({destination.Item1:0.0}, {destination.Item2:0.0}, {destination.Item3:0.0})");
        }

        void Hyperdrive.ISignalsReceptor.JumpThroughHyperspace_PreparingHyperdrive(SignalContext context, HyperspaceRoute route)
        {
            Log(LogLevel.Info, context, context.Signal);
        }

        void Hyperdrive.ISignalsReceptor.JumpThroughHyperspace_Jumping(SignalContext context, HyperspaceRoute route)
        {
            Log(LogLevel.Info, context, context.Signal);
        }

        void Hyperdrive.ISignalsReceptor.JumpThroughHyperspace_HyperjumpIsCompleted(SignalContext context, HyperspaceRoute route, HyperspaceNavigation _hyperspaceNavigation)
        {
            Tuple<double, double, double> location = _hyperspaceNavigation.CurrentLocation;
            Log(LogLevel.Info, context, $"Hyperjump is completed. Current location is ({location.Item1:0.0}, {location.Item2:0.0}, {location.Item3:0.0})");
        }
    }
}