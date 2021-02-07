using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.Audio;

namespace Terraria.ModLoader
{
	/// <summary>
	/// A ModAccessorySlot instance represents a net new accessory slot instance. You can store fields in the ModAccessorySlot class. 
	/// </summary>
	public abstract class ModAccessorySlot : ModPlayer
	{
		/// <summary>
		/// Set to true if you want to use custom visuals. See DrawModded for more details.
		/// </summary>
		public bool overrideVanillaDrawing = false;

		internal int slot;
		
		private int num20;
		private Microsoft.Xna.Framework.Color color2;
		public int yLoc = 0;
		public int xLoc = 0;
		private int xColumn;
		private int yRow;
		private static int accessoryPerColumn = 6; //TODO: figure out how to get around the 9, 10 slots being nixed by IsValidEquipmentSlot 
		private bool flag3;

		/// <summary>
		/// Called when loading characters from parent ModPlayer.
		/// Requests Player for a modded Slot, and stores assigned Slot. Player will auto-manage slots and save known in PlayerIO
		/// </summary>
		public override void Initialize() {
			int pendingID = EquipLoader.moddedAccSlots.IndexOf(this.FullName);
			if (pendingID < 0) {
				pendingID = EquipLoader.moddedAccSlots.Count;
				EquipLoader.ResizeAccesoryArrays(pendingID);
				EquipLoader.moddedAccSlots.Add(this.FullName);
			}
			this.xColumn = (int)(pendingID / accessoryPerColumn) + 1;
			this.yRow = pendingID % accessoryPerColumn;
			this.slot = pendingID;
		}

		/// <summary>
		/// Hooks into ModPlayer? to save what was in the modded slot for functional, vanity, dyes, and visiblity data.
		/// </summary>
		public override TagCompound Save() { // Needs to revisit and fix up.
			return new TagCompound {
				["functional"] = ItemIO.Save(EquipLoader.exAccessorySlot[slot]),
				["dyes"] = ItemIO.Save(EquipLoader.exDyesAccessory[slot]),
				["vanity"] = ItemIO.Save(EquipLoader.exAccessorySlot[slot + EquipLoader.moddedAccSlots.Count]),
				["visible"] = (bool) EquipLoader.exHideAccessory[slot]
			};
		}

		/// <summary>
		/// Hooks into ModPlayer? to load what was in the modded slot for functional, vanity, dyes, and visiblity data.
		/// </summary>
		public override void Load(TagCompound tag) {
			EquipLoader.exAccessorySlot[slot] = tag.Get<Item>("functional");
			EquipLoader.exDyesAccessory[slot] = tag.Get<Item>("dyes");
			EquipLoader.exAccessorySlot[slot + EquipLoader.moddedAccSlots.Count] = tag.Get<Item>("vanity");
			EquipLoader.exHideAccessory[slot] = tag.Get<bool>("visible");
		}

		/// <summary>
		/// Is run after vanilla draws normal accessory slots. Currently doesn't get called.
		/// Creates new accessory slots in a column to the left of vanilla.  
		/// </summary>
		public void Draw(int num20) { 
			this.PreDraw(num20);
			this.DrawFunctional();
			this.DrawVanity();
			this.DrawDye();
		}

		/// <summary>
		/// Is run first in this.Draw. 
		/// Initializes all fields that are used in subsequent draw events.
		/// For Overriding fields see this.PreDrawCustom
		/// </summary>
		private void PreDraw(int num20) {
			this.num20 = num20;
			this.flag3 = !Player.IsAValidEquipmentSlotForIteration(slot);
			this.PreDrawCustom();
			if (flag3)
				Main.inventoryBack = color2;
		}

		/// <summary>
		/// Is run in this.PreDraw. 
		/// Will provide default assignment to fields color2, yLoc, and/or xLoc if empty based on vanilla:
		/// <para>this.color2 = new Microsoft.Xna.Framework.Color(80, 80, 80, 80);</para>
		/// <para>this.yLoc = (int) ((float)(num20) + (float) (slot* 56) * Main.inventoryScale);</para>
		/// <para>this.xLoc = Main.screenWidth - 64 - 28 - 47 * 3 * xColumn - 50;</para>
		/// </summary>
		public void PreDrawCustom() {
			if (this.color2 == null) 
				this.color2 = new Microsoft.Xna.Framework.Color(80, 80, 80, 80);
			if (this.yLoc == 0)
				this.yLoc = (int)((float)(num20) + (float)(slot * 56) * Main.inventoryScale);
			if (this.xLoc == 0)
				this.xLoc = Main.screenWidth - 64 - 28 - 47 * 3 * xColumn - 50; // 47*3 is per column, 50 adjusts to not overlap vanilla UI
		}

		/// <summary>
		/// Is run in this.Draw. 
		/// Generates a significant amount of functionality for the slot, despite being named drawing because vanilla.
		/// At the end, calls this.DrawModded() where you can override to have custom drawing code for visuals.
		/// Also includes creating hidevisibilitybutton.
		/// </summary>
		private void DrawFunctional() {
			int yLoc2 = yLoc + (int)((float)(-2) + (float)(slot * 56) * Main.inventoryScale);
			int xLoc2 = xLoc - 58 + 64 + 28;
			int context = 10;

			Texture2D value4 = TextureAssets.InventoryTickOn.Value;
			if (EquipLoader.exHideAccessory[slot])
				value4 = TextureAssets.InventoryTickOff.Value;

			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(xLoc2, yLoc2, value4.Width, value4.Height);
			int num45 = 0;
			if (rectangle.Contains(new Microsoft.Xna.Framework.Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface) {
				Player.mouseInterface = true;
				if (Main.mouseLeft && Main.mouseLeftRelease) {
					EquipLoader.exHideAccessory[slot] = !EquipLoader.exHideAccessory[slot];
					SoundEngine.PlaySound(12);
					if (Main.netMode == 1)
						NetMessage.SendData(4, -1, -1, null, Player.whoAmI); //blindly called, won't work
				}

				num45 = ((!EquipLoader.exHideAccessory[slot]) ? 1 : 2);
			}

			else if (Main.mouseX >= xLoc && (float)Main.mouseX <= (float)xLoc + (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale && Main.mouseY >= yLoc
				&& (float)Main.mouseY <= (float)yLoc + (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale && !PlayerInput.IgnoreMouseInterface) {

				Main.armorHide = true;
				Player.mouseInterface = true;
				ItemSlot.OverrideHover(EquipLoader.exAccessorySlot, context, slot); //should work
				if (!flag3 || Main.mouseItem.IsAir)
					ItemSlot.LeftClick(EquipLoader.exAccessorySlot, context, slot); //should work

				ItemSlot.MouseHover(EquipLoader.exAccessorySlot, context, slot); //should work
			}

			this.DrawRedirect(EquipLoader.exAccessorySlot, context, slot, new Vector2(xLoc, yLoc));

			Main.spriteBatch.Draw(value4, new Vector2(xLoc2, yLoc2), Microsoft.Xna.Framework.Color.White * 0.7f);
			if (num45 > 0) {
				Main.HoverItem = new Item();
				Main.hoverItemName = Lang.inter[58 + num45].Value;
			}
		}

		/// <summary>
		/// Is run in this.Draw. 
		/// Generates a significant amount of functionality for the slot, despite being named drawing because vanilla.
		/// At the end, calls this.DrawModded() where you can override to have custom drawing code for visuals.
		/// </summary>
		private void DrawVanity() {
			bool flag7 = flag3 && !Main.mouseItem.IsAir;
			xLoc -= 47;
			int context = 11;

			if (Main.mouseX >= xLoc && (float)Main.mouseX <= (float)xLoc + (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale && Main.mouseY >= yLoc
				&& (float)Main.mouseY <= (float)yLoc + (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale && !PlayerInput.IgnoreMouseInterface) {
				Player.mouseInterface = true;
				Main.armorHide = true;
				ItemSlot.OverrideHover(EquipLoader.exAccessorySlot, context, slot); //should work
				if (!flag7) {
					ItemSlot.LeftClick(EquipLoader.exAccessorySlot, context, slot); //should work
					ItemSlot.RightClick(EquipLoader.exAccessorySlot, context, slot); //blindly called
				}

				ItemSlot.MouseHover(EquipLoader.exAccessorySlot, context, slot); //should work
			}

			this.DrawRedirect(EquipLoader.exAccessorySlot, context, slot, new Vector2(xLoc, yLoc));
		}

		/// <summary>
		/// Is run in this.Draw. 
		/// Generates a significant amount of functionality for the slot, despite being named drawing because vanilla.
		/// At the end, calls this.DrawModded() where you can override to have custom drawing code for visuals.
		/// </summary>
		private void DrawDye() {
			bool flag8 = flag3 && !Main.mouseItem.IsAir;
			xLoc -= 47;
			int context = 12;

			if (Main.mouseX >= xLoc && (float)Main.mouseX <= (float)xLoc + (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale && Main.mouseY >= yLoc
				&& (float)Main.mouseY <= (float)yLoc + (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale && !PlayerInput.IgnoreMouseInterface) {
				Player.mouseInterface = true;
				Main.armorHide = true;
				ItemSlot.OverrideHover(EquipLoader.exDyesAccessory, context, slot); //should work
				if (!flag8) {
					if (Main.mouseRightRelease && Main.mouseRight)
						ItemSlot.RightClick(EquipLoader.exDyesAccessory, context, slot); //blindly called

					ItemSlot.LeftClick(EquipLoader.exDyesAccessory, context, slot); //should work
				}

				ItemSlot.MouseHover(EquipLoader.exDyesAccessory, context, slot); //should work
			}
			this.DrawRedirect(EquipLoader.exDyesAccessory, context, slot, new Vector2(xLoc, yLoc));
		}

		private void DrawRedirect(Item[] inv, int context, int slot, Vector2 position) {
			if (!this.overrideVanillaDrawing)
				ItemSlot.Draw(Main.spriteBatch, inv, context, slot, position); //blindly called
			else
				this.DrawModded(inv, context, slot, position);
		}



		/// <summary>
		/// If overrideVanillaDrawing is true, then this method will be called to draw instead. 
		/// Note that this should be treated as an alternative to vanilla's ItemSlot.Draw.
		/// Receives parameters:
		/// <para><paramref name="inv"/> :: the array containing all accessory slots, yours is inv[slot] </para>
		/// <para><paramref name="slot"/> :: which is the index for inventory that you were assigned </para>
		/// <para><paramref name="position"/> :: is the position of where the ItemSlot will be drawn </para>
		/// <para><paramref name="context"/> :: 12=>dye; 9,11 => vanity; 8,10 => functional </para>
		/// </summary>
		public virtual void DrawModded(Item[] inv, int context, int slot, Vector2 position) {
		}
	}
}
