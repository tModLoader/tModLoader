using System;

namespace Terraria.ModLoader
{
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
	}
}
