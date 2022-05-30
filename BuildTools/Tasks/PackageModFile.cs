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
	public string Test { get; set; } = string.Empty;

	[Output]
	public string OutTest { get; set; } = string.Empty;

	protected override void Run() {
		Log.LogMessage(MessageImportance.Low, "Executing stuff...");
		Log.LogMessage(Test);
		OutTest = Test;

		Dictionary<string, ITaskItem> nugetLookup = PackageReferences.ToDictionary(x => x.ItemSpec);
		if (nugetLookup.ContainsKey("tModLoader.CodeAssist")) nugetLookup.Remove("tModLoader.CodeAssist");

		List<ITaskItem> nugetReferences = new List<ITaskItem>();
		foreach (ITaskItem referencePath in ReferencePaths) {
			var hintPath = referencePath.GetMetadata("HintPath");
			var nugetPackageId = referencePath.GetMetadata("NuGetPackageId");
			var nugetPackageVersion = referencePath.GetMetadata("NuGetPackageVersion");

			if (string.IsNullOrEmpty(nugetPackageId)) continue;
			if (!nugetLookup.ContainsKey(nugetPackageId)) continue;

			Log.LogMessage(MessageImportance.Low, $"{nugetPackageId} - v{nugetPackageVersion} - Found at: {hintPath}");
			nugetReferences.Add(nugetLookup[nugetPackageId]);
		}

		Log.LogMessage(MessageImportance.Normal, $"Found {nugetReferences.Count} nuget references.");

		if (nugetLookup.Count != nugetReferences.Count)
			Log.LogWarning($"Expected {nugetLookup.Count} nuget references but found {nugetReferences.Count}.");
	}
}