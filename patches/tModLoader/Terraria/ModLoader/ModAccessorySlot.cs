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
	public abstract class ModAccessorySlot : ModType
	{
		public Player Player { get; internal set; }

		internal int slot;
		internal bool overrideVanillaConditions = false;

		public void GetModdedAccessorySlot() {
			int pendingID = Player.moddedAccSlots.IndexOf(this.FullName);
			if (pendingID < 0) {
				pendingID = Player.moddedAccSlots.Count;
				Player.moddedAccSlots.Add(this.FullName);
			}
			slot = pendingID + Player.vanillaEquipSlots;
		}

		public TagCompound Save() { // Needs to revisit and fix up.
			return new TagCompound {
				["functional"] = Player.armor[slot],
				["dyes"] = Player.dye[slot],
				["vanity"] = Player.armor[slot + Player.totalEquipSlots],
				["visible"] = (bool) Player.hideVisibleAccessory[slot]
			};
		}

		internal void Load(TagCompound tag) {
			Player.armor[slot] = tag.Get<Item>("functional");
			Player.dye[slot] = tag.Get<Item>("dyes");
			Player.armor[slot + Player.totalEquipSlots] = tag.Get<Item>("functional");
			Player.hideVisibleAccessory[slot] = tag.Get<bool>("visible");
		}

		internal void Draw(int num20) {
			//int num20 = 174 + Main.mh; 
			Microsoft.Xna.Framework.Color color2 = new Microsoft.Xna.Framework.Color(80, 80, 80, 80);
			Microsoft.Xna.Framework.Color color = Main.inventoryBack;
			int dyes = slot + Player.vanillaEquipSlots;
			int functional = dyes;
			int vanity = dyes + Player.totalEquipSlots;

			int yLoc = (int)((float)(num20) + (float)(slot * 56) * Main.inventoryScale);
			int yLoc2 = (int)((float)(num20 - 2) + (float)(slot * 56) * Main.inventoryScale);
			bool flag3 = !Player.IsAValidEquipmentSlotForIteration(dyes);

			int xLoc = Main.screenWidth - 64 - 28 - 47 * 3;

			int xLoc2 = Main.screenWidth - 58 - 47 * 3;
			
			new Microsoft.Xna.Framework.Color(100, 100, 100, 100);
			if (flag3)
				Main.inventoryBack = color2;

			/////////////////////////////////////////////////// Functional
			int context = 8;

			Texture2D value4 = TextureAssets.InventoryTickOn.Value;
			if (Player.hideVisibleAccessory[functional])
				value4 = TextureAssets.InventoryTickOff.Value;

			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(xLoc2, yLoc2, value4.Width, value4.Height);
			int num45 = 0;
			if (functional > Player.totalArmorSlots - 1 && rectangle.Contains(new Microsoft.Xna.Framework.Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface) {
				Player.mouseInterface = true;
				if (Main.mouseLeft && Main.mouseLeftRelease) {
					Player.hideVisibleAccessory[functional] = !Player.hideVisibleAccessory[functional];
					SoundEngine.PlaySound(12);
					if (Main.netMode == 1)
						NetMessage.SendData(4, -1, -1, null, Player.whoAmI);
				}

				num45 = ((!Player.hideVisibleAccessory[functional]) ? 1 : 2);
			}
			else if (Main.mouseX >= xLoc && (float)Main.mouseX <= (float)xLoc + (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale && Main.mouseY >= yLoc 
				&& (float)Main.mouseY <= (float)yLoc + (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale && !PlayerInput.IgnoreMouseInterface) {
					
				Main.armorHide = true;
				Player.mouseInterface = true;
				ItemSlot.OverrideHover(Player.armor, context, functional);
				if (!flag3 || Main.mouseItem.IsAir)
					ItemSlot.LeftClick(Player.armor, context, functional);

				ItemSlot.MouseHover(Player.armor, context, functional);
			}

			this.DrawModded(Player.armor, context, functional, new Vector2(xLoc, yLoc));

			//TODO: figure out if below code is actually needed.
			Main.spriteBatch.Draw(value4, new Vector2(xLoc2, yLoc2), Microsoft.Xna.Framework.Color.White * 0.7f);
			if (num45 > 0) {
				Main.HoverItem = new Item();
				Main.hoverItemName = Lang.inter[58 + num45].Value;
			}
			
			///////////////////////////////////////////////// Vanity
			bool flag7 = flag3 && !Main.mouseItem.IsAir;
			xLoc -= 47;
				
			context = 9;
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
			
			//////////////////////////////////////////////// Dyes
			bool flag8 = flag3 && !Main.mouseItem.IsAir;
			xLoc -= 47;

			if (Main.mouseX >= xLoc && (float)Main.mouseX <= (float)xLoc + (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale && Main.mouseY >= yLoc 
				&& (float)Main.mouseY <= (float)yLoc + (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale && !PlayerInput.IgnoreMouseInterface) {
				Player.mouseInterface = true;
				Main.armorHide = true;
				ItemSlot.OverrideHover(Player.dye, 12, dyes);
				if (!flag8) {
					if (Main.mouseRightRelease && Main.mouseRight)
						ItemSlot.RightClick(Player.dye, 12, dyes);

					ItemSlot.LeftClick(Player.dye, 12, dyes);
				}

				ItemSlot.MouseHover(Player.dye, 12, dyes);
			}
			this.DrawModded(Player.armor, 12, dyes, new Vector2(xLoc, yLoc));
		}
		
		//TODO: Make this overrideable... somehow. This is where custom draw can be done. Should default execute as written,
		public void DrawModded(Item[] armor, int context, int slot, Vector2 position) {
			ItemSlot.Draw(Main.spriteBatch, armor, context, slot, position);
		}
	}
}
