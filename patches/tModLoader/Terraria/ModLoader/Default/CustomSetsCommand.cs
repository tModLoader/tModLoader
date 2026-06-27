using System.IO;
using System.Text;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader.Default;

internal class CustomSetsCommand : ModCommand
{
	public override string Command => "customsets";
	public override CommandType Type => CommandType.Chat | CommandType.Console;
	public override string Description => Language.GetTextValue("tModLoader.CommandCustomSetsDescription");
	public override string Usage => Language.GetTextValue("tModLoader.CommandCustomSetsUsage");

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		bool printValues = false;
		bool openFile = false;
		string searchTerm = null;
		for (int i = 0; i < args.Length; i++) {
			if (args[i] == "-h") {
				caller.Reply(Usage);
				return;
			}
			else if (args[i] == "-v") {
				printValues = true;
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
			sb.AppendLine($"Outputting all named ID sets from the search term '{searchTerm}':");
		}
		foreach (var factory in SetFactory.SetFactories) {
			string metadata = factory.CustomMetadataInfo(searchTerm, printValues);
			sb.Append(metadata);
		}

		string outputText = sb.ToString();
		caller.Reply(outputText);

		string outputPath = Path.Combine(Logging.LogDir, "CustomSets.txt");
		File.WriteAllText(outputPath, outputText);
		if (openFile) {
			Utils.OpenFolder(Logging.LogDir);
		}

		caller.Reply($"Data written to '{outputPath}'");
	}
}
