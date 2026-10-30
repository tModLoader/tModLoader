using Terraria.ModLoader;

namespace ExampleMod.Content.CreativeCategories
{
	// A ModCreativeCategory adds a button to the main Journey Mode power strip, next to vanilla's Duplication, Research, Time, Weather and Personal categories.
	// Clicking it opens a strip with every ModCreativePower whose Category is this category's Type, here ExampleHealButton and ExampleRunSpeedSlider.
	// The icon on the button is ExampleCreativeCategory.png, found automatically because it's next to this file and has the same name.
	// Its name and description come from Mods.ExampleMod.CreativeCategories.ExampleCreativeCategory in the localization files. The description is shown when the player has power descriptions turned on, like vanilla categories.
	// For content that isn't a power, such as a window like vanilla's duplication window, override CreateContent and return the element to show.
	public class ExampleCreativeCategory : ModCreativeCategory
	{
	}
}
