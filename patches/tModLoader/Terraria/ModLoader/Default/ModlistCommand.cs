using Microsoft.Xna.Framework;
using System.Linq;
using Terraria.Chat;
using Terraria.Localization;

namespace Terraria.ModLoader.Default;

internal class ModlistCommand : ModCommand
{
	public override string Command => "modlist";
	//note that Chat | Server is a strange combination, as Chat overrides Server. Normally one would use World
	public override CommandType Type => CommandType.Chat | CommandType.Server | CommandType.Console;
	public override string Description => Language.GetTextValue("tModLoader.CommandModListDescription");
	public override void Action(CommandCaller caller, string input, string[] args)
	{
		var mods = ModLoader.Mods.Skip(1);//ignore the built in Modloader mod

		if (Main.netMode == 1) { //multiplayer client
			//send the command to the server
			ChatHelper.SendChatMessageFromClient(new ChatMessage(input));

			var client = mods.Where(m => m.Side == ModSide.Client || m.Side == ModSide.NoSync).ToArray();
			if (client.Length > 0) {
				caller.Reply(Language.GetTextValue("tModLoader.CommandModListClientMods"), Color.Yellow);
				foreach (var mod in client)
					caller.Reply(mod.DisplayName);
			}
		}
		else if (caller.CommandType == CommandType.Server) { //server from a player
			var server = mods.Where(m => m.Side == ModSide.Server || m.Side == ModSide.NoSync).ToArray();
			if (server.Length > 0) {
				caller.Reply(Language.GetTextValue("tModLoader.CommandModListServerMods"), Color.Yellow);
				foreach (var mod in server)
					caller.Reply(mod.DisplayName);
			}
			caller.Reply(Language.GetTextValue("tModLoader.CommandModListSyncedMods"), Color.Yellow);
			foreach (var mod in mods.Where(m => m.Side == ModSide.Both))
				caller.Reply(mod.DisplayName);
		}
		else { //console or singleplayer
			if (caller.CommandType == CommandType.Chat)
				caller.Reply(Language.GetTextValue("tModLoader.CommandModListModlist"), Color.Yellow);

			foreach (var mod in mods)
				caller.Reply(caller.CommandType == CommandType.Chat ? mod.DisplayName : mod.DisplayNameClean);
		}
	}
}
