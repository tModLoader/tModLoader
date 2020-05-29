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
		private HashSet<SyntaxNode> modifyNodes = new HashSet<SyntaxNode>();
		public override SyntaxNode VisitBlock(BlockSyntax node) {

			var stmts = node.Statements;
			for (int i = 0; i < stmts.Count - 1; i++) {
				var prev = stmts[i];
				var next = stmts[i + 1];
				if (!prev.SpansSingleLine() && !next.GetLeadingTrivia().FirstOrDefault().IsKind(SyntaxKind.EndOfLineTrivia))
					modifyNodes.Add(next);
			}

			return base.VisitBlock(node);
		}

		public override SyntaxNode Visit(SyntaxNode node) {
			if (modifyNodes.Remove(node))
				node = node.WithLeadingTrivia(node.GetLeadingTrivia().Insert(0, SyntaxFactory.EndOfLine(Environment.NewLine)));

			return base.Visit(node);
		}
	}
}
