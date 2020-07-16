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
			Tooltip.SetDefault("This is a modded sword."); //The (English) text shown below your weapon's name
		}

		public override void SetDefaults() {
			item.width = 40; //The item texture's width
			item.height = 40; //The item texture's height

			item.useStyle = ItemUseStyleID.Swing; //The useStyle of the item.
			item.useTime = 20; //The time span of using the weapon. Remember in terraria, 60 frames is a second.
			item.useAnimation = 20; //The time span of the using animation of the weapon, suggest setting it the same as useTime.
			item.autoReuse = true; //Whether the weapon can be used more than once automatically by holding the use button.

			item.melee = true; //Whether your item is part of the melee class
			item.damage = 50; //The damage your item deals
			item.knockBack = 6; //The force of knockback of the weapon. Maximum is 20
			item.crit = 6; //The critical strike chance the weapon has. The player, by default, has a 4% critical strike chance.

			item.value = Item.buyPrice(gold: 1); //The value of the weapon in copper coins.
			item.rare = ItemRarityID.Green; //The rarity of the weapon.
			item.UseSound = SoundID.Item1; //The sound when the weapon is being used.
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(Mod); //This creates a new ModRecipe, associated with the mod that this content piece comes from.
			recipe.AddIngredient(ItemType<ExampleItem>(), 10); //ItemType<Class Of A Modded Item>() returns the id of the provided ModItem class' item. Here, 10 is the amount of that ingredient required for this recipe.
			recipe.AddIngredient(ItemID.Wood); //You can use ItemID.TheItemYouWantToUse to get IDs of vanilla items. Note that the amount argument is ommited here, defaulting to 1.
			recipe.AddTile(TileType<ExampleWorkbench>()); //Set the crafting tile to ExampleWorkbench
			recipe.SetResult(this); //Set the result to this item (ExampleSword)
			recipe.AddRecipe(); //When you're done, call this to register the recipe.
		}

		public override void MeleeEffects(Player player, Rectangle hitbox) {
			if (Main.rand.NextBool(3)) {
				//Emit dusts when the sword is swung
				Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustType<Dusts.Sparkle>());
			}
		}

		public override void OnHitNPC(Player player, NPC target, int damage, float knockback, bool crit) {
			//Inflict the OnFire debuff for 1 second onto any NPC/Monster that this hits.
			//60 frames = 1 second
			target.AddBuff(BuffID.OnFire, 60);
		}
	}
}