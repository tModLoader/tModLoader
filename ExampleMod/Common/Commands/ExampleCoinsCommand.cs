using ExampleMod.Common.UI.ExampleCoinsUI;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Common.Commands
{
	public class ExampleCoinsCommand : ModCommand
	{
		public static LocalizedText DescriptionText { get; private set; }

		public override void SetStaticDefaults() {
			DescriptionText = Mod.GetLocalization($"Commands.{nameof(ExampleCoinsCommand)}.Description");
		}

		// CommandType.Chat means that command can be used in Chat in SP and MP
		public override CommandType Type
			=> CommandType.Chat;

		// The desired text to trigger this command
		public override string Command
			=> "coins";

		// A short description of this command
		public override string Description
			=> DescriptionText.Value;

		public override void Action(CommandCaller caller, string input, string[] args) {
			ModContent.GetInstance<ExampleCoinsUISystem>().ShowMyUI();
		}
	}
}