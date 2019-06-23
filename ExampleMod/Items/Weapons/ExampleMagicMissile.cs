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
		public override void GetManaCost(Player player, ref int mana) {
			float currentTime = (float)Main.time;
			int maxTime = Main.dayTime ? 54000 : 32400;
			int time12 = Main.dayTime ? 28800 : 18000;
			if (currentTime > time12) {
				time12 = maxTime - time12;
				currentTime = maxTime - currentTime;
			}
			float multi = 1 + currentTime / time12 * 0.5f * Main.dayTime.ToDirectionInt();
			mana = (int)(mana * multi);
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