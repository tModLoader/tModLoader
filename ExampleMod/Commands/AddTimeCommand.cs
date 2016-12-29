using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class AddTimeCommand : ModCommand
	{
		public override CommandType Type
		{
			get { return CommandType.Chat; }
		}

		public override string Command
		{
			get { return "addTime"; }
		}

		public override string Usage
		{
			get { return "/addTime numTicks"; }
		}

		public override bool Show
		{
			get { return false; }
		}

		public override bool VerifyArguments(string[] args)
		{
			int amount;
			return args.Length == 1 && int.TryParse(args[0], out amount);
		}

		public override void Action(string[] args)
		{
			Main.time += int.Parse(args[0]);
		}
	}
}