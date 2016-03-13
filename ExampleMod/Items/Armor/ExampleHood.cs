using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Armor
{
	public class ExampleHood : ModItem
	{
		public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
		{
			equips.Add(EquipType.Head);
			return true;
		}

		public override void SetDefaults()
		{
			item.name = "Example Hood";
			item.width = 18;
			item.height = 18;
			item.toolTip = "This is a modded helmet.";
			item.value = 10000;
			item.rare = 2;
			item.defense = 30;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == mod.ItemType("ExampleBreastplate") && legs.type == mod.ItemType("ExampleLeggings");
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = "trollface.jpg";
			player.AddBuff(BuffID.Cursed, 2);
			player.AddBuff(BuffID.Darkness, 2);
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "EquipMaterial", 30);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}