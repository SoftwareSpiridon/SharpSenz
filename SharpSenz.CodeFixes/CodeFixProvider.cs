using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Formatting;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using SharpSenz.Analyzers;


namespace SharpSenz.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixProvider)), Shared]
    public class CodeFixProvider : Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider
    {
        private delegate Task<Document> TypeCodeFixDelegate(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken);
        private delegate Task<Document> SignalCodeFixDelegate(Document document, SyntaxTrivia syntaxTrivia, CancellationToken cancellationToken);

        private static readonly ImmutableDictionary<string, Tuple<string, TypeCodeFixDelegate>> TypeCodeFixes = new Dictionary<string, Tuple<string, TypeCodeFixDelegate>>
        {
            { Analyzer.NonPartialClass.Id, Tuple.Create<string, TypeCodeFixDelegate>("Make class partial", MakePartialAsync) },
            { Analyzer.MissingSignalsMultiplexClass.Id, Tuple.Create<string, TypeCodeFixDelegate>("Add Signals Multiplex partial class", AddSignalsMultiplexClassAsync) },
            { Analyzer.MissingSignalsMultiplexMember.Id, Tuple.Create<string, TypeCodeFixDelegate>("Add Signals Multiplex member", AddSignalsMultiplexMemberAsync)},
        }.ToImmutableDictionary();

        private static readonly ImmutableDictionary<string, Tuple<string, SignalCodeFixDelegate>> SignalCodeFixes = new Dictionary<string, Tuple<string, SignalCodeFixDelegate>>
        {
            { Analyzer.MissingSignalsCall.Id, Tuple.Create<string, SignalCodeFixDelegate>("Add signals member call", AddSignalsMemberCallAsync) },
        }.ToImmutableDictionary();

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(TypeCodeFixes.Keys.Concat(SignalCodeFixes.Keys).ToArray()); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                var diagnosticSpan = diagnostic.Location.SourceSpan;

                if (TypeCodeFixes.ContainsKey(diagnostic.Id))
                {
                    TypeDeclarationSyntax typeDeclaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: TypeCodeFixes[diagnostic.Id].Item1,
                            createChangedDocument: c => TypeCodeFixes[diagnostic.Id].Item2(context.Document, typeDeclaration, c),
                            equivalenceKey: TypeCodeFixes[diagnostic.Id].Item1),
                        diagnostic);
                }

                if (SignalCodeFixes.ContainsKey(diagnostic.Id))
                {
                    SyntaxTrivia signalTrivia = root.FindTrivia(diagnosticSpan.Start);

                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: SignalCodeFixes[diagnostic.Id].Item1,
                            createChangedDocument: c => SignalCodeFixes[diagnostic.Id].Item2(context.Document, signalTrivia, c),
                            equivalenceKey: SignalCodeFixes[diagnostic.Id].Item1),
                        diagnostic);
                }
            }
        }

        private static async Task<Document> MakePartialAsync(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            SyntaxToken partialToken = SyntaxFactory.Token(SyntaxKind.PartialKeyword);

            SyntaxTokenList newModifiers = typeDecl.Modifiers.Add(partialToken);
            TypeDeclarationSyntax newType = typeDecl.WithModifiers(newModifiers);

            SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot = oldRoot.ReplaceNode(typeDecl, newType);

            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> AddSignalsMultiplexClassAsync(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            CompilationUnitSyntax compilationUnit = SyntaxFactory.ParseCompilationUnit($"public partial class {Analyzer.SignalsMultiplexClassName} {{ }}\r\n");
            ClassDeclarationSyntax signalsMultiplexDeclaration = compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            signalsMultiplexDeclaration = signalsMultiplexDeclaration.WithLeadingTrivia(GetMemberLeadingWhitespace(typeDecl));

            TypeDeclarationSyntax newType = typeDecl.WithMembers(typeDecl.Members.Insert(0, signalsMultiplexDeclaration));

            SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot = oldRoot.ReplaceNode(typeDecl, newType);

            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> AddSignalsMultiplexMemberAsync(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            CompilationUnitSyntax compilationUnit = SyntaxFactory.ParseCompilationUnit($"public readonly {Analyzer.SignalsMultiplexClassName} signals = new {Analyzer.SignalsMultiplexClassName}();\r\n");
            MemberDeclarationSyntax signalsMemberDeclaration = compilationUnit.DescendantNodes().OfType<MemberDeclarationSyntax>().FirstOrDefault();
            signalsMemberDeclaration = signalsMemberDeclaration.WithLeadingTrivia(GetMemberLeadingWhitespace(typeDecl));

            TypeDeclarationSyntax newType = typeDecl.WithMembers(typeDecl.Members.Insert(1, signalsMemberDeclaration));

            SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot = oldRoot.ReplaceNode(typeDecl, newType);

            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> AddSignalsMemberCallAsync(Document document, SyntaxTrivia signalTrivia, CancellationToken cancellationToken)
        {
            SyntaxTrivia leadingWhitespaceTrivia = GetSignalCommentLeadingWhitespace(signalTrivia);
            string signalLeadingWhitespace = leadingWhitespaceTrivia.ToFullString();
            string signalsFieldName = GetSignalsMemberNameForSignal(signalTrivia);
            string methodName = GetMethodNameForSignalTrivia(signalTrivia);

            CompilationUnitSyntax compilationUnit = SyntaxFactory.ParseCompilationUnit($"{signalLeadingWhitespace}{signalsFieldName}.{methodName}();\r\n");
            StatementSyntax signalsMemberInvocationDeclaration = compilationUnit.DescendantNodes().OfType<StatementSyntax>().FirstOrDefault();

            BlockSyntax oldBlock = signalTrivia.Token.Parent.AncestorsAndSelf().OfType<BlockSyntax>().FirstOrDefault();
            BlockSyntax newBlock = oldBlock;

            StatementSyntax oldStatement = signalTrivia.Token.Parent.AncestorsAndSelf().OfType<StatementSyntax>().FirstOrDefault();
            if (oldStatement is BlockSyntax)
            {
                // comments in the last line of the method
                SyntaxToken closeBraceToken = oldStatement.GetLastToken();
                SyntaxTriviaList syntaxTriviaList = closeBraceToken.LeadingTrivia;
                Tuple<SyntaxTriviaList, SyntaxTriviaList> triviaSplit = SplitTrivia(syntaxTriviaList, signalTrivia);

                signalsMemberInvocationDeclaration = signalsMemberInvocationDeclaration.WithLeadingTrivia(triviaSplit.Item1.Add(leadingWhitespaceTrivia));

                newBlock = oldBlock.ReplaceToken(closeBraceToken, closeBraceToken.WithLeadingTrivia(triviaSplit.Item2));
                newBlock = newBlock.WithStatements(newBlock.Statements.Add(signalsMemberInvocationDeclaration));
            }
            else
            {
                // comments on some line in the middle of the method
                SyntaxTriviaList syntaxTriviaList = oldStatement.GetLeadingTrivia();
                Tuple<SyntaxTriviaList, SyntaxTriviaList> triviaSplit = SplitTrivia(syntaxTriviaList, signalTrivia);

                signalsMemberInvocationDeclaration = signalsMemberInvocationDeclaration.WithLeadingTrivia(triviaSplit.Item1.Add(leadingWhitespaceTrivia));

                int oldStatementIndex = Enumerable.Range(0, oldBlock.Statements.Count).Where(index => oldBlock.Statements[index] == oldStatement).First();

                newBlock = oldBlock.ReplaceNode(oldStatement, oldStatement.WithLeadingTrivia(triviaSplit.Item2));
                newBlock = newBlock.WithStatements(newBlock.Statements.Insert(oldStatementIndex, signalsMemberInvocationDeclaration));
            }

            SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot = oldRoot.ReplaceNode(oldBlock, newBlock);

            return document.WithSyntaxRoot(newRoot);
        }

        private static Tuple<SyntaxTriviaList, SyntaxTriviaList> SplitTrivia(SyntaxTriviaList oldTrivia, SyntaxTrivia signalTrivia)
        {
            int signalTriviaIndex = Enumerable.Range(0, oldTrivia.Count).Where(index => oldTrivia[index] == signalTrivia).First();
            return Tuple.Create(new SyntaxTriviaList(oldTrivia.Take(signalTriviaIndex + 2)), new SyntaxTriviaList(oldTrivia.Skip(signalTriviaIndex + 2)));
        }

        private static SyntaxTriviaList GetMemberLeadingWhitespace(TypeDeclarationSyntax typeDeclaration)
        {
            return typeDeclaration.CloseBraceToken.LeadingTrivia.Add(SyntaxFactory.Whitespace("    "));
        }

        private static string GetSignalsMemberNameForSignal(SyntaxTrivia signalTrivia)
        {
            ClassDeclarationSyntax classDeclaration = signalTrivia.Token.Parent.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            return Analyzer.FindSignalsMultiplexFieldName(classDeclaration);
        }

        private static SyntaxTrivia GetSignalCommentLeadingWhitespace(SyntaxTrivia signalTrivia)
        {
            SyntaxTriviaList leadingTriviaList = signalTrivia.Token.LeadingTrivia;
            int signalTriviaIndex = Enumerable.Range(0, leadingTriviaList.Count).Where(triviaIndex => leadingTriviaList[triviaIndex] == signalTrivia).First();
            int whitespaceTriviaIndex = Enumerable.Range(0, signalTriviaIndex).Reverse().Where(triviaIndex => leadingTriviaList[triviaIndex].IsKind(SyntaxKind.WhitespaceTrivia)).First();
            return leadingTriviaList[whitespaceTriviaIndex];
        }

        private static string GetMethodNameForSignalTrivia(SyntaxTrivia signalTrivia)
        {
            string signalString = Analyzer.GetSignalStringFromComment(signalTrivia);
            string methodName = signalTrivia.Token.Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First().Identifier.Text;
            string signalStringForMethodName = GetSignalStringForMethodNameName(signalString);

            return $"{methodName}_{signalStringForMethodName}";
        }

        private static string GetSignalStringForMethodNameName(string signalString)
        {
            List<string> words = new List<string>();
            foreach(string word in signalString.Split(' '))
            {
                if (!string.IsNullOrWhiteSpace(word.Trim()))
                {
                    words.Add(word[0].ToString().ToUpper() + word.Substring(1));
                }
            }

            return string.Join("", words);
        }
    }
}
