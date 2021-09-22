using Microsoft.Xna.Framework.Graphics;
using System;
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
	//TODO: further documentation
	/// <summary>
	/// This serves as a central place to store equipment slots and their corresponding textures. You will use this to obtain the IDs for your equipment textures.
	/// </summary>
	public class AccessorySlotLoader : Loader<ModAccessorySlot> {
		static Player Player => Main.LocalPlayer;
		static internal ModAccessorySlotPlayer ModSlotPlayer(Player player) => player.GetModPlayer<ModAccessorySlotPlayer>();

		public AccessorySlotLoader() => Initialize(0);

		public ModAccessorySlot Get(int id) => list[id % list.Count];

		public const int MaxVanillaSlotCount = 2 + 5;

		// DRAWING CODE ///////////////////////////////////////////////////////////////////
		internal int GetAccessorySlotPerColumn(int num20) {
			float minimumClearance = num20 + 2 * 56 * Main.inventoryScale + 4;
			return (int)((Main.screenHeight - minimumClearance) / (56 * Main.inventoryScale) - 1.8f);
		}

		public void DrawAccSlots(int num20) {
			int skip = 0;
			Color color = Main.inventoryBack;

			for (int vanillaSlot = 3; vanillaSlot < Player.dye.Length; vanillaSlot++) {
				if (!Draw(num20, skip, false, vanillaSlot, color)) {
					skip++;
				}
			}

			for (int modSlot = 0; modSlot < list.Count; modSlot++) {
				if (!Draw(num20, skip, true, modSlot, color))
					skip++;
			}

			if (!(list.Count == 0)) {
				DrawScrollSwitch(num20);

				if (ModSlotPlayer(Player).scrollSlots) {
					DrawScrollbar(num20, skip);
				}
			}
			else
				ModSlotPlayer(Player).scrollbarSlotPosition = 0;
		}

		public static string[] scrollStackLang = { Language.GetTextValue("tModLoader.slotStack"), Language.GetTextValue("tModLoader.slotScroll") }; 

		internal void DrawScrollSwitch(int num20) {
			Texture2D value4 = TextureAssets.InventoryTickOn.Value;
			if (ModSlotPlayer(Player).scrollSlots)
				value4 = TextureAssets.InventoryTickOff.Value;

			int xLoc2 = Main.screenWidth - 64 - 28 + 47 + 9;
			int yLoc2 = (int)((float)(num20) + (float)((0 + 3) * 56) * Main.inventoryScale) - 10;

			Main.spriteBatch.Draw(value4, new Vector2(xLoc2, yLoc2), Color.White * 0.7f);

			Rectangle rectangle = new Rectangle(xLoc2, yLoc2, value4.Width, value4.Height);
			if (!(rectangle.Contains(new Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface)) {
				return;
			}

			Player player = Main.LocalPlayer;
			player.mouseInterface = true;
			if (Main.mouseLeft && Main.mouseLeftRelease) {
				ModSlotPlayer(Player).scrollSlots = !ModSlotPlayer(Player).scrollSlots;
				SoundEngine.PlaySound(12);
			}

			int num45 = ((!ModSlotPlayer(Player).scrollSlots) ? 0 : 1);
			Main.HoverItem = new Item();
			Main.hoverItemName = scrollStackLang[num45];
		}

		// This is a hacky solution to make it very vanilla-esque, at the cost of not actually using a UI proper. 
		internal void DrawScrollbar(int num20, int skip) {
			int xLoc = Main.screenWidth - 64 - 28;

			int accessoryPerColumn = GetAccessorySlotPerColumn(num20);
			int slotsToRender = list.Count + MaxVanillaSlotCount - skip;
			int scrollIncrement = slotsToRender - accessoryPerColumn;

			if (scrollIncrement < 0) {
				accessoryPerColumn = slotsToRender;
				scrollIncrement = 0;
			}

			int chkMax = (int)((float)(num20) + (float)(((accessoryPerColumn) + 3) * 56) * Main.inventoryScale) + 4;
			int chkMin = (int)((float)(num20) + (float)((0 + 3) * 56) * Main.inventoryScale) + 4;

			UIScrollbar scrollbar = new UIScrollbar();

			Rectangle rectangle = new Rectangle(xLoc + 47 + 6, chkMin, 5, chkMax - chkMin);
			scrollbar.DrawBar(Main.spriteBatch, Main.Assets.Request<Texture2D>("Images/UI/Scrollbar").Value, rectangle, Color.White);
			
			int barSize = (chkMax - chkMin) / (scrollIncrement + 1);
			rectangle = new Rectangle(xLoc + 47 + 5, chkMin + ModSlotPlayer(Player).scrollbarSlotPosition * barSize, 3, barSize);
			scrollbar.DrawBar(Main.spriteBatch, Main.Assets.Request<Texture2D>("Images/UI/ScrollbarInner").Value, rectangle, Color.White);

			rectangle = new Rectangle(xLoc - 47 * 2, chkMin, 47 * 3, chkMax - chkMin);
			if (!(rectangle.Contains(new Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface)) {
				return;
			}

			PlayerInput.LockVanillaMouseScroll("ModLoader/Acc");

			int scrollDelta = ModSlotPlayer(Player).scrollbarSlotPosition + (int)PlayerInput.ScrollWheelDelta / 120;
			scrollDelta = Math.Min(scrollDelta, scrollIncrement);
			scrollDelta = Math.Max(scrollDelta, 0);

			ModSlotPlayer(Player).scrollbarSlotPosition = scrollDelta;
			PlayerInput.ScrollWheelDelta = 0;
		}		

		/// <summary>
		/// Draws Vanilla and Modded Accessory Slots
		/// </summary>
		public bool Draw(int num20, int skip, bool modded, int slot, Color color) {
			bool flag3;
			bool flag4 = false;

			if (modded) {
				flag3 = !ModdedIsAValidEquipmentSlotForIteration(slot);
				flag4 = !ModdedCanSlotBeShown(slot);
			}
			else {
				flag3 = !Player.IsAValidEquipmentSlotForIteration(slot);
				if (slot == 8) {
					flag4 = (slot == 8) && !Player.CanDemonHeartAccessoryBeShown();
				}
				else if (slot == 9) {
					flag4 = !Player.CanMasterModeAccessoryBeShown();
				}
			}

			if (flag4 && flag3) {
				return false;
			}

			Main.inventoryBack = flag3 ? new Color(80, 80, 80, 80) : color;

			int yLoc = 0, xLoc = 0;
			bool customLoc = false;

			if (modded) {
				ModAccessorySlot mAccSlot = list[slot];
				customLoc = mAccSlot.XLoc != 0 || mAccSlot.YLoc != 0;
				if (customLoc) {
					xLoc = mAccSlot.XLoc;
					yLoc = mAccSlot.YLoc;
				}
				else if (!SetDrawLocation(num20, slot + Player.dye.Length - 3, skip, ref xLoc, ref yLoc))
					return true;

				var thisSlot = Get(slot);

				if (thisSlot.DrawFunctionalSlot) 
					DrawFunctional(ModSlotPlayer(Player).exAccessorySlot, ModSlotPlayer(Player).exHideAccessory, -10, slot, flag3, xLoc, yLoc);
				if (thisSlot.DrawVanitySlot)
					DrawSlot(ModSlotPlayer(Player).exAccessorySlot, -11, slot + list.Count, flag3, xLoc, yLoc);
				if (thisSlot.DrawDyeSlot)
					DrawSlot(ModSlotPlayer(Player).exDyesAccessory, -12, slot, flag3, xLoc, yLoc);
			}
			else {
				if (!SetDrawLocation(num20, slot - 3, skip, ref xLoc, ref yLoc))
					return true;

				DrawFunctional(Player.armor, Player.hideVisibleAccessory, 10, slot, flag3, xLoc, yLoc);
				DrawSlot(Player.armor, 11, slot + Player.dye.Length, flag3, xLoc, yLoc);
				DrawSlot(Player.dye, 12, slot, flag3, xLoc, yLoc);
			}

			return !customLoc;
		}

		/// <summary>
		/// Applies Xloc and Yloc data for the slot, based on ModAccessorySlotPlayer.scrollSlots
		/// </summary>
		internal bool SetDrawLocation(int num20, int trueSlot, int skip, ref int xLoc, ref int yLoc) {
			int accessoryPerColumn = GetAccessorySlotPerColumn(num20);
			int xColumn = (int)(trueSlot / accessoryPerColumn);
			int yRow = trueSlot % accessoryPerColumn;
						
			if (ModSlotPlayer(Player).scrollSlots) {

				int row = yRow + (xColumn) * accessoryPerColumn - ModSlotPlayer(Player).scrollbarSlotPosition - skip;

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
		/// Is run in AccessorySlotLoader.Draw. 
		/// Generates a significant amount of functionality for the slot, despite being named drawing because vanilla.
		/// At the end, calls this.DrawModded() where you can override to have custom drawing code for visuals.
		/// Also includes creating hidevisibilitybutton.
		/// </summary>
		internal void DrawFunctional(Item[] access, bool[] visbility, int context, int slot, bool flag3, int xLoc, int yLoc) {
			int yLoc2 = yLoc - 2;
			int xLoc1 = xLoc;
			int xLoc2 = xLoc1 - 58 + 64 + 28;

			Texture2D value4 = TextureAssets.InventoryTickOn.Value;
			if (visbility[slot])
				value4 = TextureAssets.InventoryTickOff.Value;

			Rectangle rectangle = new Rectangle(xLoc2, yLoc2, value4.Width, value4.Height);
			int num45 = 0;
			bool skipCheck = false;
			if (rectangle.Contains(new Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface) {
				skipCheck = true;
				Player.mouseInterface = true;
				
				if (Main.mouseLeft && Main.mouseLeftRelease) {
					visbility[slot] = !visbility[slot];
					SoundEngine.PlaySound(12);
					
					if (Main.netMode == 1 && context > 0)
						NetMessage.SendData(4, -1, -1, null, Player.whoAmI);
				}

				num45 = ((!visbility[slot]) ? 1 : 2);
			}

			DrawSlot(access, context, slot, flag3, xLoc, yLoc, skipCheck);

			Main.spriteBatch.Draw(value4, new Vector2(xLoc2, yLoc2), Color.White * 0.7f);

			if (num45 > 0) {
				Main.HoverItem = new Item();
				Main.hoverItemName = Lang.inter[58 + num45].Value;
			}
		}

		/// <summary>
		/// Provides the Texture for a Modded Accessory Slot
		/// This probably will need optimization down the road.
		/// </summary>
		internal Texture2D GetBackgroundTexture(int slot, int context) {
			var thisSlot = Get(slot);
			switch (context) {
				case -10:
					if (ModContent.RequestIfExists<Texture2D>(thisSlot.FunctionalBackgroundTexture, out var funcTexture))
						return funcTexture.Value;
					return TextureAssets.InventoryBack3.Value;
				case -11:
					if (ModContent.RequestIfExists<Texture2D>(thisSlot.VanityBackgroundTexture, out var vanityTexture))
						return vanityTexture.Value;
					return TextureAssets.InventoryBack8.Value;
				case -12:
					if (ModContent.RequestIfExists<Texture2D>(thisSlot.DyeBackgroundTexture, out var dyeTexture))
						return dyeTexture.Value;
					return TextureAssets.InventoryBack12.Value;
			}
			
			// Default to a functional slot
			return TextureAssets.InventoryBack3.Value;
		}

		internal void DrawSlotTexture(Texture2D value6, Vector2 position, Rectangle rectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, int slot, int context) {
			var thisSlot = Get(slot);
			Texture2D texture = null;
			switch (context) {
				case -10:
					if (ModContent.RequestIfExists<Texture2D>(thisSlot.FunctionalTexture, out var funcTexture))
						texture = funcTexture.Value;
					break;
				case -11:
					if (ModContent.RequestIfExists<Texture2D>(thisSlot.VanityTexture, out var vanityTexture))
						texture = vanityTexture.Value;
					break;
				case -12:
					if (ModContent.RequestIfExists<Texture2D>(thisSlot.DyeTexture, out var dyeTexture))
						texture = dyeTexture.Value;
					break;
			}

			if (texture == null)
				texture = value6;
			else
				rectangle = new Rectangle(0, 0, 32, 32);

			Main.spriteBatch.Draw(texture, position, rectangle, color, rotation, origin, scale, effects, layerDepth);
		}

		/// <summary>
		/// Is run in AccessorySlotLoader.Draw. 
		/// Generates a significant amount of functionality for the slot, despite being named drawing because vanilla.
		/// At the end, calls this.DrawRedirect to enable custom drawing
		/// </summary>
		internal void DrawSlot(Item[] items, int context, int slot, bool flag3, int xLoc, int yLoc, bool skipCheck = false) {
			bool flag = flag3 && !Main.mouseItem.IsAir;
			int xLoc1 = xLoc - 47 * (Math.Abs(context) - 10);

			if (!skipCheck && Main.mouseX >= xLoc1 && (float)Main.mouseX <= (float)xLoc1 + (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale && Main.mouseY >= yLoc
				&& (float)Main.mouseY <= (float)yLoc + (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale && !PlayerInput.IgnoreMouseInterface) {

				Player.mouseInterface = true;
				Main.armorHide = true;
				ItemSlot.OverrideHover(items, Math.Abs(context), slot);

				if (!flag) {
					if (Math.Abs(context) == 12) {
						if (Main.mouseRightRelease && Main.mouseRight)
							ItemSlot.RightClick(items, Math.Abs(context), slot);

						ItemSlot.LeftClick(items, context, slot);
					}
					else if (Math.Abs(context) == 11) {
						ItemSlot.LeftClick(items, context, slot);
						ItemSlot.RightClick(items, Math.Abs(context), slot);
					}
					else if (Math.Abs(context) == 10) {
						ItemSlot.LeftClick(items, context, slot);
					}
				}

				ItemSlot.MouseHover(items, Math.Abs(context), slot);
			}
			DrawRedirect(items, context, slot, new Vector2(xLoc1, yLoc));
		}

		internal void DrawRedirect(Item[] inv, int context, int slot, Vector2 location) {
			if (context < 0) {
				Get(slot).DrawModded(inv, context, slot, location);
			} else {
				ItemSlot.Draw(Main.spriteBatch, inv, context, slot, location);
			}
		}

		// VANILLA FUNCTIONALITY CODE ////////////////////////////////////////////////////////////////////////////
		public bool ModdedIsAValidEquipmentSlotForIteration(int index) => Get(index).IsEnabled();

		public bool ModdedCanSlotBeShown(int index) => Get(index).IsVisibleWhenNotEnabled();

		public bool CanAcceptItem(int index, Item checkItem) => Get(index).CanAcceptItem(checkItem);

		/// <summary>
		/// Checks if the provided item can go in to the provided slot. 
		/// Includes checking if the item already exists in either of Player.Armor or ModSlotPlayer.exAccessorySlot
		/// Invokes directly ItemSlot.AccCheck & ModSlot.CanAcceptItem
		/// </summary>
		public bool ModSlotCheck(Item checkItem, int slot) => CanAcceptItem(slot, checkItem) &&
			!ItemSlot.AccCheck(Player.armor.Concat(ModSlotPlayer(Player).exAccessorySlot).ToArray(), checkItem, slot + Player.armor.Length);

		/// <summary>
		/// Runs a simplified version of Player.UpdateEquips for the Modded Accessory Slots
		/// </summary>
		public void UpdateEquips(Player player) {
			for (int k = 0; k < ModSlotPlayer(player).SlotCount(); k++) {
				if (ModdedIsAValidEquipmentSlotForIteration(k)) {
					Item item = ModSlotPlayer(player).exAccessorySlot[k];

					player.VanillaUpdateEquip(item);
					player.ApplyEquipFunctional(item, ModSlotPlayer(player).exHideAccessory[k]);

					if (MusicLoader.itemToMusic.ContainsKey(item.type))
						Main.musicBox2 = MusicLoader.itemToMusic[item.type];
				}
			}

			UpdateVisibleAccessories(player);
		}

		/// <summary>
		/// Updates all vanity information on the player for Mod Slots, in a similar fashion to Player.UpdateVisibleAccessories
		/// </summary>
		public void UpdateVisibleAccessories(Player player) {
			var modSlotPlayer = ModSlotPlayer(player);
			for (int k = 0; k < modSlotPlayer.SlotCount(); k++) {
				if (ModdedIsAValidEquipmentSlotForIteration(k)) {
					player.ApplyEquipVanity(modSlotPlayer.exAccessorySlot[k + modSlotPlayer.SlotCount()]);
					player.UpdateVisibleAccessories(modSlotPlayer.exAccessorySlot[k], modSlotPlayer.exAccessorySlot[k + modSlotPlayer.SlotCount()], modSlotPlayer.exHideAccessory[k], k, true);
				}
			}
		}

		/// <summary>
		/// Mirrors Player.UpdateDyes() for modded slots
		/// </summary>
		public void UpdateDyes(Player player) {
			var modSlotPlayer = ModSlotPlayer(player);
			for (int i = 0; i < modSlotPlayer.SlotCount() * 2; i++) {
				if (ModdedIsAValidEquipmentSlotForIteration(i)) {
					int num = i % modSlotPlayer.exDyesAccessory.Length;
					player.UpdateItemDye(i < modSlotPlayer.exDyesAccessory.Length, modSlotPlayer.exHideAccessory[num], modSlotPlayer.exAccessorySlot[i], modSlotPlayer.exDyesAccessory[num]);
				}
			}
		}

		/// <summary>
		/// Mirrors Player.GetPreferredGolfBallToUse.
		/// Provides the golf ball projectile from an accessory slot. 
		/// </summary>
		public bool PreferredGolfBall(ref int projType) {
			for (int num = ModSlotPlayer(Player).SlotCount() * 2 - 1; num >= 0; num--) {
				if (ModdedIsAValidEquipmentSlotForIteration(num)) {
					Item item2 = ModSlotPlayer(Player).exAccessorySlot[num];
					if (!item2.IsAir && item2.shoot > 0 && ProjectileID.Sets.IsAGolfBall[item2.shoot]) {
						projType = item2.shoot;
						return true;
					}
				}
			}
			return false;
		}
	}
}