using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.GameInput;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
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
		public virtual void UpdateMusic(ref int music)
		{
		}

		/// <summary>
		/// Called when a hotkey is pressed. Check against the name to verify particular hotkey that was pressed. (Using the ModHotKey is more recommended.)
		/// </summary>
		/// <param name="name">The display name of the hotkey.</param>
		public virtual void HotKeyPressed(string name)
		{
		}

		/// <summary>
		/// Called whenever a net message / packet is received from a client (if this is a server) or the server (if this is a client). whoAmI is the ID of whomever sent the packet (equivalent to the Main.myPlayer of the sender), and reader is used to read the binary data of the packet.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="whoAmI">The player the message is from.</param>
		public virtual void HandlePacket(BinaryReader reader, int whoAmI)
		{
		}

		/// <summary>
		/// Allows you to modify net message / packet information that is received before the game can act on it.
		/// </summary>
		/// <param name="messageType">Type of the message.</param>
		/// <param name="reader">The reader.</param>
		/// <param name="playerNumber">The player number the message is from.</param>
		/// <returns></returns>
		public virtual bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber)
		{
			return false;
		}

		/// <summary>
		/// Hijacks the send data method. Only use if you absolutely know what you are doing. If any hooks return true, the message is not sent.
		/// </summary>
		public virtual bool HijackSendData(int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
		{
			return false;
		}

		/// <summary>
		/// Allows you to set the transformation of the screen that is drawn. (Translations, rotations, scales, etc.)
		/// </summary>
		public virtual Matrix ModifyTransformMatrix(Matrix Transform)
		{
			return Transform;
		}

		/// <summary>
		/// Allows you to modify the elements of the in-game interface that get drawn. MethodSequenceListItem can be found in the Terraria.DataStructures namespace. Check https://github.com/blushiemagic/tModLoader/wiki/Vanilla-Interface-layers-values for vanilla interface layer names
		/// </summary>
		/// <param name="layers">The layers.</param>
		public virtual void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
		}

		/// <summary>
		/// Allows you to modify color of light the sun emits.
		/// </summary>
		/// <param name="tileColor">Tile lighting color</param>
		/// <param name="backgroundColor">Background lighting color</param>
		public virtual void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
		{
		}

		/// <summary>
		/// Allows you to modify overall brightness of lights. Can be used to create effects similiar to what night vision and darkness (de)buffs give you. Values too high or too low might result in glitches. For night vision effect use scale 1.03
		/// </summary>
		/// <param name="scale">Brightness scale</param>
		public virtual void ModifyLightingBrightness(ref float scale)
		{
		}

		/// <summary>
		/// Called after interface is drawn but right before mouse and mouse hover text is drawn. Allows for drawing interface.
		/// 
		/// Note: This hook should no longer be used. It is better to use the ModifyInterfaceLayers hook.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch.</param>
		public virtual void PostDrawInterface(SpriteBatch spriteBatch)
		{
		}

		/// <summary>
		/// Called while the fullscreen map is active. Allows custom drawing to the map.
		/// </summary>
		/// <param name="mouseText">The mouse text.</param>
		public virtual void PostDrawFullscreenMap(ref string mouseText)
		{
		}

		/// <summary>
		/// Called after the input keys are polled. Allows for modifying things like scroll wheel if your custom drawing should capture that.
		/// </summary>
		public virtual void PostUpdateInput()
		{
		}

		/// <summary>
		/// Called in SP or Client when the Save and Quit button is pressed. One use for this hook is clearing out custom UI slots to return items to the player.  
		/// </summary>
		public virtual void PreSaveAndQuit()
		{
		}
	}

	internal static class ModHooks
	{
		//in Terraria.Main.UpdateMusic before updating music boxes call ModHooks.UpdateMusic(ref this.newMusic);
		internal static void UpdateMusic(ref int music)
		{
			foreach (Mod mod in ModLoader.mods.Values)
			{
				mod.UpdateMusic(ref music);
			}
		}

		// Pretty much deprecated. 
		internal static void HotKeyPressed()
		{
			foreach (var modHotkey in ModLoader.modHotKeys)
			{
				if (PlayerInput.Triggers.Current.KeyStatus[modHotkey.Value.displayName])
				{
					modHotkey.Value.mod.HotKeyPressed(modHotkey.Value.name);
				}
			}
		}

		internal static Matrix ModifyTransformMatrix(Matrix Transform)
		{
			foreach (Mod mod in ModLoader.mods.Values)
			{
				Transform = mod.ModifyTransformMatrix(Transform);
			}
			return Transform;
		}

		internal static void ModifySunLight(ref Color tileColor, ref Color backgroundColor)
		{
			if (Main.gameMenu) return;
			foreach (Mod mod in ModLoader.mods.Values)
			{
				mod.ModifySunLightColor(ref tileColor, ref backgroundColor);
			}
		}

		internal static void ModifyLightingBrightness(ref float negLight, ref float negLight2)
		{
			float scale = 1f;
			foreach (Mod mod in ModLoader.mods.Values)
			{
				mod.ModifyLightingBrightness(ref scale);
			}
			if (Lighting.NotRetro)
			{
				negLight *= scale;
				negLight2 *= scale;
			}
			else
			{
				negLight -= (scale - 1f) / 2.307692307692308f;
				negLight2 -= (scale - 1f) / 0.75f;
			}
			negLight = Math.Max(negLight, 0.001f);
			negLight2 = Math.Max(negLight2, 0.001f);
		}

		internal static void PostDrawFullscreenMap(ref string mouseText)
		{
			foreach (Mod mod in ModLoader.mods.Values)
			{
				mod.PostDrawFullscreenMap(ref mouseText);
			}
		}

		internal static void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			foreach (GameInterfaceLayer layer in layers)
			{
				layer.Active = true;
			}
			foreach (Mod mod in ModLoader.mods.Values)
			{
				mod.ModifyInterfaceLayers(layers);
			}
		}

		internal static void PostDrawInterface(SpriteBatch spriteBatch)
		{
			foreach (Mod mod in ModLoader.mods.Values)
			{
				mod.PostDrawInterface(spriteBatch);
			}
		}

		internal static void PostUpdateInput()
		{
			foreach (Mod mod in ModLoader.mods.Values)
			{
				mod.PostUpdateInput();
			}
		}

		internal static void PreSaveAndQuit()
		{
			foreach (Mod mod in ModLoader.mods.Values)
			{
				mod.PreSaveAndQuit();
			}
		}
	}
}
