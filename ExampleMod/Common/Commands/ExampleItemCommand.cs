using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Common.Commands
{
	public class ExampleItemCommand : ModCommand
	{
		public static LocalizedText UsageText { get; private set; }
		public static LocalizedText DescriptionText { get; private set; }
		public static LocalizedText[] ErrorText { get; private set; }

		public override void SetStaticDefaults() {
			string key = $"Commands.{nameof(ExampleItemCommand)}.";
			UsageText = Mod.GetLocalization($"{key}Usage");
			DescriptionText = Mod.GetLocalization($"{key}Description");
			ErrorText = Enumerable.Range(0, 3).Select(i => Mod.GetLocalization($"{key}Error_{i}")).ToArray();
		}

		// CommandType.Chat means that command can be used in Chat in SP and MP
		public override CommandType Type
			=> CommandType.Chat;

		// The desired text to trigger this command
		public override string Command
			=> "item";

		// A short usage explanation for this command
		public override string Usage
			=> UsageText.Value;

		// A short description of this command
		public override string Description
			=> DescriptionText.Value;

		public override void Action(CommandCaller caller, string input, string[] args) {
			// Checking input Arguments
			if (args.Length == 0)
				throw new UsageException(ErrorText[0].Value);

			// If we can't parse the int, it means we have a name (or a wrong use of the command)
			// In that case type be equal to 0
			if (!int.TryParse(args[0], out int type)) {
				// Replacing the underscore in an element name with spaces
				string name = args[0].Replace("_", " ");

				// We go through all the subjects to find the required typeId
				// Only if the name of the item matches the desired one in the current localization (no case sensitive) 
				for (int k = 1; k < ItemLoader.ItemCount; k++) {
					if (name.ToLower() == Lang.GetItemNameValue(k).ToLower()) {
						type = k;
						break;
					}
				}
			}

			if (type <= 0 || type >= ItemLoader.ItemCount)
				throw new UsageException(ErrorText[1].Format(ItemLoader.ItemCount));

			// If the command has at least two arguments, we try to get the stack value
			// Default stack is 1
			int stack = 1;
			if (args.Length >= 2) {
				if (!int.TryParse(args[1], out stack))
					throw new UsageException(ErrorText[2].Format(args[1]));
			}

			// Spawn the item where the calling player is
			caller.Player.QuickSpawnItem(new EntitySource_DebugCommand($"{nameof(ExampleMod)}_{nameof(ExampleItemCommand)}"), type, stack);
		}
	}
}