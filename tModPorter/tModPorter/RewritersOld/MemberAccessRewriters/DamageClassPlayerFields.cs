using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace tModPorter.Rewriters.MemberAccessRewriters; 

// https://github.com/tModLoader/tModLoader/pull/2196
public class DamageClassPlayerFields : BaseRewriter {
	private static readonly Dictionary<string, string> Map = new() {
		// ReSharper disable once StringLiteralTypo
		{"minionKB", "GetKnockback(DamageClass.Summon).Base"},
		{"armorPenetration", "GetArmorPenetration(DamageClass.Generic)"},
		{"meleeSpeed", "GetAttackSpeed(DamageClass.Melee)"},
		{"whipUseTimeMultiplier", "GetAttackSpeed(DamageClass.SummonMeleeSpeed)"},
	};

	// Implement base constructor
	public DamageClassPlayerFields(SemanticModel model, List<string> usingList,
		HashSet<(BaseRewriter rewriter, SyntaxNode originalNode)> nodesToRewrite,
		HashSet<(BaseRewriter rewriter, SyntaxToken originalToken)> tokensToRewrite)
		: base(model, usingList, nodesToRewrite, tokensToRewrite) { }

	public override RewriterType RewriterType => RewriterType.MemberAccess;

	public override void VisitNode(SyntaxNode node) {
		if (node is not MemberAccessExpressionSyntax memberAccess) return;
		
		// expression.name
		if (!Map.ContainsKey(memberAccess.Name.ToString())) return;

		if (!HasSymbol(memberAccess.Expression, out _)) {
			RequestRewriteRepeat();
			return;
		}

		if (GetTypeName(memberAccess.Expression) != "Player") return;
		
		AddNodeToRewrite(memberAccess);
	}

	public override SyntaxNode RewriteNode(SyntaxNode node) {
		MemberAccessExpressionSyntax memberAccess = (MemberAccessExpressionSyntax) node;

		if (!Map.TryGetValue(memberAccess.Name.ToString(), out string newExpression)) return node;
		
		// Joins "Player." and the new expression
		return ParseExpression(memberAccess.Expression + "." + newExpression).WithTriviaFrom(memberAccess);
	}
}