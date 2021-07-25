﻿using Microsoft.Xna.Framework.Audio;
using ReLogic.Content;
using ReLogic.Content.Sources;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Terraria.Localization;
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

			ModSourceBestiaryInfoElement = new GameContent.Bestiary.ModSourceBestiaryInfoElement(this, DisplayName);

			IList<Type> modSounds = new List<Type>();

			Type modType = GetType();
			foreach (Type type in Code.GetTypes().OrderBy(type => type.FullName, StringComparer.InvariantCulture)) {
				if (type == modType) continue;
				if (type.IsAbstract) continue;
				if (type.ContainsGenericParameters) continue;
				if (type.GetConstructor(new Type[0]) == null) continue;//don't autoload things with no default constructor

				if (type.IsSubclassOf(typeof(ModSound))) {
					modSounds.Add(type);
				}
				else if (typeof(ILoadable).IsAssignableFrom(type)) {
					var autoload = AutoloadAttribute.GetValue(type);
					if (autoload.NeedsAutoloading) {
						AddContent((ILoadable)Activator.CreateInstance(type));
					}
				}
			}

			// Skip loading client assets if this is a dedicated server;
			if (Main.dedServ)
				return;

			if (Properties.AutoloadGores)
				GoreLoader.AutoloadGores(this);
			
			if (Properties.AutoloadSounds)
				AutoloadSounds(modSounds);

			if (Properties.AutoloadBackgrounds)
				BackgroundTextureLoader.AutoloadBackgrounds(this);
		}

		internal void PrepareAssets()
		{
			fileHandle = File?.Open();
			RootContentSource = CreateDefaultContentSource();
			Assets = new AssetRepository(Main.instance.Services.Get<AssetReaderCollection>(), new[] { RootContentSource }) {
				AssetLoadFailHandler = Main.OnceFailedLoadingAnAsset
			};
		}

		private void AutoloadSounds(IList<Type> modSounds) {
			var modSoundNames = modSounds.ToDictionary(t => t.FullName);

			const string SoundFolder = "Sounds/";

			foreach (string soundPath in RootContentSource.EnumerateAssets().Where(t => t.Contains(SoundFolder))) {
				string substring = soundPath.Substring(soundPath.IndexOf(SoundFolder) + SoundFolder.Length);
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

			// todo: musics never set/added to
			foreach (string music in musics.Keys.Where(t => t.StartsWith("Sounds/"))) {
				string substring = music.Substring("Sounds/".Length);

				if (substring.StartsWith("Music/")) {
					AddSound(SoundType.Music, Name + '/' + music);
				}
			}
		}

		internal void TransferAllAssets() {
			Assets.TransferAllAssets();
		}
	}
}
