using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameInput;
using Terraria.ModLoader.Default;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.Audio;

//TODO: Does not Support: ItemLoader.GetGamepadInstructions and related gamepad stuff.

namespace Terraria.ModLoader
{
	/// <summary>
	/// A ModAccessorySlot instance represents a net new accessory slot instance. You can store fields in the ModAccessorySlot class. 
	/// </summary>
	public abstract class ModAccessorySlot : ModType
	{
		internal virtual bool skipRegister => false;

		internal int slot;
		internal int index;
		internal static int accessoryPerColumn = 5;

		/// <summary>
		/// Called when loading characters from parent ModPlayer.
		/// Requests for a modded Slot, and stores assigned Slot. DefaultPlayer will auto-manage slots.
		/// </summary>
		internal protected void Initialize() {
			if (skipRegister) {
				return;
			}

			int pendingID = ModPlayer.moddedAccSlots.IndexOf(this.FullName);
			if (pendingID < 0) {
				pendingID = ModPlayer.moddedAccSlots.Count;
				ModPlayer.moddedAccSlots.Add(this.FullName);
			}

			this.slot = pendingID;
		}

		protected sealed override void Register() {
			ModTypeLookup<ModAccessorySlot>.Register(this);
			this.Initialize();
		}

		internal void DrawRedirect(Item[] inv, int context, int slot, Vector2 position) {
			if (!this.DrawModded(inv, context, slot, position))
				EquipLoader.DefaultDrawModSlots(Main.spriteBatch, inv, context, slot, position); 
		}

		/// <summary>
		/// This function allows for custom textures and colours to be drawn for the accessory slot. Called for Dyes, Vanity, and Functionals.
		/// Runs in place of EquipLoader.DefaultDrawModSlots if this returns true.
		/// Receives data:
		/// <para><paramref name="inv"/> :: the array containing all accessory slots, yours is inv[slot] </para>
		/// <para><paramref name="slot"/> :: which is the index for inventory that you were assigned </para>
		/// <para><paramref name="position"/> :: is the position of where the ItemSlot will be drawn </para>
		/// <para><paramref name="context"/> :: 12 => dye; 11 => vanity; 10 => functional </para>
		/// </summary>
		public virtual bool DrawModded(Item[] inv, int context, int slot, Vector2 position) {
			return false;
		}

		/// <summary>
		/// Override to set conditions on when the slot is available, Example: the demonHeart is consumed and in Expert mode in Vanilla.
		/// </summary>
		public virtual bool CanUseSlot() {
			return true;
		}

		/// <summary>
		/// Override to set conditions on what can go in slot. Return false to prevent the item going in slot. Example: only wings can go in slot.
		/// Receives data:
		/// <para><paramref name="checkItem"/> :: the item that is attempting to enter the slot </para>
		/// </summary>
		public virtual bool LimitWhatCanGoInSlot(Item checkItem) {
			return true;
		}
	}
}
