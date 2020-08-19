using ExampleMod.Content.Rarities;
using ExampleMod.Content.Tiles.Furniture;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent; //This lets us access methods (like ItemType) from ModContent without having to type its name.

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleSword : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded sword."); //The (English) text shown below your weapon's name.
		}

		public override void SetDefaults() {
			item.width = 40; //The item texture's width.
			item.height = 40; //The item texture's height.

			item.useStyle = ItemUseStyleID.Swing; // The useStyle of the item.
			item.useTime = 20; // The time span of using the weapon. Remember in terraria, 60 frames is a second.
			item.useAnimation = 20; // The time span of the using animation of the weapon, suggest setting it the same as useTime.
			item.autoReuse = true; // Whether the weapon can be used more than once automatically by holding the use button.

			item.melee = true; //Whether your item is part of the melee class.
			item.damage = 50; //The damage your item deals.
			item.knockBack = 6; //The force of knockback of the weapon. Maximum is 20
			item.crit = 6; //The critical strike chance the weapon has. The player, by default, has a 4% critical strike chance.

			item.value = Item.buyPrice(gold: 1); //The value of the weapon in copper coins.
			item.rare = RarityType<ExampleModRarity>(); // Give this item our custom rarity.
			item.UseSound = SoundID.Item1; //The sound when the weapon is being used.
		}

		public override void MeleeEffects(Player player, Rectangle hitbox) {
			if (Main.rand.NextBool(3)) {
				// Emit dusts when the sword is swung
				Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustType<Dusts.Sparkle>());
			}
		}

		public override void OnHitNPC(Player player, NPC target, int damage, float knockback, bool crit) {
			// Inflict the OnFire debuff for 1 second onto any NPC/Monster that this hits.
			// 60 frames = 1 second
			target.AddBuff(BuffID.OnFire, 60);
		}

		// Please see ExampleItem.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(10)
				.AddIngredient(ItemID.Wood)
				.AddTile<ExampleWorkbench>()
				.Register();
		}
	}
}
