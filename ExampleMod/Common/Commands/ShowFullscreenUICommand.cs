using ExampleMod.Common.UI.ExampleFullscreenUI;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExampleMod.Common.Commands
{
	// This command will show ExampleFullscreenUI
	public class ShowFullscreenUICommand : ModCommand
	{
		public static LocalizedText CommandDescription { get; private set; }

		public override CommandType Type
			=> CommandType.Chat;
		public override string Command
			=> "fullscreenui";

		public override string Description
			=> CommandDescription.Value; // "Show the example fullscreen UI";

		public override void SetStaticDefaults() {
			CommandDescription = Mod.GetLocalization($"Commands.{nameof(ShowFullscreenUICommand)}.Description");
		}

		public override void Action(CommandCaller caller, string input, string[] args) {
			IngameFancyUI.OpenUIState(ExampleFullscreenUI.instance);

			// Since ExampleFullscreenUI is ILoadable, the following would also work:
			// IngameFancyUI.OpenUIState(ContentInstance<ExampleFullscreenUI>.Instance);
		}
	}
}
