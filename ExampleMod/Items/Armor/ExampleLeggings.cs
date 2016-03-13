using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Items.Armor
{
	public class ExampleLeggings : ModItem
	{
		public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
		{
			equips.Add(EquipType.Legs);
			return true;
		}

		public override void SetDefaults()
		{
			item.name = "Example Leggings";
			item.width = 18;
			item.height = 18;
			AddTooltip("This is a modded leg armor.");
			AddTooltip2("5% increased movement speed");
			item.value = 10000;
			item.rare = 2;
			item.defense = 45;
		}

		public override void UpdateEquip(Player player)
		{
			player.moveSpeed += 0.05f;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "EquipMaterial", 45);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}