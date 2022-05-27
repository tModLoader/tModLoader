using Microsoft.Build.Framework;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace tModLoader.BuildTools.Tasks;

public class PackageModFile : TaskBase
{
	[Required]
	public ITaskItem[] NugetReferences { get; set; }

	[Required]
	public string Test { get; set; } = string.Empty;

	[Output]
	public string OutTest { get; set; } = string.Empty;

	protected override void Run() {
		Log.LogMessage(MessageImportance.Low, $"Executing stuff...");
		Log.LogMessage(Test);
		OutTest = Test;

		foreach (ITaskItem nugetReference in NugetReferences) {
			var identifier = nugetReference.GetMetadata("Identifier");
			var version = nugetReference.GetMetadata("Version");
			var spec = nugetReference.ItemSpec;
			Log.LogMessage($"{identifier} - v{version} - {spec}");
		}
		// Log.LogMessage(ProjectDepsFilePath);
		// Log.LogMessage(LibrariesDir);
		// Log.LogMessage(ProjectRuntimeConfigFilePath);
		//
		// // if (!AddProbingPaths()) {
		// // 	Log.LogMessage(MessageImportance.High, $"'{Path.GetFileName(ProjectRuntimeConfigFilePath)}' is missing, skipping dependency reorganization.");
		// // 	return;
		// // }
		//
		// MoveFiles();
	}
}