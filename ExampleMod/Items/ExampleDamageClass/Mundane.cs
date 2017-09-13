using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.ExampleDamageClass
{
	public class Mundane : ExampleDamageItem
	{
		public override string Texture
		{
			get { return "Terraria/Item_" + ItemID.HellwingBow; }
		}

		// Our ExampleDamageItem abstract class handles all code related to our custom damage class
		public override void SafeSetDefaults()
		{
			item.CloneDefaults(ItemID.WoodenBow);
			item.Size = new Vector2(18, 46);
			item.damage = 20;
			item.crit = 20;
			item.knockBack = 2;
			item.rare = 10;
		}
	}
}
