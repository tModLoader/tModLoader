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
	}
}
