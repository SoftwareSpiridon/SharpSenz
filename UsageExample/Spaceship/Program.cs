using Hyperspace;

var hyperspaceNavigation = new HyperspaceNavigation();

var hyperdrive = new Hyperdrive(hyperspaceNavigation);

hyperdrive.JumpThroughHyperspace(Tuple.Create(0.0, 0.0, 0.0), Tuple.Create(Double.MaxValue, Double.MaxValue, Double.MaxValue));
