using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace tModPorter.Rewriters.MemberAccessRewriters;

public class TextureExistsRewriter : BaseRewriter {
	public TextureExistsRewriter(SemanticModel model, List<string> usingList,
		HashSet<(BaseRewriter rewriter, SyntaxNode originalNode)> nodesToRewrite,
		HashSet<(BaseRewriter rewriter, SyntaxToken originalToken)> tokensToRewrite)
		: base(model, usingList, nodesToRewrite, tokensToRewrite) { }

	public override RewriterType RewriterType => RewriterType.MemberAccess;

	public override void VisitNode(SyntaxNode node) {
		// Visiting ModContent.TextureExists, only add TextureExists
		if (node is not MemberAccessExpressionSyntax access) return;

		if (access.Name.ToString() != "TextureExists" || access.Expression.ToString() != "ModContent") return;

		AddNodeToRewrite(access.Name);
	}

	public override SyntaxNode RewriteNode(SyntaxNode node) {
		// Change TextureExists to HasAsset
		if (node is not IdentifierNameSyntax) return node;

		SyntaxNode newNode = IdentifierName("HasAsset").WithTriviaFrom(node);

		return newNode;
	}
}