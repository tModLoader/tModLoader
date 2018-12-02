using Microsoft.Xna.Framework;
using Terraria.ID;

namespace ExampleMod.Items.ExampleDamageClass
{
	public class Mundane : ExampleDamageItem
	{
		public override string Texture => "Terraria/Item_" + ItemID.HellwingBow;

		// Our ExampleDamageItem abstract class handles all code related to our custom damage class
		public override void SafeSetDefaults() {
			item.CloneDefaults(ItemID.WoodenBow);
			item.Size = new Vector2(18, 46);
			item.damage = 20;
			item.crit = 20;
			item.knockBack = 2;
			item.rare = 10;
		}
	}
}
