using System;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class AddTimeCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;
		public override string Command => "addTime";
		public override string Usage => "/addTime numTicks";
		public override bool Show => false;

		public override bool VerifyArguments(string[] args)
		{
			int amount;
			return args.Length == 1 && int.TryParse(args[0], out amount);
		}

		public override void Action(string[] args) => Main.time += int.Parse(args[0]);
	}
}