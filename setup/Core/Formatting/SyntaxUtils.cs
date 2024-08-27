using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Terraria.ModLoader.Setup.Core.Formatting
{
	public static class SyntaxUtils
	{
		public static bool SpansSingleLine(this SyntaxNode node) =>
			!node.DescendantTrivia(node.Span).Any(t => t.IsKind(SyntaxKind.EndOfLineTrivia));

		public static bool IsWhitespace(this SyntaxTrivia trivia) =>
			trivia.IsKind(SyntaxKind.WhitespaceTrivia) || trivia.IsKind(SyntaxKind.EndOfLineTrivia);


		public static string GetIndentation(this SyntaxNode node) => GetIndentation(node.SyntaxTree, node.SpanStart);
		public static string GetIndentation(this SyntaxToken node) => GetIndentation(node.SyntaxTree, node.SpanStart);

		public static string GetIndentation(SyntaxTree? tree, int position)
		{
			int? start = tree?.GetText().Lines.GetLineFromPosition(position).Start;
			SyntaxTrivia? whitespace = tree?.GetRoot().FindTrivia(start!.Value, true);
			return whitespace?.ToFullString() ?? string.Empty;
		}

		public static T WithLeadingWhitespace<T>(this T node, string indent) where T : SyntaxNode =>
			(T)WithLeadingWhitespace((SyntaxNodeOrToken)node, indent)!;

		public static SyntaxToken WithLeadingWhitespace(this SyntaxToken node, string indent) =>
			WithLeadingWhitespace((SyntaxNodeOrToken)node, indent).AsToken();

		public static SyntaxNodeOrToken WithLeadingWhitespace(this SyntaxNodeOrToken node, string indent)
		{
			SyntaxTriviaList t = node.GetLeadingTrivia();
			SyntaxTrivia last = t.Last();
			if (!last.IsKind(SyntaxKind.WhitespaceTrivia)) {
				t = t.Add(SyntaxFactory.Whitespace(indent));
			}
			else if (last.ToFullString() == indent) {
				return node;
			}
			else {
				t = t.Replace(last, SyntaxFactory.Whitespace(indent));
			}

			return node.WithLeadingTrivia(t);
		}
	}
}