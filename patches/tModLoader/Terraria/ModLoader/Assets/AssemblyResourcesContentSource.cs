using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ReLogic.Content.Sources;

#pragma warning disable IDE0051 // Remove unused private members

namespace Terraria.ModLoader.Assets;

public sealed class AssemblyResourcesContentSource : ContentSource
{
	private readonly string rootPath;
	private readonly Assembly assembly;
	private readonly string[] excludedStartingPaths;

	public override string FileWatcherPath => null;

	[Obsolete]
	private AssemblyResourcesContentSource(Assembly assembly, string rootPath)
		: this(assembly, rootPath, null) { }

	public AssemblyResourcesContentSource(Assembly assembly, string rootPath = null, IEnumerable<string> excludedStartingPaths = null)
	{
		this.rootPath = rootPath ?? "";
		this.assembly = assembly;
		this.excludedStartingPaths = excludedStartingPaths?.ToArray() ?? [];
		Refresh();
	}

	public override Stream OpenStream(string assetName) => assembly.GetManifestResourceStream(rootPath + assetName);

	public override void Refresh()
	{
		IEnumerable<string> resourceNames = assembly.GetManifestResourceNames();

		foreach (string startingPath in excludedStartingPaths ?? Enumerable.Empty<string>()) {
			resourceNames = resourceNames.Where(p => !p.StartsWith(startingPath));
		}

		if (!string.IsNullOrEmpty(rootPath)) {
			resourceNames = resourceNames
				.Where(p => p.StartsWith(rootPath))
				.Select(p => p.Substring(rootPath.Length));
		}

		SetAssetNames(resourceNames);
	}
}
