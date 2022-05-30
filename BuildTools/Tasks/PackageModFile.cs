using Microsoft.Build.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace tModLoader.BuildTools.Tasks;

public class PackageModFile : TaskBase
{
	[Required]
	public ITaskItem[] PackageReferences { get; set; } = Array.Empty<ITaskItem>();

	[Required]
	public ITaskItem[] ReferencePaths { get; set; } = Array.Empty<ITaskItem>();

	[Required]
	public ITaskItem[] ModReferences { get; set; } = Array.Empty<ITaskItem>();

	protected override void Run() {
		Log.LogMessage(MessageImportance.Low, "Executing stuff...");

		List<ITaskItem> nugetReferences = GetNugetReferences();

		List<ITaskItem> modReferences = GetModReferences();
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
				throw new Exception("A mod reference must have an identity (Include=\"ModName\").");
			}
			if (!isWeak && string.IsNullOrEmpty(modPath)) {
				throw new Exception("A mod reference must be defined as weak or have a HintPath.");
			}

			Log.LogMessage(MessageImportance.Low, $"{modName} [Weak: {isWeak}] - Found at: {modPath}");
			modReferences.Add(modReference);
		}

		Log.LogMessage(MessageImportance.Normal, $"Found {modReferences.Count} mod references.");
		return modReferences;
	}
}