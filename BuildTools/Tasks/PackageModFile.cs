using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

	private static readonly IList<string> SourceExtensions = new List<string> { ".csproj", ".cs", ".sln" };

	protected override void Run() {
		string modDllName = Path.ChangeExtension(AssemblyName, ".dll");
		string modDllPath = Path.Combine(ProjectDirectory, OutputPath, modDllName);
		if (!File.Exists(modDllPath))
			throw new FileNotFoundException("Mod dll not found.", modDllPath);
		Log.LogMessage(MessageImportance.Low, $"Found mod's dll file: {modDllPath}");

		BuildProperties modProperties = GetModProperties();
		Log.LogMessage(MessageImportance.Low, $"Loaded build properties: {modProperties}");

		TmodFile tmodFile = new(OutputTmodPath, AssemblyName, modProperties.Version, Version.Parse(TmlVersion));

		tmodFile.AddFile(modDllName, File.ReadAllBytes(modDllPath));
		AddAllReferences(tmodFile, modProperties);
		tmodFile.AddFile("Info", modProperties.ToBytes(Version.Parse(TmlVersion)));

		Log.LogMessage(MessageImportance.Low, "Adding resources...");
		List<string> resources = Directory.GetFiles(ProjectDirectory, "*", SearchOption.AllDirectories)
			.Where(res => !IgnoreResource(modProperties, res))
			.ToList();
		Parallel.ForEach(resources, resource => AddResource(tmodFile, resource));

		Log.LogMessage(MessageImportance.Low, "Saving mod file...");
		tmodFile.Save();
	}

	private void AddAllReferences(TmodFile tmodFile, BuildProperties modProperties) {
		List<ITaskItem> nugetReferences = GetNugetReferences();
		List<ITaskItem> modReferences = GetModReferences();

		// Assumes all dll references are under the mod's folder (at same level or in subfolders).
		// Letting dll references be anywhere would mean doing some weird filters on references,
		// or using a custom `<DllReference>` thing that would get translated to a `<Reference>`.
		List<ITaskItem> dllReferences = ReferencePaths.Where(x => x.GetMetadata("FullPath").StartsWith(ProjectDirectory)).ToList();
		Log.LogMessage(MessageImportance.Low, $"Found {dllReferences.Count} dll references.");

		foreach (ITaskItem taskItem in nugetReferences) {
			string nugetName = "lib/" + taskItem.GetMetadata("NuGetPackageId") + ".dll";
			string nugetFile = taskItem.GetMetadata("HintPath");

			Log.LogMessage(MessageImportance.Low, $"Adding nuget {nugetName} with path {nugetFile}");
			tmodFile.AddFile(nugetName, File.ReadAllBytes(nugetFile));
			modProperties.AddDllReference(taskItem.GetMetadata("NuGetPackageId"));
		}

		foreach (ITaskItem dllReference in dllReferences) {
			string dllPath = dllReference.GetMetadata("FullPath");
			string dllName = Path.GetFileNameWithoutExtension(dllPath);

			Log.LogMessage(MessageImportance.Low, $"Adding dll reference with path {dllPath}");
			tmodFile.AddFile($"lib/{dllName}.dll", File.ReadAllBytes(dllPath));
			modProperties.AddDllReference(dllName);
		}

		foreach (ITaskItem modReference in modReferences) {
			string? modName = modReference.GetMetadata("Identity");
			string? weakRef = modReference.GetMetadata("Weak");

			Log.LogMessage(MessageImportance.Low, $"Adding mod reference with mod name {modName} [Weak: {weakRef}]");
			modProperties.AddModReference(modName, string.Equals(weakRef, "true", StringComparison.OrdinalIgnoreCase));
		}
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
			string? modPath = modReference.GetMetadata("HintPath");
			string? modName = modReference.GetMetadata("Identity");
			string? weakRef = modReference.GetMetadata("Weak");
			bool isWeak = string.Equals(weakRef, "true", StringComparison.OrdinalIgnoreCase);

			if (string.IsNullOrEmpty(modName))
				throw new Exception("A mod reference must have an identity (Include=\"ModName\"). It should match the internal name of the mod you are referencing.");

			Log.LogMessage(MessageImportance.Low, $"{modName} [Weak: {isWeak}] - Found at: {modPath}");
			modReferences.Add(modReference);
		}

		Log.LogMessage(MessageImportance.Normal, $"Found {modReferences.Count} mod references.");
		return modReferences;
	}

	private BuildProperties GetModProperties() {
		string buildInfoFile = Path.Combine(ProjectDirectory, "build.txt");
		if (File.Exists(buildInfoFile)) {
			Log.LogWarning("Using deprecated build.txt file");
			return BuildProperties.ReadBuildInfo(buildInfoFile);
		}

		BuildProperties properties = BuildProperties.ReadTaskItems(ModProperties);
		string descriptionFilePath = Path.Combine(ProjectDirectory, "description.txt");
		if (!File.Exists(descriptionFilePath)) {
			Log.LogWarning("Mod description not found with path: " + descriptionFilePath);
			return properties;
		}
		properties.SetDescription(File.ReadAllText(descriptionFilePath));

		return properties;
	}

	private bool IgnoreResource(BuildProperties properties, string resourcePath) {
		string relPath = resourcePath.Substring(ProjectDirectory.Length + 1);
		return properties.IgnoreFile(relPath) ||
		       relPath[0] == '.' ||
		       relPath.StartsWith("bin" + Path.DirectorySeparatorChar) ||
		       relPath.StartsWith("obj" + Path.DirectorySeparatorChar) ||
		       relPath == "build.txt" || // For mod's that still use a build.txt
		       !properties.IncludeSource && SourceExtensions.Contains(Path.GetExtension(resourcePath));
	}

	private void AddResource(TmodFile tmodFile, string resourcePath) {
		string relPath = resourcePath.Substring(ProjectDirectory.Length + 1);

		Log.LogMessage("Adding resource: {0}", relPath);

		using FileStream src = File.OpenRead(resourcePath);
		using MemoryStream dst = new MemoryStream();

		if (!ContentConverters.Convert(ref relPath, src, dst))
			src.CopyTo(dst);

		tmodFile.AddFile(relPath, dst.ToArray());
		src.Dispose();
	}
}