using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria.GameInput;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.ModLoader
{
	public abstract partial class Mod
	{
		public virtual void ChatInput(string text)
		{
		}

		public virtual void UpdateMusic(ref int music)
		{
		}

		public virtual void HotKeyPressed(string name)
		{
		}

		public virtual void HandlePacket(BinaryReader reader, int whoAmI)
		{
		}

		public virtual bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber)
		{
			return false;
		}

		public virtual Matrix ModifyTransformMatrix(Matrix Transform)
		{
			return Transform;
		}

		public virtual void PostDrawInterface(SpriteBatch spriteBatch)
		{
		}

		public virtual void PostDrawFullscreenMap(ref string mouseText)
		{
		}

		public virtual void PostUpdateInput()
		{
		}

		public virtual void FillUndergroundBackgroundArray(int backgroundStyle, int[] textureSlots)
		{
		}

		public virtual void ChooseUndergroundBackgroundStyle(ref int backgroundStyle)
		{
		}

		public virtual void ChooseSurfaceBackgroundStyle(ref int backgroundStyle)
		{
		}

		public virtual void ModifyFarSurfaceBackgroundFades(int backgroundStyle, float[] farBackgroundFades, float transitionSpeed)
		{
		}

		public virtual void ChooseFarSurfaceBackground(int backgroundStyle, ref int backgroundSlot)
		{
		}

		public virtual void ChooseMiddleSurfaceBackground(int backgroundStyle, ref int backgroundSlot)
		{
		}

		public virtual void DrawCloseSurfaceBackgroundManual(int backgroundStyle, ref bool manual)
		{
		}

		public virtual void ChooseCloseSurfaceBackground(int backgroundStyle, ref int backgroundSlot, ref float scale, ref double parallax, ref float a, ref float b)
		{
		}
	}

	internal static class ModHooks
	{
		//in Terraria.Main.do_Update after processing chat input call ModHooks.ChatText(Main.chatText);
		//in Terraria.Main.do_Update for if statement checking whether chat can be opened remove Main.netMode == 1
		internal static void ChatInput(string text)
		{
			if (text.Length > 0)
			{
				foreach (Mod mod in ModLoader.mods.Values)
				{
					mod.ChatInput(text);
				}
			}
		}
		//in Terraria.Main.UpdateMusic before updating music boxes call ModHooks.UpdateMusic(ref this.newMusic);
		internal static void UpdateMusic(ref int music)
		{
			foreach (Mod mod in ModLoader.mods.Values)
			{
				mod.UpdateMusic(ref music);
			}
		}

		internal static void HotKeyPressed()
		{
			foreach (var modHotkey in ModLoader.modHotKeys)
			{
				if (PlayerInput.Triggers.Current.KeyStatus[modHotkey.Value.displayName])
				{
					modHotkey.Value.mod.HotKeyPressed(modHotkey.Value.name);
				}
				// TODO - KeyDown, KeyUp, Down?
				//if (PlayerInput.Triggers.JustPressed.KeyStatus[modHotkey.Value.name])
				//{
				//}
				//if (PlayerInput.Triggers.JustReleased.KeyStatus[modHotkey.Value.name])
				//{
				//}
				//if (PlayerInput.Triggers.Old.KeyStatus[modHotkey.Value.name])
				//{
				//}
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

		internal static void PostDrawFullscreenMap(ref string mouseText)
		{
			foreach (Mod mod in ModLoader.mods.Values)
			{
				mod.PostDrawFullscreenMap(ref mouseText);
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

		internal static void FillUndergroundBackgroundArray(int backgroundStyle, int[] textureSlots)
		{
			foreach (Mod mod in ModLoader.mods.Values)
			{
				mod.FillUndergroundBackgroundArray(backgroundStyle, textureSlots);
			}
		}

		internal static void ChooseUndergroundBackgroundStyle(ref int backgroundStyle)
		{
			foreach (Mod mod in ModLoader.mods.Values)
			{
				mod.ChooseUndergroundBackgroundStyle(ref backgroundStyle);
			}
		}

		internal static void ChooseSurfaceBackgroundStyle(ref int backgroundStyle)
		{
			foreach (Mod mod in ModLoader.mods.Values)
			{
				mod.ChooseSurfaceBackgroundStyle(ref backgroundStyle);
			}
		}

		internal static void ModifyFarSurfaceBackgroundFades(int backgroundStyle, float[] farBackgroundFades, float transitionSpeed)
		{
			foreach (Mod mod in ModLoader.mods.Values)
			{
				mod.ModifyFarSurfaceBackgroundFades(backgroundStyle, farBackgroundFades, transitionSpeed);
			}
		}

		internal static void ChooseFarSurfaceBackground(int backgroundStyle, ref int backgroundSlot)
		{
			foreach (Mod mod in ModLoader.mods.Values)
			{
				mod.ChooseFarSurfaceBackground(backgroundStyle, ref backgroundSlot);
			}
		}

		internal static void ChooseMiddleSurfaceBackground(int backgroundStyle, ref int backgroundSlot)
		{
			foreach (Mod mod in ModLoader.mods.Values)
			{
				mod.ChooseMiddleSurfaceBackground(backgroundStyle, ref backgroundSlot);
			}
		}

		internal static void DrawCloseSurfaceBackgroundManual(int backgroundStyle, ref bool manual)
		{
			foreach (Mod mod in ModLoader.mods.Values)
			{
				mod.DrawCloseSurfaceBackgroundManual(backgroundStyle, ref manual);
			}
		}

		internal static void ChooseCloseSurfaceBackground(int backgroundStyle, ref int backgroundSlot, ref float scale, ref double parallax, ref float a, ref float b)
		{
			foreach (Mod mod in ModLoader.mods.Values)
			{
				mod.ChooseCloseSurfaceBackground(backgroundStyle, ref backgroundSlot, ref scale, ref parallax, ref a, ref b);
			}
		}
	}
}
