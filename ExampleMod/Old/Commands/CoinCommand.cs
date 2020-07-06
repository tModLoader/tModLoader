using ExampleMod.UI;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class CoinCommand : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "coin";

		public override string Description
			=> "Show the coin rate UI";

		public override void Action(CommandCaller caller, string input, string[] args) {
			ExampleUI.Visible = true;
		}
	}
}