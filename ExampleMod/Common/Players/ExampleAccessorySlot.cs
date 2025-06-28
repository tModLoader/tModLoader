using ExampleMod.Common.Configs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleModAccessorySlot1 : ModAccessorySlot
	{
		// If the class is empty, everything will default to a basic vanilla slot.
	}

	public class ExampleCustomLocationAndTextureSlot : ModAccessorySlot
	{
		// We will place the slot to be at the center of the map, making the decision not to follow the internal UI handling
		public override Vector2? CustomLocation => new Vector2(Main.screenWidth / 2, 3 * Main.screenHeight / 4);

		// We will draw the vanity slot when there's a dye
		public override bool DrawVanitySlot => !DyeItem.IsAir;

		//     We will use our 'custom' textures
		// Background Textures -> In general, you can use most of the existing vanilla ones to get different colors
		public override string VanityBackgroundTexture => "Terraria/Images/Inventory_Back14"; // yellow
		public override string FunctionalBackgroundTexture => "Terraria/Images/Inventory_Back7"; // pale blue
		public override string DyeBackgroundTexture => "Terraria/Images/Inventory_Back13"; // white. Since it is white, the color assigned in BackgroundDrawColor will be the exact color it appears as.

		// Icon textures. Nominal image size is 32x32. Piggy bank is 16x24 but it still works as it's drawn centered.
		public override string VanityTexture => "Terraria/Images/Item_" + ItemID.PiggyBank;

		// We will keep it hidden most of the time so that it isn't an intrusive example
		public override bool IsHidden() {
			return IsEmpty; // Only show when it contains an item, items can end up in functional slots via quick swap (right click accessory)
		}

		public override void BackgroundDrawColor(AccessorySlotType context, ref Color color) {
			if (context == AccessorySlotType.DyeSlot) {
				color = Main.DiscoColor * (Main.invAlpha / 255);
			}
		}
	}

	public class ExampleModWingSlot : ModAccessorySlot
	{
		public static LocalizedText WingsText { get; private set; }
		public static LocalizedText WingsDyeText { get; private set; }

		// This slot can toggle between supporting loadouts and not supporting loadouts. By not supporting loadouts, a player would only need to craft a single Wing instead of one for each loadout they plan to switch between.
		// This setting must be server-side and requires a reload if changed.
		public override bool HasEquipmentLoadoutSupport => ModContent.GetInstance<ExampleModConfig>().WingSlotLoadoutSupportToggle;

		public override void SetupContent() {
			WingsText = Mod.GetLocalization($"{nameof(ExampleModWingSlot)}.Wings");
			WingsDyeText = Mod.GetLocalization($"{nameof(ExampleModWingSlot)}.WingsDye");
		}

		public override bool CanAcceptItem(Item checkItem, AccessorySlotType context) {
			if (checkItem.wingSlot > 0) // if is Wing, then can go in slot
				return true;

			return false; // Otherwise nothing in slot
		}

		// Designates our slot to be a priority for putting wings in to. NOTE: use ItemLoader.CanEquipAccessory if aiming for restricting other slots from having wings!
		public override bool ModifyDefaultSwapSlot(Item item, int accSlotToSwapTo) {
			if (item.wingSlot > 0) // If is Wing, then we want to prioritize it to go in to our slot.
				return true;

			return false;
		}

		public override bool IsEnabled() {
			if (Player.armor[0].headSlot >= 0) // if player is wearing a helmet, because flight safety
				return true; // Then can use Slot

			return false; // Can't use slot
		}

		// Overrides the default behavior where a disabled accessory slot will allow retrieve items if it contains items
		public override bool IsVisibleWhenNotEnabled() {
			return false; // We set to false to just not display if not Enabled. NOTE: this does not affect behavior when mod is unloaded!
		}

		// Icon textures. Nominal image size is 32x32. Will be centered on the slot.
		public override string FunctionalTexture => "Terraria/Images/Item_" + ItemID.CreativeWings;

		// Can be used to modify stuff while the Mouse is hovering over the slot.
		public override void OnMouseHover(AccessorySlotType context) {
			// We will modify the hover text while an item is not in the slot, so that it says "Wings".
			switch (context) {
				case AccessorySlotType.FunctionalSlot:
				case AccessorySlotType.VanitySlot:
					Main.hoverItemName = WingsText.Value;
					break;
				case AccessorySlotType.DyeSlot:
					Main.hoverItemName = WingsDyeText.Value;
					break;
			}
		}
	}
}
