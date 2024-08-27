using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Terraria.ModLoader.Setup.Core.Formatting
{
	public class CollectionInitializerFormatter : CSharpSyntaxRewriter
	{
		private readonly IDictionary<SyntaxToken, SyntaxToken> changes = new Dictionary<SyntaxToken, SyntaxToken>();

		public override SyntaxNode? VisitInitializerExpression(InitializerExpressionSyntax node)
		{
			if (!node.DescendantTrivia().All(SyntaxUtils.IsWhitespace)) {
				return node; // nothing to do here
			}

			if (node.IsKind(SyntaxKind.ComplexElementInitializerExpression)) {
				node = VisitElementInitializer(node);
			}

			if (node.IsKind(SyntaxKind.CollectionInitializerExpression) ||
			    node.IsKind(SyntaxKind.ArrayInitializerExpression)) {
				FixIndentation(node);
			}

			return base.VisitInitializerExpression(node);
		}

		// the C# formatter sometimes just doesn't get these right...
		private void FixIndentation(InitializerExpressionSyntax node)
		{
			string indent = node.GetIndentation();

			SeparatedSyntaxList<ExpressionSyntax> exprList = node.Expressions;
			SyntaxNodeOrToken prevNode = node.OpenBraceToken;
			foreach (SyntaxNodeOrToken nodeOrToken in exprList.GetWithSeparators()) {
				if (nodeOrToken.IsNode && prevNode.GetTrailingTrivia().Any(SyntaxKind.EndOfLineTrivia)) {
					SyntaxToken tok = nodeOrToken.AsNode()!.GetFirstToken();
					AddChange(tok, tok.WithLeadingWhitespace(indent + '\t'));
				}

				prevNode = nodeOrToken;
			}

			if (prevNode.GetTrailingTrivia().Any(SyntaxKind.EndOfLineTrivia)) {
				AddChange(node.CloseBraceToken, node.CloseBraceToken.WithLeadingWhitespace(indent));
			}
		}

		private void AddChange(SyntaxToken node, SyntaxToken replacement)
		{
			if (node != replacement) {
				changes.Add(node, replacement);
			}
		}

		public override SyntaxToken VisitToken(SyntaxToken token)
		{
			if (changes.TryGetValue(token, out SyntaxToken replacement)) {
				token = replacement;
			}

			return base.VisitToken(token);
		}

		private InitializerExpressionSyntax VisitElementInitializer(InitializerExpressionSyntax node)
		{
			if (node.Expressions.Count != 2 || !node.Expressions.All(SyntaxUtils.SpansSingleLine)) {
				return node;
			}

			SeparatedSyntaxList<ExpressionSyntax> exprs = node.Expressions;
			exprs = SyntaxFactory.SeparatedList<ExpressionSyntax>(SyntaxFactory.NodeOrTokenList(
				exprs[0].WithoutTrivia(),
				exprs.GetSeparator(0).WithLeadingTrivia().WithTrailingTrivia(SyntaxFactory.Whitespace(" ")),
				exprs[1].WithLeadingTrivia().WithTrailingTrivia(SyntaxFactory.Whitespace(" "))
			));

			SyntaxToken closeBrace = node.CloseBraceToken.WithLeadingTrivia(SyntaxTriviaList.Empty);
			if (node != ((InitializerExpressionSyntax?)node.Parent)?.Expressions.Last()) {
				closeBrace = closeBrace.WithTrailingTrivia(SyntaxTriviaList.Empty);
			}

			return node
				.WithOpenBraceToken(node.OpenBraceToken.WithTrailingTrivia(SyntaxFactory.Whitespace(" ")))
				.WithExpressions(exprs)
				.WithCloseBraceToken(closeBrace);
		}
	}
}