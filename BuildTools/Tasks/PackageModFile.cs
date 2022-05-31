using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using tModLoader.BuildTools.ModFile;

namespace tModLoader.BuildTools.Tasks;

public class PackageModFile : TaskBase
{
	[Required]
	public ITaskItem[] PackageReferences { get; set; } = Array.Empty<ITaskItem>();

	[Required]
	public ITaskItem[] ReferencePaths { get; set; } = Array.Empty<ITaskItem>();

	[Required]
	public ITaskItem[] ModReferences { get; set; } = Array.Empty<ITaskItem>();

	[Required]
	public string ProjectDirectory { get; set; } = string.Empty;

	[Required]
	public string OutputPath { get; set; } = string.Empty;

	[Required]
	public string AssemblyName { get; set; } = string.Empty;

	[Required]
	public string TmlVersion { get; set; } = string.Empty;

	[Required]
	public string OutputTmodPath { get; set; } = string.Empty;

	[Required]
	public ITaskItem[] ModProperties { get; set; } = Array.Empty<ITaskItem>();

	protected override void Run() {
		List<ITaskItem> nugetReferences = GetNugetReferences();

		List<ITaskItem> modReferences = GetModReferences();

		// Assumes all dll references are under the mod's folder (at same level or in subfolders).
		// Letting dll references be anywhere would mean doing some weird filters on references,
		// or using a custom `<DllReference>` thing that would get translated to a `<Reference>`.
		IEnumerable<ITaskItem> dllReferences = ReferencePaths.Where(x => x.GetMetadata("FullPath").StartsWith(ProjectDirectory));
		Log.LogMessage(MessageImportance.Low, $"Found {dllReferences.Count()} dll references.");

		string modDll = Path.Combine(ProjectDirectory, OutputPath, Path.ChangeExtension(AssemblyName, ".dll"));
		if (!File.Exists(modDll)) {
			throw new FileNotFoundException("Mod dll not found.", modDll);
		}
		Log.LogMessage(MessageImportance.Low, $"Found mod's dll file: {modDll}");

		Dictionary<string, string> modProperties = GetModProperties();

		TmodFile modFile = new(OutputTmodPath, AssemblyName, Version.Parse(modProperties["Version"]), Version.Parse(TmlVersion));

		// 1) Get mod .dll file - DONE
		// 2) Create Info file from .csproj
		// 3) Copy .dll to TmodFile
		// 4) Copy references to TmodFile
		// 5) Get all resources, convert them, and copy them to the TmodFile
	}

	private List<ITaskItem> GetNugetReferences() {
		Dictionary<string, ITaskItem> nugetLookup = PackageReferences.ToDictionary(x => x.ItemSpec);
		if (nugetLookup.ContainsKey("tModLoader.CodeAssist")) nugetLookup.Remove("tModLoader.CodeAssist");

		List<ITaskItem> nugetReferences = new List<ITaskItem>();
		foreach (ITaskItem referencePath in ReferencePaths) {
			string? hintPath = referencePath.GetMetadata("HintPath");
			string? nugetPackageId = referencePath.GetMetadata("NuGetPackageId");
			string? nugetPackageVersion = referencePath.GetMetadata("NuGetPackageVersion");

			if (string.IsNullOrEmpty(nugetPackageId)) continue;
			if (!nugetLookup.ContainsKey(nugetPackageId)) continue;

			Log.LogMessage(MessageImportance.Low, $"{nugetPackageId} - v{nugetPackageVersion} - Found at: {hintPath}");
			nugetReferences.Add(nugetLookup[nugetPackageId]);
		}

		Log.LogMessage(MessageImportance.Normal, $"Found {nugetReferences.Count} nuget references.");

		if (nugetLookup.Count != nugetReferences.Count)
			Log.LogWarning($"Expected {nugetLookup.Count} nuget references but found {nugetReferences.Count}.");
		return nugetReferences;
	}

	private List<ITaskItem> GetModReferences() {
		List<ITaskItem> modReferences = new List<ITaskItem>();
		foreach (ITaskItem modReference in ModReferences) {
			string? modPath = modReference.GetMetadata("HintPath");
			string? modName = modReference.GetMetadata("Identity");
			string? weakRef = modReference.GetMetadata("Weak");
			bool isWeak = string.Equals(weakRef, "true", StringComparison.OrdinalIgnoreCase);

			if (string.IsNullOrEmpty(modName)) {
				throw new Exception("A mod reference must have an identity (Include=\"ModName\"). It should match the internal name of the mod you are referencing.");
			}

			Log.LogMessage(MessageImportance.Low, $"{modName} [Weak: {isWeak}] - Found at: {modPath}");
			modReferences.Add(modReference);
		}

		Log.LogMessage(MessageImportance.Normal, $"Found {modReferences.Count} mod references.");
		return modReferences;
	}

	private Dictionary<string, string> GetModProperties() {
		Dictionary<string, string> modProperties = new Dictionary<string, string>();

		string descriptionFilePath = Path.Combine(ProjectDirectory, "description.txt");
		if (!File.Exists(descriptionFilePath))
			throw new FileNotFoundException("Mod description not found.", descriptionFilePath);
		modProperties.Add("description", File.ReadAllText(descriptionFilePath));

		foreach (ITaskItem modProperty in ModProperties) {
			string? modPropertyName = modProperty.ItemSpec;
			string? modPropertyValue = modProperty.GetMetadata("Value");

			Log.LogMessage(MessageImportance.Low, $"{modPropertyName} - Value: {modPropertyValue}");
			modProperties.Add(modPropertyName, modPropertyValue);
		}

		return modProperties;
	}
}