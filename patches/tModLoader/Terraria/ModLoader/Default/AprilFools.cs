using System;
using Terraria.ID;

namespace Terraria.ModLoader.Default
{
	public class AprilFools : ModLoaderModItem
	{
		public override string Texture => "Terraria/Images/Item_3389";

		public static bool CheckAprilFools() {
			DateTime now = DateTime.Now;
			return now.Month == 4 && now.Day <= 2;
		}

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("{$tModLoader.AprilFoolsJoke}");
		}

		public override void SetDefaults() {
			if (CheckAprilFools()) {
				Item.SetNameOverride(Lang.GetItemNameValue(ItemID.Terrarian) + "...?");
			}
			Item.useStyle = 4;
			Item.width = 24;
			Item.height = 24;
			Item.UseSound = SoundID.Item2;
			Item.melee = true;
			Item.noMelee = true;
			Item.useAnimation = 25;
			Item.useTime = 25;
			Item.damage = 190;
			Item.knockBack = 6.5f;
			Item.rare = 10;
		}

		public override bool? UseItem(Player player) {
			const int Time = 3600 * 60;

			player.AddBuff(BuffID.Wet, Time);
			player.AddBuff(BuffID.Lovestruck, Time);
			player.AddBuff(BuffID.Stinky, Time);
			player.AddBuff(BuffID.Slimed, Time);

			return true;
		}

		public override void AddRecipes() {
			if (CheckAprilFools()) {
				CreateRecipe()
					.AddIngredient(ItemID.DirtBlock)
					.Register();
			}
		}
	}
}
