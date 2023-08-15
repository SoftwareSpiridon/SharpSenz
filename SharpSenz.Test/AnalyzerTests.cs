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
    public class AnalyzerTests
    {
        [TestMethod]
        public async Task SignalsSourceIsAPartialClassAndHasSignalsMultiplex_Analyze_NoIssuesFound()
        {
            var test = @"
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

            await GetAnalyzer(test).RunAsync();
        }

        [TestMethod]
        public async Task SignalsSourceIsNotAPartialClassAndHasNoSignalsMultiplex_Analyze_TwoIssuesFound()
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

            await GetAnalyzer(test,
                              new DiagnosticResult(Analyzer.NonPartialClass).WithSpan(7, 9, 16, 10).WithArguments("Signaller"),
                              new DiagnosticResult(Analyzer.MissingSignalsMultiplexClass).WithSpan(7, 9, 16, 10).WithArguments("Signaller")
                             ).RunAsync();
        }

        [TestMethod]
        public async Task SignalsSourceIsAPartialClassAndHasSignalsMultiplexClassButNoMember_Analyze_OneIssuesFound()
        {
            var test = @"
    using System;
    using SharpSenz;

    namespace ConsoleApplication1
    {
        [SignalsSource]
        public partial class Signaller
        {
            public partial class SignalsMultiplex { }

            public void Method()
            {
                int a = 10;

                int b = a + 5;
            }
        }
    }
";

            await GetAnalyzer(test,
                              new DiagnosticResult(Analyzer.MissingSignalsMultiplexMember).WithSpan(7, 9, 18, 10).WithArguments("Signaller")
                             ).RunAsync();
        }

        [TestMethod]
        public async Task SignalsSourceSignalsMultiplexButSignalCallIsMissing_Analyze_OneIssuesFound()
        {
            var test = @"
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

                // SIG: Some Signal

                int b = a + 5;
            }
        }
    }
";

            await GetAnalyzer(test,
                              new DiagnosticResult(Analyzer.MissingSignalsCall).WithSpan(18, 17, 18, 36).WithArguments("Some Signal")
                             ).RunAsync();
        }

        [TestMethod]
        public async Task SignalsSourceSignalsMultiplexWithSignalsMemberCall_Analyze_NoIssuesFound()
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
                public void SomeSignal() { }
            }

            public readonly SignalsMultiplex signals = new SignalsMultiplex();

            public void Method()
            {
                int a = 10;

                // SIG: Some Signal
                signals.SomeSignal();

                int b = a + 5;
            }
        }
    }
";

            await GetAnalyzer(test).RunAsync();
        }

        private static CSharpAnalyzerTest<Analyzer, MSTestVerifier> GetAnalyzer(string test, params DiagnosticResult[] expectedDiagnostics)
        {
            var analyzerTest = new CSharpAnalyzerTest<Analyzer, MSTestVerifier>();
            analyzerTest.TestState.Sources.Add(test);
            analyzerTest.TestState.ExpectedDiagnostics.AddRange(expectedDiagnostics);
            analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(typeof(SignalsSourceAttribute).Assembly.Location));

            return analyzerTest;
        }
    }
}
