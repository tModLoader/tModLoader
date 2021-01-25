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
		internal bool overrideVanillaConditions = false;

		internal int slot;
		private int num20;
		private Microsoft.Xna.Framework.Color color2;
		private int yLoc;
		private int xLoc;
		private bool flag3;

		protected void GetModdedAccessorySlot() {
			int pendingID = Player.moddedAccSlots.IndexOf(this.FullName);
			if (pendingID < 0) {
				pendingID = Player.moddedAccSlots.Count;
				Player.moddedAccSlots.Add(this.FullName);
			}
			this.slot = pendingID + Player.vanillaEquipSlots;
		}

		public override TagCompound Save() { // Needs to revisit and fix up.
			return new TagCompound {
				["slot"] = ItemIO.Save(Player.armor[slot]),
				["dyes"] = ItemIO.Save(Player.dye[slot]),
				["vanity"] = ItemIO.Save(Player.armor[slot + Player.totalEquipSlots]),
				["visible"] = (bool) Player.hideVisibleAccessory[slot]
			};
		}

		public override void Load(TagCompound tag) {
			Player.armor[slot] = tag.Get<Item>("slot");
			Player.dye[slot] = tag.Get<Item>("dyes");
			Player.armor[slot + Player.totalEquipSlots] = tag.Get<Item>("slot");
			Player.hideVisibleAccessory[slot] = tag.Get<bool>("visible");
		}

		public void Draw(int num20) { 
			this.PreDraw(num20);
			this.DrawFunctional();
			this.DrawVanity();
			this.DrawDye();
		}

		internal void PreDraw(int num20) {
			this.num20 = num20;
			this.flag3 = !Player.IsAValidEquipmentSlotForIteration(slot);
			//int num20 = 174 + Main.mh; 
			this.PreDrawCustom();
			if (flag3)
				Main.inventoryBack = color2;
		}

		// Override Method for setting custom colour and/or custom position
		public void PreDrawCustom() {
			this.color2 = new Microsoft.Xna.Framework.Color(80, 80, 80, 80);
			this.yLoc = (int)((float)(num20) + (float)(slot * 56) * Main.inventoryScale);
			this.xLoc = Main.screenWidth - 64 - 28 - 47 * 3;
		}

		internal void DrawFunctional() {
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

		internal void DrawVanity() {
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

		internal void DrawDye() {
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

		// Override here for creating custom drawing at generated position
		public void DrawModded(Item[] inv, int context, int slot, Vector2 position) {
			ItemSlot.Draw(Main.spriteBatch, inv, context, slot, position);
		}
	}
}
