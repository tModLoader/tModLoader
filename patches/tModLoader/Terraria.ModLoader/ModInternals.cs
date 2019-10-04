using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MP3Sharp;
using NVorbis;
using ReLogic.Graphics;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Terraria.GameContent.Liquid;
using Terraria.ID;
using Terraria.Localization;
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
		internal readonly IDictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
		internal readonly IDictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();
		internal readonly IDictionary<string, Music> musics = new Dictionary<string, Music>();
		internal readonly IDictionary<string, DynamicSpriteFont> fonts = new Dictionary<string, DynamicSpriteFont>();
		internal readonly IDictionary<string, Effect> effects = new Dictionary<string, Effect>();
		internal readonly IList<ModRecipe> recipes = new List<ModRecipe>();
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

		private void LoadTexture(string path, Stream stream, bool rawimg) {
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
		}

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
						var buffer = new byte[reader.TotalSamples * 2 * reader.Channels];
						var floatBuf = new float[buffer.Length / 2];
						reader.ReadSamples(floatBuf, 0, floatBuf.Length);
						MusicStreamingOGG.Convert(floatBuf, buffer);
						return new SoundEffect(buffer, reader.SampleRate, (AudioChannels)reader.Channels);
					}
			}
			throw new ResourceLoadException("Unknown sound extension "+extension);
		}

		private Music LoadMusic(string path, string extension) {
			path = "tmod:"+Name+'/'+path+extension;
			switch (extension) {
				case ".wav": return new MusicStreamingWAV(path);
				case ".mp3": return new MusicStreamingMP3(path);
				case ".ogg": return new MusicStreamingOGG(path);
			}
			throw new ResourceLoadException("Unknown music extension "+extension);
		}

		internal void SetupContent() {
			foreach (ModItem item in items.Values) {
				ItemLoader.SetDefaults(item.item, false);
				item.AutoStaticDefaults();
				item.SetStaticDefaults();
			}
			foreach (ModPrefix prefix in prefixes.Values) {
				prefix.AutoDefaults();
				prefix.SetDefaults();
			}
			foreach (ModDust dust in dusts.Values) {
				dust.SetDefaults();
			}
			foreach (ModTile tile in tiles.Values) {
				Main.tileTexture[tile.Type] = ModContent.GetTexture(tile.texture);
				TileLoader.SetDefaults(tile);
				if (TileID.Sets.HasOutlines[tile.Type]) {
					Main.highlightMaskTexture[tile.Type] = ModContent.GetTexture(tile.HighlightTexture);
				}
				if (!string.IsNullOrEmpty(tile.chest)) {
					TileID.Sets.BasicChest[tile.Type] = true;
				}
			}
			foreach (GlobalTile globalTile in globalTiles.Values) {
				globalTile.SetDefaults();
			}
			foreach (ModWall wall in walls.Values) {
				Main.wallTexture[wall.Type] = ModContent.GetTexture(wall.texture);
				wall.SetDefaults();
			}
			foreach (GlobalWall globalWall in globalWalls.Values) {
				globalWall.SetDefaults();
			}
			foreach (ModProjectile projectile in projectiles.Values) {
				ProjectileLoader.SetDefaults(projectile.projectile, false);
				projectile.AutoStaticDefaults();
				projectile.SetStaticDefaults();
			}
			foreach (ModNPC npc in npcs.Values) {
				NPCLoader.SetDefaults(npc.npc, false);
				npc.AutoStaticDefaults();
				npc.SetStaticDefaults();
			}
			foreach (ModMountData modMountData in mountDatas.Values) {
				var mountData = modMountData.mountData;
				mountData.modMountData = modMountData;
				MountLoader.SetupMount(mountData);
				Mount.mounts[modMountData.Type] = mountData;
			}
			foreach (ModBuff buff in buffs.Values) {
				Main.buffTexture[buff.Type] = ModContent.GetTexture(buff.texture);
				buff.SetDefaults();
			}
			foreach (ModWaterStyle waterStyle in waterStyles.Values) {
				LiquidRenderer.Instance._liquidTextures[waterStyle.Type] = ModContent.GetTexture(waterStyle.texture);
				Main.liquidTexture[waterStyle.Type] = ModContent.GetTexture(waterStyle.blockTexture);
			}
			foreach (ModWaterfallStyle waterfallStyle in waterfallStyles.Values) {
				Main.instance.waterfallManager.waterfallTexture[waterfallStyle.Type]
					= ModContent.GetTexture(waterfallStyle.texture);
			}
		}

		internal void UnloadContent() {
			Unload();
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
				foreach (var effect in effects)
				{
					effect.Value.Dispose();
				}
				foreach (var texture in textures)
				{
					texture.Value.Dispose();
				}
				*/
			}
			sounds.Clear();
			effects.Clear();
			foreach (var tex in textures.Values)
				tex?.Dispose();
			textures.Clear();
			musics.Clear();
			fonts.Clear();
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
			foreach (Type type in Code.GetTypes().OrderBy(type => type.FullName, StringComparer.InvariantCulture)) {
				if (type.IsAbstract || type.GetConstructor(new Type[0]) == null)//don't autoload things with no default constructor
				{
					continue;
				}
				if (type.IsSubclassOf(typeof(ModItem))) {
					AutoloadItem(type);
				}
				else if (type.IsSubclassOf(typeof(GlobalItem))) {
					AutoloadGlobalItem(type);
				}
				else if (type.IsSubclassOf(typeof(ModPrefix))) {
					AutoloadPrefix(type);
				}
				else if (type.IsSubclassOf(typeof(ModDust))) {
					AutoloadDust(type);
				}
				else if (type.IsSubclassOf(typeof(ModTile))) {
					AutoloadTile(type);
				}
				else if (type.IsSubclassOf(typeof(GlobalTile))) {
					AutoloadGlobalTile(type);
				}
				else if (type.IsSubclassOf(typeof(ModTileEntity))) {
					AutoloadTileEntity(type);
				}
				else if (type.IsSubclassOf(typeof(ModWall))) {
					AutoloadWall(type);
				}
				else if (type.IsSubclassOf(typeof(GlobalWall))) {
					AutoloadGlobalWall(type);
				}
				else if (type.IsSubclassOf(typeof(ModProjectile))) {
					AutoloadProjectile(type);
				}
				else if (type.IsSubclassOf(typeof(GlobalProjectile))) {
					AutoloadGlobalProjectile(type);
				}
				else if (type.IsSubclassOf(typeof(ModNPC))) {
					AutoloadNPC(type);
				}
				else if (type.IsSubclassOf(typeof(GlobalNPC))) {
					AutoloadGlobalNPC(type);
				}
				else if (type.IsSubclassOf(typeof(ModPlayer))) {
					AutoloadPlayer(type);
				}
				else if (type.IsSubclassOf(typeof(ModBuff))) {
					AutoloadBuff(type);
				}
				else if (type.IsSubclassOf(typeof(GlobalBuff))) {
					AutoloadGlobalBuff(type);
				}
				else if (type.IsSubclassOf(typeof(ModMountData))) {
					AutoloadMountData(type);
				}
				else if (type.IsSubclassOf(typeof(ModGore))) {
					modGores.Add(type);
				}
				else if (type.IsSubclassOf(typeof(ModSound))) {
					modSounds.Add(type);
				}
				else if (type.IsSubclassOf(typeof(ModWorld))) {
					AutoloadModWorld(type);
				}
				else if (type.IsSubclassOf(typeof(ModUgBgStyle))) {
					AutoloadUgBgStyle(type);
				}
				else if (type.IsSubclassOf(typeof(ModSurfaceBgStyle))) {
					AutoloadSurfaceBgStyle(type);
				}
				else if (type.IsSubclassOf(typeof(GlobalBgStyle))) {
					AutoloadGlobalBgStyle(type);
				}
				else if (type.IsSubclassOf(typeof(ModWaterStyle))) {
					AutoloadWaterStyle(type);
				}
				else if (type.IsSubclassOf(typeof(ModWaterfallStyle))) {
					AutoloadWaterfallStyle(type);
				}
				else if (type.IsSubclassOf(typeof(GlobalRecipe))) {
					AutoloadGlobalRecipe(type);
				}
				else if (type.IsSubclassOf(typeof(ModCommand))) {
					AutoloadCommand(type);
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

		private void AutoloadItem(Type type) {
			ModItem item = (ModItem)Activator.CreateInstance(type);
			item.mod = this;
			string name = type.Name;
			if (item.Autoload(ref name)) {
				AddItem(name, item);
				var autoloadEquip = type.GetAttribute<AutoloadEquip>();
				if (autoloadEquip != null)
					foreach (var equip in autoloadEquip.equipTypes)
						AddEquipTexture(item, equip, item.Name, item.Texture + '_' + equip,
							item.Texture + "_Arms", item.Texture + "_FemaleBody");
			}
		}

		private void AutoloadGlobalItem(Type type) {
			GlobalItem globalItem = (GlobalItem)Activator.CreateInstance(type);
			globalItem.mod = this;
			string name = type.Name;
			if (globalItem.Autoload(ref name)) {
				AddGlobalItem(name, globalItem);
			}
		}

		private void AutoloadPrefix(Type type) {
			ModPrefix prefix = (ModPrefix)Activator.CreateInstance(type);
			prefix.mod = this;
			string name = type.Name;
			if (prefix.Autoload(ref name)) {
				AddPrefix(name, prefix);
			}
		}

		private void AutoloadDust(Type type) {
			ModDust dust = (ModDust)Activator.CreateInstance(type);
			dust.mod = this;
			string name = type.Name;
			string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
			if (dust.Autoload(ref name, ref texture)) {
				AddDust(name, dust, texture);
			}
		}

		private void AutoloadTile(Type type) {
			ModTile tile = (ModTile)Activator.CreateInstance(type);
			tile.mod = this;
			string name = type.Name;
			string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
			if (tile.Autoload(ref name, ref texture)) {
				AddTile(name, tile, texture);
			}
		}

		private void AutoloadGlobalTile(Type type) {
			GlobalTile globalTile = (GlobalTile)Activator.CreateInstance(type);
			globalTile.mod = this;
			string name = type.Name;
			if (globalTile.Autoload(ref name)) {
				AddGlobalTile(name, globalTile);
			}
		}

		private void AutoloadTileEntity(Type type) {
			ModTileEntity tileEntity = (ModTileEntity)Activator.CreateInstance(type);
			tileEntity.mod = this;
			string name = type.Name;
			if (tileEntity.Autoload(ref name)) {
				AddTileEntity(name, tileEntity);
			}
		}

		private void AutoloadWall(Type type) {
			ModWall wall = (ModWall)Activator.CreateInstance(type);
			wall.mod = this;
			string name = type.Name;
			string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
			if (wall.Autoload(ref name, ref texture)) {
				AddWall(name, wall, texture);
			}
		}

		private void AutoloadGlobalWall(Type type) {
			GlobalWall globalWall = (GlobalWall)Activator.CreateInstance(type);
			globalWall.mod = this;
			string name = type.Name;
			if (globalWall.Autoload(ref name)) {
				AddGlobalWall(name, globalWall);
			}
		}

		private void AutoloadProjectile(Type type) {
			ModProjectile projectile = (ModProjectile)Activator.CreateInstance(type);
			projectile.mod = this;
			string name = type.Name;
			if (projectile.Autoload(ref name)) {
				AddProjectile(name, projectile);
			}
		}

		private void AutoloadGlobalProjectile(Type type) {
			GlobalProjectile globalProjectile = (GlobalProjectile)Activator.CreateInstance(type);
			globalProjectile.mod = this;
			string name = type.Name;
			if (globalProjectile.Autoload(ref name)) {
				AddGlobalProjectile(name, globalProjectile);
			}
		}

		private void AutoloadNPC(Type type) {
			ModNPC npc = (ModNPC)Activator.CreateInstance(type);
			npc.mod = this;
			string name = type.Name;
			if (npc.Autoload(ref name)) {
				AddNPC(name, npc);
				var autoloadHead = type.GetAttribute<AutoloadHead>();
				if (autoloadHead != null) {
					string headTexture = npc.HeadTexture;
					AddNPCHeadTexture(npc.npc.type, headTexture);
				}
				var autoloadBossHead = type.GetAttribute<AutoloadBossHead>();
				if (autoloadBossHead != null) {
					string headTexture = npc.BossHeadTexture;
					AddBossHeadTexture(headTexture, npc.npc.type);
				}
			}
		}

		private void AutoloadGlobalNPC(Type type) {
			GlobalNPC globalNPC = (GlobalNPC)Activator.CreateInstance(type);
			globalNPC.mod = this;
			string name = type.Name;
			if (globalNPC.Autoload(ref name)) {
				AddGlobalNPC(name, globalNPC);
			}
		}

		private void AutoloadPlayer(Type type) {
			ModPlayer player = (ModPlayer)Activator.CreateInstance(type);
			player.mod = this;
			string name = type.Name;
			if (player.Autoload(ref name)) {
				AddPlayer(name, player);
			}
		}

		private void AutoloadBuff(Type type) {
			ModBuff buff = (ModBuff)Activator.CreateInstance(type);
			buff.mod = this;
			string name = type.Name;
			string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
			if (buff.Autoload(ref name, ref texture)) {
				AddBuff(name, buff, texture);
			}
		}

		private void AutoloadGlobalBuff(Type type) {
			GlobalBuff globalBuff = (GlobalBuff)Activator.CreateInstance(type);
			globalBuff.mod = this;
			string name = type.Name;
			if (globalBuff.Autoload(ref name)) {
				AddGlobalBuff(name, globalBuff);
			}
		}

		private void AutoloadMountData(Type type) {
			ModMountData mount = (ModMountData)Activator.CreateInstance(type);
			mount.mod = this;
			string name = type.Name;
			string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
			var extraTextures = new Dictionary<MountTextureType, string>();
			foreach (MountTextureType textureType in Enum.GetValues(typeof(MountTextureType))) {
				extraTextures[textureType] = texture + "_" + textureType.ToString();
			}
			if (mount.Autoload(ref name, ref texture, extraTextures)) {
				AddMount(name, mount, texture, extraTextures);
			}
		}

		private void AutoloadModWorld(Type type) {
			ModWorld modWorld = (ModWorld)Activator.CreateInstance(type);
			modWorld.mod = this;
			string name = type.Name;
			if (modWorld.Autoload(ref name)) {
				AddModWorld(name, modWorld);
			}
		}

		private void AutoloadBackgrounds() {
			foreach (string texture in textures.Keys.Where(t => t.StartsWith("Backgrounds/"))) {
				AddBackgroundTexture(Name + '/' + texture);
			}
		}

		private void AutoloadUgBgStyle(Type type) {
			ModUgBgStyle ugBgStyle = (ModUgBgStyle)Activator.CreateInstance(type);
			ugBgStyle.mod = this;
			string name = type.Name;
			if (ugBgStyle.Autoload(ref name)) {
				AddUgBgStyle(name, ugBgStyle);
			}
		}

		private void AutoloadSurfaceBgStyle(Type type) {
			ModSurfaceBgStyle surfaceBgStyle = (ModSurfaceBgStyle)Activator.CreateInstance(type);
			surfaceBgStyle.mod = this;
			string name = type.Name;
			if (surfaceBgStyle.Autoload(ref name)) {
				AddSurfaceBgStyle(name, surfaceBgStyle);
			}
		}

		private void AutoloadGlobalBgStyle(Type type) {
			GlobalBgStyle globalBgStyle = (GlobalBgStyle)Activator.CreateInstance(type);
			globalBgStyle.mod = this;
			string name = type.Name;
			if (globalBgStyle.Autoload(ref name)) {
				AddGlobalBgStyle(name, globalBgStyle);
			}
		}

		private void AutoloadWaterStyle(Type type) {
			ModWaterStyle waterStyle = (ModWaterStyle)Activator.CreateInstance(type);
			waterStyle.mod = this;
			string name = type.Name;
			string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
			string blockTexture = texture + "_Block";
			if (waterStyle.Autoload(ref name, ref texture, ref blockTexture)) {
				AddWaterStyle(name, waterStyle, texture, blockTexture);
			}
		}

		private void AutoloadWaterfallStyle(Type type) {
			ModWaterfallStyle waterfallStyle = (ModWaterfallStyle)Activator.CreateInstance(type);
			waterfallStyle.mod = this;
			string name = type.Name;
			string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
			if (waterfallStyle.Autoload(ref name, ref texture)) {
				AddWaterfallStyle(name, waterfallStyle, texture);
			}
		}

		private void AutoloadGores(IList<Type> modGores) {
			var modGoreNames = modGores.ToDictionary(t => t.Namespace + "." + t.Name);
			foreach (var texture in textures.Keys.Where(t => t.StartsWith("Gores/"))) {
				ModGore modGore = null;
				Type t;
				if (modGoreNames.TryGetValue(Name + "." + texture.Replace('/', '.'), out t))
					modGore = (ModGore)Activator.CreateInstance(t);

				AddGore(Name + '/' + texture, modGore);
			}
		}

		private void AutoloadSounds(IList<Type> modSounds) {
			var modSoundNames = modSounds.ToDictionary(t => t.Namespace + "." + t.Name);
			foreach (var sound in sounds.Keys.Where(t => t.StartsWith("Sounds/"))) {
				string substring = sound.Substring("Sounds/".Length);
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
				Type t;
				if (modSoundNames.TryGetValue((Name + '/' + sound).Replace('/', '.'), out t))
					modSound = (ModSound)Activator.CreateInstance(t);

				AddSound(soundType, Name + '/' + sound, modSound);
			}
			foreach (var music in musics.Keys.Where(t => t.StartsWith("Sounds/"))) {
				string substring = music.Substring("Sounds/".Length);
				if (substring.StartsWith("Music/")) {
					AddSound(SoundType.Music, Name + '/' + music);
				}
			}
		}

		private void AutoloadGlobalRecipe(Type type) {
			GlobalRecipe globalRecipe = (GlobalRecipe)Activator.CreateInstance(type);
			globalRecipe.mod = this;
			string name = type.Name;
			if (globalRecipe.Autoload(ref name)) {
				AddGlobalRecipe(name, globalRecipe);
			}
		}

		private void AutoloadCommand(Type type) {
			var mc = (ModCommand)Activator.CreateInstance(type);
			mc.mod = this;
			var name = type.Name;
			if (mc.Autoload(ref name))
				AddCommand(name, mc);
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
						string value = line.Substring(split + 1).Trim();
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
