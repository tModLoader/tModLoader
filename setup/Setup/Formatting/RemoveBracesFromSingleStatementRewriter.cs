using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Terraria.ModLoader.Setup.Formatting
{
	public class RemoveBracesFromSingleStatementRewriter : CSharpSyntaxRewriter
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

		private static bool SpansMultipleLines(StatementSyntax node) {
			var lineSpan = node.SyntaxTree.GetLineSpan(node.Span);
			return lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line;
		}

		private static SyntaxToken EnsureEndsLine(SyntaxToken token) => token.WithTrailingTrivia(EnsureEndsLine(token.TrailingTrivia));
		private static StatementSyntax EnsureEndsLine(StatementSyntax node) => node.WithTrailingTrivia(EnsureEndsLine(node.GetTrailingTrivia()));

		private static SyntaxTriviaList EnsureEndsLine(SyntaxTriviaList trivia) =>
			trivia.LastOrDefault().IsKind(SyntaxKind.EndOfLineTrivia) ? trivia : trivia.Add(SyntaxFactory.EndOfLine(Environment.NewLine));

		private static StatementSyntax RemoveBraces(BlockSyntax block) => EnsureEndsLine(block.Statements[0]);

		private static bool IsWhitespaceTrivia(SyntaxTrivia trivia) => trivia.IsKind(SyntaxKind.WhitespaceTrivia) || trivia.IsKind(SyntaxKind.EndOfLineTrivia);
	}
}
