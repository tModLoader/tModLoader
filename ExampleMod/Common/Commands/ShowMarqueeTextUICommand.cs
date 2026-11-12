using ExampleMod.Common.UI.ExampleMarqueeText;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExampleMod.Common.Commands
{
	// This command will show ExampleMarqueeTextUI
	public class ShowMarqueeTextUICommand : ModCommand
	{
		public static LocalizedText CommandDescription { get; private set; }

		public override CommandType Type
			=> CommandType.Chat;
		public override string Command
			=> "marqueetextdemo";

		public override string Description
			=> CommandDescription.Value; // "Show the example marquee text UI";

		public override void SetStaticDefaults() {
			CommandDescription = Mod.GetLocalization($"Commands.{nameof(ShowFullscreenUICommand)}.Description");
		}

		public override void Action(CommandCaller caller, string input, string[] args) {
			IngameFancyUI.OpenUIState(ExampleMarqueeTextUI.instance);
		}
	}
}
