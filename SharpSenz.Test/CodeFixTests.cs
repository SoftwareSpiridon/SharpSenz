using Microsoft;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSenz.Analyzers;
using SharpSenz.CodeFixes;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace SharpSenz.Test
{
    [TestClass]
    public class CodeFixTests
    {
        [TestMethod]
        public async Task SignalsSourceIsNotAPartialClassAndHasNoSignalsMultiplex_FixCode_TheCodeIsFixed()
        {
            var test = @"
    using System;
    using SharpSenz;

    namespace ConsoleApplication1
    {
        [SignalsSource]
        public class Signaller
        {
            public void Method()
            {
                int a = 10;

                int b = a + 5;
            }
        }
    }
";

            var fixtest = @"
    using System;
    using SharpSenz;

    namespace ConsoleApplication1
    {
        [SignalsSource]
        public partial class Signaller
        {
            public partial class SignalsMultiplex { }
            public readonly SignalsMultiplex signals = new SignalsMultiplex();
            public void Method()
            {
                int a = 10;

                int b = a + 5;
            }
        }
    }
";

            await GetCodeFix(test,
                             fixtest,
                             3,
                             new DiagnosticResult(Analyzer.NonPartialClass).WithSpan(7, 9, 16, 10).WithArguments("Signaller"),
                             new DiagnosticResult(Analyzer.MissingSignalsMultiplexClass).WithSpan(7, 9, 16, 10).WithArguments("Signaller")
                             ).RunAsync();
        }

        [TestMethod]
        public async Task SignalsSourceHasSignalsMultiplexButSignalsMemberCallIsMissingAtTheBeginningOfTheMethodOnCommentLine_FixCode_TheCodeIsFixed()
        {
            var test = @"
    using System;
    using SharpSenz;

    namespace ConsoleApplication1
    {
        [SignalsSource]
        public partial class Signaller
        {
            public partial class SignalsMultiplex
            {
                public void Method_SomeSignal() { }
            }

            public readonly SignalsMultiplex signals = new SignalsMultiplex();

            public void Method()
            {
                // SIG: Some Signal

                int a = 10;
                int b = a + 5;
            }
        }
    }
";

            var fixtest = @"
    using System;
    using SharpSenz;

    namespace ConsoleApplication1
    {
        [SignalsSource]
        public partial class Signaller
        {
            public partial class SignalsMultiplex
            {
                public void Method_SomeSignal() { }
            }

            public readonly SignalsMultiplex signals = new SignalsMultiplex();

            public void Method()
            {
                // SIG: Some Signal
                signals.Method_SomeSignal();

                int a = 10;
                int b = a + 5;
            }
        }
    }
";

            await GetCodeFix(test,
                             fixtest,
                             0,
                             new DiagnosticResult(Analyzer.MissingSignalsCall).WithSpan(19, 17, 19, 36).WithArguments("Some Signal")
                             ).RunAsync();
        }

        [TestMethod]
        public async Task SignalsSourceHasSignalsMultiplexButSignalsMemberCallIsMissingInTheMiddleOfTheMethodOneCommentLine_FixCode_TheCodeIsFixed()
        {
            var test = @"
    using System;
    using SharpSenz;

    namespace ConsoleApplication1
    {
        [SignalsSource]
        public partial class Signaller
        {
            public partial class SignalsMultiplex
            {
                public void Method_SomeSignal() { }
            }

            public readonly SignalsMultiplex signals = new SignalsMultiplex();

            public void Method()
            {
                int a = 10;

                // SIG: Some Signal

                int b = a + 5;
            }
        }
    }
";

            var fixtest = @"
    using System;
    using SharpSenz;

    namespace ConsoleApplication1
    {
        [SignalsSource]
        public partial class Signaller
        {
            public partial class SignalsMultiplex
            {
                public void Method_SomeSignal() { }
            }

            public readonly SignalsMultiplex signals = new SignalsMultiplex();

            public void Method()
            {
                int a = 10;

                // SIG: Some Signal
                signals.Method_SomeSignal();

                int b = a + 5;
            }
        }
    }
";

            await GetCodeFix(test,
                             fixtest,
                             0,
                             new DiagnosticResult(Analyzer.MissingSignalsCall).WithSpan(21, 17, 21, 36).WithArguments("Some Signal")
                             ).RunAsync();
        }

        [TestMethod]
        public async Task SignalsSourceHasSignalsMultiplexButSignalsMemberCallIsMissingInTheMiddleOfTheMethodMultipleCommentLines_FixCode_TheCodeIsFixed()
        {
            var test = @"
    using System;
    using SharpSenz;

    namespace ConsoleApplication1
    {
        [SignalsSource]
        public partial class Signaller
        {
            public partial class SignalsMultiplex
            {
                public void Method_SomeSignal() { }
            }

            public readonly SignalsMultiplex signals = new SignalsMultiplex();

            public void Method()
            {
                int a = 10;

                // Some comment before
                // SIG: Some Signal

                // Some comment after

                int b = a + 5;
            }
        }
    }
";

            var fixtest = @"
    using System;
    using SharpSenz;

    namespace ConsoleApplication1
    {
        [SignalsSource]
        public partial class Signaller
        {
            public partial class SignalsMultiplex
            {
                public void Method_SomeSignal() { }
            }

            public readonly SignalsMultiplex signals = new SignalsMultiplex();

            public void Method()
            {
                int a = 10;

                // Some comment before
                // SIG: Some Signal
                signals.Method_SomeSignal();

                // Some comment after

                int b = a + 5;
            }
        }
    }
";

            await GetCodeFix(test,
                             fixtest,
                             0,
                             new DiagnosticResult(Analyzer.MissingSignalsCall).WithSpan(22, 17, 22, 36).WithArguments("Some Signal")
                             ).RunAsync();
        }

        [TestMethod]
        public async Task SignalsSourceHasSignalsMultiplexButSignalsMemberCallIsMissingAtTheEndOfTheMethodOneCommentLine_FixCode_TheCodeIsFixed()
        {
            var test = @"
    using System;
    using SharpSenz;

    namespace ConsoleApplication1
    {
        [SignalsSource]
        public partial class Signaller
        {
            public partial class SignalsMultiplex
            {
                public void Method_SomeSignal() { }
            }

            public readonly SignalsMultiplex signals = new SignalsMultiplex();

            public void Method()
            {
                int a = 10;
                int b = a + 5;

                // SIG: Some Signal
            }
        }
    }
";

            var fixtest = @"
    using System;
    using SharpSenz;

    namespace ConsoleApplication1
    {
        [SignalsSource]
        public partial class Signaller
        {
            public partial class SignalsMultiplex
            {
                public void Method_SomeSignal() { }
            }

            public readonly SignalsMultiplex signals = new SignalsMultiplex();

            public void Method()
            {
                int a = 10;
                int b = a + 5;

                // SIG: Some Signal
                signals.Method_SomeSignal();
            }
        }
    }
";

            await GetCodeFix(test,
                             fixtest,
                             0,
                             new DiagnosticResult(Analyzer.MissingSignalsCall).WithSpan(22, 17, 22, 36).WithArguments("Some Signal")
                             ).RunAsync();
        }

        [TestMethod]
        public async Task SignalsSourceHasSignalsMultiplexButSignalsMemberCallIsMissingAtTheEndOfTheMethodMultipleCommentLines_FixCode_TheCodeIsFixed()
        {
            var test = @"
    using System;
    using SharpSenz;

    namespace ConsoleApplication1
    {
        [SignalsSource]
        public partial class Signaller
        {
            public partial class SignalsMultiplex
            {
                public void Method_SomeSignal() { }
            }

            public readonly SignalsMultiplex signals = new SignalsMultiplex();

            public void Method()
            {
                int a = 10;
                int b = a + 5;

                // Some comment before
                // SIG: Some Signal

                // Some Comment after

            }
        }
    }
";

            var fixtest = @"
    using System;
    using SharpSenz;

    namespace ConsoleApplication1
    {
        [SignalsSource]
        public partial class Signaller
        {
            public partial class SignalsMultiplex
            {
                public void Method_SomeSignal() { }
            }

            public readonly SignalsMultiplex signals = new SignalsMultiplex();

            public void Method()
            {
                int a = 10;
                int b = a + 5;

                // Some comment before
                // SIG: Some Signal
                signals.Method_SomeSignal();

                // Some Comment after

            }
        }
    }
";

            await GetCodeFix(test,
                             fixtest,
                             0,
                             new DiagnosticResult(Analyzer.MissingSignalsCall).WithSpan(23, 17, 23, 36).WithArguments("Some Signal")
                             ).RunAsync();
        }

        private static CSharpCodeFixTest<Analyzer, CodeFixProvider, MSTestVerifier> GetCodeFix(string test, string fixedCode, int iterations = 1, params DiagnosticResult[] expectedDiagnostics)
        {
            var codeFixTest = new CSharpCodeFixTest<Analyzer, CodeFixProvider, MSTestVerifier>();
            codeFixTest.TestState.Sources.Add(test);
            codeFixTest.FixedCode = fixedCode;
            codeFixTest.TestState.ExpectedDiagnostics.AddRange(expectedDiagnostics);
            codeFixTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(typeof(SignalsSourceAttribute).Assembly.Location));
            if (iterations > 0)
            {
                codeFixTest.NumberOfFixAllIterations = iterations;
                codeFixTest.NumberOfIncrementalIterations = iterations;
            }

            return codeFixTest;
        }
    }
}
