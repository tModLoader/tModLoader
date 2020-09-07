using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MP3Sharp;
using NVorbis;
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
using Terraria.ModLoader.IO;
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
		internal readonly IDictionary<string, ModTranslation> translations = new Dictionary<string, ModTranslation>();
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
			foreach(var loadable in content.Reverse()) {
				loadable.Unload();
			}
			content.Clear();

			equipTextures.Clear();
			translations.Clear();

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

			AutoloadLocalization();
			IList<Type> modGores = new List<Type>();
			IList<Type> modSounds = new List<Type>();


			Type modType = GetType();
			foreach (Type type in Code.GetTypes().OrderBy(type => type.FullName, StringComparer.InvariantCulture)) {
				if (type == modType){continue;}
				if (type.IsAbstract){continue;}
				if (type.GetConstructor(new Type[0]) == null){continue;}//don't autoload things with no default constructor

				if (type.IsSubclassOf(typeof(ModGore))) {
					modGores.Add(type);
				}
				else if (type.IsSubclassOf(typeof(ModSound))) {
					modSounds.Add(type);
				}
				else if (typeof(ILoadable).IsAssignableFrom(type)) {
					var autoload = AutoloadAttribute.GetValue(type);
					if (autoload.NeedsAutoloading) {
						AddContent((ILoadable)Activator.CreateInstance(type));
					}
				}
			}
			if (Properties.AutoloadGores) {
				AutoloadGores(modGores);
			}
			if (Properties.AutoloadSounds) {
				AutoloadSounds(modSounds);
			}
			if (Properties.AutoloadBackgrounds) {
				AutoloadBackgrounds();
			}
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

		private void AutoloadBackgrounds() {
			foreach (string texture in Assets.EnumeratePaths<Texture2D>().Where(t => t.StartsWith("Backgrounds/"))) {
				AddBackgroundTexture($"{Name}/{texture}");
			}
		}

		private void AutoloadGores(IList<Type> modGores) {
			var modGoreNames = modGores.ToDictionary(t => t.FullName);

			foreach (string texturePath in Assets.EnumeratePaths<Texture2D>().Where(t => t.StartsWith("Gores/"))) {
				ModGore modGore = null;
				if (modGoreNames.TryGetValue($"{Name}.{texturePath.Replace('/', '.')}", out Type t))
					modGore = (ModGore)Activator.CreateInstance(t);

				AddGore($"{Name}/{texturePath}", modGore);
			}
		}

		private void AutoloadSounds(IList<Type> modSounds) {
			var modSoundNames = modSounds.ToDictionary(t => t.FullName);

			const string SoundFolder = "Sounds/";

			foreach (string soundPath in Assets.EnumeratePaths<SoundEffect>().Where(t => t.StartsWith(SoundFolder))) {
				string substring = soundPath.Substring(SoundFolder.Length);
				SoundType soundType = SoundType.Custom;

				if (substring.StartsWith("Item/")) {
					soundType = SoundType.Item;
				}
				else if (substring.StartsWith("NPCHit/")) {
					soundType = SoundType.NPCHit;
				}
				else if (substring.StartsWith("NPCKilled/")) {
					soundType = SoundType.NPCKilled;
				}

				ModSound modSound = null;
				if (modSoundNames.TryGetValue($"{Name}/{soundPath}".Replace('/', '.'), out Type t))
					modSound = (ModSound)Activator.CreateInstance(t);

				AddSound(soundType, $"{Name}/{soundPath}", modSound);
			}

			foreach (string music in musics.Keys.Where(t => t.StartsWith("Sounds/"))) {
				string substring = music.Substring("Sounds/".Length);

				if (substring.StartsWith("Music/")) {
					AddSound(SoundType.Music, Name + '/' + music);
				}
			}
		}

		/// <summary>
		/// Loads .lang files
		/// </summary>
		private void AutoloadLocalization() {
			var modTranslationDictionary = new Dictionary<string, ModTranslation>();
			foreach (var translationFile in File.Where(entry => Path.GetExtension(entry.Name) == ".lang")) {
				// .lang files need to be UTF8 encoded.
				string translationFileContents = System.Text.Encoding.UTF8.GetString(File.GetBytes(translationFile));
				GameCulture culture = GameCulture.FromName(Path.GetFileNameWithoutExtension(translationFile.Name));

				using (StringReader reader = new StringReader(translationFileContents)) {
					string line;
					while ((line = reader.ReadLine()) != null) {
						int split = line.IndexOf('=');
						if (split < 0)
							continue; // lines witout a = are ignored
						string key = line.Substring(0, split).Trim().Replace(" ", "_");
						string value = line.Substring(split + 1); // removed .Trim() since sometimes it is desired.
						if (value.Length == 0) {
							continue;
						}
						value = value.Replace("\\n", "\n");
						// TODO: Maybe prepend key with filename: en.US.ItemName.lang would automatically assume "ItemName." for all entries.
						//string key = key;
						if (!modTranslationDictionary.TryGetValue(key, out ModTranslation mt))
							modTranslationDictionary[key] = mt = CreateTranslation(key);
						mt.AddTranslation(culture, value);
					}
				}
			}

			foreach (var value in modTranslationDictionary.Values) {
				AddTranslation(value);
			}
		}
	}
}
