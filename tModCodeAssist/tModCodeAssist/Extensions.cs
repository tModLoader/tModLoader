using Microsoft.CodeAnalysis;

namespace tModCodeAssist;

public static class Extensions
{
	public static string ToErrorFormatString(this ISymbol symbol)
	{
		return symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
	}

	public static string ToFullyQualifiedString(this ISymbol symbol)
	{
		return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
	}

	public static bool IsSameAsFullyQualifiedString(this ISymbol symbol, string other)
	{
		return symbol.ToFullyQualifiedString().Equals(other);
	}
}
