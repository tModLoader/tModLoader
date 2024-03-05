using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default;

public class UnloadedPlayer : ModPlayer
{
	internal IList<TagCompound> data;
	internal IList<TagCompound> unloadedResearch;

	public override void Initialize()
	{
		data = new List<TagCompound>();
		unloadedResearch = new List<TagCompound>();
	}

	public override void SaveData(TagCompound tag)
	{
		tag["list"] = data;
		tag["unloadedResearch"] = unloadedResearch;
	}

	public override void LoadData(TagCompound tag)
	{
		PlayerIO.LoadModData(Player, tag.GetList<TagCompound>("list"));
		PlayerIO.LoadResearch(Player, tag.GetList<TagCompound>("unloadedResearch"));
	}

	public override void OnEnterWorld()
	{
		if (Main.netMode != 1 && Main.ActiveWorldFileData.ModSaveErrors.Any()) {
			string fullError = Language.GetTextValue("tModLoader.WorldCustomDataSaveFail") + "\n" + string.Join("\n", Main.ActiveWorldFileData.ModSaveErrors.Select(x => $"{x.Key}: {x.Value}"));
			Main.NewText(fullError, Microsoft.Xna.Framework.Color.OrangeRed);
		}
		if(Player.saveErrorMessage != null) {
			// Main.NewText won't work in MP, DisplayMessageOnClient will cache the message if needed.
			Chat.ChatHelper.DisplayMessageOnClient(NetworkText.FromLiteral(Player.saveErrorMessage), Microsoft.Xna.Framework.Color.OrangeRed, Main.myPlayer);
			Logging.tML.Warn(Player.saveErrorMessage);
		}
	}
}
