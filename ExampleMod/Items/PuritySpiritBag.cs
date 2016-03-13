using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class PuritySpiritBag : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Treasure Bag";
			item.maxStack = 999;
			item.consumable = true;
			item.width = 24;
			item.height = 24;
			item.toolTip = "Right click to open";
			item.rare = 11;
			item.expert = true;
			bossBagNPC = mod.NPCType("PuritySpirit");
		}

		public override bool CanRightClick()
		{
			return true;
		}

		public override void OpenBossBag(Player player)
		{
			player.TryGettingDevArmor();
			player.TryGettingDevArmor();
			int choice = Main.rand.Next(7);
			if (choice == 0)
			{
				player.QuickSpawnItem(mod.ItemType("PuritySpiritMask"));
			}
			else if (choice == 1)
			{
				player.QuickSpawnItem(mod.ItemType("BunnyMask"));
			}
			if (choice != 1)
			{
				player.QuickSpawnItem(ItemID.Bunny);
			}
			player.QuickSpawnItem(mod.ItemType("PurityShield"));
		}
	}
}