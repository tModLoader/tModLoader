using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.Graphics;
using Terraria.UI;

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

		internal static void Add(ModSystem modSystem) {
			string modName = modSystem.Mod.Name;

			if (!SystemsByMod.TryGetValue(modName, out var list)) {
				SystemsByMod[modName] = list = new List<ModSystem>();
			}

			list.Add(modSystem);
			Systems.Add(modSystem);
		}

		internal static void Unload() {
			Systems.Clear();
			SystemsByMod.Clear();
		}

		internal static void Load(Mod mod) {
			if (!SystemsByMod.TryGetValue(mod.Name, out var list)) {
				return;
			}

			foreach (var system in list) {
				system.Load();
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

		public static void MidUpdatePlayerNPC() {
			foreach (var system in Systems) {
				system.MidUpdatePlayerNPC();
			}
		}

		public static void MidUpdateNPCGore() {
			foreach (var system in Systems) {
				system.MidUpdateNPCGore();
			}
		}

		public static void MidUpdateGoreProjectile() {
			foreach (var system in Systems) {
				system.MidUpdateGoreProjectile();
			}
		}

		public static void MidUpdateProjectileItem() {
			foreach (var system in Systems) {
				system.MidUpdateProjectileItem();
			}
		}

		public static void MidUpdateItemDust() {
			foreach (var system in Systems) {
				system.MidUpdateItemDust();
			}
		}

		public static void MidUpdateDustTime() {
			foreach (var system in Systems) {
				system.MidUpdateDustTime();
			}
		}

		public static void MidUpdateTimeWorld() {
			foreach (var system in Systems) {
				system.MidUpdateTimeWorld();
			}
		}

		public static void MidUpdateInvasionNet() {
			foreach (var system in Systems) {
				system.MidUpdateInvasionNet();
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
	}
}