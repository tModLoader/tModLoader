using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.ModLoader.Setup.Formatting
{
	public class AddVisualNewlinesRewriter : CSharpSyntaxRewriter
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

		private static bool SpansMultipleLines(StatementSyntax node) {
			var lineSpan = node.SyntaxTree.GetLineSpan(node.Span);
			return lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line;
		}

		private static IEnumerable<StatementSyntax> StatementsRequiringNewlines(SyntaxList<StatementSyntax> statements) {
			for (int i = 0; i < statements.Count - 1; i++) {
				if (SpansMultipleLines(statements[i]))
					yield return statements[i + 1];
			}
		}
	}
}
