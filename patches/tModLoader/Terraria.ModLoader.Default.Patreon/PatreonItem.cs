using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria.Localization;

namespace Terraria.ModLoader.Default.Patreon
{
	abstract class PatreonItem : ModItem
	{
		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			var line = new TooltipLine(mod, "PatreonThanks", Language.GetTextValue("tModLoader.PatreonSetTooltip"));
			line.overrideColor = Color.Aquamarine;
			tooltips.Add(line);
		}

		internal static bool TryGettingPatreonArmor(Player player)
		{
			if (Main.rand.NextBool(20))
			{
				Mod mod = ModLoader.GetMod("ModLoader");
				switch (Main.rand.Next(4))
				{
					case 0:
						player.QuickSpawnItem(mod.ItemType<toplayz_Head>());
						player.QuickSpawnItem(mod.ItemType<toplayz_Body>());
						player.QuickSpawnItem(mod.ItemType<toplayz_Legs>());
						return true;
					case 1:
						player.QuickSpawnItem(mod.ItemType<PotyBlank_Head>());
						player.QuickSpawnItem(mod.ItemType<PotyBlank_Body>());
						player.QuickSpawnItem(mod.ItemType<PotyBlank_Legs>());
						return true;
					case 2:
						player.QuickSpawnItem(mod.ItemType<litcherally_Head>());
						player.QuickSpawnItem(mod.ItemType<litcherally_Body>());
						player.QuickSpawnItem(mod.ItemType<litcherally_Legs>());
						return true;
					case 3:
						player.QuickSpawnItem(mod.ItemType<KittyKitCatCat_Head>());
						player.QuickSpawnItem(mod.ItemType<KittyKitCatCat_Body>());
						player.QuickSpawnItem(mod.ItemType<KittyKitCatCat_Legs>());
						return true;
				}
			}
			return false;
		}
	}
}
