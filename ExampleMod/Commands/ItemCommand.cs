using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class ItemCommand : ModCommand
	{
		public override CommandType Type
		{
			get { return CommandType.Chat; }
		}

		public override string Command
		{
			get { return "item"; }
		}

		public override string Usage
		{
			get { return "/item [type|name] [stack]"; }
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
			var player = Main.LocalPlayer;
			int type;
			if (!int.TryParse(args[0], out type))
			{
				args[0] = args[0].Replace("_", " ");
				for (var k = 0; k < Main.itemName.Length; k++)
					if (args[0] == Main.itemName[k])
					{
						type = k;
						break;
					}
			}
			int stack;
			if (args.Length < 2 || !int.TryParse(args[1], out stack))
				stack = 1;
			player.QuickSpawnItem(type, stack);
		}
	}
}