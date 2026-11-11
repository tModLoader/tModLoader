using System;
using System.IO;
using System.Linq;
using System.Text;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader.Default;

internal class GlobalSubstitutionsCommand : ModCommand
{
	public override string Command => "globalsubstitutions";
	public override CommandType Type => CommandType.Chat | CommandType.Console;
	public override string Description => Language.GetTextValue("tModLoader.CommandGlobalSubstitutionsDescription");
	public override string Usage => Language.GetTextValue("tModLoader.CommandGlobalSubstitutionsUsage");

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		bool openFile = false;
		string searchTerm = null;
		for (int i = 0; i < args.Length; i++) {
			if (args[i] == "-h") {
				caller.Reply(Usage);
				return;
			}
			else if (args[i] == "-o") {
				openFile = true;
			}
			else {
				searchTerm = args[i];
			}
		}

		var sb = new StringBuilder();
		if (searchTerm != null) {
			sb.AppendLine($"Outputting all global substitutions from the search term '{searchTerm}':");
		}

		foreach (var globalSubstitution in Lang.GetGlobalSubstitutions()) {
			if(searchTerm != null && !globalSubstitution.Key.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) {
				continue;
			}

			string metadata = $"{globalSubstitution.Key}: {globalSubstitution.Value()}";
			sb.AppendLine(metadata);
		}

		string outputText = sb.ToString();
		caller.Reply(outputText);

		string outputPath = Path.Combine(Logging.LogDir, "GlobalSubstitutions.txt");
		File.WriteAllText(outputPath, outputText);
		if (openFile) {
			Utils.OpenFolder(Logging.LogDir);
		}

		caller.Reply($"Data written to '{outputPath}'");
	}
}
