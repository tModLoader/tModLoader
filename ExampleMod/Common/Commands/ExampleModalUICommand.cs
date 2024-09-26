using ExampleMod.Common.UI.ExampleModalUI;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExampleMod.Common.Commands
{
	// This command will show ExampleModalUI
	public class ExampleModalUICommand : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;
		public override string Command
			=> "modal";

		public override string Description
			=> "Show the example modal UI";

		public override void Action(CommandCaller caller, string input, string[] args) {
			IngameFancyUI.OpenUIState(ModContent.GetInstance<ExampleModalUI>());
		}
	}
}
