using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReLogic.Content.Sources;

public abstract class ContentSource : IContentSource
{
	public IContentValidator ContentValidator { get; set; }

	public RejectedAssetCollection Rejections { get; } = new ();

	protected string[] assetPaths;
	protected Dictionary<string, string> assetExtensions = new();

	protected void SetAssetNames(IEnumerable<string> paths) {
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
