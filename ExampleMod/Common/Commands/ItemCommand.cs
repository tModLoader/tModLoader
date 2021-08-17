using System;
using Terraria;
using Terraria.ModLoader;

#nullable enable
namespace ExampleMod.Common.Commands
{
	public class ItemCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "item";

		public override string Usage =>
			"/item <type|name> [stack]" +
			"\nReplace spaces in item name with underscores";

		public override string Description => "Spawn an item";

		public override void Action(CommandCaller caller, string input, string[] args) {
			// If no arguments are supplied this will throw and print usage
			string name = args[0];

			if (!int.TryParse(name, out int type)) {
				name = name.Replace("_", " ");
				for (int i = 1; i < ItemLoader.ItemCount; i++) {
					if (name.Equals(Lang.GetItemNameValue(i), StringComparison.OrdinalIgnoreCase)) {
						type = i;
						break;
					}
				}
			}

			if (type == 0 || type >= ItemLoader.ItemCount) {
				throw new UsageException($"Unknown item: {name}");
			}

			int stack = 1;
			if (args.Length >= 2 && !int.TryParse(args[1], out stack)) {
				throw new UsageException("Stack must be a whole number");
			}

			caller.Player.QuickSpawnItem(type, stack);
		}
	}
}