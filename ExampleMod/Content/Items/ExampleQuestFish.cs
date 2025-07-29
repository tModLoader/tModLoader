using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleQuestFish : ModItem
	{
		public static LocalizedText DescriptionText { get; private set; }
		public static LocalizedText CatchLocationText { get; private set; }

		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 2;
			ItemID.Sets.CanBePlacedOnWeaponRacks[Type] = true; // All vanilla fish can be placed in a weapon rack.

			DescriptionText = this.GetLocalization("Description");
			CatchLocationText = this.GetLocalization("CatchLocation");
		}

		public override void SetDefaults() {
			// DefaultToQuestFish sets quest fish properties.
			// Of note, it sets rare to ItemRarityID.Quest, which is the special rarity for quest items.
			// It also sets uniqueStack to true, which prevents players from picking up a 2nd copy of the item into their inventory.
			Item.DefaultToQuestFish();
		}

		public override bool IsQuestFish() => true; // Makes the item a quest fish

		public override bool IsAnglerQuestAvailable() => Main.hardMode; // Makes the quest only appear in hard mode. Adding a '!' before Main.hardMode makes it ONLY available in pre-hardmode.

		public override void AnglerQuestChat(ref string description, ref string catchLocation) {
			// How the angler describes the fish to the player.
			description = DescriptionText.Value;
			// What it says on the bottom of the angler's text box of how to catch the fish.
			catchLocation = CatchLocationText.Value;
		}
	}
}
