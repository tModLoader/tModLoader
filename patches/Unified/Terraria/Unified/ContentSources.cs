using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ReLogic.Content;
using ReLogic.Content.Sources;
using Terraria.Initializers;

namespace Terraria.Unified;

internal static class ContentSources
{
	public abstract class AbstractContentSource : IContentSource
	{
		public IContentValidator ContentValidator { get; set; }

		public RejectedAssetCollection Rejections { get; } = new();

		protected string[] assetPaths;
		protected Dictionary<string, string> assetExtensions = new();

		protected void SetAssetNames(IEnumerable<string> paths)
		{
			assetPaths = paths.ToArray();
			assetExtensions.Clear();

			foreach (var path in assetPaths) {
				var ext = Path.GetExtension(path);

				// ReLogic sets all assets to use Path.DirectorySepChar in their paths in AssetPathHelper.
				var name = AssetPathHelper.CleanPath(path[..^ext.Length]);

				if (assetExtensions.TryGetValue(name, out var ext2))
					throw new Exception($"Multiple extensions for asset {name}, ({ext}, {ext2})");

				assetExtensions[name] = ext;
			}
		}

		public IEnumerable<string> EnumerateAssets() => assetPaths;

		// Use CleanPath to ensure match the assetName path to the 'cleaned path' in assetExtensions for mods, keeping patches minimal.
		public string GetExtension(string assetName) => assetExtensions.TryGetValue(AssetPathHelper.CleanPath(assetName), out var ext) ? ext : null;

		public abstract Stream OpenStream(string fullAssetName);
	}

	public sealed class AssemblyResourcesContentSource : AbstractContentSource
	{
		private readonly string rootPath;
		private readonly Assembly assembly;

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

	public static AssetRepository ManifestAssets { get; set; }

	public static AssemblyResourcesContentSource ManifestContentSource { get; set; }

	public static void PrepareAssets()
	{
		ManifestContentSource = new AssemblyResourcesContentSource(
			Assembly.GetExecutingAssembly(),
			excludedStartingPaths: []
		);

		ManifestAssets = new AssetRepository(AssetInitializer.assetReaderCollection, [ManifestContentSource]) {
			AssetLoadFailHandler = Main.OnceFailedLoadingAnAsset,
		};
	}
}
