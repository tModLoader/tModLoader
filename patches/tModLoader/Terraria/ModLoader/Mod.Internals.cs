using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Content.Readers;
using ReLogic.Content.Sources;
using ReLogic.Graphics;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader.Assets;
using Terraria.ModLoader.Audio;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader
{
	partial class Mod
	{
		internal bool loading;

		private readonly Queue<Task> AsyncLoadQueue = new Queue<Task>();

		//Entities
		internal readonly IDictionary<string, Music> musics = new Dictionary<string, Music>();
		internal readonly IDictionary<Tuple<string, EquipType>, EquipTexture> equipTextures = new Dictionary<Tuple<string, EquipType>, EquipTexture>();
		internal readonly IList<ILoadable> content = new List<ILoadable>();

		private Music LoadMusic(string path, string extension) {
			path = $"tmod:{Name}/{path}{extension}";
			
			switch (extension) {
				case ".wav": return new MusicStreamingWAV(path);
				case ".mp3": return new MusicStreamingMP3(path);
				case ".ogg": return new MusicStreamingOGG(path);
			}

			throw new ResourceLoadException($"Unknown music extension {extension}");
		}

		internal void SetupContent() {
			foreach (var e in content.OfType<ModType>()) {
				e.SetupContent();
			}
		}

		internal void UnloadContent() {
			Unload();

			foreach (var loadable in content.Reverse()) {
				loadable.Unload();
			}
			content.Clear();

			equipTextures.Clear();

			if (!Main.dedServ) {
				// TODO: restore this
				// Manually Dispose IDisposables to free up unmanaged memory immediately
				/* Skip this for now, too many mods don't unload properly and run into exceptions.
				foreach (var sound in sounds)
				{
					sound.Value.Dispose();
				}
				foreach (var texture in textures)
				{
					texture.Value.Dispose();
				}
				*/
			}

			musics.Clear();

			Assets?.Dispose();
		}

		internal void Autoload() {
			if (Code == null)
				return;

			Interface.loadMods.SubProgressText = Language.GetTextValue("tModLoader.MSFinishingResourceLoading");
			while (AsyncLoadQueue.Count > 0)
				AsyncLoadQueue.Dequeue().Wait();

			LocalizationLoader.Autoload(this);

			ModSourceBestiaryInfoElement = new GameContent.Bestiary.ModSourceBestiaryInfoElement(this, DisplayName, Assets);

			IList<Type> modSounds = new List<Type>();

			Type modType = GetType();
			foreach (Type type in Code.GetTypes().OrderBy(type => type.FullName, StringComparer.InvariantCulture)) {
				//don't autoload things with no default constructor
				if (type == modType || type.IsAbstract || type.ContainsGenericParameters || type.GetConstructor(new Type[0]) == null) {
					continue;
				}

				if (typeof(ILoadable).IsAssignableFrom(type)) {
					var autoload = AutoloadAttribute.GetValue(type);

					if (autoload.NeedsAutoloading) {
						AddContent((ILoadable)Activator.CreateInstance(type));
					}
				}
			}
			
			if (Properties.AutoloadSounds)
				SoundLoader.AutoloadSounds(this);
			
			if (Properties.AutoloadGores)
				GoreLoader.AutoloadGores(this);

			if (Properties.AutoloadBackgrounds)
				BackgroundTextureLoader.AutoloadBackgrounds(this);
		}

		internal void PrepareAssets()
		{
			//Open the file.

			if (File != null)
				fileHandle = File.Open();

			//Create the asset repository

			var sources = new List<IContentSource>();

			if (File!=null) {
				sources.Add(new TModContentSource(File));
			}

			var assetReaderCollection = new AssetReaderCollection();

			if (!Main.dedServ) {
				//TODO: Now, how do we unhardcode this?
				
				//Ambiguous
				assetReaderCollection.RegisterReader(new XnbReader(Main.instance.Services), ".xnb");
				//Textures
				assetReaderCollection.RegisterReader(new PngReader(Main.instance.Services.Get<IGraphicsDeviceService>().GraphicsDevice), ".png");
				assetReaderCollection.RegisterReader(new RawImgReader(Main.instance.Services.Get<IGraphicsDeviceService>().GraphicsDevice), ".rawimg");
				//Audio
				assetReaderCollection.RegisterReader(new WavReader(), ".wav");
				assetReaderCollection.RegisterReader(new MP3Reader(), ".mp3");
				assetReaderCollection.RegisterReader(new OggReader(), ".ogg");
			}

			var delayedLoadTypes = new List<Type> {
				typeof(Texture2D),
				typeof(DynamicSpriteFont),
				typeof(SpriteFont),
				typeof(Effect)
			};

			SetupAssetRepository(sources, assetReaderCollection, delayedLoadTypes);

			var asyncAssetLoader = new AsyncAssetLoader(assetReaderCollection, 20);

			foreach (var type in delayedLoadTypes) {
				asyncAssetLoader.RequireTypeCreationOnTransfer(type);
			}

			var assetLoader = new AssetLoader(assetReaderCollection);

			Assets = new ModAssetRepository(assetReaderCollection, assetLoader, asyncAssetLoader, sources.ToArray());
		}
	}
}
