using Microsoft.Xna.Framework;
using System.IO;
using System.Linq;
using System.Text;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader.Default;

internal class CustomSetsCommand : ModCommand
{
	public override string Command => "customsets";
	public override CommandType Type => CommandType.Chat | CommandType.Console;
	public override string Description => Language.GetTextValue("tModLoader.CommandCustomSetsDescription");

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		var sb = new StringBuilder();
		foreach (var factory in SetFactory.SetFactories) {
			string metadata = factory.CustomMetadataInfo();
			sb.Append(metadata);
		}

		string outputText = sb.ToString();
		caller.Reply(outputText);

		string outputPath = Path.Combine(Logging.LogDir, "CustomSets.txt");
		File.WriteAllText(outputPath, outputText);

		caller.Reply($"Data written to '{outputPath}'");
	}
}
