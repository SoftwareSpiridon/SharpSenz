using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace SharpSenz.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class Analyzer : DiagnosticAnalyzer
    {
        public const string Category = "SharpSenz";
        public const string SignalsMultiplexClassName = "SignalsMultiplex";

        public static readonly Regex SignalRegex = new Regex(@"SIG\s*:\s*(?<Message>[^$]+)$", RegexOptions.Singleline | RegexOptions.Compiled);

        public static readonly DiagnosticDescriptor NonPartialClass
            = new DiagnosticDescriptor(id: "SZ001",
                                       title: "Signals Source must be a partial class",
                                       messageFormat: "Type {0} marked with the [SignalsSource] attribute has to be a partial class",
                                       category: Category,
                                       defaultSeverity: DiagnosticSeverity.Error,
                                       isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MissingSignalsMultiplexClass
            = new DiagnosticDescriptor(id: "SZ002",
                                       title: "Signals Source must have a Signals Multiplex class declaration",
                                       messageFormat: "Type {0} doesn't have a Signals Multiplex class declaration",
                                       category: Category,
                                       defaultSeverity: DiagnosticSeverity.Error,
                                       isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor NotPublicSignalsMultiplexClass
            = new DiagnosticDescriptor(id: "SZ003",
                                       title: "Signals Multiplex class must be public",
                                       messageFormat: "Type {0} Signals Multiplex nested class is not public",
                                       category: Category,
                                       defaultSeverity: DiagnosticSeverity.Error,
                                       isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor NotPartialSignalsMultiplexClass
            = new DiagnosticDescriptor(id: "SZ004",
                                       title: "Signals Multiplex class must be partial",
                                       messageFormat: "Type {0} Signals Multiplex nested class is not partial",
                                       category: Category,
                                       defaultSeverity: DiagnosticSeverity.Error,
                                       isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MissingSignalsMultiplexMember
            = new DiagnosticDescriptor(id: "SZ005",
                                       title: "Signals Source must have a Signals Multiplex member",
                                       messageFormat: "Type {0} doesn't have a Signals Multiplex member",
                                       category: Category,
                                       defaultSeverity: DiagnosticSeverity.Error,
                                       isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MissingSignalsCall
            = new DiagnosticDescriptor(id: "SZ010",
                                       title: "Signals Multiplex member must be called on a signal",
                                       messageFormat: "Signal '{0}' has no Signals Multiplex member call",
                                       category: Category,
                                       defaultSeverity: DiagnosticSeverity.Error,
                                       isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(NonPartialClass,
                                             MissingSignalsMultiplexClass,
                                             NotPublicSignalsMultiplexClass,
                                             NotPartialSignalsMultiplexClass,
                                             MissingSignalsMultiplexMember,
                                             MissingSignalsCall);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSemanticModelAction(AnalyzeSemanticModel);
        }

        private static void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
        {
            SyntaxNode syntaxRoot = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);

            foreach (ClassDeclarationSyntax classDeclaration in syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                AnalyzeClassDeclaration(context, classDeclaration);
            }
        }

        private static void AnalyzeClassDeclaration(SemanticModelAnalysisContext context, ClassDeclarationSyntax classDeclaration)
        {
            bool isSignalsSource = IsClassASignalsSource(context.SemanticModel, classDeclaration);
            if (!isSignalsSource)
            {
                return;
            }

            if (!IsClassAPartialClass(classDeclaration))
            {
                context.ReportDiagnostic(Diagnostic.Create(NonPartialClass, Location.Create(classDeclaration.SyntaxTree, classDeclaration.Span), classDeclaration.Identifier.Text));
            }

            ClassDeclarationSyntax signalsMultiplexDeclaration = FindSignalsMultiplexClass(classDeclaration);

            if (signalsMultiplexDeclaration == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(MissingSignalsMultiplexClass, Location.Create(classDeclaration.SyntaxTree, classDeclaration.Span), classDeclaration.Identifier.Text));
                return;
            }

            if (!IsClassAPublicClass(signalsMultiplexDeclaration))
            {
                context.ReportDiagnostic(Diagnostic.Create(NotPublicSignalsMultiplexClass, Location.Create(classDeclaration.SyntaxTree, classDeclaration.Span), classDeclaration.Identifier.Text));
            }

            if (!IsClassAPartialClass(signalsMultiplexDeclaration))
            {
                context.ReportDiagnostic(Diagnostic.Create(NotPartialSignalsMultiplexClass, Location.Create(classDeclaration.SyntaxTree, classDeclaration.Span), classDeclaration.Identifier.Text));
            }

            string signalsMultiplexFieldName = FindSignalsMultiplexFieldName(classDeclaration);

            if (string.IsNullOrWhiteSpace(signalsMultiplexFieldName))
            {
                context.ReportDiagnostic(Diagnostic.Create(MissingSignalsMultiplexMember, Location.Create(classDeclaration.SyntaxTree, classDeclaration.Span), classDeclaration.Identifier.Text));
            }

            foreach (MethodDeclarationSyntax methodDeclaration in classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                foreach (Tuple<SyntaxTrivia, string> signalComment in FindSignalComments(methodDeclaration))
                {
                    SyntaxTrivia syntaxTrivia = signalComment.Item1;
                    string signalMessage = signalComment.Item2;
                    if (syntaxTrivia.Token.Text != signalsMultiplexFieldName)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(MissingSignalsCall, Location.Create(syntaxTrivia.SyntaxTree, syntaxTrivia.Span), signalMessage));
                    }
                }
            }
        }

        private static ClassDeclarationSyntax FindSignalsMultiplexClass(ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.DescendantNodes().OfType<ClassDeclarationSyntax>().Where(cd => cd.Identifier.Text == SignalsMultiplexClassName).FirstOrDefault();
        }

        public static bool IsClassASignalsSource(SemanticModel semanticModel, ClassDeclarationSyntax classDeclaration)
        {
            string signalsSourceAttributeTypeName = typeof(SignalsSourceAttribute).FullName;
            INamedTypeSymbol signalsSourceAttributeType = semanticModel.Compilation.GetTypeByMetadataName(signalsSourceAttributeTypeName);
            foreach (AttributeSyntax attributeSyntax in classDeclaration.AttributeLists.SelectMany(al => al.Attributes))
            {
                TypeInfo attributeTypeInfo = semanticModel.GetTypeInfo(attributeSyntax);
                if (SymbolEqualityComparer.Default.Equals(signalsSourceAttributeType, attributeTypeInfo.Type))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsClassAPublicClass(ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        }

        public static bool IsClassAPartialClass(ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
        }

        public static string FindSignalsMultiplexFieldName(ClassDeclarationSyntax classDeclaration)
        {
            foreach (FieldDeclarationSyntax fieldDeclaration in classDeclaration.DescendantNodes().OfType<FieldDeclarationSyntax>())
            {
                IdentifierNameSyntax identifierName = fieldDeclaration.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
                if (identifierName == null)
                {
                    continue;
                }

                if (identifierName.Identifier.Text != SignalsMultiplexClassName)
                {
                    continue;
                }

                VariableDeclaratorSyntax variableDeclarator = fieldDeclaration.DescendantNodes().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
                if (variableDeclarator == null)
                {
                    continue;
                }

                return variableDeclarator.Identifier.ValueText;
            }

            return null;
        }

        public static IEnumerable<Tuple<SyntaxTrivia, string>> FindSignalComments(MethodDeclarationSyntax methodDeclaration)
        {
            foreach (SyntaxTrivia syntaxTrivia in methodDeclaration.DescendantTrivia().Where(c => c.IsKind(SyntaxKind.SingleLineCommentTrivia)))
            {
                string signalMessage = GetSignalMessageFromComment(syntaxTrivia);
                if (string.IsNullOrWhiteSpace(signalMessage))
                {
                    continue;
                }

                yield return Tuple.Create(syntaxTrivia, signalMessage);
            }
        }

        public static string GetSignalMessageFromComment(SyntaxTrivia syntaxTrivia)
        {
            Match match = SignalRegex.Match(syntaxTrivia.ToFullString());
            if (match.Success)
            {
                return match.Groups["Message"].Value;
            }

            return null;
        }
    }
}
