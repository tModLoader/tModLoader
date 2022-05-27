using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace tModPorter.Rewriters;

public class GenerationUsingRewriter : BaseRewriter {
	public GenerationUsingRewriter(SemanticModel model, List<string> usingList,
		HashSet<(BaseRewriter rewriter, SyntaxNode originalNode)> nodesToRewrite,
		HashSet<(BaseRewriter rewriter, SyntaxToken originalToken)> tokensToRewrite)
		: base(model, usingList, nodesToRewrite, tokensToRewrite) { }

	public override RewriterType RewriterType => RewriterType.UsingDirective;

	public override void VisitNode(SyntaxNode node) {
		if (node is not UsingDirectiveSyntax nodeSyntax)
			return;

		if (nodeSyntax.Name.ToString() == "Terraria.World.Generation")
			AddNodeToRewrite(nodeSyntax.Name);
	}

	public override SyntaxNode RewriteNode(SyntaxNode node) => IdentifierName("Terraria.WorldBuilding");
}