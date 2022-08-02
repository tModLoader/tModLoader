using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static tModPorter.Rewriters.SimpleSyntaxFactory;

namespace tModPorter.Rewriters;

public static class Extensions
{
	public static T WithTriviaFrom<T>(this T node, SyntaxNode other) where T : SyntaxNode =>
		node.WithLeadingTrivia(other.GetLeadingTrivia()).WithTrailingTrivia(other.GetTrailingTrivia());

	public static SyntaxTokenList WithTriviaFrom(this SyntaxTokenList node, SyntaxTokenList other) {
		if (!node.Any() || !other.Any())
			return node;

		if (other.First().HasLeadingTrivia)
			node = node.Replace(node.First(), node.First().WithLeadingTrivia(other.First().LeadingTrivia));

		if (other.Last().HasTrailingTrivia)
			node = node.Replace(node.Last(), node.Last().WithTrailingTrivia(other.Last().TrailingTrivia));

		return node;
	}

	public static T TrimTrailingSpace<T>(this T node) where T : SyntaxNode {
		var trivia = node.GetTrailingTrivia();
		return trivia.Count == 1 && trivia[0].IsKind(SyntaxKind.WhitespaceTrivia) ? node.WithoutTrailingTrivia() : node;
	}

	public static SyntaxToken WithText(this SyntaxToken token, string text) => text == token.Text ? token : Identifier(text).WithTriviaFrom(token);

	public static IdentifierNameSyntax WithIdentifier(this IdentifierNameSyntax name, string text) => text == name.Identifier.Text ? name : name.WithIdentifier(name.Identifier.WithText(text));

	public static T WithBlockComment<T>(this T node, string comment) where T : SyntaxNode {
		if (comment == null)
			return node;

		var trivia = node.GetTrailingTrivia();
		if (trivia.Any(t => t.IsKind(SyntaxKind.MultiLineCommentTrivia) && t.ToString().Contains("tModPorter")))
			return node;

		return node.WithTrailingTrivia(trivia.Insert(0, Comment($"/* tModPorter {comment} */")));
	}

	public static SyntaxToken WithBlockComment(this SyntaxToken node, string comment) {
		if (comment == null)
			return node;

		var trivia = node.TrailingTrivia;
		if (trivia.Any(t => t.IsKind(SyntaxKind.MultiLineCommentTrivia) && t.ToString().Contains("tModPorter")))
			return node;

		return node.WithTrailingTrivia(trivia.Insert(0, Comment($"/* tModPorter {comment} */")));
	}

	public static T WithTrailingCommentsFrom<T>(this T node, SyntaxNode other) where T : SyntaxNode {
		var comments = other.GetTrailingTrivia().Where(t => !string.IsNullOrWhiteSpace(t.ToString())).ToImmutableArray();
		if (comments.Length == 0)
			return node;

		var existing = node.GetTrailingTrivia();
		int i = existing.Count;
		while (i > 0 && existing[i - 1].IsKind(SyntaxKind.EndOfLineTrivia))
			i--;

		return node.WithTrailingTrivia(existing.InsertRange(i, comments));
	}

	public static bool Contains(this SyntaxList<UsingDirectiveSyntax> usings, string @namespace) => usings.Any(u => u.Name.ToString() == @namespace);

	public static SyntaxList<UsingDirectiveSyntax> WithUsingNamespace(this SyntaxList<UsingDirectiveSyntax> usings, string @namespace) {
		if (usings.Contains(@namespace))
			return usings;

		int idx = 0;
		while (idx < usings.Count && string.Compare(usings[idx].Name.ToString(), @namespace) < 0)
			idx++;

		return usings.Insert(idx, SimpleUsing(@namespace));
	}

	public static bool InheritsFrom(this ITypeSymbol type, string fromTypeName) =>
		type.ToString() == fromTypeName ||
		type.BaseType != null && InheritsFrom(type.BaseType, fromTypeName) ||
		type.Interfaces.Any(i => InheritsFrom(i, fromTypeName));

	public static T LookupMember<T>(this ITypeSymbol type, string name, bool includeObsolete = false) where T : class, ISymbol {
		var members = type.GetMembers(name).OfType<T>().Where(m => includeObsolete || !m.IsObsolete()).ToArray();
		if (members.Length == 1)
			return members[0];

		if (members.Length > 1 || type.BaseType == null)
			return null;

		return LookupMember<T>(type.BaseType, name);
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

	public static bool NonDefault(this SemanticModel model, ExpressionSyntax x, object defaultValue) => x != null && (dynamic)model.GetOperation(x).ConstantValue.Value != (dynamic)defaultValue;

	public static bool IsObsolete(this ISymbol sym) => sym.GetAttributes().Any(a => a.AttributeClass?.Name == "ObsoleteAttribute");
}

