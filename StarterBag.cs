using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
namespace BlooodMod.Items.Misc.Bags
{
    public class StarterBag : ModItem
    {
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Explorer's Bag");
			Tooltip.SetDefault("<right> for goodies!");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 20;
			Item.rare = ItemRarityID.Blue;
		}

		public override bool CanRightClick()
		{
			return true;
		}

		public override void RightClick(Player player)
		{
			player.QuickSpawnItem(ItemID.LifeCrystal, 15);
			player.QuickSpawnItem(ItemID.LifeFruit, 20);
			player.QuickSpawnItem(ItemID.ManaCrystal, 9);
			player.QuickSpawnItem(ItemID.AnkhShield);
			player.QuickSpawnItem(ItemID.FrostsparkBoots);
			player.QuickSpawnItem(ItemID.LavaWaders);
			player.QuickSpawnItem(ItemID.CellPhone);
			player.QuickSpawnItem(ItemID.SuspiciousLookingTentacle);
		}

	}
}
