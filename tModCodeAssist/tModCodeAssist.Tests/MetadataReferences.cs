using System.IO;
using Microsoft.CodeAnalysis;

namespace tModCodeAssist.Tests;

public static class MetadataReferences
{
	public static readonly MetadataReference TmlReference = CreateTmlReference();

	private static MetadataReference CreateTmlReference()
	{
		string assemblyPath = typeof(Terraria.ModLoader.Mod).Assembly.Location;
		string documentationPath = Path.ChangeExtension(assemblyPath, ".xml");
		var documentation = XmlDocumentationProvider.CreateFromFile(documentationPath);
		var reference = MetadataReference.CreateFromFile(assemblyPath, documentation: documentation);

		return reference;
	}
}
