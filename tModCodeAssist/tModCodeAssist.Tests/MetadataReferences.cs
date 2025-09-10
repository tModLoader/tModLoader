using System;
using System.IO;
using Microsoft.CodeAnalysis;

namespace tModCodeAssist.Tests;

public static class MetadataReferences
{
	public static MetadataReference TmlReference => _lazyTml.Value;
	private static readonly Lazy<MetadataReference> _lazyTml = new Lazy<MetadataReference>(() => {
		string assemblyPath = typeof(Terraria.ModLoader.Mod).Assembly.Location;
		string documentationPath = Path.ChangeExtension(assemblyPath, ".xml");

		var documentation = XmlDocumentationProvider.CreateFromFile(documentationPath);
		var reference = MetadataReference.CreateFromFile(assemblyPath, documentation: documentation);

		return reference;
	});

	public static MetadataReference FnaReference => _lazyFna.Value;
	private static readonly Lazy<MetadataReference> _lazyFna = new Lazy<MetadataReference>(() => {
		string assemblyPath = typeof(Microsoft.Xna.Framework.Vector2).Assembly.Location;
		string documentationPath = Path.ChangeExtension(assemblyPath, ".xml");

		var documentation = XmlDocumentationProvider.CreateFromFile(documentationPath);
		var reference = MetadataReference.CreateFromFile(assemblyPath, documentation: documentation);

		return reference;
	});

	public static MetadataReference ReLogicReference => _lazyReLogic.Value;
	private static readonly Lazy<MetadataReference> _lazyReLogic = new Lazy<MetadataReference>(() => {
		string assemblyPath = typeof(ReLogic.Content.Asset<>).Assembly.Location;
		string documentationPath = Path.ChangeExtension(assemblyPath, ".xml");

		var documentation = XmlDocumentationProvider.CreateFromFile(documentationPath);
		var reference = MetadataReference.CreateFromFile(assemblyPath, documentation: documentation);

		return reference;
	});
}
