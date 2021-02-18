using Microsoft.Xna.Framework;
using Terraria.UI;

//NOTE: Does not fully support: ItemLoader.GetGamepadInstructions and related gamepad stuff.

namespace Terraria.ModLoader
{
	/// <summary>
	/// A ModAccessorySlot instance represents a net new accessory slot instance. You can store fields in the ModAccessorySlot class. 
	/// </summary>
	public abstract class ModAccessorySlot : ModType
	{
		// Suppresses adding it to AccessorySlotLoader.moddedAccSlots. Used internally for UnloadedSlots
		internal virtual bool suppressUnloadedSlot => false;

		internal int slot;
		internal int index;

		// Fields to preset a location for the accessory slot
		public virtual int XLoc => 0;
		public virtual int YLoc => 0;
				
		protected sealed override void Register() {
			ModTypeLookup<ModAccessorySlot>.Register(this);

			if (suppressUnloadedSlot) {
				return;
			}

			int pendingID = AccessorySlotLoader.moddedAccSlots.IndexOf(this.FullName);
			if (pendingID < 0) {
				pendingID = AccessorySlotLoader.moddedAccSlots.Count;
				AccessorySlotLoader.moddedAccSlots.Add(this.FullName);
			}

			this.slot = pendingID;
		}

		/// <summary>
		/// This function allows for custom textures and colours to be drawn for the accessory slot. Called for Dyes, Vanity, and Functionals.
		/// Runs in place of ItemSlot.Draw() if this returns true.
		/// Receives data:
		/// <para><paramref name="inv"/> :: the array containing all accessory slots, yours is inv[slot] </para>
		/// <para><paramref name="slot"/> :: which is the index for inventory that you were assigned </para>
		/// <para><paramref name="position"/> :: is the position of where the ItemSlot will be drawn </para>
		/// <para><paramref name="context"/> :: 12 => dye; 11 => vanity; 10 => functional </para>
		/// </summary>
		public virtual void DrawModded(Item[] inv, int context, int slot, Vector2 position) {
			ItemSlot.Draw(Main.spriteBatch, inv, context, slot, position);
		}

		/// <summary>
		/// Override to set conditions on when the slot is visible. Does not force visbility if an item is in the slot by default.
		/// Example: the demonHeart is consumed and in Expert mode in Vanilla.
		/// </summary>
		public virtual bool IsSlotVisible() {
			return true;
		}

		/// <summary>
		/// Override to set conditions on what can go in slot. Return false to prevent the item going in slot. Return true for dyes, if you want dyes. Example: only wings can go in slot.
		/// Receives data:
		/// <para><paramref name="checkItem"/> :: the item that is attempting to enter the slot </para>
		/// </summary>
		public virtual bool SlotCanAcceptItem(Item checkItem) {
			return true;
		}
	}
}
