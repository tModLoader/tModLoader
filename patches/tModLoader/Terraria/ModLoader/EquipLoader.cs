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
			ModContent.TryFind<ModAccessorySlot>(ModPlayer.moddedAccSlots[slot], out ModAccessorySlot mAccSlot);
			if (mAccSlot == null) {
				mAccSlot = new UnloadedAccessorySlot(slot);
			}

			return mAccSlot;
		}

		public static void DrawModAccSlots(int num20) {

			if (ModPlayer.moddedAccSlots.Count > ModAccessorySlot.accessoryPerColumn) {
				DrawScrollSwitch(num20);

				if (ModPlayer.scrollSlots) {
					DrawScrollbar(num20);
				}
			}

			for (int modSlot = 0; modSlot < ModPlayer.moddedAccSlots.Count; modSlot++) {
				ModAccessorySlot mAccSlot = GetModAccessorySlot(modSlot);
				mAccSlot.Draw(num20);
			}
		}

		//TODO: Change the tooltip to be Scroll/Stack and put a custom sprite instead of reusing visibility.
		internal static void DrawScrollSwitch(int num20) {
			Texture2D value4 = TextureAssets.InventoryTickOn.Value;
			if (ModPlayer.scrollSlots)
				value4 = TextureAssets.InventoryTickOff.Value;

			int xLoc2 = Main.screenWidth - 64 - 28 - 47 * 3 - 50 - 24;
			int yLoc2 = (int)((float)(num20) + (float)((0 + 3) * 56) * Main.inventoryScale) - 28;

			Main.spriteBatch.Draw(value4, new Vector2(xLoc2, yLoc2), Microsoft.Xna.Framework.Color.White * 0.7f);

			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(xLoc2, yLoc2, value4.Width, value4.Height);
			if (!(rectangle.Contains(new Microsoft.Xna.Framework.Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface)) {
				return;
			}

			Player player = Main.LocalPlayer;
			player.mouseInterface = true;
			if (Main.mouseLeft && Main.mouseLeftRelease) {
				ModPlayer.scrollSlots = !ModPlayer.scrollSlots;
				SoundEngine.PlaySound(12);
			}

			int num45 = ((!ModPlayer.scrollSlots) ? 1 : 2);
			Main.HoverItem = new Item();
			Main.hoverItemName = Lang.inter[58 + num45].Value;
		}

		//TODO: Actually implement a UI properly.
		internal static void DrawScrollbar(int num20) {
			int xLoc = Main.screenWidth - 64 - 28 - 47 * 3 - 50;
			int chkMax = (int)((float)(num20) + (float)(((ModAccessorySlot.accessoryPerColumn) + 3) * 56) * Main.inventoryScale) + 4;
			int chkMin = (int)((float)(num20) + (float)((0 + 3) * 56) * Main.inventoryScale) + 4;

			UIScrollbar scrollbar = new UIScrollbar();
			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(xLoc - 47 * 2 - 6, chkMin, 5, chkMax - chkMin);

			scrollbar.DrawBar(Main.spriteBatch, Main.Assets.Request<Texture2D>("Images/UI/Scrollbar").Value, rectangle, Color.White);

			rectangle = new Microsoft.Xna.Framework.Rectangle(xLoc - 47 * 2, chkMin, 47 * 3, chkMax - chkMin);
			if (!(rectangle.Contains(new Microsoft.Xna.Framework.Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface)) {
				return;
			}

			int scrollDelta = ModPlayer.scrollbarSlotPosition + (int)PlayerInput.ScrollWheelDelta / 120;
			scrollDelta = Math.Min(scrollDelta, ModPlayer.moddedAccSlots.Count - ModAccessorySlot.accessoryPerColumn);
			scrollDelta = Math.Max(scrollDelta, 0);
			ModPlayer.scrollbarSlotPosition = scrollDelta;
			PlayerInput.ScrollWheelDelta = 0;
		}

		public static bool ModdedIsAValidEquipmentSlotForIteration(int index) {
			index = index % ModPlayer.moddedAccSlots.Count;
			ModAccessorySlot mAccSlot = GetModAccessorySlot(index);
			return mAccSlot.CanUseSlot();
		}

		public static void VanillaUpdateEquipsMirror(Player player) {
			ModPlayer dPlayer = Main.LocalPlayer.GetModPlayer<DefaultPlayer>();
			Item item = null;
			Item vItem = null;
			for (int k = 0; k < ModPlayer.moddedAccSlots.Count; k++) {
				if (ModdedIsAValidEquipmentSlotForIteration(k)) {
					item = dPlayer.exAccessorySlot[k];
					vItem = dPlayer.exAccessorySlot[k + ModPlayer.moddedAccSlots.Count];
					player.VanillaUpdateEquip(item);
					player.ApplyEquipFunctional(item, dPlayer.exHideAccessory[k]);
					if (SoundLoader.itemToMusic.ContainsKey(item.type))
						Main.musicBox2 = SoundLoader.itemToMusic[item.type];
					VanillaVanityEquipMirror(item, vItem, player, k, dPlayer);
				}

			}
		}

		public static void VanillaVanityEquipMirror(Item item, Item vItem, Player player, int k, ModPlayer dPlayer) {
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

		public static bool VanillaPreferredGolfBall(ref int projType, Player player) {
			ModPlayer dPlayer = Main.LocalPlayer.GetModPlayer<DefaultPlayer>();
			for (int num = ModPlayer.moddedAccSlots.Count * 2 - 1; num >= 0; num--) {
				if (ModdedIsAValidEquipmentSlotForIteration(num)) {
					_ = num % 10;
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
			ModPlayer dPlayer = Main.LocalPlayer.GetModPlayer<DefaultPlayer>();
			Item item2 = item;
			int dyeSlotCount = ModPlayer.moddedAccSlots.Count;

			for (int i = 0; i < ModPlayer.moddedAccSlots.Count; i++) {
				if (dPlayer.exDyesAccessory[i].type == 0) {
					dyeSlotCount = i;
					break;
				}
			}

			if (dyeSlotCount >= ModPlayer.moddedAccSlots.Count) {
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
			ModPlayer dPlayer = Main.LocalPlayer.GetModPlayer<DefaultPlayer>();
			int num2 = 0;
			int accSlotToSwapTo = ModPlayer.moddedAccSlots.Count;
			success = false;

			for (int i = 0; i < ModPlayer.moddedAccSlots.Count; i++) {
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

				if (j < ModPlayer.moddedAccSlots.Count && item.wingSlot > 0 && dPlayer.exAccessorySlot[j].wingSlot > 0)
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
			ModPlayer dPlayer = newPlayer.GetModPlayer<DefaultPlayer>();
			for (int i = 0; i < ModPlayer.moddedAccSlots.Count; i++) {
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
			ModPlayer dPlayer = Main.LocalPlayer.GetModPlayer<DefaultPlayer>();
			int index = slot % ModPlayer.moddedAccSlots.Count;
			ModAccessorySlot mAccSlot = GetModAccessorySlot(index);
			return mAccSlot.LimitWhatCanGoInSlot(checkItem) && !ItemSlot.AccCheck(dPlayer.exAccessorySlot, checkItem, slot);
		}

		public static void DefaultDrawModSlots(SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor = default(Color)) {
			Item item = inv[slot];
			float inventoryScale = Main.inventoryScale;
			Color color = Color.White;
			if (lightColor != Color.Transparent)
				color = lightColor;

			int num = -1;
			bool flag = false;
			int num2 = 0;
			if (PlayerInput.UsingGamepadUI) {
				switch (context) {
					case -10:
					case -11:
						int num3 = slot;
						if (!ModdedIsAValidEquipmentSlotForIteration(slot))
							num3--;

						num = 100 + num3;
						break;
					case -12:
						int num4 = slot;
						if (!ModdedIsAValidEquipmentSlotForIteration(slot))
							num4--;

						num = 120 + num4;
						break;
				}

				flag = (UILinkPointNavigator.CurrentPoint == num);
			}

			Texture2D value = TextureAssets.InventoryBack.Value;
			Color color2 = Main.inventoryBack;
			bool flag2 = false;

			if (item.type > 0 && item.stack > 0 && item.favorited) {
				value = TextureAssets.InventoryBack10.Value;
			}

			else if (item.type > 0 && item.stack > 0 && ItemSlot.Options.HighlightNewItems && item.newAndShiny) {
				value = TextureAssets.InventoryBack15.Value;
				float num5 = (float)(int)Main.mouseTextColor / 255f;
				num5 = num5 * 0.2f + 0.8f;
				color2 = color2.MultiplyRGBA(new Color(num5, num5, num5));
			}

			else if (PlayerInput.UsingGamepadUI && item.type > 0 && item.stack > 0 && num2 != 0) {
				value = TextureAssets.InventoryBack15.Value;
				float num6 = (float)(int)Main.mouseTextColor / 255f;
				num6 = num6 * 0.2f + 0.8f;
				color2 = ((num2 != 1) ? color2.MultiplyRGBA(new Color(num6 / 2f, num6, num6 / 2f)) : color2.MultiplyRGBA(new Color(num6, num6 / 2f, num6 / 2f)));
			}

			else {
				switch (context) {
					case -10:
						value = TextureAssets.InventoryBack3.Value;
						break;
					case -11:
						value = TextureAssets.InventoryBack8.Value;
						break;
					case -12:
						value = TextureAssets.InventoryBack12.Value;
						break;
				}
			}

			if (flag) {
				value = TextureAssets.InventoryBack14.Value;
				color2 = Color.White;
			}

			if (!flag2)
				spriteBatch.Draw(value, position, null, color2, 0f, default(Vector2), inventoryScale, SpriteEffects.None, 0f);

			int num10 = -1;
			switch (context) {
				case -10:
					num10 = 11;
					break;
				case -11:
					num10 = 2;
					break;
				case -12:
					num10 = 1;
					break;
			}

			if ((item.type <= 0 || item.stack <= 0) && num10 != -1) {
				Texture2D value6 = TextureAssets.Extra[54].Value;
				Rectangle rectangle = value6.Frame(3, 6, num10 % 3, num10 / 3);
				rectangle.Width -= 2;
				rectangle.Height -= 2;
				spriteBatch.Draw(value6, position + value.Size() / 2f * inventoryScale, rectangle, Color.White * 0.35f, 0f, rectangle.Size() / 2f, inventoryScale, SpriteEffects.None, 0f);
			}

			Vector2 vector = value.Size() * inventoryScale;
			if (item.type > 0 && item.stack > 0) {
				Main.instance.LoadItem(item.type);
				Texture2D value7 = TextureAssets.Item[item.type].Value;
				Rectangle rectangle2 = (Main.itemAnimations[item.type] == null) ? value7.Frame() : Main.itemAnimations[item.type].GetFrame(value7);
				Color currentColor = color;
				float scale3 = 1f;
				ItemSlot.GetItemLight(ref currentColor, ref scale3, item);
				float num11 = 1f;
				if (rectangle2.Width > 32 || rectangle2.Height > 32)
					num11 = ((rectangle2.Width <= rectangle2.Height) ? (32f / (float)rectangle2.Height) : (32f / (float)rectangle2.Width));

				num11 *= inventoryScale;
				Vector2 position2 = position + vector / 2f - rectangle2.Size() * num11 / 2f;
				Vector2 origin = rectangle2.Size() * (scale3 / 2f - 0.5f);
				if (!ItemLoader.PreDrawInInventory(item, spriteBatch, position2, rectangle2, item.GetAlpha(currentColor), item.GetColor(color), origin, num11 * scale3))
					goto skip;

				spriteBatch.Draw(value7, position2, rectangle2, item.GetAlpha(currentColor), 0f, origin, num11 * scale3, SpriteEffects.None, 0f);
				if (item.color != Color.Transparent)
					spriteBatch.Draw(value7, position2, rectangle2, item.GetColor(color), 0f, origin, num11 * scale3, SpriteEffects.None, 0f);

				skip:
				ItemLoader.PostDrawInInventory(item, spriteBatch, position2, rectangle2, item.GetAlpha(currentColor), item.GetColor(color), origin, num11 * scale3);
				if (ItemID.Sets.TrapSigned[item.type])
					spriteBatch.Draw(TextureAssets.Wire.Value, position + new Vector2(40f, 40f) * inventoryScale, new Rectangle(4, 58, 8, 8), color, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);

				if (item.stack > 1)
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, item.stack.ToString(), position + new Vector2(10f, 26f) * inventoryScale, color, 0f, Vector2.Zero, new Vector2(inventoryScale), -1f, inventoryScale);

				//TODO: Something needs to be done with this code block from a modded slot perspective, I think. Not sure what, so leave as does nothing
				if ((context == 10) && ((item.expertOnly && !Main.expertMode) || (item.masterOnly && !Main.masterMode))) {
					Vector2 position4 = position + value.Size() * inventoryScale / 2f - TextureAssets.Cd.Value.Size() * inventoryScale / 2f;
					Color white = Color.White;
					spriteBatch.Draw(TextureAssets.Cd.Value, position4, null, white, 0f, default(Vector2), num11, SpriteEffects.None, 0f);
				}
			}

			if (num != -1)
				UILinkPointNavigator.SetPosition(num, position + vector * 0.75f);
		}
	}
}