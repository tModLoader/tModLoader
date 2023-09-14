using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using tModLoader.BuildTools.ModFile;

namespace tModLoader.BuildTools.Tasks;

public class PackageModFile : TaskBase
{
	/// <summary>
	/// NuGet package references.
	/// </summary>
	[Required]
	public ITaskItem[] PackageReferences { get; set; } = Array.Empty<ITaskItem>();

	/// <summary>
	/// Project references.
	/// </summary>
	public ITaskItem[] ProjectReferences { get; set; } = Array.Empty<ITaskItem>();


	/// <summary>
	/// Assembly references.
	/// </summary>
	[Required]
	public ITaskItem[] ReferencePaths { get; set; } = Array.Empty<ITaskItem>();

	/// <summary>
	/// Mod references.
	/// </summary>
	[Required]
	public ITaskItem[] ModReferences { get; set; } = Array.Empty<ITaskItem>();

	/// <summary>
	/// The directory where the .csproj is.
	/// </summary>
	[Required]
	public string ProjectDirectory { get; set; } = string.Empty;


	/// <summary>
	/// The directory where the compiled mod assembly is (relative to <see cref="ProjectDirectory"/>).
	/// </summary>
	[Required]
	public string OutputPath { get; set; } = string.Empty;

	/// <summary>
	/// The assembly name of the mod.
	/// </summary>
	[Required]
	public string AssemblyName { get; set; } = string.Empty;

	/// <summary>
	/// The version of tModLoader this mod was built for.
	/// </summary>
	[Required]
	public string TmlVersion { get; set; } = string.Empty;

	/// <summary>
	/// The path to tModLoader's assembly file.
	/// </summary>
	[Required]
	public string TmlDllPath { get; set; } = string.Empty;

	/// <summary>
	/// The path where the .tmod file should be written to.
	/// </summary>
	public string OutputTmodPath { get; set; } = string.Empty;

	/// <summary>
	/// The mod properties. (Previously found in <c>build.txt</c>)
	/// </summary>
	[Required]
	public ITaskItem[] ModProperties { get; set; } = Array.Empty<ITaskItem>();

	private static readonly IList<string> SourceExtensions = new List<string> {".csproj", ".cs", ".sln"};
	private static readonly IList<string> IgnoredNugetPackages = new List<string> {"tModLoader.CodeAssist"};

	protected override void Run() {
		// Verify the .tmod path exists, and find it if it doesn't
		if (string.IsNullOrEmpty(OutputTmodPath))
			OutputTmodPath = SavePathLocator.FindSavePath(Log, TmlDllPath, AssemblyName);
		Log.LogMessage(MessageImportance.Normal, $"Using path for .tmod file: {OutputTmodPath}");

		// Check the dll exists
		string modDllName = Path.ChangeExtension(AssemblyName, ".dll");
		string modDllPath = Path.Combine(ProjectDirectory, OutputPath, modDllName);
		if (!File.Exists(modDllPath))
			throw new FileNotFoundException("Mod dll not found.", modDllPath);
		Log.LogMessage(MessageImportance.Normal, $"Found mod's dll file: {modDllPath}");

		// Load the mod properties from the .csproj or build.txt
		BuildProperties modProperties = GetModProperties();
		Log.LogMessage(MessageImportance.Normal, $"Loaded build properties: {modProperties}");

		TmodFile tmodFile = new(OutputTmodPath, AssemblyName, modProperties.Version, Version.Parse(TmlVersion));

		// Add files to the .tmod file
		tmodFile.AddFile(modDllName, File.ReadAllBytes(modDllPath));
		AddAllReferences(tmodFile, modProperties);
		tmodFile.AddFile("Info", modProperties.ToBytes(TmlVersion));

		string modPdbPath = Path.ChangeExtension(modDllPath, ".pdb");
		string modPdbName = Path.ChangeExtension(modDllName, ".pdb");
		if (File.Exists(modPdbPath))
			tmodFile.AddFile(modPdbName, File.ReadAllBytes(modPdbPath));

		Log.LogMessage(MessageImportance.Normal, "Adding resources...");
		List<string> resources = Directory.GetFiles(ProjectDirectory, "*", SearchOption.AllDirectories)
			.Where(res => !IgnoreResource(modProperties, res))
			.ToList();
		Parallel.ForEach(resources, resource => AddResource(tmodFile, resource));

		// Save it
		Log.LogMessage(MessageImportance.Normal, "Saving mod file...");
		try {
			tmodFile.Save();
		}
		catch (Exception e) {
			Log.LogError("Failed to create .tmod file. Check that the mod isn't enabled if tModLoader is open.\n" +
			             "Full error: " + e);
			return;
		}

		// Enable the mod
		string? modsFolder = Path.GetDirectoryName(OutputTmodPath);
		if (modsFolder is null) {
			Log.LogWarning("Couldn't get directory from .tmod output path.");
			return;
		}

		EnableMod(AssemblyName, modsFolder);
	}

	private void AddAllReferences(TmodFile tmodFile, BuildProperties modProperties) {
		List<ITaskItem> nugetReferences = GetNugetReferences();
		List<ITaskItem> modReferences = GetModReferences();
		List<ITaskItem> projectReferences = GetProjectReferences();

		// Assumes all dll references are under the mod's folder (at same level or in subfolders).
		// Letting dll references be anywhere would mean doing some weird filters on references,
		// or using a custom `<DllReference>` thing that would get translated to a `<Reference>`.
		List<ITaskItem> dllReferences = ReferencePaths.Where(x => x.GetMetadata("FullPath").StartsWith(ProjectDirectory)).ToList();
		Log.LogMessage(MessageImportance.Normal, $"Found {dllReferences.Count} dll references.");

		foreach (ITaskItem taskItem in nugetReferences) {
			string nugetName = "lib/" + taskItem.GetMetadata("NuGetPackageId") + ".dll";
			string nugetFile = taskItem.GetMetadata("HintPath");

			if (string.Equals(taskItem.GetMetadata("Private"), "true", StringComparison.OrdinalIgnoreCase)) {
				Log.LogMessage(MessageImportance.Normal, $"Skipping private reference: {nugetName}");
				continue;
			}

			Log.LogMessage(MessageImportance.Normal, $"Adding nuget {nugetName} with path {nugetFile}");
			tmodFile.AddFile(nugetName, File.ReadAllBytes(nugetFile));
			modProperties.AddDllReference(taskItem.GetMetadata("NuGetPackageId"));
		}

		foreach (ITaskItem dllReference in dllReferences) {
			string dllPath = dllReference.GetMetadata("FullPath");
			string dllName = Path.GetFileNameWithoutExtension(dllPath);

			if (string.Equals(dllReference.GetMetadata("Private"), "true", StringComparison.OrdinalIgnoreCase)) {
				Log.LogMessage(MessageImportance.Normal, $"Skipping private reference: {dllName}");
				continue;
			}

			Log.LogMessage(MessageImportance.Normal, $"Adding dll reference with path {dllPath}");
			tmodFile.AddFile($"lib/{dllName}.dll", File.ReadAllBytes(dllPath));
			modProperties.AddDllReference(dllName);
		}

		foreach (ITaskItem projectReference in projectReferences) {
			string dllPath = projectReference.ItemSpec;
			string outputPath = "lib/" + Path.GetFileName(dllPath);
			string originalItemSpec = projectReference.GetMetadata("OriginalItemSpec"); // Path to .csproj

			if (string.Equals(projectReference.GetMetadata("Private"), "true", StringComparison.OrdinalIgnoreCase)) {
				Log.LogMessage(MessageImportance.Normal, $"Skipping private project reference: {originalItemSpec}");
				continue;
			}

			Log.LogMessage(MessageImportance.Normal, $"Adding project reference with path {originalItemSpec}");
			tmodFile.AddFile(outputPath, File.ReadAllBytes(dllPath));
			modProperties.AddDllReference(Path.GetFileNameWithoutExtension(dllPath));
		}

		foreach (ITaskItem modReference in modReferences) {
			string modName = modReference.GetMetadata("Identity");
			string weakRef = modReference.GetMetadata("Weak");

			Log.LogMessage(MessageImportance.Normal, $"Adding mod reference with mod name {modName} [Weak: {weakRef}]");
			modProperties.AddModReference(modName, string.Equals(weakRef, "true", StringComparison.OrdinalIgnoreCase));
		}

		// Add modReferences to sortAfter if they are not already in sortBefore
		modProperties.SortAfter = modProperties.GetDistinctRefs();
	}

	private List<ITaskItem> GetNugetReferences() {
		Dictionary<string, ITaskItem> nugetLookup = PackageReferences.ToDictionary(x => x.ItemSpec);
		// Check if any packages in IgnoredNugetPackages are present in nugetLookup, and if they are, remove them
		foreach (string ignoredNugetPackage in IgnoredNugetPackages) {
			if (nugetLookup.ContainsKey(ignoredNugetPackage)) {
				Log.LogMessage(MessageImportance.Normal, $"Ignoring nuget package: {ignoredNugetPackage}");
				nugetLookup.Remove(ignoredNugetPackage);
			}
		}

		List<ITaskItem> nugetReferences = new List<ITaskItem>();
		foreach (ITaskItem referencePath in ReferencePaths) {
			string? hintPath = referencePath.GetMetadata("HintPath");
			string? nugetPackageId = referencePath.GetMetadata("NuGetPackageId");
			string? nugetPackageVersion = referencePath.GetMetadata("NuGetPackageVersion");

			if (string.IsNullOrEmpty(nugetPackageId) || !nugetLookup.ContainsKey(nugetPackageId)) continue;

			Log.LogMessage(MessageImportance.Normal, $"{nugetPackageId} - v{nugetPackageVersion} - Found at: {hintPath}");
			nugetReferences.Add(referencePath);
		}

		Log.LogMessage(MessageImportance.Normal, $"Found {nugetReferences.Count} nuget references.");

		if (nugetLookup.Count != nugetReferences.Count)
			Log.LogWarning($"Expected {nugetLookup.Count} nuget references but found {nugetReferences.Count}.");
		return nugetReferences;
	}

	private List<ITaskItem> GetModReferences() {
		List<ITaskItem> modReferences = new List<ITaskItem>();
		foreach (ITaskItem modReference in ModReferences) {
			string modPath = modReference.GetMetadata("HintPath");
			if (modPath.Length == 0)
				modPath = modReference.GetMetadata("ProjectPath");
			string modName = modReference.GetMetadata("Identity");
			string weakRef = modReference.GetMetadata("Weak");
			bool isWeak = string.Equals(weakRef, "true", StringComparison.OrdinalIgnoreCase);

			if (modName.Length == 0)
				throw new Exception("A mod reference must have an identity (Include=\"ModName\"). It should match the internal name of the mod you are referencing.");

			Log.LogMessage(MessageImportance.Normal, $"{modName} [Weak: {isWeak}] - Found at: {modPath}");
			modReferences.Add(modReference);
		}

		Log.LogMessage(MessageImportance.Normal, $"Found {modReferences.Count} mod references.");
		return modReferences;
	}

	private List<ITaskItem> GetProjectReferences() {
		List<ITaskItem> projectReferences = new();
		foreach (ITaskItem projectReference in ProjectReferences) {
			string dllPath = projectReference.ItemSpec;
			if (!File.Exists(dllPath)) {
				Log.LogWarning("Project reference dll not found: " + dllPath);
				continue;
			}

			projectReferences.Add(projectReference);
		}

		return projectReferences;
	}

	private BuildProperties GetModProperties() {
		// Check there are at least 2 properties because `Version` always exists
		if (ModProperties.Length < 2) {
			Log.LogMessage(MessageImportance.Low, "No mod properties found in csproj.");
			string buildInfoFile = Path.Combine(ProjectDirectory, "build.txt");
			if (File.Exists(buildInfoFile)) {
				Log.LogWarning("Using deprecated build.txt file");
				return BuildProperties.ReadBuildInfo(buildInfoFile);
			}
		}

		BuildProperties properties = BuildProperties.ReadTaskItems(ModProperties);
		string descriptionFilePath = Path.Combine(ProjectDirectory, "description.txt");
		if (!File.Exists(descriptionFilePath)) {
			Log.LogWarning("Mod description not found with path: " + descriptionFilePath);
			return properties;
		}
		properties.Description = File.ReadAllText(descriptionFilePath);

		return properties;
	}

	private void EnableMod(string modName, string modsFolderPath){
		string enabledPath = Path.Combine(modsFolderPath, "enabled.json");
		if (!File.Exists(enabledPath)) {
			Log.LogMessage(MessageImportance.Low, $"enabled.json not found at '{enabledPath}', the mod will not be enabled.");
			return;
		}

		string enabledJson = File.ReadAllText(enabledPath);
		try {
			List<string> enabled = JsonConvert.DeserializeObject<List<string>>(enabledJson) ?? new List<string>();
			if (!enabled.Contains(modName)) {
				enabled.Add(modName);
				File.WriteAllText(enabledPath, JsonConvert.SerializeObject(enabled, Formatting.Indented));
			}
		}
		catch (Exception e) {
			Log.LogWarning($"Failed to enable mod {modName}: {e}");
		}
	}

	private bool IgnoreResource(BuildProperties properties, string resourcePath) {
		string relPath = resourcePath.Substring(ProjectDirectory.Length + 1);
		return properties.IgnoreFile(relPath) ||
		       relPath[0] == '.' ||
		       relPath.StartsWith("bin" + Path.DirectorySeparatorChar) ||
		       relPath.StartsWith("obj" + Path.DirectorySeparatorChar) ||
		       relPath == "build.txt" || // For mods that still use a build.txt
		       !properties.IncludeSource && SourceExtensions.Contains(Path.GetExtension(resourcePath)) ||
		       Path.GetFileName(resourcePath) == "Thumbs.db";
	}

	private void AddResource(TmodFile tmodFile, string resourcePath) {
		string relPath = resourcePath.Substring(ProjectDirectory.Length + 1);

		Log.LogMessage(MessageImportance.Low, "Adding resource: {0}", relPath);

		using FileStream src = File.OpenRead(resourcePath);
		using MemoryStream dst = new MemoryStream();

		if (!ContentConverters.Convert(ref relPath, src, dst))
			src.CopyTo(dst);

		tmodFile.AddFile(relPath, dst.ToArray());
		src.Dispose();
	}
}