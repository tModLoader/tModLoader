using System;
using System.IO;

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

		internal static void HotKeyPressed(string key)
		{
			// Key is name, value is keycode
			foreach (var item in ModLoader.modHotKeys)
			{
				if (item.Value.Item2.Equals(key))
				{
					foreach (Mod mod in ModLoader.mods.Values)
					{
						mod.HotKeyPressed(item.Key);
					}
				}
			}
		}
	}
}
