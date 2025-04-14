using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;

namespace ExampleMod.Common.CustomEquipmentSlot
{
	/// <summary>
	/// EarringsPlayer stores which earring slot will be drawn.
	/// </summary>
	public class EarringsPlayer : ModPlayer
	{
		// The earring equipment slot ID.
		public int earring = -1; // Actually, should this be ear?
		public int earringShader;

		public override void ResetEffects() {
			// TODO: Do we need a ResetVisibleAccessories? Player.Update and UpdateDead don't have the usual ResetEffects->ResetVisibleAccessories call order so there might be some edge case.

			earring = -1;
		}

		public override void UpdateDyes() {
			// TODO: This could use a GlobalItem.UpdateDyes hook to be better.

			earringShader = 0;

			// We need to manually check each accessory slot in the correct order to determine which dye's shader to apply to our modded equipment slot. 
			for (int i = 0; i < 20; i++) {
				if (Player.IsItemSlotUnlockedAndUsable(i)) {
					int num = i % 10;
					UpdateItemDye(i < 10, Player.hideVisibleAccessory[num], Player.armor[i], Player.dye[num]);
				}
				if (i == 9) {
					UpdateModdedSlotDyes(socialSlots: false);
				}
				if (i == 19) {
					UpdateModdedSlotDyes(socialSlots: true);
				}
			}
		}

		private void UpdateModdedSlotDyes(bool socialSlots) {
			var loader = LoaderManager.Get<AccessorySlotLoader>();
			var ModAccessorySlotPlayer = Player.GetModPlayer<ModAccessorySlotPlayer>();

			for (int i = 0; i < ModAccessorySlotPlayer.SlotCount; i++) {
				if (loader.ModdedIsItemSlotUnlockedAndUsable(i, Player)) {
					var slot = loader.Get(i, Player);
					UpdateItemDye(!socialSlots, slot.HideVisuals, socialSlots ? slot.VanityItem : slot.FunctionalItem, slot.DyeItem);
				}
			}
		}

		private void UpdateItemDye(bool isNotInVanitySlot, bool isSetToHidden, Item armorItem, Item dyeItem) {
			// This method does the actual assignment to earringShader.
			if (armorItem.IsAir) {
				return;
			}

			// We check if the item is either in a vanity slot or a non-hidden normal slot.
			bool shouldNotApplyShader = isNotInVanitySlot && isSetToHidden;
			if (shouldNotApplyShader) {
				return;
			}

			// If it is, and has an earringSlot value, we store the associated dye value so we can use it later in EarringsDrawLayer.
			if (armorItem.GetGlobalItem<EarringsGlobalItem>().earringSlot > 0) {
				earringShader = dyeItem.dye;
			}
		}
	}
}
