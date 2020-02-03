using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Terraria.ModLoader.Setup
{
	public class FormatTask : RoslynTask
	{
		protected override string Status => "Formatting";
		//protected override int MaxDegreeOfParallelism => 1;

		public FormatTask(ITaskInterface taskInterface) : base(taskInterface) { }

		protected override async Task<Document> Process(Document doc) => await Format(doc, taskInterface.CancellationToken);

		public static async Task<Document> Format(Document doc, CancellationToken cancellationToken) {
			doc = await Visit(doc, new RemoveBracesFromSingleStatementRewriter());
			doc = await Visit(doc, new AddVisualNewlinesRewriter());
			doc = await	RoslynFormat(doc, cancellationToken);
			return doc;
		}

		private static async Task<Document> RoslynFormat(Document document, CancellationToken cancellationToken) {
			OptionSet options = await document.GetOptionsAsync();
			options = options.WithChangedOption(new OptionKey(FormattingOptions.UseTabs, LanguageNames.CSharp), true)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInProperties, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAccessors, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAnonymousMethods, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInControlBlocks, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAnonymousTypes, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInObjectCollectionArrayInitializers, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInLambdaExpressionBody, false);

			return await Formatter.FormatAsync(document, options, cancellationToken);
		}

		private static bool SpansMultipleLines(StatementSyntax node) {
			var lineSpan = node.SyntaxTree.GetLineSpan(node.Span);
			return lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line;
		}

		private class RemoveBracesFromSingleStatementRewriter : CSharpSyntaxRewriter
		{
			private static SyntaxAnnotation processedAnnotation = new SyntaxAnnotation();

			public override SyntaxNode VisitIfStatement(IfStatementSyntax node) {
				if (node.HasAnnotation(processedAnnotation))
					return base.VisitIfStatement(node);

				if (NoClausesNeedBraces(node))
					node = RemoveClauseBraces(node); //TODO: trailing space

				node = AnnotateTreeProcessed(node);
				return base.VisitIfStatement(node);
			}

			public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax method) {
				if (method.Body != null &&
					CanStripSurroundingBraces(method.Body) &&
					method.Body.DescendantTrivia().All(IsWhitespaceTrivia) &&
					method.Body.Statements[0] is ReturnStatementSyntax returnStatement &&
					returnStatement.Expression != null) {

					method = method
						.WithBody(null)
						.WithExpressionBody(SyntaxFactory.ArrowExpressionClause(returnStatement.Expression))
						.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

					//TODO: remove newlines between method and arrowexpression
				}

				return base.VisitMethodDeclaration(method);
			}

			private static IfStatementSyntax AnnotateTreeProcessed(IfStatementSyntax node) {
				if (node.Else?.Statement is IfStatementSyntax elseIfStmt)
					node = node.WithElse(node.Else.WithStatement(AnnotateTreeProcessed(elseIfStmt)));

				return node.WithAdditionalAnnotations(processedAnnotation);
			}

			private static bool NoClausesNeedBraces(IfStatementSyntax ifStmt) {

				// lets not destroy the stack
				while (true) {
					if (!CanStripSurroundingBraces(ifStmt.Statement))
						return false;

					var elseStmt = ifStmt.Else?.Statement;
					if (elseStmt == null)
						return true;

					if (elseStmt is IfStatementSyntax elseifStmt)
						ifStmt = elseifStmt;
					else
						return CanStripSurroundingBraces(elseStmt);
				}
			}

			private static bool CanStripSurroundingBraces(StatementSyntax node) {
				switch (node) {
					case BlockSyntax block:
						return block.Statements.Count == 1 &&
							block.GetLeadingTrivia().All(IsWhitespaceTrivia) &&
							block.GetTrailingTrivia().All(IsWhitespaceTrivia) &&
							CanStripSurroundingBraces(block.Statements[0]);
					case IfStatementSyntax _:  // removing braces around if statements can change semantics
					case LocalDeclarationStatementSyntax _:  // removing braces around variable declarations is invalid
						return false;
					default:
						return !SpansMultipleLines(node);
				}
			}

			private static IfStatementSyntax RemoveClauseBraces(IfStatementSyntax node) {
				if (node.Statement is BlockSyntax block)
					node = node
						.WithStatement(RemoveBraces(block))
						.WithCloseParenToken(EnsureEndsLine(node.CloseParenToken));

				if (node.Else is ElseClauseSyntax elseClause) {
					if (elseClause.Statement is IfStatementSyntax elseif)
						elseClause = elseClause
							.WithStatement(RemoveClauseBraces(elseif));
					else if (elseClause.Statement is BlockSyntax elseBlock)
						elseClause = elseClause
							.WithStatement(RemoveBraces(elseBlock))
							.WithElseKeyword(EnsureEndsLine(elseClause.ElseKeyword));

					node = node.WithElse(elseClause);
				}

				return node;
			}

			private static SyntaxToken EnsureEndsLine(SyntaxToken token) => token.WithTrailingTrivia(EnsureEndsLine(token.TrailingTrivia));
			private static StatementSyntax EnsureEndsLine(StatementSyntax node) => node.WithTrailingTrivia(EnsureEndsLine(node.GetTrailingTrivia()));

			private static SyntaxTriviaList EnsureEndsLine(SyntaxTriviaList trivia) =>
				trivia.LastOrDefault().IsKind(SyntaxKind.EndOfLineTrivia) ? trivia : trivia.Add(SyntaxFactory.EndOfLine(Environment.NewLine));

			private static StatementSyntax RemoveBraces(BlockSyntax block) => EnsureEndsLine(block.Statements[0]);

			private static bool IsWhitespaceTrivia(SyntaxTrivia trivia) => trivia.IsKind(SyntaxKind.WhitespaceTrivia) || trivia.IsKind(SyntaxKind.EndOfLineTrivia);
		}

		private class AddVisualNewlinesRewriter : CSharpSyntaxRewriter
		{
			public override SyntaxNode VisitBlock(BlockSyntax node) {
				node = node.ReplaceNodes(StatementsRequiringNewlines(node.Statements), (_, n) => EnsureStartsWithBlankLine(n));
				return base.VisitBlock(node);
			}

			private SyntaxNode EnsureStartsWithBlankLine(StatementSyntax n) {
				var trivia = n.GetLeadingTrivia();
				if (trivia.FirstOrDefault().IsKind(SyntaxKind.EndOfLineTrivia))
					return n;

				return n.WithLeadingTrivia(trivia.Insert(0, SyntaxFactory.EndOfLine(Environment.NewLine)));
			}

			private static IEnumerable<StatementSyntax> StatementsRequiringNewlines(SyntaxList<StatementSyntax> statements) {
				for (int i = 0; i < statements.Count - 1; i++) {
					if (SpansMultipleLines(statements[i]))
						yield return statements[i + 1];
				}
			}
		}
	}
}
