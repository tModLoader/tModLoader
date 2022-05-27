using Microsoft.CodeAnalysis;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace tModPorter.Rewriters;

public static class Extensions
{
	public static T WithTriviaFrom<T>(this T node, SyntaxNode other) where T : SyntaxNode =>
		node.WithLeadingTrivia(other.GetLeadingTrivia()).WithTrailingTrivia(other.GetTrailingTrivia());

	public static SyntaxToken WithTriviaFrom(this SyntaxToken node, SyntaxToken other) =>
		node.WithLeadingTrivia(other.LeadingTrivia).WithTrailingTrivia(other.TrailingTrivia);

	public static SyntaxToken WithText(this SyntaxToken token, string text) => Identifier(text).WithTriviaFrom(token);

	public static bool InheritsFrom(this ITypeSymbol type, string fromTypeName) =>
		type.ToString() == fromTypeName ||
		type.BaseType != null && InheritsFrom(type.BaseType, fromTypeName) ||
		type.Interfaces.Any(i => InheritsFrom(i, fromTypeName));

}

