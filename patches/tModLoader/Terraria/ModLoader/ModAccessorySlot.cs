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
		/// Currently never gets checked, so this is useless. 
		/// Set to true if you want the slot to ignore vanilla limitations on what can be placed.
		/// Warning: SlotCustomCondition() runs independant of this bool
		/// </summary>
		public bool overrideVanillaConditions = false;

		internal int slot;
		
		private int num20;
		private Microsoft.Xna.Framework.Color color2;
		private int yLoc;
		private int xLoc;
		private int xColumn;
		private static int AccessoryPerColumn = 5;
		private bool flag3;

		/// <summary>
		/// Called when loading characters from parent ModPlayer.
		/// Requests Player for a modded Slot, and stores assigned Slot. Player will auto-manage slots and save known in PlayerIO
		/// </summary>
		public override void Initialize() {
			int pendingID = Player.moddedAccSlots.IndexOf(this.FullName);
			if (pendingID < 0) {
				pendingID = Player.moddedAccSlots.Count;
				Player.moddedAccSlots.Add(this.FullName);
			}
			this.xColumn = (int)(pendingID / AccessoryPerColumn) + 1;
			this.slot = pendingID % AccessoryPerColumn + Player.vanillaEquipSlots;
		}

		/// <summary>
		/// Hooks into ModPlayer? to save what was in the modded slot for functional, vanity, dyes, and visiblity data.
		/// </summary>
		public override TagCompound Save() { // Needs to revisit and fix up.
			return new TagCompound {
				["functional"] = ItemIO.Save(Player.armor[slot]),
				["dyes"] = ItemIO.Save(Player.dye[slot]),
				["vanity"] = ItemIO.Save(Player.armor[slot + Player.totalEquipSlots]),
				["visible"] = (bool) Player.hideVisibleAccessory[slot]
			};
		}

		/// <summary>
		/// Hooks into ModPlayer? to load what was in the modded slot for functional, vanity, dyes, and visiblity data.
		/// </summary>
		public override void Load(TagCompound tag) {
			Player.armor[slot] = tag.Get<Item>("functional");
			Player.dye[slot] = tag.Get<Item>("dyes");
			Player.armor[slot + Player.totalEquipSlots] = tag.Get<Item>("vanity");
			Player.hideVisibleAccessory[slot] = tag.Get<bool>("visible");
		}

		/// <summary>
		/// Currently never gets called, so this is useless. 
		/// This is deprecated in favour of ItemLoader.CanEquipAccessory
		/// </summary>
		public virtual void SlotCustomCondition() {
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
			this.num20 = 174 + Main.mH;
			this.flag3 = !Player.IsAValidEquipmentSlotForIteration(slot);
			this.PreDrawCustom();
			if (flag3)
				Main.inventoryBack = color2;
		}

		/// <summary>
		/// Is run in this.PreDraw. Override to your leisure. 
		/// Contains default assignment to fields color2, yLoc, and xLoc based on vanilla:
		/// <para>this.color2 = new Microsoft.Xna.Framework.Color(80, 80, 80, 80);</para>
		/// <para>this.yLoc = (int) ((float)(num20) + (float) (slot* 56) * Main.inventoryScale);</para>
		/// <para>this.xLoc = Main.screenWidth - 64 - 28 - 47 * 3;</para>
		/// </summary>
		public void PreDrawCustom() {
			this.color2 = new Microsoft.Xna.Framework.Color(80, 80, 80, 80);
			this.yLoc = (int)((float)(num20) + (float)(slot * 56) * Main.inventoryScale);
			this.xLoc = Main.screenWidth - 64 - 28 - 47 * 3 * xColumn - 50; // 47*3 is per column, 50 adjusts to not overlap vanilla UI
		}

		/// <summary>
		/// Is run in this.Draw. 
		/// Generates a significant amount of functionality for the slot, despite being for drawing because vanilla.
		/// At the end, calls this.DrawModded() where you can override to have custom drawing code for visuals.
		/// Also includes creating hidevisibilitybutton.
		/// </summary>
		private void DrawFunctional() {
			int yLoc2 = yLoc + (int)((float)(-2) + (float)(slot * 56) * Main.inventoryScale);
			int xLoc2 = xLoc - 58 + 64 + 28;
			int context = 8;

			Texture2D value4 = TextureAssets.InventoryTickOn.Value;
			if (Player.hideVisibleAccessory[slot])
				value4 = TextureAssets.InventoryTickOff.Value;

			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(xLoc2, yLoc2, value4.Width, value4.Height);
			int num45 = 0;
			if (rectangle.Contains(new Microsoft.Xna.Framework.Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface) {
				Player.mouseInterface = true;
				if (Main.mouseLeft && Main.mouseLeftRelease) {
					Player.hideVisibleAccessory[slot] = !Player.hideVisibleAccessory[slot];
					SoundEngine.PlaySound(12);
					if (Main.netMode == 1)
						NetMessage.SendData(4, -1, -1, null, Player.whoAmI);
				}

				num45 = ((!Player.hideVisibleAccessory[slot]) ? 1 : 2);
			}
			else if (Main.mouseX >= xLoc && (float)Main.mouseX <= (float)xLoc + (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale && Main.mouseY >= yLoc
				&& (float)Main.mouseY <= (float)yLoc + (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale && !PlayerInput.IgnoreMouseInterface) {

				Main.armorHide = true;
				Player.mouseInterface = true;
				ItemSlot.OverrideHover(Player.armor, context, slot);
				if (!flag3 || Main.mouseItem.IsAir)
					ItemSlot.LeftClick(Player.armor, context, slot);

				ItemSlot.MouseHover(Player.armor, context, slot);
			}

			this.DrawModded(Player.armor, context, slot, new Vector2(xLoc, yLoc));

			//TODO: figure out below code include or not.
			Main.spriteBatch.Draw(value4, new Vector2(xLoc2, yLoc2), Microsoft.Xna.Framework.Color.White * 0.7f);
			if (num45 > 0) {
				Main.HoverItem = new Item();
				Main.hoverItemName = Lang.inter[58 + num45].Value;
			}
		}

		/// <summary>
		/// Is run in this.Draw. 
		/// Generates a significant amount of functionality for the slot, despite being for drawing because vanilla.
		/// At the end, calls this.DrawModded() where you can override to have custom drawing code for visuals.
		/// </summary>
		private void DrawVanity() {
			int vanity = slot + Player.totalEquipSlots;
			bool flag7 = flag3 && !Main.mouseItem.IsAir;
			xLoc -= 47;

			int context = 9;
			if (vanity > Player.totalEquipSlots + Player.totalArmorSlots - 1)
				context = 11;

			if (Main.mouseX >= xLoc && (float)Main.mouseX <= (float)xLoc + (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale && Main.mouseY >= yLoc
				&& (float)Main.mouseY <= (float)yLoc + (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale && !PlayerInput.IgnoreMouseInterface) {
				Player.mouseInterface = true;
				Main.armorHide = true;
				ItemSlot.OverrideHover(Player.armor, context, vanity);
				if (!flag7) {
					ItemSlot.LeftClick(Player.armor, context, vanity);
					ItemSlot.RightClick(Player.armor, context, vanity);
				}

				ItemSlot.MouseHover(Player.armor, context, vanity);
			}

			this.DrawModded(Player.armor, context, vanity, new Vector2(xLoc, yLoc));
		}

		/// <summary>
		/// Is run in this.Draw. 
		/// Generates a significant amount of functionality for the slot, despite being for drawing because vanilla.
		/// At the end, calls this.DrawModded() where you can override to have custom drawing code for visuals.
		/// </summary>
		private void DrawDye() {
			bool flag8 = flag3 && !Main.mouseItem.IsAir;
			xLoc -= 47;

			if (Main.mouseX >= xLoc && (float)Main.mouseX <= (float)xLoc + (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale && Main.mouseY >= yLoc
				&& (float)Main.mouseY <= (float)yLoc + (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale && !PlayerInput.IgnoreMouseInterface) {
				Player.mouseInterface = true;
				Main.armorHide = true;
				ItemSlot.OverrideHover(Player.dye, 12, slot);
				if (!flag8) {
					if (Main.mouseRightRelease && Main.mouseRight)
						ItemSlot.RightClick(Player.dye, 12, slot);

					ItemSlot.LeftClick(Player.dye, 12, slot);
				}

				ItemSlot.MouseHover(Player.dye, 12, slot);
			}
			this.DrawModded(Player.armor, 12, slot, new Vector2(xLoc, yLoc));
		}

		/// <summary>
		/// Is run in each of the DrawX methods. Defaults to run Vanilla ItemSlot.Draw().
		/// Is responsible for actually drawing the visual and could be overriden. Receives parameters:
		/// <para><paramref name="inv"/> :: the array containing all accessory slots, yours is inv[slot] </para>
		/// <para><paramref name="slot"/> :: which is the index for inventory that you were assigned </para>
		/// <para><paramref name="position"/> :: is the position of where the ItemSlot will be drawn </para>
		/// <para><paramref name="context"/> :: 12=>dye; 9,11 => vanity; 8,10 => functional </para>
		/// </summary>
		public void DrawModded(Item[] inv, int context, int slot, Vector2 position) {
			ItemSlot.Draw(Main.spriteBatch, inv, context, slot, position);
		}
	}
}
