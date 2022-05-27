using Microsoft.Build.Framework;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace tModLoader.BuildTools.Tasks;

public class PackageModFile : TaskBase
{
	[Required]
	public string ProjectDepsFilePath { get; set; } = string.Empty;

	[Required]
	public string LibrariesDir { get; set; } = string.Empty;

	[Required]
	public string ProjectRuntimeConfigFilePath { get; set; } = string.Empty;

	[Required]
	public string AssemblyName { get; set; } = string.Empty;

	[Required]
	public string OutputPath { get; set; } = string.Empty;

	[Required]
	public string Test { get; set; } = string.Empty;

	[Output]
	public string OutTest { get; set; } = string.Empty;

	protected override void Run() {
		Log.LogMessage(MessageImportance.Low, $"Executing stuff...");
		Log.LogMessage(Test);
		OutTest = Test;
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

	private bool AddProbingPaths()
	{
		if (!File.Exists(ProjectRuntimeConfigFilePath)) {
			return false;
		}

		var runtimeConfigJson = JObject.Parse(File.ReadAllText(ProjectRuntimeConfigFilePath));

		const string RuntimeOptionsKey = "runtimeOptions";
		const string AdditionalProbingPathsKey = "additionalProbingPaths";

		if (runtimeConfigJson[RuntimeOptionsKey] is not JObject runtimeOptionsObject) {
			runtimeConfigJson[RuntimeOptionsKey] = runtimeOptionsObject = new JObject();
		}

		if (runtimeOptionsObject[AdditionalProbingPathsKey] is not JArray additionalProbingPathsArray) {
			runtimeOptionsObject[AdditionalProbingPathsKey] = additionalProbingPathsArray = new JArray();
		}

		if (!additionalProbingPathsArray.Any(j => j is JValue jValue && jValue.Value is string jString && jString == LibrariesDir)) {
			additionalProbingPathsArray.Add(LibrariesDir);
		}

		File.WriteAllText(ProjectRuntimeConfigFilePath, runtimeConfigJson.ToString());

		return true;
	}

	private void MoveFiles()
	{
		if (!File.Exists(ProjectDepsFilePath)) {
			return;
		}

		var depsJson = JObject.Parse(File.ReadAllText(ProjectDepsFilePath));

		if (depsJson["targets"] is not JObject targetsObject) {
			return;
		}

		foreach (var targetPair in targetsObject) {
			if (targetPair.Value is not JObject targetObject) {
				continue;
			}

			foreach (var libraryPair in targetObject) {
				if (libraryPair.Value is not JObject libraryObject) {
					continue;
				}

				string[] libraryKeySplit = libraryPair.Key.Split('/');

				if (libraryKeySplit.Length != 2) {
					return;
				}

				string libraryName = libraryKeySplit[0];
				string libraryVersion = libraryKeySplit[1];

				if (libraryName == AssemblyName) {
					continue;
				}

				Log.LogMessage($"MoveManagedLibraries - {libraryName} - {libraryVersion} - {libraryObject}");

				MoveManagedLibraries(libraryObject, libraryName, libraryVersion);
				// MoveNativeLibraries(libraryObject, libraryName, libraryVersion);

			}
		}

		File.WriteAllText(ProjectDepsFilePath, depsJson.ToString());
	}

	private void MoveManagedLibraries(JObject libraryObject, string libraryName, string libraryVersion)
	{
		if (libraryObject["runtime"] is not JObject runtimeObject) {
			return;
		}

		List<(string keyOld, string keyNew)>? dllRenames = null;

		foreach (var dllPair in runtimeObject) {
			if (dllPair.Value is not JObject assemblyObject) {
				Log.LogMessage("dllPair.Value is not JObject");
				continue;
			}

			string dllKey = dllPair.Key;
			string dllFileName = Path.GetFileName(dllKey);
			string dllPath = Path.GetFullPath(Path.Combine(OutputPath, dllFileName));

			if (!File.Exists(dllPath)) {
				Log.LogMessage(dllPath + " does not exist");
				continue;
			}

			string dllDestinationPath = Path.GetFullPath(Path.Combine(OutputPath, LibrariesDir, libraryName, libraryVersion, dllFileName));
			string dllDestinationDir = Path.GetDirectoryName(dllDestinationPath);

			if (dllPath == dllDestinationPath) {
				Log.LogMessage("dllDestinationPath == dllPath");
				continue;
			}

			Log.LogMessage(MessageImportance.Low, $"Moving library: '{dllFileName}' to '{dllDestinationPath}'...");

			if (File.Exists(dllDestinationPath)) {
				//File.Delete(dllDestinationPath);
			}

			// Directory.CreateDirectory(dllDestinationDir);
			// File.Move(dllPath, dllDestinationPath);

			// Check for pdb too...
			string pdbPath = Path.ChangeExtension(dllPath, "pdb");

			if (File.Exists(pdbPath)) {
				string pdbDestinationPath = Path.ChangeExtension(dllDestinationPath, "pdb");

				if (File.Exists(pdbDestinationPath)) {
					// File.Delete(pdbDestinationPath);
				}

				// File.Move(pdbPath, pdbDestinationPath);
			}

			// Enqueue a rename of this key.
			if (dllKey != dllFileName) {
				//(dllRenames ??= new()).Add((dllKey, dllFileName));
			}
		}

		if (dllRenames != null) {
			foreach (var (keyOld, keyNew) in dllRenames) {
				runtimeObject[keyNew] = runtimeObject[keyOld];

				runtimeObject.Remove(keyOld);
			}
		}
	}
}