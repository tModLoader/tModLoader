using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.ExampleDamageClass
{
	public class ExampleDamageAccessory : ModItem
	{
		public override string Texture
		{
			get { return "Terraria/Item_" + ItemID.AnglerEarring; }
		}

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("20% increased example damage" +
			                   "\n15% increased example critical strike chance" +
			                   "\n10 increased increased example knockback");
		}

		public override void SetDefaults()
		{
			item.Size = new Vector2(34);
			item.rare = 10;
			item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			ExampleDamagePlayer modPlayer = ExampleDamagePlayer.ModPlayer(player);
			modPlayer.exampleDamage += 0.2f;
			modPlayer.exampleCrit += 15;
			modPlayer.exampleKnockback += 10;
		}
	}
}
