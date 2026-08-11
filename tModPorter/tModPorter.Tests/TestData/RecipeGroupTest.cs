using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

public class RecipeGroupTest : ModSystem
{
	public static RecipeGroup GelLike;

	public override void AddRecipeGroups()
	{
		GelLike = new RecipeGroup(
			() => $"{Language.GetTextValue("LegacyMisc.37")} Gel-like items",
			ItemID.Gel, ItemID.GelDye
		);
		RecipeGroup.RegisterGroup("TestMod:Gels", GelLike);

		RecipeGroup SilverBarRecipeGroup = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.SilverBar)}",
		ItemID.SilverBar, ItemID.TungstenBar, ModContent.ItemType<global::ModItemTest>());
		RecipeGroup.RegisterGroup(nameof(ItemID.SilverBar), SilverBarRecipeGroup);

		RecipeGroup.recipeGroups[RecipeGroupID.Sand].ValidItems.Add(ItemID.SandstoneBrick);
	}

	public override void AddRecipes()
	{
		Recipe.Create(ItemID.Gel)
			.AddRecipeGroup(RecipeGroupID.Wood)
			.AddRecipeGroup(RecipeGroupID.IronBar, 2)
			.AddRecipeGroup(GelLike, 2)
			.AddRecipeGroup("Wood")
			.AddRecipeGroup("TestMod:Gels", 2)
			.AddRecipeGroup(nameof(ItemID.SilverBar))
			.Register();
	}
}
