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

	[Obsolete]
	private AssemblyResourcesContentSource(Assembly assembly, string rootPath)
		: this(assembly, rootPath, null) { }

	public AssemblyResourcesContentSource(Assembly assembly, string rootPath = null, IEnumerable<string> excludedStartingPaths = null)
	{
		this.assembly = assembly;
		excludedStartingPaths ??= Enumerable.Empty<string>();

		IEnumerable<string> resourceNames = assembly.GetManifestResourceNames();

		foreach (string startingPath in excludedStartingPaths ?? Enumerable.Empty<string>()) {
			resourceNames = resourceNames.Where(p => !p.StartsWith(startingPath));
		}

		if (rootPath != null) {
			resourceNames = resourceNames
				.Where(p => p.StartsWith(rootPath))
				.Select(p => p.Substring(rootPath.Length));
		}

		this.rootPath = rootPath ?? "";
		SetAssetNames(resourceNames);
	}

	public override Stream OpenStream(string assetName) => assembly.GetManifestResourceStream(rootPath + assetName);
}
