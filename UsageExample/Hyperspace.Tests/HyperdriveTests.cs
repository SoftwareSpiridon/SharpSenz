namespace Hyperspace.Tests
{
    public class HyperdriveTests
    {
        [Test]
        public void TestHyperjump()
        {
            var hyperspaceNavigation = new HyperspaceNavigation
            {
                CurrentLocation = Tuple.Create(3.0, 4.0, 5.0)
            };

            var hyperdrive = new Hyperdrive(hyperspaceNavigation);

            hyperdrive.JumpThroughHyperspace(Tuple.Create(103.0, 104.0, 105.0));

            Assert.That(hyperspaceNavigation.CurrentLocation.Item1, Is.EqualTo(103.0));
            Assert.That(hyperspaceNavigation.CurrentLocation.Item2, Is.EqualTo(104.0));
            Assert.That(hyperspaceNavigation.CurrentLocation.Item3, Is.EqualTo(105.0));
        }
    }
}
