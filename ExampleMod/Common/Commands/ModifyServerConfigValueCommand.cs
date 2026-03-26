using ExampleMod.Common.Configs.ModConfigShowcases;
using System.Linq;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Common.Commands
{
	// This example, like ExampleFullscreenUI.cs, modifies a loaded ModConfig. This example simply shows that calling ModConfig.SaveChanges on the server for ServerSide configs is supported. (Since this command is CommandType.World, it runs on the server in multiplayer.)
	public class ModifyServerConfigValueCommand : ModCommand
	{
		public static LocalizedText UsageText { get; private set; }
		public static LocalizedText DescriptionText { get; private set; }
		public static LocalizedText[] ErrorText { get; private set; }

		public override void SetStaticDefaults() {
			string key = $"Commands.{nameof(ModifyServerConfigValueCommand)}.";
			UsageText = Mod.GetLocalization($"{key}Usage");
			DescriptionText = Mod.GetLocalization($"{key}Description");
			ErrorText = Enumerable.Range(0, 2).Select(i => Mod.GetLocalization($"{key}Error_{i}")).ToArray();
		}

		// CommandType.World means that command can be used in Chat in SP and MP, but executes on the Server in MP
		public override CommandType Type => CommandType.World;
		public override string Command => "setSomeNumber";
		public override string Usage => UsageText.Value;
		public override string Description => DescriptionText.Value;

		public override void Action(CommandCaller caller, string input, string[] args) {
			// Checking input Arguments
			if (args.Length == 0) {
				throw new UsageException(ErrorText[0].Value);
			}
			if (!int.TryParse(args[0], out int userInput)) {
				throw new UsageException(ErrorText[1].Format(args[0]));
			}

			var config = ModContent.GetInstance<ModConfigShowcaseAcceptClientChanges>();
			config.SomeNumber = userInput;
			config.SaveChanges();

			/* If you need the status, you could do this instead:
			(string text, Color color) statusResult = ("", Color.White);
			config.SaveChanges(status: (text, color) => { statusResult = (text, color); });
			caller.Reply($"SaveChanges status: {statusResult.text}", statusResult.color);
			*/
		}
	}
}
