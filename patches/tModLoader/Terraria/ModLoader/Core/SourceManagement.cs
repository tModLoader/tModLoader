using System.Collections.Generic;
using System.IO;
using System.Text;
using Stubble.Core;

#nullable enable

namespace Terraria.ModLoader.Core;

// Functions for creating and upgrading mod source code directories.
internal static class SourceManagement
{
	public record struct TemplateParameters
	{
		public string ModName;
		public string ModDisplayName;
		public string ModAuthor;
		public string ModVersion;
		public string ItemName;
		public string ItemDisplayName;

		public readonly bool IncludeItem => ItemName != string.Empty;
	}

	private const string TemplateResourcePrefix = $"Terraria/ModLoader/Templates/";
	private const string ModNameVar = "{{" + nameof(TemplateParameters.ModName) + "}}";

	private static readonly HashSet<string> textExtensions = new() {
		".txt", ".json", ".hjson", ".toml", ".cs", ".csproj", ".sln"
	};

	/// <summary> Writes mod template files to the provided source-code directory. </summary>
	public static void WriteModTemplate(string modSrcDirectory, in TemplateParameters templateParameters)
	{
		var modLoaderAssembly = typeof(ModLoader).Assembly;
		object boxedParameters = templateParameters;

		Directory.CreateDirectory(modSrcDirectory);

		foreach (string resourceKey in modLoaderAssembly.GetManifestResourceNames()) {
			TryWriteModTemplateFile(modSrcDirectory, resourceKey, boxedParameters);
		}
	}

	/// <summary> Writes a single mod template file to the provided source-code directory. </summary>
	public static bool TryWriteModTemplateFile(string modSrcDirectory, string resourceKey, object boxedParameters)
	{
		var assembly = typeof(ModLoader).Assembly;

		if (!resourceKey.StartsWith(TemplateResourcePrefix)) {
			return false;
		}

		string extension = Path.GetExtension(resourceKey);
		string relativePathTemplate = resourceKey[TemplateResourcePrefix.Length..];
		string relativePath = StaticStubbleRenderer.Render(relativePathTemplate, boxedParameters);
		string relativeDirectory = Path.GetDirectoryName(relativePath)!;

		// Files are skipped when their filenames render to nothing.
		if (string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(relativePath))) {
			return false;
		}

		byte[] data;
		using var resourceStream = assembly.GetManifestResourceStream(resourceKey)!;

		if (textExtensions.Contains(extension)) {
			using var streamReader = new StreamReader(resourceStream, leaveOpen: false);
			string contentsRaw = streamReader.ReadToEnd();
			string contents = StaticStubbleRenderer.Render(contentsRaw, boxedParameters);

			// Files are skipped when their templates render to nothing.
			if (string.IsNullOrWhiteSpace(contents)) {
				return false;
			}

			data = Encoding.UTF8.GetBytes(contents);
		} else {
			data = new BinaryReader(resourceStream).ReadBytes((int)resourceStream.Length);
		}

		Directory.CreateDirectory(Path.Combine(modSrcDirectory, relativeDirectory));
		File.WriteAllBytes(Path.Combine(modSrcDirectory, relativePath), data);

		return true;
	}

	/// <summary> Returns whether the provided source-code directory requires an upgrade. </summary>
	public static bool SourceUpgradeNeeded(string modSrcDirectory)
	{
		string modName = Path.GetFileName(modSrcDirectory);
		string csprojFile = Path.Combine(modSrcDirectory, $"{modName}.csproj");

		if (!File.Exists(csprojFile))
			return true;

		string csprojContents = File.ReadAllText(csprojFile);

		if (!csprojContents.Contains(@"..\tModLoader.targets"))
			return true;

		if (!csprojContents.Contains(@"<TargetFramework>net6.0</TargetFramework>"))
			return true;

		return false;
	}

	public static void UpgradeSource(string modSrcDirectory)
	{
		var properties = BuildProperties.ReadBuildFile(modSrcDirectory);

		TemplateParameters parameters;
		parameters.ModName = Path.GetFileName(modSrcDirectory);
		parameters.ModDisplayName = properties.displayName;
		parameters.ModAuthor = properties.author;
		parameters.ModVersion = properties.version.ToString();
		parameters.ItemName = string.Empty;
		parameters.ItemDisplayName = string.Empty;
		object boxedParameters = parameters;

		TryWriteModTemplateFile(modSrcDirectory, $"{TemplateResourcePrefix}{ModNameVar}.csproj", boxedParameters);
		TryWriteModTemplateFile(modSrcDirectory, $"{TemplateResourcePrefix}Properties/launchSettings.json", boxedParameters);

		static void DeleteIfExists(FileSystemInfo entry)
		{
			switch (entry) {
				case FileSystemInfo when !entry.Exists: return;
				case FileInfo file: file.Delete(); break;
				case DirectoryInfo directory: directory.Delete(recursive: true); break;
			}
		}

		try {
			// Old files can cause some issues.
			DeleteIfExists(new DirectoryInfo(Path.Combine(modSrcDirectory, "obj")));
			DeleteIfExists(new DirectoryInfo(Path.Combine(modSrcDirectory, "bin")));

			//TODO: Why do we do this?
			DeleteIfExists(new FileInfo(Path.Combine(modSrcDirectory, "Properties", "AssemblyInfo.cs")));
		}
		catch { }
	}

	private static Stream GetResourceStream(string key)
		=> typeof(ModLoader).Assembly.GetManifestResourceStream(key)!;
}
