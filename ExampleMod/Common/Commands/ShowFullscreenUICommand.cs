using ExampleMod.Common.UI.ExampleFullscreenUI;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExampleMod.Common.Commands
{
	// This command will show ExampleFullscreenUI
	public class ShowFullscreenUICommand : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;
		public override string Command
			=> "fullscreenui";

		public override string Description
			=> "Show the example fullscreen UI";

		public override void Action(CommandCaller caller, string input, string[] args) {
			IngameFancyUI.OpenUIState(ExampleFullscreenUI.instance);
		}
	}
}
