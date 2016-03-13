using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Armor
{
	public class ExampleBreastplate : ModItem
	{
		public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
		{
			equips.Add(EquipType.Body);
			return true;
		}

		public override void SetDefaults()
		{
			item.name = "Example Breastplate";
			item.width = 18;
			item.height = 18;
			AddTooltip("This is a modded body armor.");
			AddTooltip2("Immunity to 'On Fire!'");
			AddTooltip2("+20 max mana and +1 max minions");
			item.value = 10000;
			item.rare = 2;
			item.defense = 60;
		}

		public override void UpdateEquip(Player player)
		{
			player.buffImmune[BuffID.OnFire] = true;
			player.statManaMax2 += 20;
			player.maxMinions++;
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