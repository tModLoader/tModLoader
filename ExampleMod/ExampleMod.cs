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
			// The unload method can be used for unloading/disposing/clearing special objects, unsubscribing from events, or for undoing some of your mod's actions.
			// Be sure to always write unloading code when there is a chance of some of your mod's objects being kept present inside the vanilla assembly.
			// The most common reason for that to happen comes from using events, NOT counting On.* and IL.* TerrariaHooks namespaces. If you subscribe to one - be sure to eventually unsubscribe from it. 
			// However, there is rarely a need to null values of static fields, since TML aims to completely dispose mod assemblies in-between mod reloads.
		}
	}
}