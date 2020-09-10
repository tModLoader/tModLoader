using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleQuestFish : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Upside-down Fish");
		}

		public override void SetDefaults() {
			item.questItem = true;
			item.maxStack = 1;
			item.width = 26;
			item.height = 26;
			item.uniqueStack = true; // Make this item only stack one time.
			item.rare = ItemRarityID.Quest; // Sets the item's rarity. This exact line uses a special rarity for quest items.
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
