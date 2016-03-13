using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class ExampleShield : ModItem
	{
		public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
		{
			equips.Add(EquipType.Shield);
			return true;
		}

		public override void SetDefaults()
		{
			item.name = "Example Shield";
			item.width = 24;
			item.height = 28;
			item.toolTip = "This is a modded accessory.";
			item.toolTip2 = "Only equip if your character's name is bluemagic123";
			item.value = 10000;
			item.rare = 2;
			item.accessory = true;
			item.defense = 1000;
			item.lifeRegen = 19;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			if (player.name == "bluemagic123")
			{
				player.meleeDamage += 19f;
				player.thrownDamage += 19f;
				player.rangedDamage += 19f;
				player.magicDamage += 19f;
				player.minionDamage += 19f;
				player.endurance = 1f - 0.1f * (1f - player.endurance);
			}
			else
			{
				player.statDefense = 0;
				player.meleeDamage = 0.1f;
				player.thrownDamage = 0.1f;
				player.rangedDamage = 0.1f;
				player.magicDamage = 0.1f;
				player.minionDamage = 0.1f;
				player.lifeRegen = -120;
			}
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "EquipMaterial", 60);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}