using ExampleMod.Content.Tiles.Furniture;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	/// <summary>
	///     Star Wrath/Starfury style weapon. Spawn projectiles from sky that aim towards mouse.
	///     See Source code for Star Wrath projectile to see how it passes through tiles.
	///     For a detailed sword guide see <see cref="ExampleSword" />
	/// </summary>
	public class ExampleShootingSword : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded sword that shoots Star Wrath-like projectiles.");
		}

		public override void SetDefaults() {
			item.width = 26;
			item.height = 42;

			item.useStyle = ItemUseStyleID.Swing;
			item.useTime = 20;
			item.useAnimation = 20;
			item.autoReuse = true;

			item.melee = true;
			item.damage = 50;
			item.knockBack = 6;
			item.crit = 6;

			item.value = Item.buyPrice(gold: 5);
			item.rare = ItemRarityID.Pink;
			item.UseSound = SoundID.Item1;

			item.shoot = ProjectileID.StarWrath; // ID of the projectiles the sword will shoot
			item.shootSpeed = 8f; // Speed of the projectiles the sword will shoot
		}
		// This method gets called when firing your weapon/sword.
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			Vector2 target = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY);
			float ceilingLimit = target.Y;
			if (ceilingLimit > player.Center.Y - 200f) {
				ceilingLimit = player.Center.Y - 200f;
			}
			// Loop these functions 3 times.
			for (int i = 0; i < 3; i++) {
				position = player.Center - new Vector2(Main.rand.NextFloat(401) * player.direction, 600f);
				position.Y -= 100 * i;
				Vector2 heading = target - position;

				if (heading.Y < 0f) {
					heading.Y *= -1f;
				}

				if (heading.Y < 20f) {
					heading.Y = 20f;
				}

				heading.Normalize();
				heading *= new Vector2(speedX, speedY).Length();
				speedX = heading.X;
				speedY = heading.Y + (Main.rand.Next(-40, 41) * 0.02f);
				Projectile.NewProjectile(position.X, position.Y, speedX, speedY, type, damage * 2, knockBack, player.whoAmI, 0f, ceilingLimit);
			}

			return false;
		}

		//Please see ExampleItem.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(100)
				.AddRecipeGroup(RecipeGroupID.Wood, 10)
				.AddTile<ExampleWorkbench>()
				.Register();
		}
	}
}
