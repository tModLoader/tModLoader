using ExampleMod.Content.Projectiles;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleWhip : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.SummonerWeaponThatScalesWithAttackSpeed[Type] = true; // This makes the item be affected by the user's melee speed.
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
			// Item.DefaultToWhip(ModContent.ProjectileType<ExampleWhipProjectile>(), 20, 2, 4); // Call this method to quickly set the whip's properties.

			Item.DamageType = DamageClass.Summon;
			Item.damage = 20;
			Item.knockBack = 2;
			Item.rare = ItemRarityID.Green;

			Item.shoot = ModContent.ProjectileType<ExampleWhipProjectile>();
			Item.shootSpeed = 4;

			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.UseSound = SoundID.Item152;
			Item.channel = true; // This is used for the charging functionality. Remove it if your whip shouldn't be chargeable.
			Item.noMelee = true;
			Item.noUseGraphic = true;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
