using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader.Default;
using Terraria.GameContent.UI.Elements;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This serves as a central place to store equipment slots and their corresponding textures. You will use this to obtain the IDs for your equipment textures.
	/// </summary>
	public static class AccessorySlotLoader {
		// List of registered identifiers for modded accessory slots. Used in ModAccessorySlotPlayer.
		internal static List<string> moddedAccSlots = new List<string>();

		static Player Player => Main.LocalPlayer;
		static internal ModAccessorySlotPlayer dPlayer => Main.LocalPlayer.GetModPlayer<ModAccessorySlotPlayer>();

		internal static ModAccessorySlot GetModAccessorySlot(int slot) {
			ModContent.TryFind<ModAccessorySlot>(moddedAccSlots[slot], out ModAccessorySlot mAccSlot);
			if (mAccSlot == null) {
				mAccSlot = new UnloadedAccessorySlot(slot);
			}

			return mAccSlot;
		}

		internal static int GetAccessorySlotPerColumn(int num20) {
			int accessoryPerColumn = 5;

			accessoryPerColumn += Player.GetAmountOfExtraAccessorySlotsToShow();

			int chkMax = (int)((float)(num20) + (float)(((accessoryPerColumn - 1) + 3) * 56) * Main.inventoryScale) + 4;
			float possibleSpace = (Main.screenHeight - chkMax) / (56 * Main.inventoryScale) - 1.8f;
			if (possibleSpace >= 1) {
				accessoryPerColumn += (int)possibleSpace;
			}

			return accessoryPerColumn;
		}

		public static void DrawAccSlots(int num20) {
			int skip = 0;
			Color color = Main.inventoryBack;

			for (int vanillaSlot = 3; vanillaSlot < Player.dye.Length; vanillaSlot++) {
				if (!Draw(num20, skip, false, vanillaSlot, color)) {
					skip++;
				}
			}

			for (int modSlot = 0; modSlot < moddedAccSlots.Count; modSlot++) {
				if (!Draw(num20, skip, true, modSlot, color))
					skip++;
			}

			if (!(moddedAccSlots.Count == 0)) {
				DrawScrollSwitch(num20);

				if (dPlayer.scrollSlots) {
					DrawScrollbar(num20, skip);
				}
			}
			else
				dPlayer.scrollbarSlotPosition = 0;
		}

		public static string[] scrollStackLang = { Language.GetTextValue("tModLoader.slotStack"), Language.GetTextValue("tModLoader.slotScroll") }; 

		internal static void DrawScrollSwitch(int num20) {
			Texture2D value4 = TextureAssets.InventoryTickOn.Value;
			if (dPlayer.scrollSlots)
				value4 = TextureAssets.InventoryTickOff.Value;

			int xLoc2 = Main.screenWidth - 64 - 28 + 47 + 9;
			int yLoc2 = (int)((float)(num20) + (float)((0 + 3) * 56) * Main.inventoryScale) - 10;

			Main.spriteBatch.Draw(value4, new Vector2(xLoc2, yLoc2), Microsoft.Xna.Framework.Color.White * 0.7f);

			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(xLoc2, yLoc2, value4.Width, value4.Height);
			if (!(rectangle.Contains(new Microsoft.Xna.Framework.Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface)) {
				return;
			}

			Player player = Main.LocalPlayer;
			player.mouseInterface = true;
			if (Main.mouseLeft && Main.mouseLeftRelease) {
				dPlayer.scrollSlots = !dPlayer.scrollSlots;
				SoundEngine.PlaySound(12);
			}

			int num45 = ((!dPlayer.scrollSlots) ? 0 : 1);
			Main.HoverItem = new Item();
			Main.hoverItemName = scrollStackLang[num45];
		}

		// This is a hacky solution to make it very vanilla-esque, at the cost of not actually using a UI proper. 
		internal static void DrawScrollbar(int num20, int skip) {
			int xLoc = Main.screenWidth - 64 - 28;
			int accessoryPerColumn = GetAccessorySlotPerColumn(num20);
			int chkMax = (int)((float)(num20) + (float)(((accessoryPerColumn) + 3) * 56) * Main.inventoryScale) + 4;
			int chkMin = (int)((float)(num20) + (float)((0 + 3) * 56) * Main.inventoryScale) + 4;

			UIScrollbar scrollbar = new UIScrollbar();
			int correctedSlotCount = moddedAccSlots.Count + (Player.dye.Length - 3) - skip - accessoryPerColumn;

			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(xLoc + 47 + 6, chkMin, 5, chkMax - chkMin);
			scrollbar.DrawBar(Main.spriteBatch, Main.Assets.Request<Texture2D>("Images/UI/Scrollbar").Value, rectangle, Color.White);

			int barSize = (chkMax - chkMin) / (correctedSlotCount + 1);
			rectangle = new Microsoft.Xna.Framework.Rectangle(xLoc + 47 + 5, chkMin + dPlayer.scrollbarSlotPosition * barSize, 3, barSize);
			scrollbar.DrawBar(Main.spriteBatch, Main.Assets.Request<Texture2D>("Images/UI/ScrollbarInner").Value, rectangle, Color.White);

			rectangle = new Microsoft.Xna.Framework.Rectangle(xLoc - 47 * 2, chkMin, 47 * 3, chkMax - chkMin);
			if (!(rectangle.Contains(new Microsoft.Xna.Framework.Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface)) {
				return;
			}

			PlayerInput.LockVanillaMouseScroll("ModLoader/Acc");
			int scrollDelta = dPlayer.scrollbarSlotPosition + (int)PlayerInput.ScrollWheelDelta / 120;
			scrollDelta = Math.Min(scrollDelta, correctedSlotCount);
			scrollDelta = Math.Max(scrollDelta, 0);
			dPlayer.scrollbarSlotPosition = scrollDelta;
			PlayerInput.ScrollWheelDelta = 0;
		}

		

		/// <summary>
		/// Draws Vanilla and Modded Accessory Slots
		/// </summary>
		public static bool Draw(int num20, int skip, bool modded, int slot, Color color) {
			bool flag3;
			bool flag4 = false;
			if (modded) {
				flag3 = !ModdedIsAValidEquipmentSlotForIteration(slot);
				flag4 = !ModdedCanSlotBeShown(slot);
			}
			else {
				flag3 = !Player.IsAValidEquipmentSlotForIteration(slot);
				if (slot == 8) {
					flag4 = !Player.CanDemonHeartAccessoryBeShown();
				}
				if (slot == 9) {
					flag4 = !Player.CanMasterModeAccessoryBeShown();
				}
			}

			if (flag4 && flag3) {
				return false;
			}

			Main.inventoryBack = flag3 ? new Microsoft.Xna.Framework.Color(80, 80, 80, 80) : color;
		

			int yLoc = 0, xLoc = 0;
			bool customLoc = false;

			if (modded) {
				ModAccessorySlot mAccSlot = GetModAccessorySlot(slot);
				customLoc = mAccSlot.XLoc != 0 || mAccSlot.YLoc != 0;
				if (customLoc) {
					xLoc = mAccSlot.XLoc;
					yLoc = mAccSlot.YLoc;
				}
				else if (!SetDrawLocation(num20, slot + Player.dye.Length - 3, skip, ref xLoc, ref yLoc))
					return true;

				DrawFunctional(dPlayer.exAccessorySlot, dPlayer.exHideAccessory, -10, slot, flag3, xLoc, yLoc);
				DrawVanity(dPlayer.exAccessorySlot, -11, slot + moddedAccSlots.Count, flag3, xLoc, yLoc);
				DrawDye(dPlayer.exDyesAccessory, -12, slot, flag3, xLoc, yLoc);
			}
			else {
				if (!SetDrawLocation(num20, slot - 3, skip, ref xLoc, ref yLoc))
					return true;

				DrawFunctional(Player.armor, Player.hideVisibleAccessory, 10, slot, flag3, xLoc, yLoc);
				DrawVanity(Player.armor, 11, slot + Player.dye.Length, flag3, xLoc, yLoc);
				DrawDye(Player.dye, 12, slot, flag3, xLoc, yLoc);
			}

			return !customLoc;
		}

		/// <summary>
		/// Applies Xloc and Yloc data for the slot, based on ModAccessorySlotPlayer.scrollSlots
		/// </summary>
		internal static bool SetDrawLocation(int num20, int trueSlot, int skip, ref int xLoc, ref int yLoc) {
			int accessoryPerColumn = GetAccessorySlotPerColumn(num20);
			int xColumn = (int)(trueSlot / accessoryPerColumn);
			int yRow = trueSlot % accessoryPerColumn;
						
			if (dPlayer.scrollSlots) {

				int row = yRow + (xColumn) * accessoryPerColumn - dPlayer.scrollbarSlotPosition - skip;

				yLoc = (int)((float)(num20) + (float)((row + 3) * 56) * Main.inventoryScale) + 4;
				int chkMin = (int)((float)(num20) + (float)((0 + 3) * 56) * Main.inventoryScale) + 4;
				int chkMax = (int)((float)(num20) + (float)(((accessoryPerColumn - 1) + 3) * 56) * Main.inventoryScale) + 4;

				if (yLoc > chkMax || yLoc < chkMin) {
					return false;
				}

				xLoc = Main.screenWidth - 64 - 28;
			}

			else {
				int row = yRow, tempSlot = trueSlot, col = xColumn;
				if (skip > 0) {
					tempSlot -= skip;
					row = tempSlot % accessoryPerColumn;
					col = tempSlot / accessoryPerColumn;
				}

				yLoc = (int)((float)(num20) + (float)((row + 3) * 56) * Main.inventoryScale) + 4;
				if (col > 0) {
					xLoc = Main.screenWidth - 64 - 28 - 47 * 3 * col - 50;
				}
				else {
					xLoc = Main.screenWidth - 64 - 28 - 47 * 3 * col;
				}
			}

			
			return true;
		}

		/// <summary>
		/// Is run in this.Draw. 
		/// Generates a significant amount of functionality for the slot, despite being named drawing because vanilla.
		/// At the end, calls this.DrawModded() where you can override to have custom drawing code for visuals.
		/// Also includes creating hidevisibilitybutton.
		/// </summary>
		internal static void DrawFunctional(Item[] access, bool[] visbility, int context, int slot, bool flag3, int xLoc, int yLoc) {
			int yLoc2 = yLoc - 2;
			int xLoc1 = xLoc;
			int xLoc2 = xLoc1 - 58 + 64 + 28;

			Texture2D value4 = TextureAssets.InventoryTickOn.Value;
			if (visbility[slot])
				value4 = TextureAssets.InventoryTickOff.Value;

			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(xLoc2, yLoc2, value4.Width, value4.Height);
			int num45 = 0;
			if (rectangle.Contains(new Microsoft.Xna.Framework.Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface) {
				Player.mouseInterface = true;
				if (Main.mouseLeft && Main.mouseLeftRelease) {
					visbility[slot] = !visbility[slot];
					SoundEngine.PlaySound(12);
					if (Main.netMode == 1 && context > 0)
						NetMessage.SendData(4, -1, -1, null, Player.whoAmI);
				}

				num45 = ((!visbility[slot]) ? 1 : 2);
			}

			else if (Main.mouseX >= xLoc1 && (float)Main.mouseX <= (float)xLoc1 + (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale && Main.mouseY >= yLoc
				&& (float)Main.mouseY <= (float)yLoc + (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale && !PlayerInput.IgnoreMouseInterface) {

				Main.armorHide = true;
				Player.mouseInterface = true;
				ItemSlot.OverrideHover(access, Math.Abs(context), slot);
				if (!flag3 || Main.mouseItem.IsAir)
					ItemSlot.LeftClick(access, context, slot);

				ItemSlot.MouseHover(access, Math.Abs(context), slot);
			}

			DrawRedirect(access, context, slot, new Vector2(xLoc1, yLoc));

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
		internal static void DrawVanity(Item[] vAccess, int context, int vSlot, bool flag3, int xLoc, int yLoc) {
			bool flag7 = flag3 && !Main.mouseItem.IsAir;
			int xLoc1 = xLoc - 47;

			if (Main.mouseX >= xLoc1 && (float)Main.mouseX <= (float)xLoc1 + (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale && Main.mouseY >= yLoc
				&& (float)Main.mouseY <= (float)yLoc + (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale && !PlayerInput.IgnoreMouseInterface) {
				Player.mouseInterface = true;
				Main.armorHide = true;
				ItemSlot.OverrideHover(vAccess, Math.Abs(context), vSlot);
				if (!flag7) {
					ItemSlot.LeftClick(vAccess, context, vSlot);
					ItemSlot.RightClick(vAccess, Math.Abs(context), vSlot);
				}

				ItemSlot.MouseHover(vAccess, Math.Abs(context), vSlot);
			}

			DrawRedirect(vAccess, context, vSlot, new Vector2(xLoc1, yLoc));
		}

		/// <summary>
		/// Is run in ModAccessorySlot.Draw. 
		/// Generates a significant amount of functionality for the slot, despite being named drawing because vanilla.
		/// At the end, calls this.DrawModded() where you can override to have custom drawing code for visuals.
		/// </summary>
		internal static void DrawDye(Item[] dyes, int context, int slot, bool flag3, int xLoc, int yLoc) {
			bool flag8 = flag3 && !Main.mouseItem.IsAir;
			int xLoc1 = xLoc - 47 * 2;

			if (Main.mouseX >= xLoc1 && (float)Main.mouseX <= (float)xLoc1 + (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale && Main.mouseY >= yLoc
				&& (float)Main.mouseY <= (float)yLoc + (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale && !PlayerInput.IgnoreMouseInterface) {
				Player.mouseInterface = true;
				Main.armorHide = true;
				ItemSlot.OverrideHover(dyes, Math.Abs(context), slot);
				if (!flag8) {
					if (Main.mouseRightRelease && Main.mouseRight)
						ItemSlot.RightClick(dyes, Math.Abs(context), slot);

					ItemSlot.LeftClick(dyes, context, slot);
				}

				ItemSlot.MouseHover(dyes, Math.Abs(context), slot);
			}
			DrawRedirect(dyes, context, slot, new Vector2(xLoc1, yLoc));
		}

		internal static void DrawRedirect(Item[] inv, int context, int slot, Vector2 location) {
			if (context < 0) {
				int tempSlot = slot % moddedAccSlots.Count;
				ModAccessorySlot mAccSlot = GetModAccessorySlot(tempSlot);
				mAccSlot.DrawModded(inv, context, slot, location);
			} else {
				ItemSlot.Draw(Main.spriteBatch, inv, context, slot, location);
			}
		}

		public static bool ModdedIsAValidEquipmentSlotForIteration(int index) {
			index = index % moddedAccSlots.Count;
			ModAccessorySlot mAccSlot = GetModAccessorySlot(index);
			return mAccSlot.IsSlotValid();
		}

		public static bool ModdedCanSlotBeShown(int index) {
			index = index % moddedAccSlots.Count;
			ModAccessorySlot mAccSlot = GetModAccessorySlot(index);
			return mAccSlot.IsSlotVisibleButNotValid();
		}

		public static void VanillaUpdateEquipsMirror(Player player) {
			Item item = null;
			Item vItem = null;
			for (int k = 0; k < moddedAccSlots.Count; k++) {
				if (ModdedIsAValidEquipmentSlotForIteration(k)) {
					item = dPlayer.exAccessorySlot[k];
					vItem = dPlayer.exAccessorySlot[k + moddedAccSlots.Count];
					player.VanillaUpdateEquip(item);
					player.ApplyEquipFunctional(item, dPlayer.exHideAccessory[k]);
					if (SoundLoader.itemToMusic.ContainsKey(item.type))
						Main.musicBox2 = SoundLoader.itemToMusic[item.type];
					VanillaVanityEquipMirror(item, vItem, player, k);
				}
			}
		}

		public static void VanillaVanityEquipMirror(Item item, Item vItem, Player player, int k) {
			if (player.eocDash > 0 && player.shield == -1 && item.shieldSlot != -1) {
				player.shield = item.shieldSlot;
				if (player.cShieldFallback != -1)
					player.cShield = player.cShieldFallback;
			}

			if (player.shieldRaised && player.shield == -1 && item.shieldSlot != -1) {
				player.shield = item.shieldSlot;
				if (player.cShieldFallback != -1)
					player.cShield = player.cShieldFallback;
			}

			if (player.ItemIsVisuallyIncompatible(item))
				return;

			if (item.wingSlot > 0) {
				if (dPlayer.exHideAccessory[k] && (player.velocity.Y == 0f || player.mount.Active))
					return;

				player.wings = item.wingSlot;
			}

			if (!dPlayer.exHideAccessory[k])
				player.UpdateVisibleAccessory(k, item);

			if (!player.ItemIsVisuallyIncompatible(vItem))
				player.UpdateVisibleAccessory(k, vItem);
		}

		public static bool VanillaPreferredGolfBallMirror(ref int projType) {
			for (int num = moddedAccSlots.Count * 2 - 1; num >= 0; num--) {
				if (ModdedIsAValidEquipmentSlotForIteration(num)) {
					_ = num % moddedAccSlots.Count;
					Item item2 = dPlayer.exAccessorySlot[num];
					if (!item2.IsAir && item2.shoot > 0 && ProjectileID.Sets.IsAGolfBall[item2.shoot]) {
						projType = item2.shoot;
						return true;
					}
				}
			}
			return false;
		}

		public static Item VanillaDyeSwapMirror(Item item, out bool success) {
			Item item2 = item;
			int dyeSlotCount = moddedAccSlots.Count;

			for (int i = 0; i < moddedAccSlots.Count; i++) {
				if (dPlayer.exDyesAccessory[i].type == 0) {
					dyeSlotCount = i;
					break;
				}
			}

			if (dyeSlotCount >= moddedAccSlots.Count) {
				success = false;
				return item2;
			}

			item2 = dPlayer.exDyesAccessory[dyeSlotCount].Clone();
			dPlayer.exDyesAccessory[dyeSlotCount] = item.Clone();

			SoundEngine.PlaySound(7);
			Recipe.FindRecipes();
			success = true;
			return item2;
		}

		public static void VanillaLastMinuteFixesMirror(Player newPlayer) {
			ModAccessorySlotPlayer dPlayer = newPlayer.GetModPlayer<ModAccessorySlotPlayer>();
			for (int i = 0; i < moddedAccSlots.Count; i++) {
				int type = dPlayer.exAccessorySlot[i].type;
				if (type == 908 || type == 4874 || type == 5000)
					newPlayer.lavaMax += 420;

				if (type == 906 || type == 4038)
					newPlayer.lavaMax += 420;

				if (newPlayer.wingsLogic == 0 && dPlayer.exAccessorySlot[i].wingSlot >= 0) {
					newPlayer.wingsLogic = dPlayer.exAccessorySlot[i].wingSlot;
					newPlayer.equippedWings = dPlayer.exAccessorySlot[i];
				}
				
				if (type == 158 || type == 396 || type == 1250 || type == 1251 || type == 1252)
					newPlayer.noFallDmg = true;

				newPlayer.lavaTime = newPlayer.lavaMax;
			}
		}

		public static bool ModSlotCheck(Item checkItem, int slot) {
			int index = slot % dPlayer.exDyesAccessory.Length;
			ModAccessorySlot mAccSlot = GetModAccessorySlot(index);
			return mAccSlot.SlotCanAcceptItem(checkItem) && !ItemSlot.AccCheck(Player.armor.Concat(dPlayer.exAccessorySlot).ToArray(), checkItem, slot + Player.armor.Length);
		}
	}
}