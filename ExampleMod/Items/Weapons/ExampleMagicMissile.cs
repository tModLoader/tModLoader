using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Weapons
{
	public class ExampleMagicMissile : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This magic weapon shoots missiles that follow your cursor."
				+ "\nIncreased mana usage during the day, decreased mana usage at night.");
		}

		public override void SetDefaults() {
			item.damage = 25;
			item.magic = true;
			item.mana = 14;
			item.width = 26;
			item.height = 26;
			item.useTime = 15;
			item.useAnimation = 15;
			item.useStyle = 1;
			item.noMelee = true;
			item.channel = true; //Channel so that you can held the weapon [Important]
			item.knockBack = 8;
			item.value = Item.sellPrice(silver : 50);
			item.rare = 3;
			item.UseSound = SoundID.Item9;
			item.shoot = mod.ProjectileType<Projectiles.MagicMissile>();
			item.shootSpeed = 10f;
		}

		// This item's mana usage changes through the day, peaking at 1.5x mana usage at noon, and 0.5x mana usage at midnight.
		public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
			float currentTime = (float)Main.time;
			// The time at which it changes from day to night and vice versa.
			int maxTime = Main.dayTime ? 54000 : 32400;
			// The time at which it is 12pm or 12am, which isn't exactly half the values above.
			int time12 = Main.dayTime ? 28800 : 18000;
			// If the time it after 12, recalculate the current time to account for the length of the second half of the day instead of the first.
			if (currentTime > time12) {
				currentTime -= time12;
				time12 = maxTime - time12;
				currentTime += time12;
			}
			// If it is night time, add twice the duration of the current half of the night.
			if (!Main.dayTime) {
				currentTime += time12 * 2;
			}
			// We divide the current time by the current half of the day or night's time, then by 2.
			// The result would gradually go from 0 at 4:30am, 0.5 at 12pm, 1 at 7:30pm and 1.5f at 12 am.
			float timeMult = currentTime / time12 / 2;
			// Then we multiply the result above by PI, and calculate the sine of that, then multiply it by 0.5f and add 1.
			// The result would go from 1 at 4:30am, 1.5 at 12pm, 1 at 7:30pm and 0.5f at 12 am, in a sine wave pattern.
			timeMult = 1 + (float)Math.Sin(timeMult * Math.PI) * 0.5f;
			// Last, we multiply the current mana cost multiplier of the item by our multiplier.
			mult *= timeMult;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(mod.ItemType("ExampleItem"), 20);
			recipe.AddTile(mod.TileType("ExampleWorkbench"));
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}