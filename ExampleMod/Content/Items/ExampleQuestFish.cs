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
			item.uniqueStack = true;
			item.rare = ItemRarityID.Quest; //
		}

		public override bool IsQuestFish() => true; // makes the item a quest fish

		public override bool IsAnglerQuestAvailable() => Main.hardMode; // makes the quest only appear in hard mode

		public override void AnglerQuestChat(ref string description, ref string catchLocation) {
			/* how the angler describes the fish to the player*/ description = "I've heard stories of a fish that swims upside-down. Supposedly you have the stand upside-down yourself to even find one. One of those would go great on my ceiling. Go fetch!";
			/* what it says on the bottom of the angler's text box of how to catch the fish */ catchLocation = "Caught anywhere while standing upside-down.";
		}
	}
}
