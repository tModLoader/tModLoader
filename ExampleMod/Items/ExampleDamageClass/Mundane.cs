using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items.ExampleDamageClass
{
	public class Mundane : ExampleDamageItem
	{
		public override string Texture => "Terraria/Item_" + ItemID.HellwingBow;

		// Called when the mod loads, so our changes are added to the game
		public static void AddHacks()
		{
			// Set ourselves to be ranged temporarily to benefit from ranged bonuses
			// This is needed because terraria changes the variables before calling tML's method
			// based on if the item was set to be ranged. Ours isn't, but we still want our custom bow
			// to benefit from ranged bonuses, despite being an Example damage weapon.
			// This is how to do it.
			On.Terraria.Player.GetWeaponDamage += PlayerOnGetWeaponDamage;
			On.Terraria.Player.GetWeaponKnockback += PlayerOnGetWeaponKnockback;
		}

		private static float PlayerOnGetWeaponKnockback(On.Terraria.Player.orig_GetWeaponKnockback orig, Player self, Item sitem, float knockback)
		{
			bool isMundane = sitem.type == ItemType<Mundane>();
			if (isMundane)sitem.ranged = true;

			float kb = orig(self, sitem, knockback);
			if (isMundane) sitem.ranged = false;
			return kb;
		}

		private static int PlayerOnGetWeaponDamage(On.Terraria.Player.orig_GetWeaponDamage orig, Player self, Item sitem)
		{
			bool isMundane = sitem.type == ItemType<Mundane>();
			if (isMundane) sitem.ranged = true;
			
			int dmg = orig(self, sitem);
			if (isMundane) sitem.ranged = false;
			return dmg;
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

		public override void GetWeaponCrit(Player player, ref int crit)
		{
			// It is hard to hook into every place checking item's crit and fake item.ranged = true
			// Instead, we can mimick regular ranged crit assignment
			crit = Main.LocalPlayer.rangedCrit - Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem].crit + Main.HoverItem.crit;
			base.GetWeaponCrit(player, ref crit);
		}
	}
}