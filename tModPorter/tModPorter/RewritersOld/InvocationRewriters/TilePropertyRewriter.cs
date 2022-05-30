using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace tModPorter.Rewriters.InvocationRewriters;

public class TilePropertyRewriter : BaseRewriter {
	private static readonly Dictionary<string, string> IdentifierMap = new() {
		{"active", "HasTile"},
		{"nactive", "HasUnactuatedTile"},
		{"halfBrick", "IsHalfBlock"},
		{"inactive", "IsActuated"},
	};

	public TilePropertyRewriter(SemanticModel model, List<string> usingList,
		HashSet<(BaseRewriter rewriter, SyntaxNode originalNode)> nodesToRewrite,
		HashSet<(BaseRewriter rewriter, SyntaxToken originalToken)> tokensToRewrite)
		: base(model, usingList, nodesToRewrite, tokensToRewrite) { }

	public override RewriterType RewriterType => RewriterType.Invocation;

	public override void VisitNode(SyntaxNode node) {
		if (node is not InvocationExpressionSyntax invocationSyntax) return;
		
		if (invocationSyntax.Expression is not MemberAccessExpressionSyntax memberAccessSyntax) return;

		if (!IdentifierMap.ContainsKey(memberAccessSyntax.Name.ToString())) return;
		
		string typeName = GetTypeName(memberAccessSyntax.Expression);
		if (typeName != "Tilemap" && typeName != "Tile") return;
		
		AddNodeToRewrite(node);
	}

	public override SyntaxNode RewriteNode(SyntaxNode node) {
		InvocationExpressionSyntax invocationSyntax = (InvocationExpressionSyntax) node;
		MemberAccessExpressionSyntax memberAccessSyntax = (MemberAccessExpressionSyntax) invocationSyntax.Expression;

		IdentifierNameSyntax newName = IdentifierName(IdentifierMap[memberAccessSyntax.Name.ToString()]);
		newName = newName.WithTriviaFrom(memberAccessSyntax.Name).WithTrailingTrivia(invocationSyntax.ArgumentList.GetTrailingTrivia());

		return memberAccessSyntax.WithName(newName);
	}
}