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
using Terraria.GameContent;
using Terraria.GameContent.Liquid;
using Terraria.ID;
using Terraria.Initializers;
using Terraria.Localization;
using Terraria.ModLoader.Assets;
using Terraria.ModLoader.Audio;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI;
using Terraria.Utilities;

namespace Terraria.ModLoader
{
	partial class Mod
	{
		internal bool loading;

		private readonly Queue<Task> AsyncLoadQueue = new Queue<Task>();

		//Entities
		internal readonly IDictionary<string, Music> musics = new Dictionary<string, Music>();
		internal readonly IList<Recipe> recipes = new List<Recipe>();
		internal readonly IDictionary<string, ModItem> items = new Dictionary<string, ModItem>();
		internal readonly IDictionary<string, GlobalItem> globalItems = new Dictionary<string, GlobalItem>();
		internal readonly IDictionary<Tuple<string, EquipType>, EquipTexture> equipTextures = new Dictionary<Tuple<string, EquipType>, EquipTexture>();
		internal readonly IDictionary<string, ModPrefix> prefixes = new Dictionary<string, ModPrefix>();
		internal readonly IDictionary<string, ModDust> dusts = new Dictionary<string, ModDust>();
		internal readonly IDictionary<string, ModTile> tiles = new Dictionary<string, ModTile>();
		internal readonly IDictionary<string, GlobalTile> globalTiles = new Dictionary<string, GlobalTile>();
		internal readonly IDictionary<string, ModTileEntity> tileEntities = new Dictionary<string, ModTileEntity>();
		internal readonly IDictionary<string, ModWall> walls = new Dictionary<string, ModWall>();
		internal readonly IDictionary<string, GlobalWall> globalWalls = new Dictionary<string, GlobalWall>();
		internal readonly IDictionary<string, ModProjectile> projectiles = new Dictionary<string, ModProjectile>();
		internal readonly IDictionary<string, GlobalProjectile> globalProjectiles = new Dictionary<string, GlobalProjectile>();
		internal readonly IDictionary<string, ModNPC> npcs = new Dictionary<string, ModNPC>();
		internal readonly IDictionary<string, GlobalNPC> globalNPCs = new Dictionary<string, GlobalNPC>();
		internal readonly IDictionary<string, ModPlayer> players = new Dictionary<string, ModPlayer>();
		internal readonly IDictionary<string, ModMountData> mountDatas = new Dictionary<string, ModMountData>();
		internal readonly IDictionary<string, ModBuff> buffs = new Dictionary<string, ModBuff>();
		internal readonly IDictionary<string, GlobalBuff> globalBuffs = new Dictionary<string, GlobalBuff>();
		internal readonly IDictionary<string, ModWorld> worlds = new Dictionary<string, ModWorld>();
		internal readonly IDictionary<string, ModUgBgStyle> ugBgStyles = new Dictionary<string, ModUgBgStyle>();
		internal readonly IDictionary<string, ModSurfaceBgStyle> surfaceBgStyles = new Dictionary<string, ModSurfaceBgStyle>();
		internal readonly IDictionary<string, GlobalBgStyle> globalBgStyles = new Dictionary<string, GlobalBgStyle>();
		internal readonly IDictionary<string, ModWaterStyle> waterStyles = new Dictionary<string, ModWaterStyle>();
		internal readonly IDictionary<string, ModWaterfallStyle> waterfallStyles = new Dictionary<string, ModWaterfallStyle>();
		internal readonly IDictionary<string, GlobalRecipe> globalRecipes = new Dictionary<string, GlobalRecipe>();
		internal readonly IDictionary<string, ModTranslation> translations = new Dictionary<string, ModTranslation>();
		internal readonly IList<ILoadable> loadables = new List<ILoadable>();

		//TODO: (!!!) The rawimg loading here should be turned into an IAssetReader
		/*private void LoadTexture(string path, Stream stream, bool rawimg) {
			try {
				var texTask = rawimg
					? ImageIO.RawToTexture2DAsync(Main.instance.GraphicsDevice, new BinaryReader(stream))
					: ImageIO.PngToTexture2DAsync(Main.instance.GraphicsDevice, stream);
				
				AsyncLoadQueue.Enqueue(texTask.ContinueWith(t => {
					if (t.Exception != null)
						throw new ResourceLoadException(
							Language.GetTextValue("tModLoader.LoadErrorTextureFailedToLoad", path), t.Exception);

					var tex = t.Result;
					
					tex.Name = Name + "/" + path;

					lock (textures)
						textures[path] = tex;
				}));
			}
			catch (Exception e) {
				throw new ResourceLoadException(Language.GetTextValue("tModLoader.LoadErrorTextureFailedToLoad", path), e);
			}
			finally {
				stream.Close();
			}
		}*/

		private SoundEffect LoadSound(Stream stream, int length, string extension) {
			switch (extension) {
				case ".wav": 
					if (!stream.CanSeek)
						stream = new MemoryStream(stream.ReadBytes(length));

					return SoundEffect.FromStream(stream);
				case ".mp3":
					using (var mp3Stream = new MP3Stream(stream))
					using (var ms = new MemoryStream()) {
						mp3Stream.CopyTo(ms);

						return new SoundEffect(ms.ToArray(), mp3Stream.Frequency, (AudioChannels)mp3Stream.ChannelCount);
					}
				case ".ogg":
					using (var reader = new VorbisReader(stream, true)) {
						byte[] buffer = new byte[reader.TotalSamples * 2 * reader.Channels];
						float[] floatBuf = new float[buffer.Length / 2];
						
						reader.ReadSamples(floatBuf, 0, floatBuf.Length);
						MusicStreamingOGG.Convert(floatBuf, buffer);

						return new SoundEffect(buffer, reader.SampleRate, (AudioChannels)reader.Channels);
					}
			}

			throw new ResourceLoadException("Unknown sound extension "+extension);
		}

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
			foreach (ModItem item in items.Values) {
				ItemLoader.SetDefaults(item.item, false);
				
				item.AutoStaticDefaults();
				item.SetStaticDefaults();

				ItemID.Search.Add($"{item.Mod.Name}/{item.Name}", item.item.type);
			}

			foreach (ModPrefix prefix in prefixes.Values) {
				prefix.AutoDefaults();
				prefix.SetDefaults();
			}

			foreach (ModDust dust in dusts.Values) {
				dust.SetDefaults();
			}

			foreach (ModTile tile in tiles.Values) {
				TextureAssets.Tile[tile.Type] = ModContent.GetTexture(tile.Texture);

				TileLoader.SetDefaults(tile);

				if (TileID.Sets.HasOutlines[tile.Type]) {
					TextureAssets.HighlightMask[tile.Type] = ModContent.GetTexture(tile.HighlightTexture);
				}

				if (!string.IsNullOrEmpty(tile.chest)) {
					TileID.Sets.BasicChest[tile.Type] = true;
				}
				
				TileID.Search.Add($"{tile.Mod.Name}/{tile.Name}", tile.Type);
			}

			foreach (GlobalTile globalTile in globalTiles.Values) {
				globalTile.SetDefaults();
			}

			foreach (ModWall wall in walls.Values) {
				TextureAssets.Wall[wall.Type] = ModContent.GetTexture(wall.Texture);

				wall.SetDefaults();
				
				WallID.Search.Add($"{wall.Mod.Name}/{wall.Name}", wall.Type);
			}

			foreach (GlobalWall globalWall in globalWalls.Values) {
				globalWall.SetDefaults();
			}

			foreach (ModProjectile projectile in projectiles.Values) {
				ProjectileLoader.SetDefaults(projectile.projectile, false);

				projectile.AutoStaticDefaults();
				projectile.SetStaticDefaults();
				
				ProjectileID.Search.Add($"{projectile.Mod.Name}/{projectile.Name}", projectile.projectile.type);
			}

			foreach (ModNPC npc in npcs.Values) {
				NPCLoader.SetDefaults(npc.npc, false);
				npc.AutoStaticDefaults();
				npc.SetStaticDefaults();
				
				NPCID.Search.Add($"{npc.Mod.Name}/{npc.Name}", npc.npc.type);
			}

			foreach (ModMountData modMountData in mountDatas.Values) {
				var mountData = modMountData.mountData;
				mountData.modMountData = modMountData;

				MountLoader.SetupMount(mountData);

				Mount.mounts[modMountData.Type] = mountData;
			}

			foreach (ModBuff buff in buffs.Values) {
				TextureAssets.Buff[buff.Type] = ModContent.GetTexture(buff.Texture);

				buff.SetDefaults();
				
				BuffID.Search.Add($"{buff.Mod.Name}/{buff.Name}", buff.Type);
			}

			foreach (ModWaterStyle waterStyle in waterStyles.Values) {
				LiquidRenderer.Instance._liquidTextures[waterStyle.Type] = ModContent.GetTexture(waterStyle.Texture);
				TextureAssets.Liquid[waterStyle.Type] = ModContent.GetTexture(waterStyle.BlockTexture);
			}

			foreach (ModWaterfallStyle waterfallStyle in waterfallStyles.Values) {
				Main.instance.waterfallManager.waterfallTexture[waterfallStyle.Type] = ModContent.GetTexture(waterfallStyle.Texture);
			}
		}

		internal void UnloadContent() {
			Unload();
			foreach(var loadable in loadables.Reverse()) {
				loadable.Unload();
			}
			loadables.Clear();

			recipes.Clear();
			items.Clear();
			globalItems.Clear();
			equipTextures.Clear();
			prefixes.Clear();
			dusts.Clear();
			tiles.Clear();
			globalTiles.Clear();
			tileEntities.Clear();
			walls.Clear();
			globalWalls.Clear();
			players.Clear();
			projectiles.Clear();
			globalProjectiles.Clear();
			npcs.Clear();
			globalNPCs.Clear();
			buffs.Clear();
			globalBuffs.Clear();
			mountDatas.Clear();
			worlds.Clear();
			ugBgStyles.Clear();
			surfaceBgStyles.Clear();
			globalBgStyles.Clear();
			waterStyles.Clear();
			waterfallStyles.Clear();
			globalRecipes.Clear();
			translations.Clear();

			if (!Main.dedServ) {
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

			Assets.Dispose();
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
				assetReaderCollection.RegisterReader(new PngReader(Main.instance.Services.Get<IGraphicsDeviceService>().GraphicsDevice), ".png");
				assetReaderCollection.RegisterReader(new RawImgReader(Main.instance.Services.Get<IGraphicsDeviceService>().GraphicsDevice), ".rawimg");
				assetReaderCollection.RegisterReader(new XnbReader(Main.instance.Services), ".xnb");
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
