using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace ExampleMod
{
	// This is a partial class, meaning some of its parts were split into other files. See ExampleMod.*.cs for other portions.
	public partial class ExampleMod : Mod
	{
		public const string AssetPath = $"{nameof(ExampleMod)}/Assets/";

		public static ModKeybind RandomBuffKeybind;
		public static int ExampleCustomCurrencyId;

		public override void Load() {
			// Registers a new keybind
			RandomBuffKeybind = KeybindLoader.RegisterKeybind(this, "Random Buff", "P");

			// Registers a new custom currency
			ExampleCustomCurrencyId = CustomCurrencyManager.RegisterCurrency(new Content.Currencies.ExampleCustomCurrency(ModContent.ItemType<Content.Items.ExampleItem>(), 999L, "Mods.ExampleMod.Currencies.ExampleCustomCurrency"));
		}

		public override void Unload() {
			RandomBuffKeybind = null;
		}
	}
}