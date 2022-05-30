using Microsoft.CodeAnalysis;

namespace tModPorter.Rewriters;

internal static class RewriterUtilities {
	public static T WithExtraTrivia<T>(this T nodeToAddTrivia, SyntaxNode nodeWithOldTrivia) where T : SyntaxNode =>
		nodeToAddTrivia.WithLeadingTrivia(nodeWithOldTrivia.GetLeadingTrivia())
			.WithTrailingTrivia(nodeWithOldTrivia.GetTrailingTrivia());
}