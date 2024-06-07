using Microsoft.CodeAnalysis;

namespace tModLoader.Analyzers;

public static class WellKnownTypes
{
	private const string tModLoaderAssemblyName = "tModLoader";

	private const string IdTypeAttribute1TypeName = "IDTypeAttribute`1";

	public static bool IsIdTypeAttribute(ISymbol symbol)
	{
		return symbol is { ContainingAssembly.Name: tModLoaderAssemblyName, MetadataName: IdTypeAttribute1TypeName };
	}
}
