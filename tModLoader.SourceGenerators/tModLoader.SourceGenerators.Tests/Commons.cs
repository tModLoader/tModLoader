using log4net;
using Microsoft.CodeAnalysis;
using Terraria.ModLoader;

namespace tModLoader.SourceGenerators.Tests;

internal static class Commons
{
	public static readonly MetadataReference TmlReference = CreateTmlReference();
	public static readonly MetadataReference Log4netReference = CreateLog4netReference();
	public static readonly MetadataReference FnaReference = CreateFnaReference();

	private static MetadataReference CreateTmlReference()
	{
		string assemblyPath = typeof(Mod).Assembly.Location;
		string documentationPath = Path.ChangeExtension(assemblyPath, ".xml");
		var documentation = XmlDocumentationProvider.CreateFromFile(documentationPath);
		var reference = MetadataReference.CreateFromFile(assemblyPath, documentation: documentation);

		return reference;
	}

	private static MetadataReference CreateLog4netReference()
	{
		string assemblyPath = typeof(ILog).Assembly.Location;
		string documentationPath = Path.ChangeExtension(assemblyPath, ".xml");
		var documentation = XmlDocumentationProvider.CreateFromFile(documentationPath);
		var reference = MetadataReference.CreateFromFile(assemblyPath, documentation: documentation);

		return reference;
	}

	private static MetadataReference CreateFnaReference()
	{
		string assemblyPath = typeof(Microsoft.Xna.Framework.Vector2).Assembly.Location;
		string documentationPath = Path.ChangeExtension(assemblyPath, ".xml");
		var documentation = XmlDocumentationProvider.CreateFromFile(documentationPath);
		var reference = MetadataReference.CreateFromFile(assemblyPath, documentation: documentation);

		return reference;
	}
}
