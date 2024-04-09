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

	private enum UpgradeStatus
	{
		NotRequired,
		Upgradeable,
		FileMissing,
		FileBroken,
	}

	private const string TemplateResourcePrefix = $"Terraria/ModLoader/Templates/";

	private static readonly Version languageVersion = new(10, 0);
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
		var status = UpgradeStatus.NotRequired;
		string csprojPath = Path.Combine(modSrcDirectory, $"{Path.GetFileName(modSrcDirectory)}.csproj");
		
		// Check if the file even exists.
		if (!File.Exists(csprojPath))
			status = UpgradeStatus.FileMissing;

		// Try to load the XML document.
		XDocument? document = null;
		if (status is not UpgradeStatus.FileMissing && !TryLoadXmlDocument(csprojPath, LoadOptions.PreserveWhitespace, out document)) {
			status = UpgradeStatus.FileBroken;
		}

		// Try to collect required modifications.
		var modifications = new List<Action>();
		if (document != null && !TryCollectingCsprojModifications(document, modifications)) {
			status = UpgradeStatus.FileBroken;
		}

		// If there are any modifications - the file is upgradeable.
		if (modifications?.Count > 0) {
			status = UpgradeStatus.Upgradeable;
		}

		// If this is not a dry run and there's changes to be made.
		if (status is not UpgradeStatus.NotRequired) {
			result = () => {
				// Apply modifications if any exist.
				modifications!.ForEach(m => m());

				// Recreate the file from scratch if it's missing or broken, otherwise just save our XML.
				if (status is UpgradeStatus.FileMissing or UpgradeStatus.FileBroken) {
					ResetCsprojFile(csprojPath, templateParameters);
				} else {
					WriteXmlDocumentToFile(csprojPath, document!);
				}
			};

			return true;
		}

		result = default;
		return false;
	}

	/// <summary> If succesfful - populates the provided list with delegates, executing which will upgrade the provided document. </summary>
	private static bool TryCollectingCsprojModifications(XDocument document, List<Action> modifications)
	{
		// Check if this is even a C# project.
		if (document!.Root is not { Name.LocalName: "Project", FirstAttribute: { Name.LocalName: "Sdk", Value: "Microsoft.NET.Sdk" } } root) {
			return false;
		}

		void RemoveNodes(IEnumerable<XNode> nodes)
		{
			modifications.Add(() => {
				foreach (var node in nodes) {
					// Remove whitespace, which is otherwise kept due to the way we parsed the document.
					if (node.PreviousNode is XText previous && string.IsNullOrWhiteSpace(previous.Value)) {
						previous.Remove();
					}

					node.Remove();
				}
			});
		}

		var itemGroups = root.Elements("ItemGroup");
		var propertyGroups = root.Elements("PropertyGroup");

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

		// Keep LangVersion up-to-date by removing old overrides.
		RemoveNodes(propertyGroups
			.Elements("LangVersion")
			.Where(e => Version.TryParse(e.Value, out var v) && v.MajorMinor() <= languageVersion)
		);

		return true;
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
			File.Move(csprojPath, csprojPath + ".backup", overwrite: true);
		}

		// Overwrite using template.
		TryWriteModTemplateFile(
			modSrcDirectory,
			"{{" + nameof(TemplateParameters.ModName) + "}}",
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
