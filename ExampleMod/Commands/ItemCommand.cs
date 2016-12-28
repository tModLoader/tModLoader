using System;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class ItemCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;
		public override string Command => "item";
		public override string Usage => "/item [type|name] [stack]";
		public override bool Show => false;
		public override bool VerifyArguments(string[] args) => args.Length == 2;
		public override void Action(string[] args)
		{
			Player player = Main.LocalPlayer;
			int type;
			if (!Int32.TryParse(args[0], out type))
			{
				args[0] = args[0].Replace("_", " ");
				for (int k = 0; k < Main.itemName.Length; k++)
				{
					if (args[0] == Main.itemName[k])
					{
						type = k;
						break;
					}
				}
			}
			int stack;
			if (args.Length < 2 || !Int32.TryParse(args[1], out stack))
			{
				stack = 1;
			}
			player.QuickSpawnItem(type, stack);
		}
	}
}