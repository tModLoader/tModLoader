using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class ItemCommand : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "item";

		public override string Usage
			=> "/item <type|name> [stack]" +
			   "\nReplace spaces in item name with underscores";

		public override string Description
			=> "Spawn an item";

		public override void Action(CommandCaller caller, string input, string[] args) {
			if (!int.TryParse(args[0], out int type)) {
				var name = args[0].Replace("_", " ");
				var item = new Item();
				for (var k = 0; k < ItemLoader.ItemCount; k++) {
					item.SetDefaults(k, true);
					if (name != Lang.GetItemNameValue(k)) {
						continue;
					}

					type = k;
					break;
				}

				if (type == 0) {
					throw new UsageException("Unknown item: " + name);
				}
			}

			int stack = 1;
			if (args.Length >= 2) {
				stack = int.Parse(args[1]);
			}

			caller.Player.QuickSpawnItem(type, stack);
		}
	}
}