using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	/// <summary>
	/// This item showcases one of the ways for you to do something when an item is bought from an NPC with a shop.
	/// </summary>
	public class ExampleOnBuyItem : ModItem
	{
		public override void SetDefaults() {
			Item.width = 16;
			Item.height = 16;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 1, copper: 50);
			Item.maxStack = 9999;
		}

		// Note that alternatively, you can use the ModPlayer.PostBuyItem hook to achieve the same functionality!
		public override void OnCreated(ItemCreationContext context) {
			if (context is not BuyItemCreationContext buyContext) {
				return;
			}

			// For fun, we'll give the buying player a 50% chance to die whenever they buy this item from an NPC.
			if (!Main.rand.NextBool()) {
				return;
			}

			Player player = buyContext.PlayerCustomer;
			player.KillMe(PlayerDeathReason.ByCustomReason(Language.GetTextValue("Mods.ExampleMod.Items.ExampleOnBuyItem.DeathMessage", player.name)), 9999, 0);
		}
	}
}
