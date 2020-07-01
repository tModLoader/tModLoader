using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReLogic.Content;
using ReLogic.Content.Sources;

namespace Terraria.ModLoader.Assets
{
	//This could really use optimizations, perhaps through busting open the ReLogic dll and making _assets & _sources protected instead of private.
	//TODO: Add ReLogic dll patching, and make Request<T> and SetSources virtual?
	public class ModAssetRepository : AssetRepository
	{
		private readonly Dictionary<string, IAsset> Assets;

		private IEnumerable<IContentSource> Sources => (IEnumerable<IContentSource>)GetType()
			.GetField("_sources", BindingFlags.NonPublic|BindingFlags.Instance)
			.GetValue(this);

		public ModAssetRepository(IAssetLoader syncLoader, IAsyncAssetLoader asyncLoader, IEnumerable<IContentSource> sources = null) : base(syncLoader, asyncLoader)
		{
			Assets = (Dictionary<string, IAsset>)typeof(AssetRepository)
				.GetField("_assets", BindingFlags.NonPublic|BindingFlags.Instance)
				.GetValue(this);

			if(sources!=null) {
				SetSources(sources, AssetRequestMode.DoNotLoad);
			}
		}

		public bool HasAsset<T>(string assetName) where T : class
			=> Sources.Any(source => source is IModContentSource sourceExt ? sourceExt.HasAsset<T>(assetName) : source.HasAsset(assetName));
		public Asset<T> CreateAsset<T>(string assetName, T content) where T : class
			=> CreateAsset(assetName, content);
		public IEnumerable<string> EnumeratePaths<T>() where T : class
		{
			foreach (var source in Sources) {
				if (source is IModContentSource sourceExt) {
					foreach (string path in sourceExt.EnumeratePaths<T>()) {
						yield return path;
					}
				}
			}
		}
		public IEnumerable<Asset<T>> EnumerateLoadedAssets<T>() where T : class
		{
			foreach (var pair in Assets) {
				var asset = pair.Value;

				if (asset.IsLoaded && asset is Asset<T> result) {
					yield return result;
				}
			}
		}

		internal Asset<T> CreateUntrackedAsset<T>(T content) where T : class
			=> CreateAsset(string.Empty, content, true);
		internal Asset<T> CreateUntrackedAsset<T>(string assetName, T content) where T : class
			=> CreateAsset(assetName, content, true);

		private Asset<T> CreateAsset<T>(string assetName, T content, bool tracked = true) where T : class
		{
			var type = typeof(Asset<>).MakeGenericType(typeof(T));
			var constructor = type.GetConstructor(BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance, null, new[] { typeof(string) }, null);
			var asset = (Asset<T>)constructor.Invoke(new object[] { assetName });

			type.GetMethod("SubmitLoadedContent", BindingFlags.NonPublic|BindingFlags.Instance)
				.Invoke(asset, new object[] { content, null }); //It doesn't seem like a problem to set the Source to null, it's unused.

			if(tracked) {
				if(Assets.TryGetValue(assetName, out var oldAsset)) {
					oldAsset?.Dispose();
				}

				Assets[assetName] = asset;
			}

			return asset;
		}
	}
}
