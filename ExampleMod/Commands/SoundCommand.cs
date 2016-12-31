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

		public override string Description 
		{
			get { return "Play sounds by id"; }
		}

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			int type;
			if (!int.TryParse(args[0], out type))
				throw new UsageException(args[0] + " is not an integer");
			
			int style;
			if (!int.TryParse(args[1], out style))
				throw new UsageException(args[1] + " is not an integer");
			
			Main.PlaySound(type, -1, -1, style);
		}
	}
}