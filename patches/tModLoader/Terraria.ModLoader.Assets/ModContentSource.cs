using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Content.Sources;
using ReLogic.Graphics;
using Terraria.ModLoader.Audio;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader.Assets
{
	public class ModContentSource : IContentSourceExt, IDisposable
	{
		private class ContentTypeInfo
		{
			public readonly List<string> FilePaths = new List<string>();
			public readonly Dictionary<string, string> PathAssociations = new Dictionary<string, string>(); //Null value here would mean that the path is ambiguous
		}

		public static readonly IDictionary<Type, IList<string>> AssetTypeToExtensions;

		private readonly RejectedAssetCollection Rejections = new RejectedAssetCollection();
		private readonly Dictionary<Type, ContentTypeInfo> PerContentTypeInfo = new Dictionary<Type, ContentTypeInfo>();

		private TmodFile file;

		public IContentValidator ContentValidator { get; set; }

		static ModContentSource()
		{
			AssetTypeToExtensions = new Dictionary<Type, IList<string>> {
				{ typeof(Texture2D),            new List<string> { ".png", ".rawimg" } },
				{ typeof(SoundEffect),          new List<string> { ".ogg", ".mp3", ".wav" } },
				{ typeof(Music),                new List<string> { ".ogg", ".mp3", ".wav" } },
				{ typeof(DynamicSpriteFont),    new List<string> { ".xnb" }},
				{ typeof(Effect),               new List<string> { ".xnb" }},
			};
		}

		public ModContentSource(Mod mod)
		{
			file = mod.File;

			FillPathCache();
		}

		//Rejections
		public void ClearRejections() => Rejections.Clear();
		public void RejectAsset(string assetName, IRejectionReason reason) => Rejections.Reject(assetName, reason);
		public bool TryGetRejections(List<string> rejectionReasons) => Rejections.TryGetRejections(rejectionReasons);
		//Assets
		public bool HasAsset(string assetName)=> file.HasFile(assetName); //This one currently doesn't do any extension guessing
		public bool HasAsset<T>(string assetName) where T : class => file.HasFile(FilterPath(typeof(T), assetName)); //...But this one does.
		public string GetExtension(string assetName) => ""; //Path.GetExtension(assetName);
		public Stream OpenStream(string assetName) => file.GetStream(assetName);
		//Etc
		public void Dispose()
		{
			file = null;

			ClearRejections();
		}

		private string FilterPath(Type contentType, string path)
		{
			if (PerContentTypeInfo.TryGetValue(contentType, out var info)) {
				return path;
			}

			if (!info.PathAssociations.TryGetValue(path, out string pathAssociation)) {
				return path;
			}

			return pathAssociation ?? throw new ArgumentException($"Extensionless path '{path}' is ambiguous. Please provide an extension.");
		}
		private void FillPathCache()
		{
			foreach (var entry in file) {
				string fullPath = entry.Name;
				string extension = Path.GetExtension(fullPath)?.ToLower();

				if (extension==null) {
					continue;
				}

				string path = Path.ChangeExtension(fullPath, null);

				foreach (var pair in AssetTypeToExtensions) {
					var type = pair.Key;
					var extensions = pair.Value;

					if (!extensions.Contains(extension)) {
						continue;
					}

					if (!PerContentTypeInfo.TryGetValue(type, out var info)) {
						PerContentTypeInfo[type] = info = new ContentTypeInfo();
					}

					info.FilePaths.Add(fullPath);

					info.PathAssociations[path] = info.PathAssociations.ContainsKey(path) ? null : fullPath; //Null would mean that the path is ambiguous.
				}

				/*switch (extension) {
					case ".png":
					case ".rawimg":
						texturePaths[path] = fullPath;
						break;
					case ".wav":
					case ".mp3":
					case ".ogg":
						if (path.Contains("Music/"))
							musics[path] = LoadMusic(path, extension);
						else
							soundPaths[path] = fullPath;

						break;
					case ".xnb":
						if (path.StartsWith("Fonts/"))
							fontPaths[path] = fullPath;
						else if (path.StartsWith("Effects/"))
							effects[path] = Main.ShaderContentManager.Load<Effect>("tmod:"+Name+"/"+path);
						else
							throw new ResourceLoadException(Language.GetTextValue("tModLoader.LoadErrorUnknownXNBFileHint", path));

						break;
				}*/
			}
		}
	}
}
