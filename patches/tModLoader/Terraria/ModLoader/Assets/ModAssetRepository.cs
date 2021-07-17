using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Content.Sources;
using ReLogic.Graphics;
using Terraria.ModLoader.Audio;

namespace Terraria.ModLoader.Assets
{
	//This could really use optimizations, perhaps through busting open the ReLogic dll and making _assets & _sources protected instead of private.
	//TODO: Add ReLogic dll patching, and make Request<T> and SetSources virtual?
	public class ModAssetRepository : AssetRepository
	{
		private class ContentTypeInfo
		{
			public readonly HashSet<string> FilePaths = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
			public readonly Dictionary<string, string> PathAssociations = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase); //Null value here would mean that the path is ambiguous
		}

		private readonly AssetReaderCollection AssetReaderCollection;
		private readonly Dictionary<Type, ContentTypeInfo> PerContentTypeInfo = new Dictionary<Type, ContentTypeInfo>();

		public ModAssetRepository(AssetReaderCollection assetReaderCollection, IAssetLoader syncLoader, IAsyncAssetLoader asyncLoader, IEnumerable<IContentSource> sources = null) : base(syncLoader, asyncLoader) {
			AssetReaderCollection = assetReaderCollection;

			if (sources!=null) {
				SetSources(sources, AssetRequestMode.DoNotLoad);
			}
		}

		public override void SetSources(IEnumerable<IContentSource> sources, AssetRequestMode mode = AssetRequestMode.ImmediateLoad) {
			base.SetSources(sources, mode);

			FillPathCache();
		}
		public override Asset<T> Request<T>(string assetName, AssetRequestMode mode = AssetRequestMode.ImmediateLoad) {
			if (PerContentTypeInfo.TryGetValue(typeof(T), out var info) && info.PathAssociations.TryGetValue(assetName, out string guessedName)) {
				assetName = guessedName;
			}

			return base.Request<T>(assetName, mode);
		}

		public bool HasAsset<T>(string assetName) where T : class
			=> PerContentTypeInfo.TryGetValue(typeof(T), out var info) && (info.PathAssociations.ContainsKey(assetName) || info.FilePaths.Contains(assetName));
		public IEnumerable<string> EnumeratePaths<T>() where T : class {
			if (PerContentTypeInfo.TryGetValue(typeof(T), out var info)) {
				return info.FilePaths;
			}

			return Enumerable.Empty<string>();
		}
		public IEnumerable<Asset<T>> EnumerateLoadedAssets<T>() where T : class {
			foreach (var pair in _assets) {
				var asset = pair.Value;

				if (asset.IsLoaded && asset is Asset<T> result) {
					yield return result;
				}
			}
		}
		public Asset<T> CreateAsset<T>(string assetName, T content) where T : class
			=> CreateAsset(assetName, content, true);

		internal Asset<T> CreateUntrackedAsset<T>(T content) where T : class
			=> CreateAsset(string.Empty, content, true);
		internal Asset<T> CreateUntrackedAsset<T>(string assetName, T content) where T : class
			=> CreateAsset(assetName, content, true);

		private Asset<T> CreateAsset<T>(string assetName, T content, bool tracked) where T : class {
			var type = typeof(Asset<T>);
			var asset = (Asset<T>)Activator.CreateInstance(type, BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance, null, new object[] { assetName }, null);

			type.GetMethod("SubmitLoadedContent", BindingFlags.NonPublic|BindingFlags.Instance)
				.Invoke(asset, new object[] { content, _sources.First() }); //Source can not be null. It won't be used for this, but it still requires it to be non-null.

			if (tracked) {
				if (_assets.TryGetValue(assetName, out var oldAsset)) {
					oldAsset?.Dispose();
				}

				_assets[assetName] = asset;
			}

			return asset;
		}
		private void FillPathCache() {
			PerContentTypeInfo.Clear();

			foreach (var source in _sources) {
				foreach (string path in source.EnumerateFiles()) {
					string extension = Path.GetExtension(path)?.ToLower();

					if (extension==null) {
						continue;
					}

					string pathWithoutExtension = Path.ChangeExtension(path, null);

					if (!AssetReaderCollection.TryGetReader(extension, out var assetReader)) {
						continue;
					}

					var types = assetReader.GetAssociatedTypes();

					if (types==null || types.Length==0) {
						continue;
					}

					foreach (var type in types) {
						if (!PerContentTypeInfo.TryGetValue(type, out var info)) {
							PerContentTypeInfo[type] = info = new ContentTypeInfo();
						}

						info.FilePaths.Add(path);

						info.PathAssociations[pathWithoutExtension] = info.PathAssociations.ContainsKey(pathWithoutExtension) ? null : path; //Null would mean that the path is ambiguous.
					}
				}
			}
		}
		private string FilterPath(Type contentType, string path) {
			if (PerContentTypeInfo.TryGetValue(contentType, out var info)) {
				return path;
			}

			if (!info.PathAssociations.TryGetValue(path, out string pathAssociation)) {
				return path;
			}

			return pathAssociation ?? throw new ArgumentException($"Extensionless path '{path}' is ambiguous. Please provide an extension.");
		}
	}
}
