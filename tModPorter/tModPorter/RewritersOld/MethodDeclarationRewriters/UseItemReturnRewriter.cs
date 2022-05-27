using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace tModPorter.Rewriters.MethodDeclarationRewriters;

public class UseItemReturnRewriter : BaseRewriter {
	public UseItemReturnRewriter(SemanticModel model, List<string> usingList,
		HashSet<(BaseRewriter rewriter, SyntaxNode originalNode)> nodesToRewrite,
		HashSet<(BaseRewriter rewriter, SyntaxToken originalToken)> tokensToRewrite)
		: base(model, usingList, nodesToRewrite, tokensToRewrite) { }

	public override RewriterType RewriterType => RewriterType.Method;

	public override void VisitNode(SyntaxNode node) {
		if (node is not MethodDeclarationSyntax mDec) return;

		// Check if the return type of the method is nullable, or not a boolean
		if (mDec.ReturnType is NullableTypeSyntax) return;
		if (mDec.ReturnType is not PredefinedTypeSyntax preType || preType.Keyword.Kind() != SyntaxKind.BoolKeyword) return;

		if (mDec.Identifier.ToString() != "UseItem") return;

		AddNodeToRewrite(node);
	}

	public override SyntaxNode RewriteNode(SyntaxNode node) {
		if (node is not MethodDeclarationSyntax mDec) return node;

		NullableTypeSyntax newReturnType = NullableType(PredefinedType(Token(SyntaxKind.BoolKeyword)));
		SyntaxNode newNode = mDec.WithReturnType(newReturnType.WithTriviaFrom(mDec.ReturnType));
		return newNode;
	}
}