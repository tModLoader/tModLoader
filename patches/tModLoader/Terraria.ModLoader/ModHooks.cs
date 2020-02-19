using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader
{
	public abstract partial class Mod
	{
		/// <summary>
		/// Allows you to determine what music should currently play.
		/// </summary>
		/// <param name="music">The music.</param>
		/// <param name="priority">The music priority.</param>
		public virtual void UpdateMusic(ref int music, ref MusicPriority priority) {
			UpdateMusic(ref music);
		}

		/// <summary>
		/// A legacy hook that you should no longer use. Use the version with two parameters instead.
		/// </summary>
		/// <param name="music"></param>
		[Obsolete("This UpdateMusic method now obsolete, use the UpdateMusic with the MusicPriority parameter.")]
		public virtual void UpdateMusic(ref int music) {
		}

		/// <summary>
		/// Called when a hotkey is pressed. Check against the name to verify particular hotkey that was pressed. (Using the ModHotKey is more recommended.)
		/// </summary>
		/// <param name="name">The display name of the hotkey.</param>
		public virtual void HotKeyPressed(string name) {
		}

		/// <summary>
		/// Called whenever a net message / packet is received from a client (if this is a server) or the server (if this is a client). whoAmI is the ID of whomever sent the packet (equivalent to the Main.myPlayer of the sender), and reader is used to read the binary data of the packet.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="whoAmI">The player the message is from.</param>
		public virtual void HandlePacket(BinaryReader reader, int whoAmI) {
		}

		/// <summary>
		/// Allows you to modify net message / packet information that is received before the game can act on it.
		/// </summary>
		/// <param name="messageType">Type of the message.</param>
		/// <param name="reader">The reader.</param>
		/// <param name="playerNumber">The player number the message is from.</param>
		/// <returns></returns>
		public virtual bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber) {
			return false;
		}

		/// <summary>
		/// Hijacks the send data method. Only use if you absolutely know what you are doing. If any hooks return true, the message is not sent.
		/// </summary>
		public virtual bool HijackSendData(int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7) {
			return false;
		}

		/// <summary>
		/// Allows you to set the transformation of the screen that is drawn. (Translations, rotations, scales, etc.)
		/// </summary>
		public virtual void ModifyTransformMatrix(ref SpriteViewMatrix Transform) {
		}

		/// <summary>
		/// Ran every update and suitable for calling Update for UserInterface classes
		/// </summary>
		public virtual void UpdateUI(GameTime gameTime) {
		}

		/// <summary>
		/// Use this if you want to do something before anything in the World gets updated.
		/// Called after UI updates, but before anything in the World (Players, NPCs, Projectiles, Tiles) gets updated.
		/// <para />
		/// When <see cref="Main.autoPause" /> is true or <see cref="Main.FrameSkipMode" /> is 0 or 2, the game may do a partial update. This means that it only updates menus and some animations, but not the World or Entities. This hook - and every hook after it - only gets called on frames with a full update.
		/// <para />
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdatePlayerNPC" />.
		/// </summary>
		public virtual void PreUpdateEntities() {
		}

		/// <summary>
		/// Called after Players got updated, but before any NPCs get updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="PreUpdateEntities" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateNPCGore" />.
		/// </summary>
		public virtual void MidUpdatePlayerNPC() {
		}

		/// <summary>
		/// Called after NPCs got updated, but before any Gores get updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdatePlayerNPC" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateGoreProjectile" />.
		/// </summary>
		public virtual void MidUpdateNPCGore() {
		}

		/// <summary>
		/// Called after Gores got updated, but before any Projectiles get updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateNPCGore" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateProjectileItem" />.
		/// </summary>
		public virtual void MidUpdateGoreProjectile() {
		}

		/// <summary>
		/// Gets called immediately after all Projectiles are updated, but before any Items get updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateGoreProjectile" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateItemDust" />.
		/// </summary>
		public virtual void MidUpdateProjectileItem() {
		}

		/// <summary>
		/// Called after Items got updated, but before any Dust gets updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateProjectileItem" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateDustTime" />.
		/// </summary>
		public virtual void MidUpdateItemDust() {
		}

		/// <summary>
		/// Called after Dust got updated, but before Time (day/night, events, etc.) gets updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateItemDust" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateTimeWorld" />.
		/// </summary>
		public virtual void MidUpdateDustTime() {
		}

		/// <summary>
		/// Called after Time got updated, but before the World gets updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateDustTime" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateInvasionNet" />.
		/// </summary>
		public virtual void MidUpdateTimeWorld() {
		}

		/// <summary>
		/// Called after Invasions got updated. The only thing that is updated after this is the Network.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateTimeWorld" />.
		/// If you are looking to hook even after the Network is updated, see <see cref="PostUpdateEverything" />.
		/// </summary>
		public virtual void MidUpdateInvasionNet() {
		}

		/// <summary>
		/// Called after the Network got updated, this is the last hook that happens in an update.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateInvasionNet" />.
		/// </summary>
		public virtual void PostUpdateEverything() {
		}

		/// <summary>
		/// Allows you to modify the elements of the in-game interface that get drawn. GameInterfaceLayer can be found in the Terraria.UI namespace. Check https://github.com/tModLoader/tModLoader/wiki/Vanilla-Interface-layers-values for vanilla interface layer names
		/// </summary>
		/// <param name="layers">The layers.</param>
		public virtual void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
		}

		/// <summary>
		/// Allows you to modify color of light the sun emits.
		/// </summary>
		/// <param name="tileColor">Tile lighting color</param>
		/// <param name="backgroundColor">Background lighting color</param>
		public virtual void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor) {
		}

		/// <summary>
		/// Allows you to modify overall brightness of lights. Can be used to create effects similiar to what night vision and darkness (de)buffs give you. Values too high or too low might result in glitches. For night vision effect use scale 1.03
		/// </summary>
		/// <param name="scale">Brightness scale</param>
		public virtual void ModifyLightingBrightness(ref float scale) {
		}

		/// <summary>
		/// Called after interface is drawn but right before mouse and mouse hover text is drawn. Allows for drawing interface.
		/// 
		/// Note: This hook should no longer be used. It is better to use the ModifyInterfaceLayers hook.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch.</param>
		public virtual void PostDrawInterface(SpriteBatch spriteBatch) {
		}

		/// <summary>
		/// Called while the fullscreen map is active. Allows custom drawing to the map.
		/// </summary>
		/// <param name="mouseText">The mouse text.</param>
		public virtual void PostDrawFullscreenMap(ref string mouseText) {
		}

		/// <summary>
		/// Called after the input keys are polled. Allows for modifying things like scroll wheel if your custom drawing should capture that.
		/// </summary>
		public virtual void PostUpdateInput() {
		}

		/// <summary>
		/// Called in SP or Client when the Save and Quit button is pressed. One use for this hook is clearing out custom UI slots to return items to the player.  
		/// </summary>
		public virtual void PreSaveAndQuit() {
		}
	}

	internal static class ModHooks
	{
		//in Terraria.Main.UpdateMusic before updating music boxes call ModHooks.UpdateMusic(ref this.newMusic);
		internal static void UpdateMusic(ref int music, ref MusicPriority priority) {
			foreach (Mod mod in ModLoader.Mods) {
				int modMusic = -1;
				MusicPriority modPriority = MusicPriority.BiomeLow;
				mod.UpdateMusic(ref modMusic, ref modPriority);
				if (modMusic >= 0 && modPriority >= priority) {
					music = modMusic;
					priority = modPriority;
				}
			}
		}

		// Pretty much deprecated. 
		internal static void HotKeyPressed() {
			foreach (var modHotkey in HotKeyLoader.HotKeys) {
				if (PlayerInput.Triggers.Current.KeyStatus[modHotkey.uniqueName]) {
					modHotkey.mod.HotKeyPressed(modHotkey.name);
				}
			}
		}

		internal static void ModifyTransformMatrix(ref SpriteViewMatrix Transform) {
			foreach (Mod mod in ModLoader.Mods) {
				mod.ModifyTransformMatrix(ref Transform);
			}
		}

		internal static void ModifySunLight(ref Color tileColor, ref Color backgroundColor) {
			if (Main.gameMenu) return;
			foreach (Mod mod in ModLoader.Mods) {
				mod.ModifySunLightColor(ref tileColor, ref backgroundColor);
			}
		}

		internal static void ModifyLightingBrightness(ref float negLight, ref float negLight2) {
			float scale = 1f;
			foreach (Mod mod in ModLoader.Mods) {
				mod.ModifyLightingBrightness(ref scale);
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

		internal static void PostDrawFullscreenMap(ref string mouseText) {
			foreach (Mod mod in ModLoader.Mods) {
				mod.PostDrawFullscreenMap(ref mouseText);
			}
		}

		internal static void UpdateUI(GameTime gameTime) {
			if (Main.gameMenu) return;
			foreach (Mod mod in ModLoader.Mods) {
				mod.UpdateUI(gameTime);
			}
		}

		public static void PreUpdateEntities() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.PreUpdateEntities();
			}
		}

		public static void MidUpdatePlayerNPC() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.MidUpdatePlayerNPC();
			}
		}

		public static void MidUpdateNPCGore() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.MidUpdateNPCGore();
			}
		}

		public static void MidUpdateGoreProjectile() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.MidUpdateGoreProjectile();
			}
		}

		public static void MidUpdateProjectileItem() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.MidUpdateProjectileItem();
			}
		}

		public static void MidUpdateItemDust() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.MidUpdateItemDust();
			}
		}

		public static void MidUpdateDustTime() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.MidUpdateDustTime();
			}
		}

		public static void MidUpdateTimeWorld() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.MidUpdateTimeWorld();
			}
		}

		public static void MidUpdateInvasionNet() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.MidUpdateInvasionNet();
			}
		}

		public static void PostUpdateEverything() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.PostUpdateEverything();
			}
		}

		internal static void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			foreach (GameInterfaceLayer layer in layers) {
				layer.Active = true;
			}
			foreach (Mod mod in ModLoader.Mods) {
				mod.ModifyInterfaceLayers(layers);
			}
		}

		internal static void PostDrawInterface(SpriteBatch spriteBatch) {
			foreach (Mod mod in ModLoader.Mods) {
				mod.PostDrawInterface(spriteBatch);
			}
		}

		internal static void PostUpdateInput() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.PostUpdateInput();
			}
		}

		internal static void PreSaveAndQuit() {
			foreach (Mod mod in ModLoader.Mods) {
				mod.PreSaveAndQuit();
			}
		}
	}
}
