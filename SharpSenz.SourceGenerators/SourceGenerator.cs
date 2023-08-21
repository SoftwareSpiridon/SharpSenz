using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SharpSenz.Analyzers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SharpSenz.SourceGenerators
{
    [Generator]
    public class SourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext initializationContext)
        {
            var compilationProvider = initializationContext.CompilationProvider;
            initializationContext.RegisterSourceOutput(compilationProvider, GenerateSourceCode);
        }

        private static void GenerateSourceCode(SourceProductionContext context, Compilation compilation)
        {
            //var mainMethod = compilation.GetEntryPoint(context.CancellationToken);
            //var typeName = mainMethod.ContainingType.Name;

            ImmutableArray<ClassDeclarationSyntax> classesWithSignals = FindClassesWithSignals(compilation, context.CancellationToken);
            foreach (ClassDeclarationSyntax classDeclaration in classesWithSignals)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                string classNamespace = GetTypeNamespace(classDeclaration);

                string classSignalsReceptorMultiplexFileName = $"{GetClassSignalsMultiplexName(classDeclaration)}.g.cs";
                string classSignalsReceptorMultiplexSource = GenerateClassSignalsReceptorMultiplexSource(semanticModel, classNamespace, classDeclaration);
                context.AddSource(classSignalsReceptorMultiplexFileName, classSignalsReceptorMultiplexSource);

                string classIContextSignalsReceptorFileName = $"{GetIClassSignalsReceptorName(classDeclaration)}.g.cs";
                string classIContextSignalsReceptorSource = GenerateISignalsReceptorSource(semanticModel, classNamespace, classDeclaration);
                context.AddSource(classIContextSignalsReceptorFileName, classIContextSignalsReceptorSource);
            }
        }

        private static ImmutableArray<ClassDeclarationSyntax> FindClassesWithSignals(Compilation compilation, CancellationToken cancellationToken)
        {
            ImmutableArray<ClassDeclarationSyntax>.Builder classDeclarations = ImmutableArray.CreateBuilder<ClassDeclarationSyntax>();
            foreach(SyntaxTree syntaxTree in compilation.SyntaxTrees)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
                SyntaxNode syntaxRoot = syntaxTree.GetRoot(cancellationToken);
                foreach (ClassDeclarationSyntax classDeclaration in syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    if (Analyzer.IsClassASignalsSource(semanticModel, classDeclaration))
                    {
                        classDeclarations.Add(classDeclaration);
                    }
                }
            }
            return classDeclarations.ToImmutable();
        }

        private static string GetTypeNamespace(BaseTypeDeclarationSyntax syntax)
        {
            // If we don't have a namespace at all we'll return an empty string
            // This accounts for the "default namespace" case
            string nameSpace = string.Empty;

            // Get the containing syntax node for the type declaration
            // (could be a nested type, for example)
            SyntaxNode potentialNamespaceParent = syntax.Parent;

            // Keep moving "out" of nested classes etc until we get to a namespace
            // or until we run out of parents
            while ((potentialNamespaceParent != null)
                && !(potentialNamespaceParent is NamespaceDeclarationSyntax)
                && !(potentialNamespaceParent is FileScopedNamespaceDeclarationSyntax))
            {
                potentialNamespaceParent = potentialNamespaceParent.Parent;
            }

            // Build up the final namespace by looping until we no longer have a namespace declaration
            if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
            {
                // We have a namespace. Use that as the type
                nameSpace = namespaceParent.Name.ToString();

                // Keep moving "out" of the namespace declarations until we 
                // run out of nested namespace declarations
                while (true)
                {
                    if (!(namespaceParent.Parent is NamespaceDeclarationSyntax parent))
                    {
                        break;
                    }

                    // Add the outer namespace as a prefix to the final namespace
                    nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                    namespaceParent = parent;
                }
            }

            // return the final namespace
            return nameSpace;
        }

        private static string GetTypeName(BaseTypeDeclarationSyntax syntax)
        {
            return syntax.Identifier.Text;
        }

        private static string GetIClassSignalsReceptorName(BaseTypeDeclarationSyntax syntax)
        {
            return $"I{GetTypeName(syntax)}SignalsReceptor";
        }

        private static string GetClassSignalsMultiplexName(BaseTypeDeclarationSyntax syntax)
        {
            return $"{GetTypeName(syntax)}SignalsMultiplex";
        }

        private static string GenerateISignalsReceptorSource(SemanticModel semanticModel, string classNamespace, ClassDeclarationSyntax classDeclaration)
        {
            string senzNamespace = GetSenzNamespace();
            string header = $@"using {senzNamespace};

namespace {classNamespace}
{{
    public partial class {classDeclaration.Identifier.Text}
    {{
        public interface ISignalsReceptor
        {{
";

            string footer = $@"        }}
    }}
}}
";

            string signalsRepectorName = Analyzer.FindSignalsMultiplexFieldName(classDeclaration);

            StringBuilder sourceCode = new StringBuilder();
            sourceCode.Append(header);

            foreach (SignalData methodData in GetMethodDatas(semanticModel, classDeclaration, signalsRepectorName, true))
            {
                sourceCode.AppendLine($"            void {methodData.SignalMethodName}({methodData.ArgumentsDefinitions});");
            }

            sourceCode.Append(footer);
            return sourceCode.ToString();
        }

        private static string GenerateClassSignalsReceptorMultiplexSource(SemanticModel semanticModel, string classNamespace, ClassDeclarationSyntax classDeclaration)
        {
            string nSenzNamespace = GetSenzNamespace();
            string classISignalsReceptorName = GetIClassSignalsReceptorName(classDeclaration);
            string header = $@"using {nSenzNamespace};

namespace {classNamespace}
{{
    public partial class {classDeclaration.Identifier.Text}
    {{
        public partial class SignalsMultiplex
        {{
            private readonly SignalContext _signalContext = new SignalContext();

            public readonly IList<ISignalsReceptor> Receptors = new List<ISignalsReceptor>();

";

            string footer = $@"        }}
    }}
}}
";

            StringBuilder sourceCode = new StringBuilder();
            sourceCode.Append(header);

            string signalsRepectorName = Analyzer.FindSignalsMultiplexFieldName(classDeclaration);

            foreach (SignalData methodData in GetMethodDatas(semanticModel, classDeclaration, signalsRepectorName, false))
            {
                string argumentsToCall = "_signalContext";
                if (!string.IsNullOrEmpty(methodData.ArgumentsToCall))
                {
                    argumentsToCall += $", {methodData.ArgumentsToCall}";
                }
                sourceCode.AppendLine($@"            public void {methodData.SignalMethodName}({methodData.ArgumentsDefinitions})
            {{
                _signalContext.FilePath = @""{methodData.CallerFilePath}"";
                _signalContext.FileLine = {methodData.CallerFileLine};
                _signalContext.ClassName = @""{methodData.CallerClassName}"";
                _signalContext.MethodName = @""{methodData.CallerMethodName}"";
                _signalContext.Signal = @""{methodData.SignalString}"";

                foreach (ISignalsReceptor receptor in Receptors)
                {{
                    receptor.{methodData.SignalMethodName}({argumentsToCall});
                }}
            }}
");
            }

            sourceCode.Append(footer);
            return sourceCode.ToString();
        }

        private static ImmutableArray<SignalData> GetMethodDatas(SemanticModel semanticModel, ClassDeclarationSyntax classDeclaration, string signalsReceptorName, bool hasContext)
        {
            ImmutableArray<SignalData>.Builder methodDatas = ImmutableArray.CreateBuilder<SignalData>();

            foreach (MethodDeclarationSyntax methodDeclarationSyntax in classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                string methodName = methodDeclarationSyntax.Identifier.Text;

                foreach (InvocationExpressionSyntax invocationExpressionSyntax in methodDeclarationSyntax.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    MemberAccessExpressionSyntax memberAccessSyntax = invocationExpressionSyntax.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault();
                    if (memberAccessSyntax == null)
                    {
                        continue;
                    }

                    IdentifierNameSyntax memberName = memberAccessSyntax.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
                    if (memberName == null)
                    {
                        continue;
                    }

                    if (memberName.Identifier.Text != signalsReceptorName)
                    {
                        continue;
                    }

                    IdentifierNameSyntax signalMethodName = memberAccessSyntax.DescendantNodes().OfType<IdentifierNameSyntax>().Skip(1).FirstOrDefault();
                    if (signalMethodName == null)
                    {
                        continue;
                    }

                    string signalString = "";
                    if (invocationExpressionSyntax.HasLeadingTrivia)
                    {
                        foreach (SyntaxTrivia singleLineComment in invocationExpressionSyntax.GetLeadingTrivia().Where(x => x.IsKind(SyntaxKind.SingleLineCommentTrivia)))
                        {
                            Match signalMatch = Analyzer.SignalRegex.Match(singleLineComment.ToFullString());
                            if (signalMatch.Success)
                            {
                                signalString = signalMatch.Groups["Signal"].Value;
                            }
                        }
                    }

                    List<string> argumentsDefinitions = new List<string>();
                    List<string> argumentsToCall = new List<string>();
                    if (hasContext)
                    {
                        argumentsDefinitions.Add("SignalContext context");
                        argumentsToCall.Add("context");
                    }

                    ArgumentListSyntax argumentList = invocationExpressionSyntax.DescendantNodes().OfType<ArgumentListSyntax>().FirstOrDefault();
                    if (argumentList != null)
                    {
                        foreach (ArgumentSyntax argumentSyntax in argumentList.Arguments)
                        {
                            IdentifierNameSyntax argumentIdentifierSyntax = argumentSyntax.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
                            TypeInfo argTypeInfo = semanticModel.GetTypeInfo(argumentIdentifierSyntax);
                            argumentsDefinitions.Add($"{argTypeInfo.Type.ToDisplayString()} {argumentIdentifierSyntax.Identifier.Text}");
                            argumentsToCall.Add(argumentIdentifierSyntax.Identifier.Text);
                        }
                    }

                    methodDatas.Add(new SignalData(callerFilePath: invocationExpressionSyntax.SyntaxTree.FilePath,
                                                   callerFileLine: invocationExpressionSyntax.SyntaxTree.GetLineSpan(invocationExpressionSyntax.Span).StartLinePosition.Line,
                                                   callerClassName: classDeclaration.Identifier.Text,
                                                   callerMethodName: methodDeclarationSyntax.Identifier.Text,
                                                   signalString: signalString,
                                                   signalMethodName: signalMethodName.Identifier.Text,
                                                   argumentsDefinitions: string.Join(", ", argumentsDefinitions),
                                                   argumentsToCall: string.Join(", ", argumentsToCall)));
                }
            }

            return methodDatas.ToImmutable();
        }

        private static string GetSenzNamespace()
        {
            return "SharpSenz"; // TODO: Use Reflection to find the correct one dynamically
        }
    }
}
