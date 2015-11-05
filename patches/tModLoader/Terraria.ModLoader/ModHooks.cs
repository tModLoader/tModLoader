using System;

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

		public virtual void ExtractinatorUse(ref int resultType, ref int resultStack, ref int extractType)
		{
		}

		public virtual void HotKeyPressed(string name)
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

		internal static void ExtractinatorUse(ref int resultType, ref int resultStack, ref int extractType)
		{
			foreach (Mod mod in ModLoader.mods.Values)
			{
				mod.ExtractinatorUse(ref resultType, ref resultStack, ref extractType);
			}
		}

		internal static void HotKeyPressed(string key)
		{
			// Key is name, value is keycode
			foreach (var item in ModLoader.modHotKeys)
			{
				if (item.Value.Item1.Equals(key))
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
