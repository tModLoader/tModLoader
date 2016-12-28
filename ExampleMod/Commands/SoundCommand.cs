using System;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class SoundCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;
		public override string Command => "sound";
		public override string Usage => "/sound type style";
		public override bool Show => false;
		public override bool VerifyArguments(string[] args) => args.Length == 2;

		public override void Action(string[] args)
		{
			int type;
			if (!Int32.TryParse(args[0], out type))
			{
				Main.NewText(args[0] + " is not an integer");
				return;
			}
			int style;
			if (!Int32.TryParse(args[1], out style))
			{
				Main.NewText(args[1] + " is not an integer");
				return;
			}
			Main.PlaySound(type, -1, -1, style);
		}
	}
}