using Microsoft.Xna.Framework;
using Terraria.ModLoader.Default;
using Terraria.UI;

//NOTE: Does not fully support: ItemLoader.GetGamepadInstructions and related gamepad stuff.
//TODO: Documentation?

namespace Terraria.ModLoader
{
	/// <summary>
	/// A ModAccessorySlot instance represents a net new accessory slot instance. You can store fields in the ModAccessorySlot class.
	/// </summary>
	public abstract class ModAccessorySlot : ModType
	{
		public int Type { get; internal set; }

		public static Player Player => Main.CurrentPlayer;

		public ModAccessorySlotPlayer ModSlotPlayer => AccessorySlotLoader.ModSlotPlayer(Player);

		// Properties to preset a location for the accessory slot
		public virtual Vector2? CustomLocation => null;

		// Properties to change the Default Background Texture
		public virtual string DyeBackgroundTexture => null;
		public virtual string VanityBackgroundTexture => null;
		public virtual string FunctionalBackgroundTexture => null;

		// Properties to change the Default Style Image Texture
		public virtual string DyeTexture => null;
		public virtual string VanityTexture => null;
		public virtual string FunctionalTexture => null;

		// Properties to control which parts of accessory slot draw
		public virtual bool DrawFunctionalSlot => true;
		public virtual bool DrawVanitySlot => true;
		public virtual bool DrawDyeSlot => true;

		// Get/Set Properties for fetching slot information
		public Item FunctionalItem {
			get => ModSlotPlayer.exAccessorySlot[Type];
			set => ModSlotPlayer.exAccessorySlot[Type] = value;
		}

		public Item VanityItem {
			get => ModSlotPlayer.exAccessorySlot[Type + ModSlotPlayer.SlotCount];
			set => ModSlotPlayer.exAccessorySlot[Type + ModSlotPlayer.SlotCount] = value;
		}

		public Item DyeItem {
			get => ModSlotPlayer.exDyesAccessory[Type];
			set => ModSlotPlayer.exDyesAccessory[Type] = value;
		}

		public bool HideVisuals {
			get => ModSlotPlayer.exHideAccessory[Type];
			set => ModSlotPlayer.exHideAccessory[Type] = value;
		}

		public bool IsEmpty => FunctionalItem.IsAir && VanityItem.IsAir && DyeItem.IsAir;

		// Functionality and Hooks
		protected sealed override void Register() => Type = LoaderManager.Get<AccessorySlotLoader>().Register(this);

		/// <summary>
		/// Allows drawing prior to vanilla ItemSlot.Draw code. Return false to NOT call ItemSlot.Draw
		/// </summary>
		public virtual bool PreDraw(AccessorySlotType context, Item item, Vector2 position, bool isHovered) {
			return true;
		}

		public virtual void PostDraw(AccessorySlotType context, Item item, Vector2 position, bool isHovered) { }

		/// <summary>
		/// Override to replace the vanilla effect behavior of the slot with your own.
		/// By default calls:
		/// Player.VanillaUpdateEquips(FunctionalItem), Player.ApplyEquipFunctional(FunctionalItem, ShowVisuals), Player.ApplyEquipVanity(VanityItem)
		/// </summary>
		public virtual void ApplyEquipEffects() {
			Player.VanillaUpdateEquip(FunctionalItem);
			Player.ApplyEquipFunctional(FunctionalItem, HideVisuals);
			Player.ApplyEquipVanity(VanityItem);
		}

		/// <summary>
		/// Override to set conditions on what can be placed in the slot. Default is to return false only when item property FitsAccessoryVanity says can't go in to a vanity slot.
		/// Return false to prevent the item going in slot. Return true for dyes, if you want dyes. Example: only wings can go in slot.
		/// Receives data:
		/// <para><paramref name="checkItem"/> :: the item that is attempting to enter the slot </para>
		/// </summary>
		public virtual bool CanAcceptItem(Item checkItem, AccessorySlotType context) {
			if (context == AccessorySlotType.VanitySlot) {
				return checkItem.FitsAccessoryVanitySlot;
			}

			return true;
		}

		/// <summary>
		/// After checking for empty slots in ItemSlot.AccessorySwap, this allows for changing what the default target slot (accSlotToSwapTo) will be.
		/// DOES NOT affect vanilla behaviour of swapping items like for like where existing in a slot
		/// Return true to set this slot as the default targetted slot.
		/// </summary>
		public virtual bool ModifyDefaultSwapSlot(Item item, int accSlotToSwapTo) => false;

		/// <summary>
		/// Override to control whether or not drawing will be skipped during the given frame.
		/// NOTE: Nothing will be drawn, nor will subsequent drawing hooks be called on this slot for the frame while true
		/// </summary>
		public virtual bool IsHidden() => false;

		/// <summary>
		/// Override to set conditions on when the slot is valid for stat/vanity calculations and player usage.
		/// Example: the demonHeart is consumed and in Expert mode in Vanilla.
		/// </summary>
		public virtual bool IsEnabled() => true;

		/// <summary>
		/// Override to change the condition on when the slot is visible, but otherwise non-functional for stat/vanity calculations.
		/// Defaults to check 'property' IsEmpty
		/// </summary>
		/// <returns></returns>
		public virtual bool IsVisibleWhenNotEnabled() => !IsEmpty;

		/// <summary>
		/// Allows you to do stuff while the player is hovering over this slot.
		/// </summary>
		public virtual void OnMouseHover(AccessorySlotType context) { }
	}
}
