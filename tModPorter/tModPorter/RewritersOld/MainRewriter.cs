using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace tModPorter.Rewriters;

public class MainRewriter : CSharpSyntaxRewriter {
	private readonly HashSet<(BaseRewriter rewriter, SyntaxNode originalNode)> _nodesToRewrite = new();
	private readonly HashSet<(BaseRewriter rewriter, SyntaxToken originalToken)> _tokensToRewrite = new();
	private ILookup<RewriterType, BaseRewriter> _rewriterLookup;
	private readonly HashSet<BaseRewriter> _rewritersToRepeat = new();
	private readonly List<string> _usingList = new();
	private SemanticModel _model;
	private Document _document;

	public MainRewriter(Document document, SemanticModel model) {
		_model = model;
		_document = document;

		Type baseType = typeof(BaseRewriter);
		IEnumerable<Type> types = baseType.Assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract);

		List<BaseRewriter> rewriters = types
			.Select(type => (BaseRewriter) Activator.CreateInstance(type, _model, _usingList, _nodesToRewrite, _tokensToRewrite))
			.ToList();
		_rewriterLookup = rewriters.ToLookup(r => r.RewriterType);
	}

	public SyntaxNode RewriteNodes(SyntaxNode rootNode) {
		SyntaxNode RewriteAndReplaceNode(SyntaxNode syntaxNode) {
			Dictionary<SyntaxNode, SyntaxNode> nodeDictionary = new();
			foreach ((BaseRewriter rewriter, SyntaxNode originalNode) in _nodesToRewrite) {
				SyntaxNode newNode = rewriter.RewriteNode(originalNode);
				nodeDictionary.Add(originalNode, newNode);
			}

			Dictionary<SyntaxToken, SyntaxToken> tokenDictionary = new();
			foreach ((BaseRewriter rewriter, SyntaxToken originalToken) in _tokensToRewrite) {
				SyntaxToken newToken = rewriter.RewriteToken(originalToken);
				tokenDictionary.Add(originalToken, newToken);
			}

			syntaxNode = syntaxNode.ReplaceSyntax(
				nodeDictionary.Keys.AsEnumerable(), (original, _) => nodeDictionary[original],
				tokenDictionary.Keys.AsEnumerable(), (original, _) => tokenDictionary[original],
				Array.Empty<SyntaxTrivia>(), (original, _) => original);
			return syntaxNode;
		}

		rootNode = RewriteAndReplaceNode(rootNode);

		_document = _document.WithSyntaxRoot(rootNode);
		_model = _document.GetSemanticModelAsync().Result;
		rootNode = _document.GetSyntaxRootAsync().Result;
		
		_nodesToRewrite.Clear();
		_tokensToRewrite.Clear();

		foreach (BaseRewriter rewriter in _rewritersToRepeat) {
			rewriter.UpdateSemanticModel(_model);
		}

		_rewriterLookup = _rewritersToRepeat.ToLookup(r => r.RewriterType);
		Visit(rootNode);

		rootNode = RewriteAndReplaceNode(rootNode);

		return rootNode;
	}

	private SyntaxNode VisitRewriters(RewriterType type, SyntaxNode node) {
		// Uncomment this if you only want to port identifiers like "npc" or "item"
		//if (type != RewriterType.Identifier)
		//{
		//	finalNode = node;
		//	return true;
		//}

		foreach (BaseRewriter rewriter in _rewriterLookup[type]) {
			rewriter.VisitNode(node);
			if (rewriter.ShouldRepeatRewriter)
				_rewritersToRepeat.Add(rewriter);
		}

		return node;
	}

	public CompilationUnitSyntax AddUsingDirectives(CompilationUnitSyntax syntax) {
		List<UsingDirectiveSyntax> usingDirectives = new();
		foreach (string usingName in _usingList.Where(us => !syntax.Usings.Select(oldUsing => oldUsing.Name.ToString()).Contains(us))) {
			usingDirectives.Add(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(" " + usingName))
				.WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed));
		}
		return syntax.AddUsings(usingDirectives.ToArray());
	}

	#region Visit Nodes

	public override SyntaxNode VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node) =>
		base.VisitAnonymousMethodExpression((AnonymousMethodExpressionSyntax) VisitRewriters(RewriterType.AnonymousMethod, node));

	public override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node) =>
		base.VisitAssignmentExpression((AssignmentExpressionSyntax) VisitRewriters(RewriterType.Assignment, node));

	public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node) =>
		base.VisitIdentifierName((IdentifierNameSyntax) VisitRewriters(RewriterType.Identifier, node));

	public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node) =>
		base.VisitInvocationExpression((InvocationExpressionSyntax) VisitRewriters(RewriterType.Invocation, node));

	public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node) =>
		base.VisitMemberAccessExpression((MemberAccessExpressionSyntax) VisitRewriters(RewriterType.MemberAccess, node));

	public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node) =>
		base.VisitMethodDeclaration((MethodDeclarationSyntax) VisitRewriters(RewriterType.Method, node));

	public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node) =>
		base.VisitUsingDirective((UsingDirectiveSyntax) VisitRewriters(RewriterType.UsingDirective, node));

	#endregion
}