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
	public ITaskItem[] Items { get; set; } = null!;

	protected override void Run()
	{
		// Organize all reference-tied files that are to be copied.
		ProcessItems(Items);
	}

	private void ProcessItems(ITaskItem[] items)
	{
		foreach (var item in items) {
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

				destinationSubDirectory = Path.Combine(BaseDirectory, assemblyName.Name, VersionHack);
			}
			// Direct Managed References
			else if (referenceSourceTarget == "ResolveAssemblyReference" && assemblyName != null) {
				destinationSubDirectory = Path.Combine(BaseDirectory, assemblyName.Name, assemblyName.Version.ToString());
			}
			// NuGet Packages - This is used for all NuGet libraries, whether native or managed, whether rid-specific or agnostic.
			else if (!string.IsNullOrEmpty(nugetPackageId)) {
				string? directoryInPackage = !string.IsNullOrEmpty(pathInPackage) ? Path.GetDirectoryName(pathInPackage) : string.Empty;

				// NuGet package IDs are lowercased in folder repositories.
				string? nugetPackageIdLower = nugetPackageId.ToLower();

				destinationSubDirectory = Path.Combine(BaseDirectory, nugetPackageIdLower, nugetPackageVersion, directoryInPackage);
			}
			// Fallback
			else {
				continue;
			}

			// Adjust Content links.
			if (item.GetMetadata("Link") is string { Length: not 0 } link) {
				item.SetMetadata("Link", Path.Combine(destinationSubDirectory, link));
			}

			// Adjust Content target paths.
			if (item.GetMetadata("TargetPath") is string { Length: not 0 } targetPath) {
				item.SetMetadata("TargetPath", Path.Combine(destinationSubDirectory, targetPath));
			}

			// This MUST have a trailing slash!
			destinationSubDirectory += Path.DirectorySeparatorChar;

			// Set copying destination
			item.SetMetadata("DestinationSubDirectory", destinationSubDirectory);
		}
	}
}
