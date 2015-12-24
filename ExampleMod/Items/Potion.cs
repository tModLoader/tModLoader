using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class Potion : GlobalItem
	{
		public override bool UseItem(Item item, Player player)
		{
			if (item.healLife > 0)
			{
				ExamplePlayer modPlayer = (ExamplePlayer)player.GetModPlayer(mod, "ExamplePlayer");
				if (modPlayer.badHeal)
				{
					int heal = item.healLife;
					int damage = player.statLifeMax2 - player.statLife;
					if (heal > damage)
					{
						heal = damage;
					}
					if (heal > 0)
					{
						player.AddBuff(mod.BuffType("Undead2"), 2 * heal, false);
					}
				}
			}
			return base.UseItem(item, player);
		}
	}
}