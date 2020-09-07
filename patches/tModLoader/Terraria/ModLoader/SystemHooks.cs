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
	public static class SystemHooks
	{
		internal static readonly IList<ModSystem> systems = new List<ModSystem>();

		internal static void Add(ModSystem modSystem) => systems.Add(modSystem);

		internal static void Unload() => systems.Clear();

		public static void UpdateMusic(ref int music, ref MusicPriority priority) {
			foreach (ModSystem system in systems) {
				system.UpdateMusic(ref music, ref priority);
			}
		}

		public static void ModifyTransformMatrix(ref SpriteViewMatrix Transform) {
			foreach (ModSystem system in systems) {
				system.ModifyTransformMatrix(ref Transform);
			}
		}

		public static void ModifySunLight(ref Color tileColor, ref Color backgroundColor) {
			if (Main.gameMenu)
				return;

			foreach (ModSystem system in systems) {
				system.ModifySunLightColor(ref tileColor, ref backgroundColor);
			}
		}

		public static void ModifyLightingBrightness(ref float negLight, ref float negLight2) {
			float scale = 1f;

			foreach (ModSystem system in systems) {
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
			foreach (ModSystem system in systems) {
				system.PostDrawFullscreenMap(ref mouseText);
			}
		}

		public static void UpdateUI(GameTime gameTime) {
			if (Main.gameMenu)
				return;

			foreach (ModSystem system in systems) {
				system.UpdateUI(gameTime);
			}
		}

		public static void PreUpdateEntities() {
			foreach (ModSystem system in systems) {
				system.PreUpdateEntities();
			}
		}

		public static void MidUpdatePlayerNPC() {
			foreach (ModSystem system in systems) {
				system.MidUpdatePlayerNPC();
			}
		}

		public static void MidUpdateNPCGore() {
			foreach (ModSystem system in systems) {
				system.MidUpdateNPCGore();
			}
		}

		public static void MidUpdateGoreProjectile() {
			foreach (ModSystem system in systems) {
				system.MidUpdateGoreProjectile();
			}
		}

		public static void MidUpdateProjectileItem() {
			foreach (ModSystem system in systems) {
				system.MidUpdateProjectileItem();
			}
		}

		public static void MidUpdateItemDust() {
			foreach (ModSystem system in systems) {
				system.MidUpdateItemDust();
			}
		}

		public static void MidUpdateDustTime() {
			foreach (ModSystem system in systems) {
				system.MidUpdateDustTime();
			}
		}

		public static void MidUpdateTimeWorld() {
			foreach (ModSystem system in systems) {
				system.MidUpdateTimeWorld();
			}
		}

		public static void MidUpdateInvasionNet() {
			foreach (ModSystem system in systems) {
				system.MidUpdateInvasionNet();
			}
		}

		public static void PostUpdateEverything() {
			foreach (ModSystem system in systems) {
				system.PostUpdateEverything();
			}
		}

		public static void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			foreach (GameInterfaceLayer layer in layers) {
				layer.Active = true;
			}

			foreach (ModSystem system in systems) {
				system.ModifyInterfaceLayers(layers);
			}
		}

		public static void PostDrawInterface(SpriteBatch spriteBatch) {
			foreach (ModSystem system in systems) {
				system.PostDrawInterface(spriteBatch);
			}
		}

		public static void PostUpdateInput() {
			foreach (ModSystem system in systems) {
				system.PostUpdateInput();
			}
		}

		public static void PreSaveAndQuit() {
			foreach (ModSystem system in systems) {
				system.PreSaveAndQuit();
			}
		}
	}
}