using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
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

	/// <summary> Returns whether the provided mod source-code directory requires an upgrade. </summary>
	public static bool SourceUpgradeNeeded(string modSrcDirectory)
	{
		if (!File.Exists(Path.Combine(modSrcDirectory, "Properties", "launchSettings.json")))
			return true;

		if (CheckOrUpgradeCsprojFile(modSrcDirectory, checkOnly: true))
			return true;

		return false;
	}

	/// <summary> Runs upgrades on the provided mod source-code directory. </summary>
	public static bool UpgradeSource(string modSrcDirectory)
	{
		bool doneAnything = false;
		var parameters = ReadTemplateParameters(modSrcDirectory);
		object boxedParameters = parameters;

		doneAnything |= CheckOrUpgradeCsprojFile(modSrcDirectory, checkOnly: false, templateParameters: boxedParameters);
		doneAnything |= TryWriteModTemplateFile(modSrcDirectory, $"{TemplateResourcePrefix}Properties/launchSettings.json", boxedParameters);

		try {
			// Old files can cause some issues.
			DeleteIfExists(new DirectoryInfo(Path.Combine(modSrcDirectory, "obj")));
			DeleteIfExists(new DirectoryInfo(Path.Combine(modSrcDirectory, "bin")));

			//TODO: Why do we do this?
			DeleteIfExists(new FileInfo(Path.Combine(modSrcDirectory, "Properties", "AssemblyInfo.cs")));
		}
		catch { }

		return doneAnything;
	}

	private static TemplateParameters ReadTemplateParameters(string modSrcDirectory)
	{
		var properties = BuildProperties.ReadBuildFile(modSrcDirectory);
		TemplateParameters parameters;
		parameters.ModName = Path.GetFileName(modSrcDirectory);
		parameters.ModDisplayName = properties.displayName;
		parameters.ModAuthor = properties.author;
		parameters.ModVersion = properties.version.ToString();
		parameters.ItemName = string.Empty;
		parameters.ItemDisplayName = string.Empty;

		return parameters;
	}

	private static bool CheckOrUpgradeCsprojFile(string modSrcDirectory, bool checkOnly, object? templateParameters = null)
	{
		string csprojPath = Path.Combine(modSrcDirectory, $"{Path.GetFileName(modSrcDirectory)}.csproj");
		bool fileIsMissing = !File.Exists(csprojPath);
		bool fileIsBroken = false;
		bool fileIsUpgradeable = false;
		XDocument? document = null;

		if (!fileIsMissing) {
			using (new Logging.QuietExceptionHandle()) {
				try { document = XDocument.Parse(File.ReadAllText(csprojPath), LoadOptions.PreserveWhitespace); }
				catch { fileIsBroken = true; }
			}
		}

		if (!fileIsMissing && !fileIsBroken) {
			// Check if this is even a C# project.
			if (document!.Root is not XElement { Name.LocalName: "Project", FirstAttribute: { Name.LocalName: "Sdk", Value: "Microsoft.NET.Sdk" } } root) {
				fileIsBroken = true;
				goto SkipXmlVerification;
			}

			List<XElement>? elementsToRemove = null;
			var propertyGroups = root.Elements().Where(e => e is XElement { Name.LocalName: "PropertyGroup", IsEmpty: false });

			// Ensure that root imports tModLoader.targets.
			if (!root.Elements().Any(e => e is XElement { Name.LocalName: "Import", FirstAttribute: { Name.LocalName: "Project", Value: @"..\tModLoader.targets" } })) {
				fileIsUpgradeable = true;
				if (!checkOnly) {
					var import = new XElement("Import");
					import.SetAttributeValue("Project", @"..\tModLoader.targets");

					root.AddFirst(new object[] {
						new XText("\n\n\t"),
						new XComment(" Import tModLoader mod properties "),
						new XText("\n\t"),
						import,
					});
				}
			}

			// Get rid of Framework & Platform overrides.
			foreach (var property in propertyGroups.SelectMany(g => g.Elements()).Where(e => e is XElement { Name.LocalName: "TargetFramework" or "PlatformTarget" })) {
				fileIsUpgradeable = true;
				if (!checkOnly) {
					(elementsToRemove ??= new()).Add(property);
				}
			}

			foreach (var element in elementsToRemove ?? Enumerable.Empty<XElement>()) {
				// Remove whitespace, which is kept due to the way we parsed the document.
				if (element.PreviousNode is XText previous && string.IsNullOrWhiteSpace(previous.Value)) {
					previous.Remove();
				}

				element.Remove();
			}
		}

		SkipXmlVerification:
		if (!checkOnly) {
			// Recreate the file from scratch if it's broken or missing.
			if (fileIsMissing || fileIsBroken) {
				// Make a backup.
				if (!fileIsMissing) {
					File.Move(csprojPath, csprojPath + ".backup", overwrite: true);
				}

				templateParameters ??= ReadTemplateParameters(modSrcDirectory);

				TryWriteModTemplateFile(modSrcDirectory, $"{TemplateResourcePrefix}{ModNameVar}.csproj", templateParameters);
			}
			// Save the document if it's been modified.
			else if (fileIsUpgradeable) {
				File.WriteAllText(csprojPath, XmlToFancyString(document!));
			}
		}

		return fileIsMissing || fileIsBroken || fileIsUpgradeable;
	}

	private static string XmlToFancyString(XDocument document)
	{
		var sb = new StringBuilder();
		using var xmlWriter = XmlWriter.Create(sb, new() {
			Indent = true,
			IndentChars = "\t",
			OmitXmlDeclaration = true,
		});

		document.WriteTo(xmlWriter);
		xmlWriter.Flush();

		return sb.ToString();
	}

	private static void DeleteIfExists(FileSystemInfo entry)
	{
		switch (entry) {
			case FileSystemInfo when !entry.Exists: return;
			case FileInfo file: file.Delete(); break;
			case DirectoryInfo directory: directory.Delete(recursive: true); break;
		}
	}
}
