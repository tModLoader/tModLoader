using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleResearchPresent : ModItem
	{
		public static LocalizedText NoAccessoryText { get; private set; }
		public static LocalizedText NewAccessoryText { get; private set; }
		public static LocalizedText AllAccessoryText { get; private set; }

		public override void SetStaticDefaults() {
			// Must be researched as many times as there are items in the game.
			// If fully researched, and a new mod is added, it will become un-researched and require that much more
			// Research amount will never go down or over the max limit of 9999.
			Item.ResearchUnlockCount = Utils.Clamp(ItemLoader.ItemCount, 1, 9999);

			// Use a MonoMod hook to allow our presents to run through the Sacrifice system.
			On_CreativeUI.SacrificeItem_refItem_refInt32_bool += OnSacrificeItem;

			NoAccessoryText = this.GetLocalization("NoAccessory");
			NewAccessoryText = this.GetLocalization("NewAccessory");
			AllAccessoryText = this.GetLocalization("AllAccessory");
		}

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GoodieBag);
		}

		// This allows for the present to be researched even when you already have infinite of them.
		// This is not a standard use of the research system, but allows for re-running a 'research complete' effect
		private CreativeUI.ItemSacrificeResult OnSacrificeItem(On_CreativeUI.orig_SacrificeItem_refItem_refInt32_bool orig,
				ref Item item, out int amountWeSacrificed, bool returnRemainderToPlayer) {

			// If the item being sacrificed has the same type as us (is an ExampleResearchPresent) and is fully researched
			if (item.type == Type && CreativeUI.GetSacrificesRemaining(Type) == 0) {

				// Re-unlock all accessories, incase mods have changed
				OnResearched(true);

				// We always lose a present when researching them, even if you already had infinite of them. To show the user something happened
				item.stack -= 1;

				// This code is copied from the end of SacrificeItem
				if (item.stack > 0 && returnRemainderToPlayer) {
					item.position.X = Main.LocalPlayer.Center.X - item.width / 2;
					item.position.Y = Main.LocalPlayer.Center.Y - item.height / 2;
					item = Main.LocalPlayer.GetItem(Main.myPlayer, item, GetItemSettings.InventoryUIToInventorySettings);
				}

				// This is the amount the sacrifice counter goes up by. We didn't actually change the total number of sacrifices, so this is 0
				amountWeSacrificed = 0;

				// Return SacrificedAndDone, so the animation and effects happen
				return CreativeUI.ItemSacrificeResult.SacrificedAndDone;
			}

			// Otherwise, call the original method to run the default behavior
			return orig(ref item, out amountWeSacrificed, returnRemainderToPlayer);
		}

		public override void OnResearched(bool fullyResearched) {
			if (fullyResearched) {
				LearnAllAccessories();
			}
			else {
				// Attempt to learn a random accessory for each present sacrificed
				int count = 0;
				for (int j = Item.stack; j > 0; j--) {
					if (LearnRandomAccessory()) {
						count++;
					}
				}
				if (count == 0) {
					Main.NewText(NoAccessoryText);
				}
				else {
					Main.NewText(NewAccessoryText.Format(count));
				}
			}
		}

		// try 1000 random item ids and if we randomly select an accessory, attempt learn it
		private bool LearnRandomAccessory() {
			for (int i = 0; i < 1000; i++) {
				int type = Main.rand.Next(1, ItemLoader.ItemCount);
				if (ContentSamples.ItemsByType[type].accessory) {
					if (CreativeUI.ResearchItem(type) == CreativeUI.ItemSacrificeResult.SacrificedAndDone) {
						return true;
					}
				}
			}
			return false;
		}

		private void LearnAllAccessories() {
			for (int i = 1; i < ItemLoader.ItemCount; i++) {
				if (ContentSamples.ItemsByType[i].accessory) {
					CreativeUI.ResearchItem(i);
				}
			}

			Main.NewText(AllAccessoryText);
		}
	}
}
