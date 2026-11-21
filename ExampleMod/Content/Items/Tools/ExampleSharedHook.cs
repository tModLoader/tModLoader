using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Tools;

internal class ExampleSharedHookItem : ModItem
{
	public override string Texture => $"Terraria/Images/Item_{ItemID.AmethystHook}";

	public override void SetDefaults() {
		Item.CloneDefaults(ItemID.AmethystHook);
		Item.shoot = ModContent.ProjectileType<ExampleSharedHookProjectile>();
	}

	public override void AddRecipes() {
		CreateRecipe()
			.AddIngredient<ExampleItem>()
			.AddTile<Tiles.Furniture.ExampleWorkbench>()
			.Register();
	}
}

internal class ExampleSharedHookProjectile : ModProjectile
{
	public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.GemHookAmethyst}";

	public override void SetDefaults() {
		Projectile.CloneDefaults(ProjectileID.GemHookAmethyst);
	}

	public override void ModifyGrappleLimits(Player player, ref int launchBudget, ref int replacementThreshold) {
		// Uses vanilla's shared count, unlike ExampleHook's same-type count.
		launchBudget = 4;
	}

	public override void NumGrappleHooks(Player player, ref int numHooks) {
		numHooks = 2; // Attachment limits are separate from launch limits.
	}
}
