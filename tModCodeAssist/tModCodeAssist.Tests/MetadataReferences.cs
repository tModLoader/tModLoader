using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace tModCodeAssist.Tests;

public static class MetadataReferences
{
	private static readonly Lazy<ReferenceAssemblies> _lazyNet80 = new Lazy<ReferenceAssemblies>(delegate {
		return new ReferenceAssemblies("net8.0", new PackageIdentity("Microsoft.NETCore.App.Ref", "8.0.3"), Path.Combine("ref", "net8.0"));
	});

	private static readonly Lazy<MetadataReference> _lazyTml = new Lazy<MetadataReference>(delegate {
		string assemblyPath = typeof(Terraria.ModLoader.Mod).Assembly.Location;
		string documentationPath = Path.ChangeExtension(assemblyPath, ".xml");
		var documentation = XmlDocumentationProvider.CreateFromFile(documentationPath);
		var reference = MetadataReference.CreateFromFile(assemblyPath, documentation: documentation);

		return reference;
	});

	public static ReferenceAssemblies Net80 => _lazyNet80.Value;

	public static MetadataReference TmlReference => _lazyTml.Value;
}
