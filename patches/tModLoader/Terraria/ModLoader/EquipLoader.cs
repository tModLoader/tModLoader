using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Initializers;
using Terraria.ModLoader.Core;
using Terraria.UI;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Terraria.GameInput;
using Terraria.UI.Chat;
using Terraria.UI.Gamepad;
using Terraria.ModLoader.Default;
using Terraria.GameContent.UI.Elements;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This serves as a central place to store equipment slots and their corresponding textures. You will use this to obtain the IDs for your equipment textures.
	/// </summary>
	public static class EquipLoader {
		internal static readonly IDictionary<EquipType, int> nextEquip = new Dictionary<EquipType, int>();

		internal static readonly IDictionary<EquipType, IDictionary<int, EquipTexture>> equipTextures =
			new Dictionary<EquipType, IDictionary<int, EquipTexture>>();

		//list of equiptypes and slots registered for an item id. Used for SetDefaults
		internal static readonly IDictionary<int, IDictionary<EquipType, int>> idToSlot =
			new Dictionary<int, IDictionary<EquipType, int>>();

		//holds mappings of slot id -> item id for head/body/legs
		//used to populate Item.(head/body/leg)Type for Manequinns
		internal static readonly IDictionary<EquipType, IDictionary<int, int>> slotToId =
			new Dictionary<EquipType, IDictionary<int, int>>();

		//slot id -> texture name for body slot female/arm textures
		internal static readonly IDictionary<int, string> femaleTextures = new Dictionary<int, string>();
		internal static readonly IDictionary<int, string> armTextures = new Dictionary<int, string>();

		public static readonly EquipType[] EquipTypes = (EquipType[])Enum.GetValues(typeof(EquipType));

		static EquipLoader() {
			foreach (EquipType type in EquipTypes) {
				nextEquip[type] = GetNumVanilla(type);
				equipTextures[type] = new Dictionary<int, EquipTexture>();
			}

			slotToId[EquipType.Head] = new Dictionary<int, int>();
			slotToId[EquipType.Body] = new Dictionary<int, int>();
			slotToId[EquipType.Legs] = new Dictionary<int, int>();
		}

		internal static int ReserveEquipID(EquipType type) => nextEquip[type]++;

		/// <summary>
		/// Gets the equipment texture for the specified equipment type and ID.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="slot"></param>
		/// <returns></returns>
		public static EquipTexture GetEquipTexture(EquipType type, int slot) {
			return equipTextures[type].TryGetValue(slot, out EquipTexture texture) ? texture : null;
		}

		internal static void ResizeAndFillArrays() {
			//Textures
			Array.Resize(ref TextureAssets.ArmorHead, nextEquip[EquipType.Head]);
			Array.Resize(ref TextureAssets.ArmorBody, nextEquip[EquipType.Body]);
			Array.Resize(ref TextureAssets.FemaleBody, nextEquip[EquipType.Body]);
			Array.Resize(ref TextureAssets.ArmorArm, nextEquip[EquipType.Body]);
			Array.Resize(ref TextureAssets.ArmorLeg, nextEquip[EquipType.Legs]);
			Array.Resize(ref TextureAssets.AccHandsOn, nextEquip[EquipType.HandsOn]);
			Array.Resize(ref TextureAssets.AccHandsOff, nextEquip[EquipType.HandsOff]);
			Array.Resize(ref TextureAssets.AccBack, nextEquip[EquipType.Back]);
			Array.Resize(ref TextureAssets.AccFront, nextEquip[EquipType.Front]);
			Array.Resize(ref TextureAssets.AccShoes, nextEquip[EquipType.Shoes]);
			Array.Resize(ref TextureAssets.AccWaist, nextEquip[EquipType.Waist]);
			Array.Resize(ref TextureAssets.Wings, nextEquip[EquipType.Wings]);
			Array.Resize(ref TextureAssets.AccShield, nextEquip[EquipType.Shield]);
			Array.Resize(ref TextureAssets.AccNeck, nextEquip[EquipType.Neck]);
			Array.Resize(ref TextureAssets.AccFace, nextEquip[EquipType.Face]);
			Array.Resize(ref TextureAssets.AccBalloon, nextEquip[EquipType.Balloon]);

			//Sets
			LoaderUtils.ResetStaticMembers(typeof(ArmorIDs), true);
			WingStatsInitializer.Load();

			foreach (EquipType type in EquipTypes) {
				foreach (var entry in equipTextures[type]) {
					int slot = entry.Key;
					EquipTexture texture = entry.Value;

					GetTextureArray(type)[slot] = ModContent.GetTexture(texture.Texture);

					if (type == EquipType.Body) {
						TextureAssets.FemaleBody[slot] = ModContent.GetTexture(femaleTextures[slot]);
						TextureAssets.ArmorArm[slot] = ModContent.GetTexture(armTextures[slot]);
					}
				}
			}

			Array.Resize(ref Item.headType, nextEquip[EquipType.Head]);

			foreach (var entry in slotToId[EquipType.Head]) {
				Item.headType[entry.Key] = entry.Value;
			}

			Array.Resize(ref Item.bodyType, nextEquip[EquipType.Body]);

			foreach (var entry in slotToId[EquipType.Body]) {
				Item.bodyType[entry.Key] = entry.Value;
			}

			Array.Resize(ref Item.legType, nextEquip[EquipType.Legs]);

			foreach (var entry in slotToId[EquipType.Legs]) {
				Item.legType[entry.Key] = entry.Value;
			}
		}

		internal static void Unload() {
			foreach (EquipType type in EquipTypes) {
				nextEquip[type] = GetNumVanilla(type);
				equipTextures[type].Clear();
			}

			idToSlot.Clear();
			slotToId[EquipType.Head].Clear();
			slotToId[EquipType.Body].Clear();
			slotToId[EquipType.Legs].Clear();
			femaleTextures.Clear();
			armTextures.Clear();
		}

		internal static int GetNumVanilla(EquipType type) {
			switch (type) {
				case EquipType.Head:
					return Main.numArmorHead;
				case EquipType.Body:
					return Main.numArmorBody;
				case EquipType.Legs:
					return Main.numArmorLegs;
				case EquipType.HandsOn:
					return Main.numAccHandsOn;
				case EquipType.HandsOff:
					return Main.numAccHandsOff;
				case EquipType.Back:
					return Main.numAccBack;
				case EquipType.Front:
					return Main.numAccFront;
				case EquipType.Shoes:
					return Main.numAccShoes;
				case EquipType.Waist:
					return Main.numAccWaist;
				case EquipType.Wings:
					return Main.maxWings;
				case EquipType.Shield:
					return Main.numAccShield;
				case EquipType.Neck:
					return Main.numAccNeck;
				case EquipType.Face:
					return Main.numAccFace;
				case EquipType.Balloon:
					return Main.numAccBalloon;
			}
			return 0;
		}

		internal static Asset<Texture2D>[] GetTextureArray(EquipType type) {
			switch (type) {
				case EquipType.Head:
					return TextureAssets.ArmorHead;
				case EquipType.Body:
					return TextureAssets.ArmorBody;
				case EquipType.Legs:
					return TextureAssets.ArmorLeg;
				case EquipType.HandsOn:
					return TextureAssets.AccHandsOn;
				case EquipType.HandsOff:
					return TextureAssets.AccHandsOff;
				case EquipType.Back:
					return TextureAssets.AccBack;
				case EquipType.Front:
					return TextureAssets.AccFront;
				case EquipType.Shoes:
					return TextureAssets.AccShoes;
				case EquipType.Waist:
					return TextureAssets.AccWaist;
				case EquipType.Wings:
					return TextureAssets.Wings;
				case EquipType.Shield:
					return TextureAssets.AccShield;
				case EquipType.Neck:
					return TextureAssets.AccNeck;
				case EquipType.Face:
					return TextureAssets.AccFace;
				case EquipType.Balloon:
					return TextureAssets.AccBalloon;
			}

			return null;
		}

		internal static void SetSlot(Item item) {

			if (!idToSlot.TryGetValue(item.type, out IDictionary<EquipType, int> slots))
				return;

			foreach (var entry in slots) {
				int slot = entry.Value;

				switch (entry.Key) {
					case EquipType.Head:
						item.headSlot = slot;
						break;
					case EquipType.Body:
						item.bodySlot = slot;
						break;
					case EquipType.Legs:
						item.legSlot = slot;
						break;
					case EquipType.HandsOn:
						item.handOnSlot = (sbyte)slot;
						break;
					case EquipType.HandsOff:
						item.handOffSlot = (sbyte)slot;
						break;
					case EquipType.Back:
						item.backSlot = (sbyte)slot;
						break;
					case EquipType.Front:
						item.frontSlot = (sbyte)slot;
						break;
					case EquipType.Shoes:
						item.shoeSlot = (sbyte)slot;
						break;
					case EquipType.Waist:
						item.waistSlot = (sbyte)slot;
						break;
					case EquipType.Wings:
						item.wingSlot = (sbyte)slot;
						break;
					case EquipType.Shield:
						item.shieldSlot = (sbyte)slot;
						break;
					case EquipType.Neck:
						item.neckSlot = (sbyte)slot;
						break;
					case EquipType.Face:
						item.faceSlot = (sbyte)slot;
						break;
					case EquipType.Balloon:
						item.balloonSlot = (sbyte)slot;
						break;
				}
			}
		}

		internal static int GetPlayerEquip(Player player, EquipType type) {
			switch (type) {
				case EquipType.Head:
					return player.head;
				case EquipType.Body:
					return player.body;
				case EquipType.Legs:
					return player.legs;
				case EquipType.HandsOn:
					return player.handon;
				case EquipType.HandsOff:
					return player.handoff;
				case EquipType.Back:
					return player.back;
				case EquipType.Front:
					return player.front;
				case EquipType.Shoes:
					return player.shoe;
				case EquipType.Waist:
					return player.waist;
				case EquipType.Wings:
					return player.wings;
				case EquipType.Shield:
					return player.shield;
				case EquipType.Neck:
					return player.neck;
				case EquipType.Face:
					return player.face;
				case EquipType.Balloon:
					return player.balloon;
			}

			return 0;
		}

		/// <summary>
		/// Hook Player.PlayerFrame
		/// Calls each of the item's equipment texture's UpdateVanity hook.
		/// </summary>
		public static void EquipFrameEffects(Player player) {
			foreach (EquipType type in EquipTypes) {
				int slot = GetPlayerEquip(player, type);
				EquipTexture texture = GetEquipTexture(type, slot);
				texture?.FrameEffects(player, type);
			}
		}

		internal static ModAccessorySlot GetModAccessorySlot(int slot) {
			ModContent.TryFind<ModAccessorySlot>(ModAccessorySlot.moddedAccSlots[slot], out ModAccessorySlot mAccSlot);
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
			for (int vanillaSlot = 3; vanillaSlot < Player.dye.Length; vanillaSlot++) {
				if (!EquipLoader.Draw(num20, skip, false, vanillaSlot)) {
					skip++;
				}
			}

			for (int modSlot = 0; modSlot < ModAccessorySlot.moddedAccSlots.Count; modSlot++) {
				if (!EquipLoader.Draw(num20, skip, true, modSlot))
					skip++;
			}

			if (!(ModAccessorySlot.moddedAccSlots.Count == 0)) {
				DrawScrollSwitch(num20);

				if (ModAccessorySlot.scrollSlots) {
					DrawScrollbar(num20, skip);
				}
			}
			else
				ModAccessorySlot.scrollbarSlotPosition = 0;
		}

		//TODO: this should be localizable
		public static string[] scrollStackLang = { "Stack", "Scroll" }; 

		internal static void DrawScrollSwitch(int num20) {
			Texture2D value4 = TextureAssets.InventoryTickOn.Value;
			if (ModAccessorySlot.scrollSlots)
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
			player.mouseInterface = true;
			if (Main.mouseLeft && Main.mouseLeftRelease) {
				ModAccessorySlot.scrollSlots = !ModAccessorySlot.scrollSlots;
				SoundEngine.PlaySound(12);
			}

			int num45 = ((!ModAccessorySlot.scrollSlots) ? 0 : 1);
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
			int correctedSlotCount = ModAccessorySlot.moddedAccSlots.Count + (Player.dye.Length - 3) - skip - accessoryPerColumn;

			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(xLoc + 47 + 6, chkMin, 5, chkMax - chkMin);
			scrollbar.DrawBar(Main.spriteBatch, Main.Assets.Request<Texture2D>("Images/UI/Scrollbar").Value, rectangle, Color.White);

			int barSize = (chkMax - chkMin) / (correctedSlotCount + 1);
			rectangle = new Microsoft.Xna.Framework.Rectangle(xLoc + 47 + 5, chkMin + ModAccessorySlot.scrollbarSlotPosition * barSize, 3, barSize);
			scrollbar.DrawBar(Main.spriteBatch, Main.Assets.Request<Texture2D>("Images/UI/ScrollbarInner").Value, rectangle, Color.White);

			rectangle = new Microsoft.Xna.Framework.Rectangle(xLoc - 47 * 2, chkMin, 47 * 3, chkMax - chkMin);
			if (!(rectangle.Contains(new Microsoft.Xna.Framework.Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface)) {
				return;
			}

			int scrollDelta = ModAccessorySlot.scrollbarSlotPosition + (int)PlayerInput.ScrollWheelDelta / 120;
			scrollDelta = Math.Min(scrollDelta, correctedSlotCount);
			scrollDelta = Math.Max(scrollDelta, 0);
			ModAccessorySlot.scrollbarSlotPosition = scrollDelta;
			PlayerInput.ScrollWheelDelta = 0;
		}

		static Player Player => Main.LocalPlayer;

		/// <summary>
		/// Is run after vanilla draws normal accessory slots. Currently doesn't get called.
		/// Creates new accessory slots in a column to the left of vanilla.  
		/// </summary>
		public static bool Draw(int num20, int skip, bool modded, int slot) {
			bool flag3 = false;
			if (modded)
				flag3 = !EquipLoader.ModdedIsAValidEquipmentSlotForIteration(slot);
			else
				flag3 = !Player.IsAValidEquipmentSlotForIteration(slot);

			if (flag3) {
				return false;
			}

			int yLoc = 0, xLoc = 0;
			
			if (modded) {
				if (!EquipLoader.PreDrawCustomization(num20, slot + Player.dye.Length - 3, skip, ref xLoc, ref yLoc))
					return true;

				DefaultPlayer dPlayer = Main.LocalPlayer.GetModPlayer<DefaultPlayer>();
				EquipLoader.DrawFunctional(dPlayer.exAccessorySlot, dPlayer.exHideAccessory, -10, slot, flag3, xLoc, yLoc);
				EquipLoader.DrawVanity(dPlayer.exAccessorySlot, -11, slot + ModAccessorySlot.moddedAccSlots.Count, flag3, xLoc, yLoc);
				EquipLoader.DrawDye(dPlayer.exDyesAccessory, -12, slot, flag3, xLoc, yLoc);
			}
			else {
				if (!EquipLoader.PreDrawCustomization(num20, slot - 3, skip, ref xLoc, ref yLoc))
					return true;

				EquipLoader.DrawFunctional(Player.armor, Player.hideVisibleAccessory, 10, slot, flag3, xLoc, yLoc);
				EquipLoader.DrawVanity(Player.armor, 11, slot + Player.dye.Length, flag3, xLoc, yLoc);
				EquipLoader.DrawDye(Player.dye, 12, slot, flag3, xLoc, yLoc);
			}

			return true;
		}

		/// <summary>
		/// Is run in this.PreDraw(). Applies Xloc and Yloc data for the slot, based on ModPlayer.scrollSlots
		/// </summary>
		internal static bool PreDrawCustomization(int num20, int trueSlot, int skip, ref int xLoc, ref int yLoc) {
			int accessoryPerColumn = GetAccessorySlotPerColumn(num20);
			int xColumn = (int)(trueSlot / accessoryPerColumn);
			int yRow = trueSlot % accessoryPerColumn;
						
			if (ModAccessorySlot.scrollSlots) {

				int row = yRow + (xColumn) * accessoryPerColumn - ModAccessorySlot.scrollbarSlotPosition - skip;

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
				int tempSlot = slot % ModAccessorySlot.moddedAccSlots.Count;
				ModAccessorySlot mAccSlot = GetModAccessorySlot(tempSlot);
				mAccSlot.DrawRedirect(inv, context, slot, location);
			} else {
				ItemSlot.Draw(Main.spriteBatch, inv, context, slot, location);
			}
		}

		public static bool ModdedIsAValidEquipmentSlotForIteration(int index) {
			index = index % ModAccessorySlot.moddedAccSlots.Count;
			ModAccessorySlot mAccSlot = GetModAccessorySlot(index);
			return mAccSlot.CanUseSlot();
		}

		public static void VanillaUpdateEquipsMirror(Player player) {
			DefaultPlayer dPlayer = Main.LocalPlayer.GetModPlayer<DefaultPlayer>();
			Item item = null;
			Item vItem = null;
			for (int k = 0; k < ModAccessorySlot.moddedAccSlots.Count; k++) {
				if (ModdedIsAValidEquipmentSlotForIteration(k)) {
					item = dPlayer.exAccessorySlot[k];
					vItem = dPlayer.exAccessorySlot[k + ModAccessorySlot.moddedAccSlots.Count];
					player.VanillaUpdateEquip(item);
					player.ApplyEquipFunctional(item, dPlayer.exHideAccessory[k]);
					if (SoundLoader.itemToMusic.ContainsKey(item.type))
						Main.musicBox2 = SoundLoader.itemToMusic[item.type];
					VanillaVanityEquipMirror(item, vItem, player, k, dPlayer);
				}

			}
		}

		public static void VanillaVanityEquipMirror(Item item, Item vItem, Player player, int k, DefaultPlayer dPlayer) {
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
				player.UpdateVisibleAccessory(item);

			if (!player.ItemIsVisuallyIncompatible(vItem))
				player.UpdateVisibleAccessory(vItem);
		}

		public static bool VanillaPreferredGolfBallMirror(ref int projType, Player player) {
			DefaultPlayer dPlayer = Main.LocalPlayer.GetModPlayer<DefaultPlayer>();
			for (int num = ModAccessorySlot.moddedAccSlots.Count * 2 - 1; num >= 0; num--) {
				if (ModdedIsAValidEquipmentSlotForIteration(num)) {
					_ = num % ModAccessorySlot.moddedAccSlots.Count;
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
			DefaultPlayer dPlayer = Main.LocalPlayer.GetModPlayer<DefaultPlayer>();
			Item item2 = item;
			int dyeSlotCount = ModAccessorySlot.moddedAccSlots.Count;

			for (int i = 0; i < ModAccessorySlot.moddedAccSlots.Count; i++) {
				if (dPlayer.exDyesAccessory[i].type == 0) {
					dyeSlotCount = i;
					break;
				}
			}

			if (dyeSlotCount >= ModAccessorySlot.moddedAccSlots.Count) {
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

		public static Item VanillaArmorSwapMirror(Item item, out bool success) {
			DefaultPlayer dPlayer = Main.LocalPlayer.GetModPlayer<DefaultPlayer>();
			int num2 = 0;
			int accSlotToSwapTo = ModAccessorySlot.moddedAccSlots.Count;
			success = false;

			for (int i = 0; i < ModAccessorySlot.moddedAccSlots.Count; i++) {
				if (ModdedIsAValidEquipmentSlotForIteration(i)) {
					num2 = i;
					if (dPlayer.exAccessorySlot[i].type == 0) {
						accSlotToSwapTo = i;
						break;
					}
				}
			}

			for (int j = 0; j < dPlayer.exAccessorySlot.Length; j++) {
				if (item.IsTheSameAs(dPlayer.exAccessorySlot[j]))
					accSlotToSwapTo = j;

				if (j < ModAccessorySlot.moddedAccSlots.Count && item.wingSlot > 0 && dPlayer.exAccessorySlot[j].wingSlot > 0)
					accSlotToSwapTo = j;
			}

			for (int k = 0; k < num2; k++) {
				int index = (accSlotToSwapTo + num2) % num2;
				if (ItemLoader.CanEquipAccessory(item, index)) {
					accSlotToSwapTo = index;
					break;
				}
			}

			if (accSlotToSwapTo > num2)
				return item;

			if (ItemSlot.isEquipLocked(dPlayer.exAccessorySlot[accSlotToSwapTo].type))
				return item;

			for (int k = 0; k < dPlayer.exAccessorySlot.Length; k++) {
				if (item.IsTheSameAs(dPlayer.exAccessorySlot[k]))
					accSlotToSwapTo = k;
			}

			if (!ItemLoader.CanEquipAccessory(item, accSlotToSwapTo))
				return item;

			Item result = dPlayer.exAccessorySlot[accSlotToSwapTo].Clone();
			dPlayer.exAccessorySlot[accSlotToSwapTo] = item.Clone();

			SoundEngine.PlaySound(7);
			Recipe.FindRecipes();
			success = true;
			return result;
		}

		public static void VanillaLastMinuteFixesMirror(Player newPlayer) {
			DefaultPlayer dPlayer = newPlayer.GetModPlayer<DefaultPlayer>();
			for (int i = 0; i < ModAccessorySlot.moddedAccSlots.Count; i++) {
				int type = dPlayer.exAccessorySlot[i].type;
				if (type == 908 || type == 4874 || type == 5000)
					newPlayer.lavaMax += 420;

				if (type == 906 || type == 4038)
					newPlayer.lavaMax += 420;

				if (newPlayer.wingsLogic == 0 && dPlayer.exAccessorySlot[i].wingSlot >= 0)
					newPlayer.wingsLogic = dPlayer.exAccessorySlot[i].wingSlot;

				if (type == 158 || type == 396 || type == 1250 || type == 1251 || type == 1252)
					newPlayer.noFallDmg = true;

				newPlayer.lavaTime = newPlayer.lavaMax;
			}
		}

		public static bool ModSlotCheck(Item checkItem, int slot) {
			DefaultPlayer dPlayer = Main.LocalPlayer.GetModPlayer<DefaultPlayer>();
			int index = slot % dPlayer.exDyesAccessory.Length;
			ModAccessorySlot mAccSlot = GetModAccessorySlot(index);
			return mAccSlot.LimitWhatCanGoInSlot(checkItem) && !ItemSlot.AccCheck(dPlayer.exAccessorySlot, checkItem, slot);
		}
	}
}