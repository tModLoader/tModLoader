using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

	public static SyntaxToken WithBlockComment(this SyntaxToken token, string comment) => token.WithTrailingTrivia(token.TrailingTrivia.Insert(0, Comment($"/* {comment} */")));

	public static T WithBlockComment<T>(this T node, string comment) where T : SyntaxNode => node.WithTrailingTrivia(node.GetTrailingTrivia().Insert(0, Comment($"/* {comment} */")));

	public static bool InheritsFrom(this ITypeSymbol type, string fromTypeName) =>
		type.ToString() == fromTypeName ||
		type.BaseType != null && InheritsFrom(type.BaseType, fromTypeName) ||
		type.Interfaces.Any(i => InheritsFrom(i, fromTypeName));

	public static IMethodSymbol LookupMethod(this ITypeSymbol type, string name) {
		var members = type.GetMembers(name).OfType<IMethodSymbol>().ToArray();
		if (members.Length == 1)
			return members[0];

		if (members.Length > 1 || type.BaseType == null)
			return null;

		return LookupMethod(type.BaseType, name);
	}

	public static SyntaxKind SpecialTypeKind(this SpecialType t) => t switch {
		SpecialType.System_Void => SyntaxKind.VoidKeyword,
		SpecialType.System_SByte => SyntaxKind.SByteKeyword,
		SpecialType.System_Int16 => SyntaxKind.ShortKeyword,
		SpecialType.System_Int32 => SyntaxKind.IntKeyword,
		SpecialType.System_Int64 => SyntaxKind.LongKeyword,
		SpecialType.System_Byte => SyntaxKind.ByteKeyword,
		SpecialType.System_UInt16 => SyntaxKind.UShortKeyword,
		SpecialType.System_UInt32 => SyntaxKind.UIntKeyword,
		SpecialType.System_UInt64 => SyntaxKind.ULongKeyword,
		SpecialType.System_Single => SyntaxKind.FloatKeyword,
		SpecialType.System_Double => SyntaxKind.DoubleKeyword,
		SpecialType.System_Decimal => SyntaxKind.DecimalKeyword,
		SpecialType.System_Char => SyntaxKind.CharKeyword,
		SpecialType.System_Boolean => SyntaxKind.BoolKeyword,
		SpecialType.System_String => SyntaxKind.StringKeyword,
		SpecialType.System_Object => SyntaxKind.ObjectKeyword,
		_ => SyntaxKind.None
	};
}

