using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleQuestFish : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Upside-down Fish");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 2;
		}

		public override void SetDefaults() {
			Item.questItem = true;
			Item.maxStack = 1;
			Item.width = 26;
			Item.height = 26;
			Item.uniqueStack = true; // Make this item only stack one time.
			Item.rare = ItemRarityID.Quest; // Sets the item's rarity. This exact line uses a special rarity for quest items.
		}

		public override bool IsQuestFish() => true; // Makes the item a quest fish

		public override bool IsAnglerQuestAvailable() => Main.hardMode; // Makes the quest only appear in hard mode. Adding a '!' before Main.hardMode makes it ONLY available in pre-hardmode.

		public override void AnglerQuestChat(ref string description, ref string catchLocation) {
			// How the angler describes the fish to the player.
			description = "I've heard stories of a fish that swims upside-down. Supposedly you have the stand upside-down yourself to even find one. One of those would go great on my ceiling. Go fetch!";
			// What it says on the bottom of the angler's text box of how to catch the fish.
			catchLocation = "Caught anywhere while standing upside-down.";
		}
	}
}
