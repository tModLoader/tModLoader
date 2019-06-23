using System;
using Terraria.ID;

namespace Terraria.ModLoader.Default
{
	public class AprilFools : ModItem
	{
		public override string Texture => "Terraria/Item_3389";

		public static bool CheckAprilFools() {
			DateTime now = DateTime.Now;
			return now.Month == 4 && now.Day <= 2;
		}

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("{$tModLoader.AprilFoolsJoke}");
		}

		public override void SetDefaults() {
			if (CheckAprilFools()) {
				item.SetNameOverride(Lang.GetItemNameValue(ItemID.Terrarian) + "...?");
			}
			item.useStyle = 4;
			item.width = 24;
			item.height = 24;
			item.UseSound = SoundID.Item2;
			item.melee = true;
			item.noMelee = true;
			item.useAnimation = 25;
			item.useTime = 25;
			item.damage = 190;
			item.knockBack = 6.5f;
			item.rare = 10;
		}

		public override bool UseItem(Player player) {
			const int time = 3600 * 60;
			player.AddBuff(BuffID.Wet, time);
			player.AddBuff(BuffID.Lovestruck, time);
			player.AddBuff(BuffID.Stinky, time);
			player.AddBuff(BuffID.Slimed, time);
			return true;
		}

		public override void AddRecipes() {
			if (CheckAprilFools()) {
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.DirtBlock);
				recipe.SetResult(this);
				recipe.AddRecipe();
			}
		}
	}
}
