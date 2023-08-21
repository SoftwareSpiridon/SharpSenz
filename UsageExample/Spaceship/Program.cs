using Hyperspace;
using Hyperspace.Logging.ConsoleOut;
using Hyperspace.Logging.NLogOut;
using Hyperspace.MessageBus.RabbitMQ;
using Hyperspace.PerformanceCounters;

var hyperspaceNavigation = new HyperspaceNavigation
{
    CurrentLocation = Tuple.Create(0.0, 0.0, 0.0)
};

var hyperdrive = new Hyperdrive(hyperspaceNavigation);

hyperdrive.signals.Receptors.Add(new HyperdriveLoggerConsole());
hyperdrive.signals.Receptors.Add(new HyperdriveLoggerNLog());
hyperdrive.signals.Receptors.Add(new HyperdrivePerformanceCounters());
hyperdrive.signals.Receptors.Add(new HyperdriveMessagesProducerRabitMQ());

hyperdrive.JumpThroughHyperspace(Tuple.Create(1000.0, 1000.0, 1000.0));
