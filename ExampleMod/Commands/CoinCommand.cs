using ExampleMod.UI;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class CoinCommand : ModCommand
	{
		public override CommandType Type
		{
			get { return CommandType.Chat; }
		}

		public override string Command
		{
			get { return "coin"; }
		}

		public override string Description 
		{
			get { return "Show the coin rate UI"; }
		}

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			ExampleUI.visible = true;
		}
	}
}