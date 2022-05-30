using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace tModPorter.Rewriters.InvocationRewriters;

public class GetAssetRewriter : BaseRewriter {
	private Dictionary<string, (string type, string addUsing)> _methodToType = new() {
		{"GetTexture", ("Texture2D", "Microsoft.Xna.Framework.Graphics")},
		{"GetEffect", ("Effect", "Microsoft.Xna.Framework.Graphics")},
	};

	public GetAssetRewriter(SemanticModel model, List<string> usingList,
		HashSet<(BaseRewriter rewriter, SyntaxNode originalNode)> nodesToRewrite,
		HashSet<(BaseRewriter rewriter, SyntaxToken originalToken)> tokensToRewrite)
		: base(model, usingList, nodesToRewrite, tokensToRewrite) { }

	public override RewriterType RewriterType => RewriterType.Invocation;

	public override void VisitNode(SyntaxNode node) {
		if (node is not InvocationExpressionSyntax invocation) return;

		if (invocation.ArgumentList.Arguments.Count != 1) return;
		if (invocation.Expression is not MemberAccessExpressionSyntax member) return;
		if (!_methodToType.ContainsKey(member.Name.ToString())) return;

		AddNodeToRewrite(invocation);
	}

	public override SyntaxNode RewriteNode(SyntaxNode node) {
		if (node is not InvocationExpressionSyntax invocation) return node;
		MemberAccessExpressionSyntax accessExpression = (MemberAccessExpressionSyntax) invocation.Expression;

		(string type, string addUsing) = _methodToType[accessExpression.Name.ToString()];
		AddUsing(addUsing);

		TypeArgumentListSyntax typeArgList = TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(type)));
		GenericNameSyntax requestSyntax = GenericName("Request").WithTypeArgumentList(typeArgList);

		if (HasSymbol(accessExpression.Expression, out ISymbol symbol) && symbol.Name == "ModContent") {
			// do stuff for ModContent.Request<>
			requestSyntax = requestSyntax.WithTriviaFrom(accessExpression.Name);

			MemberAccessExpressionSyntax newAccessExpression =
				MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, accessExpression.Expression, requestSyntax)
					.WithTriviaFrom(accessExpression);

			SyntaxNode newModContentInvocation = invocation.WithExpression(newAccessExpression);
			return newModContentInvocation;
		}

		// turn 'mod' to 'Mod'
		if (!HasSymbol(accessExpression.Expression, out _) && accessExpression.Expression.ToString() == "mod")
			accessExpression = accessExpression.WithExpression(IdentifierName("Mod").WithTriviaFrom(accessExpression.Expression));


		// do stuff for Mod.Assets.Request<>()
		// Create Mod.Assets
		MemberAccessExpressionSyntax modAssetsSyntax =
			MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, accessExpression.Expression, IdentifierName("Assets"));

		// Create Mod.Assets.Request<>
		MemberAccessExpressionSyntax assetsRequestSyntax =
			MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, modAssetsSyntax, requestSyntax);

		// Create Mod.Assets.Request<>()
		InvocationExpressionSyntax requestInvocation = invocation.WithExpression(assetsRequestSyntax);

		// Create Mod.Assets.Request<>().Value
		MemberAccessExpressionSyntax assetValueAccess =
			MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, requestInvocation, IdentifierName("Value"));

		return assetValueAccess;
	}
}