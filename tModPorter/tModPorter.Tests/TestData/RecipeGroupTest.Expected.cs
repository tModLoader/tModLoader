using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

public class RecipeGroupTest : ModSystem
{
	public static RecipeGroup GelLike;

	public override void AddRecipeGroups()
	{
		// not-yet-implemented
		GelLike = RecipeGroup.Register("TestMod:Gels", "Gel-like items", ItemID.Gel, ItemID.GelDye);

		RecipeGroup SilverBarRecipeGroup = RecipeGroup.Register(nameof(ItemID.SilverBar), Lang.GetItemNameValue(ItemID.SilverBar), ItemID.SilverBar, ItemID.TungstenBar, ModContent.ItemType<global::ModItemTest>());
		// instead-expect
#if COMPILE_ERROR
		GelLike = new RecipeGroup(
			() => $"{Language.GetTextValue("LegacyMisc.37")} Gel-like items",
			ItemID.Gel, ItemID.GelDye
		);
		RecipeGroup.RegisterGroup("TestMod:Gels", GelLike)/* tModPorter Note: Removed. Replace this and "new RecipeGroup()" with RecipeGroup.Register */;

		RecipeGroup SilverBarRecipeGroup = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.SilverBar)}",
		ItemID.SilverBar, ItemID.TungstenBar, ModContent.ItemType<global::ModItemTest>());
		RecipeGroup.RegisterGroup(nameof(ItemID.SilverBar), SilverBarRecipeGroup)/* tModPorter Note: Removed. Replace this and "new RecipeGroup()" with RecipeGroup.Register */;
#endif

		// not-yet-implemented
		RecipeGroups.Sand.ValidItems.Add(ItemID.SandstoneBrick);
		// instead-expect
#if COMPILE_ERROR
		RecipeGroup.recipeGroups[RecipeGroups.Sand].ValidItems.Add(ItemID.SandstoneBrick);
#endif
	}

	public override void AddRecipes()
	{
		Recipe.Create(ItemID.Gel)
			.AddRecipeGroup(RecipeGroups.Wood)
			.AddRecipeGroup(RecipeGroups.IronBar, 2)
			.AddRecipeGroup(GelLike, 2)
			.AddRecipeGroup("Wood")
			.AddRecipeGroup("TestMod:Gels", 2)
			.AddRecipeGroup(nameof(ItemID.SilverBar))
			.Register();
	}
}
