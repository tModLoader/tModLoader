using ExampleMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleMagicRod : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is an example magic weapon");

			Item.staff[Type] = true; // Makes the weapon be held like the gem staves

			SacrificeTotal = 1;
		}

		public override void SetDefaults() {
			Item.damage = 20;
			Item.DamageType = DamageClass.Magic; // Makes the damage register as magic. If your item does not have any damage type, it becomes true damage (which means that damage scalars will not affect it). Be sure to have a damage type.
			Item.width = 26;
			Item.height = 26;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot; // Makes the player use a 'Shoot' use style for the Item.
			Item.noMelee = true; // Makes the item not do damage with it's melee hitbox.
			Item.knockBack = 3.1f;
			Item.value = 10000;
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item43;
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<ExampleSineWaveMotionProjectile>();
			Item.shootSpeed = 4; // How fast the item shoots the projectile.
			Item.mana = 18; // This is how much mana the item uses.
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			// Spawn two of the projectiles
			// The two projectiles will have a different wave parity and draw color
			Color[] colors = new Color[] { Color.Red, Color.Blue, Color.Yellow, Color.Green };
			int projectileCount = 4;

			for (int i = 0; i < projectileCount; i++) {
				// Be wary of dividing by zero when projectileCount is 1
				float waveOffset = i / (float)(projectileCount - 1);

				Projectile projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);

				ExampleSineWaveMotionProjectile modProjectile = projectile.ModProjectile as ExampleSineWaveMotionProjectile;
				modProjectile.waveOffset = waveOffset * (1f - 1f / projectileCount);  // Reduce the range so that there isn't an overlap of the first and last projectile
				modProjectile.drawColor = colors[i];
			}

			return false;
		}
	}
}
