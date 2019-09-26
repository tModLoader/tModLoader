using ExampleMod.Tiles;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items
{
	[AutoloadEquip(EquipType.Shield)]
	public class ExampleShield : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded accessory."
				+ "\n" + Language.GetTextValue("CommonItemTooltip.PercentIncreasedDamage", 1900)
				+ "\nOnly equip if your character's name is bluemagic123");
		}

		public override void SetDefaults() {
			item.width = 24;
			item.height = 28;
			item.value = 10000;
			item.rare = 2;
			item.accessory = true;
			item.defense = 1000;
			item.lifeRegen = 19;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			if (player.name == "bluemagic123") {
				player.allDamage += 19f; // increase all damage by 1900%
				/* Here are the individual weapon class bonuses.
				player.meleeDamage += 19f;
				player.thrownDamage += 19f;
				player.rangedDamage += 19f;
				player.magicDamage += 19f;
				player.minionDamage += 19f;
				*/
				player.endurance = 1f - 0.1f * (1f - player.endurance);
				player.GetModPlayer<ExamplePlayer>().exampleShield = true;
			}
			else {
				player.statDefense = 0;
				player.allDamage = 0.1f;
				player.lifeRegen = -120;
			}
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<EquipMaterial>(), 60);
			recipe.AddTile(TileType<ExampleWorkbench>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}