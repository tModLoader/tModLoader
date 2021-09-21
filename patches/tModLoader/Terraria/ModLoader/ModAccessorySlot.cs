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
		internal int type;
		internal int index;
		public ModAccessorySlotPlayer ModSlotPlayer => AccessorySlotLoader.GetModSlotPlayer(Main.LocalPlayer);

		// Properties to preset a location for the accessory slot
		public virtual int XLoc => 0;
		public virtual int YLoc => 0;

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
			get => ModSlotPlayer.exAccessorySlot[type];
			set => ModSlotPlayer.exAccessorySlot[type] = value;
		}

		public Item VanityItem {
			get => ModSlotPlayer.exAccessorySlot[type + ModSlotPlayer.SlotCount()];
			set => ModSlotPlayer.exAccessorySlot[type + ModSlotPlayer.SlotCount()] = value;
		}
		
		public Item DyeItem {
			get => ModSlotPlayer.exDyesAccessory[type];
			set => ModSlotPlayer.exDyesAccessory[type] = value;
		}

		public bool ShowVisuals {
			get => ModSlotPlayer.exHideAccessory[type];
			set => ModSlotPlayer.exHideAccessory[type] = value;
		}

		protected sealed override void Register() => type = LoaderManager.Get<AccessorySlotLoader>().Register(this);

		private bool IsEmpty() => FunctionalItem.IsAir && VanityItem.IsAir && DyeItem.IsAir;

		/// <summary>
		/// This function allows for custom textures and colours to be drawn for the accessory slot. Called for Dyes, Vanity, and Functionals.
		/// By default runs ItemSlot.Draw()
		/// Receives data:
		/// <para><paramref name="inv"/> :: the array containing all accessory slots, yours is inv[slot] </para>
		/// <para><paramref name="slot"/> :: which is the index for inventory that you were assigned </para>
		/// <para><paramref name="position"/> :: is the position of where the ItemSlot will be drawn </para>
		/// <para><paramref name="context"/> :: 12 => dye; 11 => vanity; 10 => functional </para>
		/// </summary>
		public virtual void DrawModded(Item[] inv, int context, int slot, Vector2 position) => 
			ItemSlot.Draw(Main.spriteBatch, inv, context, slot, position);

		/// <summary>
		/// Override to set conditions on when the slot is valid for stat/vanity calculations and player usage.
		/// Example: the demonHeart is consumed and in Expert mode in Vanilla.
		/// </summary>
		public virtual bool IsEnabled() => true;

		/// <summary>
		/// Override to set conditions on what can be placed in the slot. Return false to prevent the item going in slot. Return true for dyes, if you want dyes. Example: only wings can go in slot.
		/// Receives data:
		/// <para><paramref name="checkItem"/> :: the item that is attempting to enter the slot </para>
		/// </summary>
		public virtual bool CanAcceptItem(Item checkItem) => true;

		/// <summary>
		/// Override to change the condition on when the slot is visible, but otherwise non-functional for stat/vanity calculations.
		/// Defaults to check 'property' MySlotContainsAnItem()
		/// </summary>
		/// <returns></returns>
		public virtual bool IsVisibleWhenNotEnabled() => IsEmpty();
	}
}
