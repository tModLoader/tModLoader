﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;

namespace tModLoader.BuildTasks;

/// <summary>
/// Organizes output copies of Project, NuGet, and directly referenced libraries under a single tidy folder.
/// </summary>
public sealed class OrganizeReferenceDestinations : TaskBase
{
	[Required]
	public string BaseDirectory { get; set; } = null!;

	//[Required]
	//public string NativesDirectory { get; set; } = null!;

	[Required, Output]
	public ITaskItem[] ReferenceCopyLocalPaths { get; set; } = null!;

	protected override void Run()
	{
		var items = ReferenceCopyLocalPaths;

		for (int i = 0; i < items.Length; i++) {
			var item = items[i];

			string fileExtension = item.GetMetadata("Extension");
			string nugetPackageId = item.GetMetadata("NuGetPackageId");
			string nugetPackageVersion = item.GetMetadata("NuGetPackageVersion");
			string referenceSourceTarget = item.GetMetadata("ReferenceSourceTarget");
			string fusionName = item.GetMetadata("FusionName");
			string pathInPackage = item.GetMetadata("PathInPackage");
			//string fileName = item.GetMetadata("Filename");
			//string runtimeIdentifier = item.GetMetadata("RuntimeIdentifier");

			// PDBs & XMLs lack some metadata, attempt to get it from the paired .dll.
			if (string.IsNullOrEmpty(fusionName) && !".dll".Equals(fileExtension, StringComparison.OrdinalIgnoreCase)) {
				string dllSpec = Path.ChangeExtension(item.ItemSpec, ".dll");

				if (items.FirstOrDefault(i => dllSpec.Equals(i.ItemSpec, StringComparison.OrdinalIgnoreCase)) is ITaskItem dllItem) {
					fusionName = dllItem.GetMetadata("FusionName");
				}
			}

			AssemblyName? assemblyName = !string.IsNullOrEmpty(fusionName) ? new AssemblyName(fusionName) : null;

			string destinationSubDirectory;

			// Project References
			if (referenceSourceTarget == "ProjectReference" && assemblyName != null) {
				// Version is bugged in deps.json for ProjectReferences, doesn't reflect AssemblyVersion for whatever reason. Uses 1.0.0.
				const string VersionHack = "1.0.0";

				destinationSubDirectory = Path.Combine(BaseDirectory, assemblyName.Name, VersionHack) + Path.DirectorySeparatorChar;
			}
			// Direct Managed References
			else if (referenceSourceTarget == "ResolveAssemblyReference" && assemblyName != null) {
				destinationSubDirectory = Path.Combine(BaseDirectory, assemblyName.Name, assemblyName.Version.ToString()) + Path.DirectorySeparatorChar;
			}
			// NuGet Packages
			else if (!string.IsNullOrEmpty(nugetPackageId)) {
				// This is used for all NuGet libraries, whether native or managed, whether rid-specific or agnostic.
				destinationSubDirectory = Path.Combine(BaseDirectory, nugetPackageId, nugetPackageVersion, Path.GetDirectoryName(pathInPackage)) + Path.DirectorySeparatorChar;
			}
			// Fallback
			else {
				continue;
			}

			// Set copying destination
			item.SetMetadata("DestinationSubDirectory", destinationSubDirectory);
		}
	}
}
