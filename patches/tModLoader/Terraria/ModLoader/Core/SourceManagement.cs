using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Stubble.Core;

#nullable enable
#pragma warning disable IDE0057

namespace Terraria.ModLoader.Core;

/// <summary>
/// Everything related to creating and maintaining mod source-code directories.
/// </summary>
internal static class SourceManagement
{
	public record class TemplateParameters
	{
		public required string ModName { get; set; }
		public required string ModDisplayName { get; set; }
		public required string ModAuthor { get; set; }
		public required string ModVersion { get; set; }
		public string ItemName { get; set; } = string.Empty;
		public string ItemDisplayName { get; set; } = string.Empty;

		public bool IncludeItem => ItemName != string.Empty;

		public static TemplateParameters FromSourceFolder(string modSrcDirectory)
		{
			var properties = BuildProperties.ReadBuildFile(modSrcDirectory);

			TemplateParameters parameters = new TemplateParameters {
				ModName = Path.GetFileName(modSrcDirectory),
				ModDisplayName = properties.displayName,
				ModAuthor = properties.author,
				ModVersion = properties.version.ToString(),
				ItemName = string.Empty,
				ItemDisplayName = string.Empty,
			};

			return parameters;
		}
	}

	private const string TemplateResourcePrefix = $"Terraria/ModLoader/Templates/";

	// Version specifications <= of this will be removed from csproj files.
	private static readonly Version maxLanguageVersionToRemove = new(12, 0);
	private static readonly HashSet<string> textExtensions = new() {
		".txt", ".json", ".hjson", ".toml", ".cs", ".csproj", ".sln"
	};

	/// <summary> Writes mod template files to the provided source-code directory. </summary>
	public static void WriteModTemplate(string modSrcDirectory, TemplateParameters templateParameters)
	{
		var modLoaderAssembly = typeof(ModLoader).Assembly;

		Directory.CreateDirectory(modSrcDirectory);

		foreach (string resourceKey in modLoaderAssembly.GetManifestResourceNames()) {
			if (resourceKey.StartsWith(TemplateResourcePrefix)) {
				TryWriteModTemplateFile(modSrcDirectory, resourceKey.Substring(TemplateResourcePrefix.Length), templateParameters);
			}
		}
	}

	/// <summary> Writes a single mod template file to the provided source-code directory. </summary>
	public static bool TryWriteModTemplateFile(string modSrcDirectory, string partialResourceKey, TemplateParameters templateParameters)
	{
		var assembly = typeof(ModLoader).Assembly;

		string extension = Path.GetExtension(partialResourceKey);
		string relativePath = StaticStubbleRenderer.Render(partialResourceKey, templateParameters);
		string relativeDirectory = Path.GetDirectoryName(relativePath)!;

		// Files are skipped when their filenames render to nothing.
		if (string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(relativePath))) {
			return false;
		}

		byte[] data;
		string resourceKey = TemplateResourcePrefix + partialResourceKey;
		using var resourceStream = assembly.GetManifestResourceStream(resourceKey)!;

		if (textExtensions.Contains(extension)) {
			using var streamReader = new StreamReader(resourceStream, leaveOpen: false);
			string contentsRaw = streamReader.ReadToEnd();
			string contents = StaticStubbleRenderer.Render(contentsRaw, templateParameters);

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
		return CollectSourceUpgradeActions(modSrcDirectory).Count != 0;
	}

	/// <summary> Runs upgrades on the provided mod source-code directory. </summary>
	public static bool UpgradeSource(string modSrcDirectory)
	{
		var actions = CollectSourceUpgradeActions(modSrcDirectory);

		actions.ForEach(a => a());

		return actions.Count != 0;
	}

	/// <summary> Runs upgrades on the provided mod source-code directory. </summary>
	private static List<Action> CollectSourceUpgradeActions(string modSrcDirectory)
	{
		var modifications = new List<Action>();
		var templateParameters = TemplateParameters.FromSourceFolder(modSrcDirectory);

		// Handle the project file.
		if (TryGetCsprojUpgradeAction(modSrcDirectory, out var csprojUpgradeAction, templateParameters: templateParameters)) {
			modifications.Add(csprojUpgradeAction);
		}

		// Make sure that launch profiles are always present.
		if (!File.Exists(Path.Combine(modSrcDirectory, "Properties", "launchSettings.json"))) {
			modifications.Add(() => TryWriteModTemplateFile(modSrcDirectory, "Properties/launchSettings.json", templateParameters));
		}

		// Do some cleanups, but only if we already have something.
		if (modifications.Count != 0) {
			modifications.Add(() => {
				// Old files can cause some issues.
				DeleteIfExists(new DirectoryInfo(Path.Combine(modSrcDirectory, "obj")));
				DeleteIfExists(new DirectoryInfo(Path.Combine(modSrcDirectory, "bin")));

				//TODO: Why do we do this?
				DeleteIfExists(new FileInfo(Path.Combine(modSrcDirectory, "Properties", "AssemblyInfo.cs")));
			});
		}

		return modifications;
	}

	/// <summary> Checks a mod source-code directory for available upgrades, optionally applying them. </summary>
	private static bool TryGetCsprojUpgradeAction(string modSrcDirectory, [NotNullWhen(true)] out Action? result, TemplateParameters? templateParameters = null)
	{
		string csprojPath = Path.Combine(modSrcDirectory, $"{Path.GetFileName(modSrcDirectory)}.csproj");

		// Load and verify the file.
		if (File.Exists(csprojPath)
		&& TryLoadXmlDocument(csprojPath, LoadOptions.PreserveWhitespace, out var document)
		&& IsXmlAValidCsprojFile(document)
		) {
			var modifications = CollectCsprojModifications(document);

			// All is good and there's nothing to do!
			if (!modifications.Any()) {
				result = null;
				return false;
			}

			// Apply modifications and save the XML.
			result = () => {
				foreach (var action in modifications) {
					action();
				}

				WriteXmlDocumentToFile(csprojPath, document!);
			};

			return true;
		}

		// Recreate the file from scratch, as it's missing, broken, or absolutely ancient.
		result = () => ResetCsprojFile(csprojPath, templateParameters);

		return true;
	}

	private static bool IsXmlAValidCsprojFile(XDocument? document)
	{
		return document?.Root is { Name.LocalName: "Project", FirstAttribute: { Name.LocalName: "Sdk", Value: "Microsoft.NET.Sdk" } };
	}

	/// <summary> Returns an enumerable of delegates, executing which will upgrade the provided document. </summary>
	private static List<Action> CollectCsprojModifications(XDocument document)
	{
		var modifications = new List<Action>();

		void RemoveNodes(IEnumerable<XNode> nodes)
		{
			foreach (var node in nodes) {
				modifications.Add(() => {
					// Remove whitespace, which is otherwise kept due to the way we parsed the document.
					if (node.PreviousNode is XText previous && string.IsNullOrWhiteSpace(previous.Value)) {
						previous.Remove();
					}

					node.Remove();
				});
			}
		}

		var root = document.Root!;
		var itemGroups = root.Elements("ItemGroup").Where(e => e.Attribute("Condition") == null);
		var propertyGroups = root.Elements("PropertyGroup").Where(e => e.Attribute("Condition") == null);

		// Ensure that root imports tModLoader.targets.
		if (!root.Elements("Import").Any(e => e is { FirstAttribute: { Name.LocalName: "Project", Value: @"..\tModLoader.targets" } })) {
			modifications.Add(() => {
				var import = new XElement("Import");
				import.SetAttributeValue("Project", @"..\tModLoader.targets");

				root.AddFirst(new object[] {
					new XText("\n\n\t"),
					new XComment(" Import tModLoader mod properties "),
					new XText("\n\t"),
					import,
				});
			});
		}

		// Get rid of Framework & Platform overrides.
		RemoveNodes(Enumerable.Concat(
			propertyGroups.Elements("TargetFramework"),
			propertyGroups.Elements("PlatformTarget")
		));

		// Remove the analyzer package, since our targets file now handles all that.
		RemoveNodes(itemGroups.Elements("PackageReference").Where(
			e => e.Attribute("Include")?.Value == "tModLoader.CodeAssist"
		));

		// Keep LangVersion up-to-date by removing old overrides.
		RemoveNodes(propertyGroups
			.Elements("LangVersion")
			.Where(e => Version.TryParse(e.Value, out var v) && v.MajorMinor() <= maxLanguageVersionToRemove)
		);

		return modifications;
	}

	private static bool TryLoadXmlDocument(string filePath, LoadOptions loadOptions, [NotNullWhen(true)] out XDocument? document)
	{
		using var _ = new Logging.QuietExceptionHandle();

		try {
			document = XDocument.Parse(File.ReadAllText(filePath), loadOptions);
			return true;
		}
		catch {
			document = default;
			return false;
		}
	}

	private static void WriteXmlDocumentToFile(string filePath, XDocument document)
	{
		var sb = new StringBuilder();
		using var xmlWriter = XmlWriter.Create(sb, new() {
			Indent = true,
			IndentChars = "\t",
			OmitXmlDeclaration = true,
		});

		document.WriteTo(xmlWriter);
		xmlWriter.Flush();

		File.WriteAllText(filePath, sb.ToString());
	}

	private static void ResetCsprojFile(string csprojPath, TemplateParameters? templateParameters = null)
	{
		string modSrcDirectory = Path.GetDirectoryName(csprojPath)!;

		// Make a backup if the file already exists.
		if (File.Exists(csprojPath)) {
			File.Move(csprojPath, csprojPath + ".bak", overwrite: true);
		}

		// Overwrite using template.
		TryWriteModTemplateFile(
			modSrcDirectory,
			"{{" + nameof(TemplateParameters.ModName) + "}}.csproj",
			templateParameters ?? TemplateParameters.FromSourceFolder(modSrcDirectory)
		);
	}

	private static void DeleteIfExists(FileSystemInfo entry)
	{
		try {
			switch (entry) {
				case FileSystemInfo when !entry.Exists:
					return;
				case FileInfo file:
					file.Delete();
					break;
				case DirectoryInfo directory:
					directory.Delete(recursive: true);
					break;
			}
		}
		catch { }
	}
}
