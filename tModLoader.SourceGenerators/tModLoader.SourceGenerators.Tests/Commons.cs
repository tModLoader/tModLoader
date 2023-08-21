using Microsoft.CodeAnalysis;
using Terraria.ModLoader;

namespace tModLoader.SourceGenerators.Tests;

internal static class Commons {
	public static readonly MetadataReference TmlReference = CreateTmlReference();

	private static MetadataReference CreateTmlReference() {
		string assemblyPath = typeof(Mod).Assembly.Location;
		string documentationPath = Path.ChangeExtension(assemblyPath, ".xml");
		var documentation = XmlDocumentationProvider.CreateFromFile(documentationPath);
		var reference = MetadataReference.CreateFromFile(assemblyPath, documentation: documentation);

		return reference;
	}
}
