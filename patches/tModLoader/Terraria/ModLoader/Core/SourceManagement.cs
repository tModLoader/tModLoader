using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Stubble.Core;

#nullable enable
#pragma warning disable IDE0057

namespace Terraria.ModLoader.Core;

// Everything related to creating and maintaining mod source-code directories.
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

		public static TemplateParameters FromSourceFolder(string modSrcDirectory)
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
	public static void WriteModTemplate(string modSrcDirectory, in TemplateParameters templateParameters)
	{
		var modLoaderAssembly = typeof(ModLoader).Assembly;
		object boxedParameters = templateParameters;

		Directory.CreateDirectory(modSrcDirectory);

		foreach (string resourceKey in modLoaderAssembly.GetManifestResourceNames()) {
			if (resourceKey.StartsWith(TemplateResourcePrefix)) {
				TryWriteModTemplateFile(modSrcDirectory, resourceKey.Substring(TemplateResourcePrefix.Length), boxedParameters);
			}
		}
	}

	/// <summary> Writes a single mod template file to the provided source-code directory. </summary>
	public static bool TryWriteModTemplateFile(string modSrcDirectory, string partialResourceKey, object boxedParameters)
	{
		var assembly = typeof(ModLoader).Assembly;

		string extension = Path.GetExtension(partialResourceKey);
		string relativePath = StaticStubbleRenderer.Render(partialResourceKey, boxedParameters);
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
		var parameters = TemplateParameters.FromSourceFolder(modSrcDirectory);
		object boxedParameters = parameters;

		// Handle the project file.
		if (TryGetCsprojUpgradeAction(modSrcDirectory, out var csprojUpgradeAction, templateParameters: boxedParameters)) {
			modifications.Add(csprojUpgradeAction);
		}

		// Make sure that launch profiles are always present.
		if (!File.Exists(Path.Combine(modSrcDirectory, "Properties", "launchSettings.json"))) {
			modifications.Add(() => TryWriteModTemplateFile(modSrcDirectory, "Properties/launchSettings.json", boxedParameters));
		}

		// Do some cleanups, but only if we already have something.
		if (modifications.Count != 0) {
			try {
				// Old files can cause some issues.
				DeleteIfExists(new DirectoryInfo(Path.Combine(modSrcDirectory, "obj")));
				DeleteIfExists(new DirectoryInfo(Path.Combine(modSrcDirectory, "bin")));

				//TODO: Why do we do this?
				DeleteIfExists(new FileInfo(Path.Combine(modSrcDirectory, "Properties", "AssemblyInfo.cs")));
			}
			catch { }
		}

		return modifications;
	}

	/// <summary> Checks a mod source-code directory for available upgrades, optionally applying them. </summary>
	private static bool TryGetCsprojUpgradeAction(string modSrcDirectory, [NotNullWhen(true)] out Action? result, object? templateParameters = null)
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
					ResetCsprojFile(csprojPath, createBackup: status is not UpgradeStatus.FileMissing, templateParameters);
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
		if (document!.Root is not XElement { Name.LocalName: "Project", FirstAttribute: { Name.LocalName: "Sdk", Value: "Microsoft.NET.Sdk" } } root) {
			return false;
		}

		var nodesToRemove = new List<XNode>();
		var properties = root.Elements().Where(e => e is XElement { Name.LocalName: "PropertyGroup" }).SelectMany(g => g.Elements());

		// Ensure that root imports tModLoader.targets.
		if (!root.Elements().Any(e => e is XElement { Name.LocalName: "Import", FirstAttribute: { Name.LocalName: "Project", Value: @"..\tModLoader.targets" } })) {
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
		foreach (var property in properties.Where(e => e is XElement { Name.LocalName: "TargetFramework" or "PlatformTarget" })) {
			nodesToRemove.Add(property);
		}

		// Keep LangVersion up-to-date by removing old overrides.
		foreach (var property in properties.Where(e => e is XElement { Name.LocalName: "LangVersion" })) {
			if (Version.TryParse(property.Value, out var version) && version.MajorMinor() <= languageVersion) {
				nodesToRemove.Add(property);
			}
		}

		// Remove elements marked for removal.
		if (nodesToRemove.Count != 0) {
			modifications.Add(() => {
				foreach (var element in nodesToRemove) {
					// Remove whitespace, which is otherwise kept due to the way we parsed the document.
					if (element.PreviousNode is XText previous && string.IsNullOrWhiteSpace(previous.Value)) {
						previous.Remove();
					}

					element.Remove();
				}
			});
		}

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

	private static void ResetCsprojFile(string csprojPath, bool createBackup, object? templateParameters = null)
	{
		string modSrcDirectory = Path.GetDirectoryName(csprojPath)!;

		// Make a backup.
		if (createBackup) {
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
		switch (entry) {
			case FileSystemInfo when !entry.Exists: return;
			case FileInfo file: file.Delete(); break;
			case DirectoryInfo directory: directory.Delete(recursive: true); break;
		}
	}
}
