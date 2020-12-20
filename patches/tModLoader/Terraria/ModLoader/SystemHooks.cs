using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.Graphics;
using Terraria.UI;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This is where all <see cref="ModSystem"/> hooks are gathered and called.
	/// </summary>
	//TODO: Use combined delegates whenever possible.
	public static class SystemHooks
	{
		internal static readonly List<ModSystem> Systems = new List<ModSystem>();
		internal static readonly Dictionary<string, List<ModSystem>> SystemsByMod = new Dictionary<string, List<ModSystem>>();

		internal static ModSystem[] NetSystems { get; private set; }

		internal static void Add(ModSystem modSystem) {
			string modName = modSystem.Mod.Name;

			if (!SystemsByMod.TryGetValue(modName, out var list)) {
				SystemsByMod[modName] = list = new List<ModSystem>();
			}

			list.Add(modSystem);
			Systems.Add(modSystem);
		}

		internal static void ResizeArrays() => NetSystems = ModLoader.BuildGlobalHook<ModSystem, Action<BinaryWriter>>(Systems, s => s.NetSend);

		internal static void Unload() {
			Systems.Clear();
			SystemsByMod.Clear();
		}

		internal static void WriteNetSystemOrder(BinaryWriter w) {
			w.Write((short)NetSystems.Length);

			foreach (var netWorld in NetSystems) {
				w.Write(netWorld.Mod.netID);
				w.Write(netWorld.Name);
			}
		}

		internal static void ReadNetSystemOrder(BinaryReader r) {
			short n = r.ReadInt16();

			NetSystems = new ModSystem[n];

			for (short i = 0; i < n; i++) {
				NetSystems[i] = ModContent.Find<ModSystem>(ModNet.GetMod(r.ReadInt16()).Name, r.ReadString());
			}
		}

		internal static void OnModLoad(Mod mod) {
			if (!SystemsByMod.TryGetValue(mod.Name, out var list)) {
				return;
			}

			foreach (var system in list) {
				system.OnModLoad();
			}
		}

		internal static void PostSetupContent(Mod mod) {
			if (!SystemsByMod.TryGetValue(mod.Name, out var list)) {
				return;
			}

			foreach (var system in list) {
				system.PostSetupContent();
			}
		}

		internal static void OnWorldLoad() {
			foreach (var system in Systems) {
				system.OnWorldLoad();
			}
		}

		public static void UpdateMusic(ref int music, ref MusicPriority priority) {
			foreach (var system in Systems) {
				system.UpdateMusic(ref music, ref priority);
			}
		}

		public static void ModifyTransformMatrix(ref SpriteViewMatrix Transform) {
			foreach (var system in Systems) {
				system.ModifyTransformMatrix(ref Transform);
			}
		}

		public static void ModifySunLight(ref Color tileColor, ref Color backgroundColor) {
			if (Main.gameMenu)
				return;

			foreach (var system in Systems) {
				system.ModifySunLightColor(ref tileColor, ref backgroundColor);
			}
		}

		public static void ModifyLightingBrightness(ref float negLight, ref float negLight2) {
			float scale = 1f;

			foreach (var system in Systems) {
				system.ModifyLightingBrightness(ref scale);
			}

			if (Lighting.NotRetro) {
				negLight *= scale;
				negLight2 *= scale;
			}
			else {
				negLight -= (scale - 1f) / 2.307692307692308f;
				negLight2 -= (scale - 1f) / 0.75f;
			}

			negLight = Math.Max(negLight, 0.001f);
			negLight2 = Math.Max(negLight2, 0.001f);
		}

		public static void PostDrawFullscreenMap(ref string mouseText) {
			foreach (var system in Systems) {
				system.PostDrawFullscreenMap(ref mouseText);
			}
		}

		public static void UpdateUI(GameTime gameTime) {
			if (Main.gameMenu)
				return;

			foreach (var system in Systems) {
				system.UpdateUI(gameTime);
			}
		}

		public static void PreUpdateEntities() {
			foreach (var system in Systems) {
				system.PreUpdateEntities();
			}
		}

		public static void PreUpdatePlayers() {
			foreach (var system in Systems) {
				system.PreUpdatePlayers();
			}
		}

		public static void PostUpdatePlayers() {
			foreach (var system in Systems) {
				system.PostUpdatePlayers();
			}
		}

		public static void PreUpdateNPCs() {
			foreach (var system in Systems) {
				system.PreUpdateNPCs();
			}
		}

		public static void PostUpdateNPCs() {
			foreach (var system in Systems) {
				system.PostUpdateNPCs();
			}
		}

		public static void PreUpdateGores() {
			foreach (var system in Systems) {
				system.PreUpdateGores();
			}
		}

		public static void PostUpdateGores() {
			foreach (var system in Systems) {
				system.PostUpdateGores();
			}
		}

		public static void PreUpdateProjectiles() {
			foreach (var system in Systems) {
				system.PreUpdateProjectiles();
			}
		}

		public static void PostUpdateProjectiles() {
			foreach (var system in Systems) {
				system.PostUpdateProjectiles();
			}
		}

		public static void PreUpdateItems() {
			foreach (var system in Systems) {
				system.PreUpdateItems();
			}
		}

		public static void PostUpdateItems() {
			foreach (var system in Systems) {
				system.PostUpdateItems();
			}
		}

		public static void PreUpdateDusts() {
			foreach (var system in Systems) {
				system.PreUpdateDusts();
			}
		}

		public static void PostUpdateDusts() {
			foreach (var system in Systems) {
				system.PostUpdateDusts();
			}
		}

		public static void PreUpdateTime() {
			foreach (var system in Systems) {
				system.PreUpdateTime();
			}
		}

		public static void PostUpdateTime() {
			foreach (var system in Systems) {
				system.PostUpdateTime();
			}
		}

		public static void PreUpdateWorld() {
			foreach (var system in Systems) {
				system.PreUpdateWorld();
			}
		}

		public static void PostUpdateWorld() {
			foreach (var system in Systems) {
				system.PostUpdateWorld();
			}
		}

		public static void PreUpdateInvasions() {
			foreach (var system in Systems) {
				system.PreUpdateInvasions();
			}
		}

		public static void PostUpdateInvasions() {
			foreach (var system in Systems) {
				system.PostUpdateInvasions();
			}
		}

		public static void PostUpdateEverything() {
			foreach (var system in Systems) {
				system.PostUpdateEverything();
			}
		}

		public static void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			foreach (GameInterfaceLayer layer in layers) {
				layer.Active = true;
			}

			foreach (var system in Systems) {
				system.ModifyInterfaceLayers(layers);
			}
		}

		public static void PostDrawInterface(SpriteBatch spriteBatch) {
			foreach (var system in Systems) {
				system.PostDrawInterface(spriteBatch);
			}
		}

		public static void PostUpdateInput() {
			foreach (var system in Systems) {
				system.PostUpdateInput();
			}
		}

		public static void PreSaveAndQuit() {
			foreach (var system in Systems) {
				system.PreSaveAndQuit();
			}
		}

		public static void PostDrawTiles() {
			foreach (var system in Systems) {
				system.PostDrawTiles();
			}
		}

		public static void ModifyTimeRate(ref int timeRate, ref int tileUpdateRate) {
			foreach (var system in Systems) {
				system.ModifyTimeRate(ref timeRate, ref tileUpdateRate);
			}
		}

		public static void PreWorldGen() {
			foreach (var system in Systems) {
				system.PreWorldGen();
			}
		}

		public static void ModifyWorldGenTasks(List<GenPass> passes, ref float totalWeight) {
			foreach (var system in Systems) {
				system.ModifyWorldGenTasks(passes, ref totalWeight);
			}
		}

		public static void PostWorldGen() {
			foreach (var system in Systems) {
				system.PostWorldGen();
			}
		}

		public static void ResetNearbyTileEffects() {
			foreach (var system in Systems) {
				system.ResetNearbyTileEffects();
			}
		}

		public static void TileCountsAvailable(int[] tileCounts) {
			foreach (var system in Systems) {
				system.TileCountsAvailable(tileCounts);
			}
		}

		public static void ChooseWaterStyle(ref int style) {
			foreach (var system in Systems) {
				system.ChooseWaterStyle(ref style);
			}
		}

		public static void ModifyHardmodeTasks(List<GenPass> passes) {
			foreach (var system in Systems) {
				system.ModifyHardmodeTasks(passes);
			}
		}
	}
}