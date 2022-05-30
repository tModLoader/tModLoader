using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace tModPorter.Rewriters.MethodDeclarationRewriters; 

public class DangerSenseRewriter : BaseRewriter {
	public DangerSenseRewriter(SemanticModel model, List<string> usingList,
		HashSet<(BaseRewriter rewriter, SyntaxNode originalNode)> nodesToRewrite,
		HashSet<(BaseRewriter rewriter, SyntaxToken originalToken)> tokensToRewrite)
		: base(model, usingList, nodesToRewrite, tokensToRewrite) { }

	public override RewriterType RewriterType => RewriterType.Method;

	public override void VisitNode(SyntaxNode node) {
		// return if node is not MethodDeclarationSyntax
		if (node is not MethodDeclarationSyntax methodSyntax) return;
		
		// return if method identifier is not "Dangersense"
		if (methodSyntax.Identifier.Text != "Dangersense") return;
		
		// return if method return type is not bool
		if (methodSyntax.ReturnType.ToString() != "bool") return;

		// add method syntax identifier to tokens to rewrite
		AddTokenToRewrite(methodSyntax.Identifier);
		
		// add method return type to nodes tp rewrite
		AddNodeToRewrite(methodSyntax.ReturnType);
	}

	public override SyntaxNode RewriteNode(SyntaxNode node) {
		// if node is not TypeSyntax, return node
		if (node is not TypeSyntax typeSyntax) return node;

		ClassDeclarationSyntax? classAncestor = typeSyntax.FirstAncestorOrSelf<ClassDeclarationSyntax>();
		if (classAncestor is null || classAncestor.BaseList is null) return node;
		
		foreach (BaseTypeSyntax baseTypeSyntax in classAncestor.BaseList.Types) {
			if (baseTypeSyntax.ToString() == "GlobalTile") {
				typeSyntax = ParseTypeName("bool?");
				break;
			} else if (baseTypeSyntax.ToString() == "ModTile") {
				typeSyntax = ParseTypeName("bool");
				break;
			}
		}

		return typeSyntax.WithTriviaFrom(node);
	}

	public override SyntaxToken RewriteToken(SyntaxToken token) {
		// if token kind is not IdentifierToken, return token
		if (!token.IsKind(SyntaxKind.IdentifierToken)) return token;
		
		// create new SyntaxToken with "IsTileDangerous" text
		SyntaxToken newIdentifier = Identifier("IsTileDangerous");
		newIdentifier = newIdentifier.WithTriviaFrom(token);

		return newIdentifier;
	}
}