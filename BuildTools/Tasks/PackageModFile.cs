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
	public ITaskItem[] NugetReferences { get; set; } = Array.Empty<ITaskItem>();

	[Required]
	public ITaskItem[] ReferencePaths { get; set; } = Array.Empty<ITaskItem>();

	[Required]
	public string Test { get; set; } = string.Empty;

	[Output]
	public string OutTest { get; set; } = string.Empty;

	protected override void Run() {
		Log.LogMessage(MessageImportance.Low, $"Executing stuff...");
		Log.LogMessage(Test);
		OutTest = Test;

		Dictionary<string, ITaskItem> nugetLookup = NugetReferences.ToDictionary(x => x.ItemSpec);

		foreach (ITaskItem nugetReference in NugetReferences) {
			var identifier = nugetReference.GetMetadata("Identity");
			var version = nugetReference.GetMetadata("Version");
			var spec = nugetReference.ItemSpec;
			Log.LogMessage($"{spec} - v{version} - Metadatas: {string.Join(" | ", (IList<string>) nugetReference.MetadataNames)}");
			Log.LogMessage($"\t{nugetReference.GetMetadata("FullPath")}"); // FullPath points to ModFolder/NugetPackageName
		}

		foreach (ITaskItem referencePath in ReferencePaths) {
			var hintPath = referencePath.GetMetadata("HintPath");
			var nugetPackageId = referencePath.GetMetadata("NuGetPackageId");
			var nugetPackageVersion = referencePath.GetMetadata("NuGetPackageVersion");

			if (string.IsNullOrEmpty(nugetPackageId)) continue;
			if (!nugetLookup.ContainsKey(nugetPackageId)) continue;

			Log.LogMessage($"{nugetPackageId} - v{nugetPackageVersion} - Found at: {hintPath}");
		}
	}
}