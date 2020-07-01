using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Terraria.ModLoader.Assets
{
	public class AssemblyResourcesModContentSource : AssemblyResourcesContentSource
	{
		private readonly Dictionary<string, string> PathRedirects = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

		public AssemblyResourcesModContentSource(string basePath, Assembly assembly) : base(assembly) {
			foreach (string path in ResourceNames) {
				if (!path.StartsWith(basePath)) {
					continue;
				}
				
				PathRedirects[path.Substring(basePath.Length)] = path;
			}
		}

		public override bool HasAsset(string assetName) => PathRedirects.TryGetValue(assetName, out _);
		public override Stream OpenStream(string assetName) => base.OpenStream(PathRedirects[assetName]);
	}
}
