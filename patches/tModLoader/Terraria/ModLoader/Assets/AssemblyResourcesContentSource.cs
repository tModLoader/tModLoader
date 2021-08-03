using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ReLogic.Content;
using ReLogic.Content.Sources;

namespace Terraria.ModLoader.Assets
{
	public sealed class AssemblyResourcesContentSource : ContentSource
	{

		private readonly string RootPath;
		private readonly Assembly assembly;

		public AssemblyResourcesContentSource(Assembly assembly, string rootPath = null) {
			this.assembly = assembly;

			IEnumerable<string> resourceNames = assembly.GetManifestResourceNames();
			if (rootPath != null) {
				resourceNames = resourceNames
					.Where(p => p.StartsWith(rootPath))
					.Select(p => p.Substring(rootPath.Length));
			}

			RootPath = rootPath ?? "";
			SetAssetNames(resourceNames);
		}

		public override Stream OpenStream(string assetName) => assembly.GetManifestResourceStream(RootPath + assetName);
	}
}
