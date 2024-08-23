using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Terraria.ModLoader.Setup.Core.Formatting;

public class AddVisualNewlinesRewriter : CSharpSyntaxRewriter
{
	private readonly HashSet<SyntaxNode> modifyNodes = new();

	public override SyntaxNode? VisitBlock(BlockSyntax node)
	{
		SyntaxList<StatementSyntax> stmts = node.Statements;
		for (int i = 0; i < stmts.Count - 1; i++) {
			StatementSyntax prev = stmts[i];
			StatementSyntax next = stmts[i + 1];
			if (!prev.SpansSingleLine() &&
			    !next.GetLeadingTrivia().FirstOrDefault().IsKind(SyntaxKind.EndOfLineTrivia)) {
				modifyNodes.Add(next);
			}
		}

		return base.VisitBlock(node);
	}

	public override SyntaxNode? Visit(SyntaxNode? node)
	{
		if (node != null && modifyNodes.Remove(node)) {
			node = node.WithLeadingTrivia(node.GetLeadingTrivia()
				.Insert(0, SyntaxFactory.EndOfLine(Environment.NewLine)));
		}

		return base.Visit(node);
	}
}