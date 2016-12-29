using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class SoundCommand : ModCommand
	{
		public override CommandType Type
		{
			get { return CommandType.Chat; }
		}

		public override string Command
		{
			get { return "sound"; }
		}

		public override string Usage
		{
			get { return "/sound type style"; }
		}

		public override bool Show
		{
			get { return false; }
		}

		public override bool VerifyArguments(string[] args)
		{
			return args.Length == 2;
		}

		public override void Action(string[] args)
		{
			int type;
			if (!int.TryParse(args[0], out type))
			{
				Main.NewText(args[0] + " is not an integer");
				return;
			}
			int style;
			if (!int.TryParse(args[1], out style))
			{
				Main.NewText(args[1] + " is not an integer");
				return;
			}
			Main.PlaySound(type, -1, -1, style);
		}
	}
}