using SharpSenz;

namespace SharpSeszClient
{
    [SignalsSource]
    public partial class Client
    {
        public partial class SignalsMultiplex { }

        public SignalsMultiplex signals = new SignalsMultiplex();

        public void Method()
        {
            int a = 10;
            int c = 25;

            int? n = null;

            //SIG: Begin Of Calculation
            signals.Method_BeginOfCalc(a, c, n);

            int b = a + 1;

            Tuple<int, int> t = Tuple.Create(a, b);

            //SIG: End of Calculation
            signals.Method_EndOfCalc(t);
        }
    }
}
