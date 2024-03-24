using System;
using System.IO;
using Microsoft.CodeAnalysis;

namespace tModLoader.Analyzers.Tests;

public static class MetadataReferences
{
	private static readonly Lazy<MetadataReference> _lazyTml = new Lazy<MetadataReference>(delegate {
		string assemblyPath = typeof(Terraria.ModLoader.Mod).Assembly.Location;
		string documentationPath = Path.ChangeExtension(assemblyPath, ".xml");
		var documentation = XmlDocumentationProvider.CreateFromFile(documentationPath);
		var reference = MetadataReference.CreateFromFile(assemblyPath, documentation: documentation);

		return reference;
	});

	public static MetadataReference TmlReference => _lazyTml.Value;
}
