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

		public override string Usage
		{
			get { return "/coin"; }
		}

		public override bool Show
		{
			get { return false; }
		}

		public override bool VerifyArguments(string[] args)
		{
			return args.Length == 0;
		}

		public override void Action(string[] args)
		{
			ExampleUI.visible = true;
		}
	}
}