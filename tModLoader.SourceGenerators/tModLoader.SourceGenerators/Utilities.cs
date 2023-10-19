using Microsoft.CodeAnalysis;

namespace tModLoader.SourceGenerators;

internal static partial class Utilities
{
	public static bool IsPrimitiveType(this ITypeSymbol symbol) => (uint)(symbol.SpecialType - 7) <= 22 - 7;
}
