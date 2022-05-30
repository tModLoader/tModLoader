using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace tModPorter.Rewriters.MemberAccessRewriters;

public class DamageModifiersRewriter : BaseRewriter {
	private static readonly Dictionary<string, string> IdentifierMap = new() {
		{"magicDamage", "DamageClass.Magic"},
		{"magicCrit", "DamageClass.Magic"},
		{"meleeDamage", "DamageClass.Melee"},
		{"meleeCrit", "DamageClass.Melee"},
		{"rangedDamage", "DamageClass.Ranged"},
		{"rangedCrit", "DamageClass.Ranged"},
		{"minionDamage", "DamageClass.Summon"},
		{"thrownDamage", "DamageClass.Throwing"},
		{"thrownCrit", "DamageClass.Throwing"},
	};

	public DamageModifiersRewriter(SemanticModel model, List<string> usingList,
		HashSet<(BaseRewriter rewriter, SyntaxNode originalNode)> nodesToRewrite,
		HashSet<(BaseRewriter rewriter, SyntaxToken originalToken)> tokensToRewrite)
		: base(model, usingList, nodesToRewrite, tokensToRewrite) { }

	public sealed override RewriterType RewriterType => RewriterType.MemberAccess;

	public sealed override void VisitNode(SyntaxNode node) {
		if (node is not MemberAccessExpressionSyntax nodeSyntax)
			return;

		if (IdentifierMap.ContainsKey(nodeSyntax.Name.ToString()) && !HasSymbol(nodeSyntax, out _))
			AddNodeToRewrite(nodeSyntax.Name);
	}

	public override SyntaxNode RewriteNode(SyntaxNode node) {
		if (node is not IdentifierNameSyntax nodeSyntax) return node;

		string newModifier = IdentifierMap[node.ToString()];

		SyntaxNode newNode =
			IdentifierName(node.ToString().Contains("Damage") ? $"GetDamage({newModifier})" : $"GetCritChance({newModifier})");

		newNode = newNode.WithTriviaFrom(nodeSyntax);

		return newNode;
	}
}