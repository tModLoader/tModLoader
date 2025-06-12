using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Terraria.ModLoader.Setup.Core.Formatting
{
	class RemoveBracesFromSingleStatementRewriter : CSharpSyntaxRewriter
	{
		private readonly SyntaxAnnotation processedAnnotation = new();

		public override SyntaxNode? VisitIfStatement(IfStatementSyntax node) {
			if (node.HasAnnotation(processedAnnotation))
				return base.VisitIfStatement(node);

			if (NoClausesNeedBraces(node))
				node = RemoveClauseBraces(node); //TODO: trailing space

			node = AnnotateTreeProcessed(node);
			return base.VisitIfStatement(node);
		}

		private IfStatementSyntax AnnotateTreeProcessed(IfStatementSyntax node) {
			if (node.Else?.Statement is IfStatementSyntax elseIfStmt)
				node = node.WithElse(node.Else.WithStatement(AnnotateTreeProcessed(elseIfStmt)));

			return node.WithAdditionalAnnotations(processedAnnotation);
		}

		private bool NoClausesNeedBraces(IfStatementSyntax ifStmt) {
			// lets not destroy the stack
			while (true) {
				if (!StatementIsSingleLine(ifStmt.Statement))
					return false;

				var elseStmt = ifStmt.Else?.Statement;
				if (elseStmt == null)
					return true;

				if (elseStmt is IfStatementSyntax elseifStmt)
					ifStmt = elseifStmt;
				else
					return StatementIsSingleLine(elseStmt);
			}
		}

		private bool StatementIsSingleLine(StatementSyntax node) {
			switch (node) {
				case BlockSyntax block:
					return block.Statements.Count == 1 &&
						block.GetLeadingTrivia().All(SyntaxUtils.IsWhitespace) &&
						block.GetTrailingTrivia().All(SyntaxUtils.IsWhitespace) &&
						StatementIsSingleLine(block.Statements[0]);
				case IfStatementSyntax _:
					return false; // removing braces around if statements can change semantics
				case LabeledStatementSyntax _:
				case LocalDeclarationStatementSyntax _:
					return false; // single line statements cannot be labelled or contain declarations
				default:
					return node.SpansSingleLine();
			}
		}

		private IfStatementSyntax RemoveClauseBraces(IfStatementSyntax node) {
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

		private SyntaxToken EnsureEndsLine(SyntaxToken token) => token.WithTrailingTrivia(EnsureEndsLine(token.TrailingTrivia));
		private StatementSyntax EnsureEndsLine(StatementSyntax node) => node.WithTrailingTrivia(EnsureEndsLine(node.GetTrailingTrivia()));

		private SyntaxTriviaList EnsureEndsLine(SyntaxTriviaList trivia) =>
			trivia.LastOrDefault().IsKind(SyntaxKind.EndOfLineTrivia) ? trivia : trivia.Add(SyntaxFactory.EndOfLine(Environment.NewLine));

		private StatementSyntax RemoveBraces(BlockSyntax block) => EnsureEndsLine(block.Statements[0]);

		public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax method) {
			if (method.Body != null &&
				StatementIsSingleLine(method.Body) &&
				method.Body.DescendantTrivia().All(SyntaxUtils.IsWhitespace) &&
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
	}
}
