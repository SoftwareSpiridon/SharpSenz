using Hyperspace;

var hyperspaceNavigation = new HyperspaceNavigation
{
    CurrentLocation = Tuple.Create(0.0, 0.0, 0.0)
};

var hyperdrive = new Hyperdrive(hyperspaceNavigation);

hyperdrive.JumpThroughHyperspace(Tuple.Create(1000.0, 1000.0, 1000.0));
