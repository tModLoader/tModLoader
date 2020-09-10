using ExampleMod.Content.Tiles.Furniture;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleMagicWeapon : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is an example magic weapon");
		}

		public override void SetDefaults() {
			item.damage = 25;
			item.magic = true; // Makes the damage register as magic. If your item does not have any damage type, it becomes true damage (which means that damage scalars will not affect it). Be sure to have a damage type.
			item.width = 34;
			item.height = 40;
			item.useTime = 20;
			item.useAnimation = 20;
			item.useStyle = ItemUseStyleID.Shoot; // Makes the player use a 'Shoot' use style for the item.
			item.noMelee = true; // Makes the item not do damage with it's melee hitbox.
			item.knockBack = 6;
			item.value = 10000;
			item.rare = ItemRarityID.LightRed;
			item.UseSound = SoundID.Item71;
			item.autoReuse = true;
			item.shoot = ProjectileID.BlackBolt; // Shoot a black bolt, also known as the projectile shot from the onyx blaster.
			item.shootSpeed = 7; // How fast the item shoots the projectile.
			item.crit = 32; // The percent chance at hitting an enemy with a crit, plus the default amount of 4.
			item.mana = 11; // This is how much mana the item uses.
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.FallenStar, 5)
				.AddIngredient<ExampleItem>()
				.AddTile<ExampleWorkbench>()
				.Register();
		}
	}
}
