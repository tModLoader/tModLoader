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

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This serves as a central place to store equipment slots and their corresponding textures. You will use this to obtain the IDs for your equipment textures.
	/// </summary>
	public static class EquipLoader
	{
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

		// List of identifiers for the modded accessory slots.
		internal static List<string> moddedAccSlots = new List<string>();
		// Arrays for modded accessory slot save/load/stuff
		internal static Item[] exAccessorySlot = new Item[2];
		internal static Item[] exDyesAccessory = new Item[1];
		internal static bool[] exHideAccessory = new bool[1];

		internal static void ResizeAccesoryArrays(int newSize) {
			Array.Resize<Item>(ref exAccessorySlot, 2 * newSize);
			Array.Resize<Item>(ref exDyesAccessory, newSize);
			Array.Resize<bool>(ref exHideAccessory, newSize);
		}

		public static void DrawModAccSlots(int num20) {
			for (int modSlot = 0; modSlot < moddedAccSlots.Count; modSlot++) {
				ModContent.TryFind<ModAccessorySlot>(moddedAccSlots[modSlot], out ModAccessorySlot mAccSlot);
				mAccSlot.Draw(num20);
			}
		}

		internal static void VanillaUpdateEquipsMirror(Player player) {
			Item item = null;
			Item vItem = null;
			for (int k = 0; k < moddedAccSlots.Count; k++) {
				if (play.IsAValidEquipmentSlotForIteration(k)) {
					item = exAccessorySlot[k];
					vItem = exAccessorySlot[k + moddedAccSlots.Count];
					player.VanillaUpdateEquip(item);
					player.ApplyEquipFunctional(item, exHideAccessory[k]);
					if (SoundLoader.itemToMusic.ContainsKey(item.type))
						Main.musicBox2 = SoundLoader.itemToMusic[item.type];
					VanillaVanityEquipMirror(item, vItem, player, k);
				}
				
			}
		}

		private static void VanillaVanityEquipMirror(Item item, Item vItem, Player player, int k) {
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
				if (exHideAccessory[k] && (player.velocity.Y == 0f || player.mount.Active))
					return;

				player.wings = item.wingSlot;
			}

			if (!exHideAccessory[k])
				player.UpdateVisibleAccessory(k, item);

			if (!player.ItemIsVisuallyIncompatible(vItem))
				player.UpdateVisibleAccessory(k + moddedAccSlots.Count, vItem);
		}

		public static bool VanillaPreferredGolfBall(ref int projType, Player player) {
			for (int num = moddedAccSlots.Count * 2 - 1; num >= 0; num--) {
				if (play.IsAValidEquipmentSlotForIteration(num)) {
					_ = num % 10;
					Item item2 = exAccessorySlot[num];
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
				if (exDyesAccessory[i].type == 0) {
					dyeSlotCount = i;
					break;
				}
			}

			if (dyeSlotCount >= moddedAccSlots.Count ) {
				success = false;
				return item2;
			}

			item2 = exDyesAccessory[dyeSlotCount].Clone();
			exDyesAccessory[dyeSlotCount] = item.Clone();
				
			SoundEngine.PlaySound(7);
			Recipe.FindRecipes();
			success = true;
			return item2;
		}

		public static Item VanillaArmorSwapMirror(Item item, out bool success) {
			int num2 = 0;
			int accSlotToSwapTo = moddedAccSlots.Count;
			success = false;

			for (int i = 0; i < moddedAccSlots.Count; i++) {
				if (play.IsAValidEquipmentSlotForIteration(i)) {
					num2 = i;
					if (exAccessorySlot[i].type == 0) {
						accSlotToSwapTo = i;
						break;
					}
				}
			}

			for (int j = 0; j < exAccessorySlot.Length; j++) {
				if (item.IsTheSameAs(exAccessorySlot[j]))
					accSlotToSwapTo = j;

				if (j < moddedAccSlots.Count && item.wingSlot > 0 && exAccessorySlot[j].wingSlot > 0)
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

			if (ItemSlot.isEquipLocked(exAccessorySlot[accSlotToSwapTo].type))
				return item;

			for (int k = 0; k < exAccessorySlot.Length; k++) {
				if (item.IsTheSameAs(exAccessorySlot[k]))
					accSlotToSwapTo = k;
			}

			if (!ItemLoader.CanEquipAccessory(item, accSlotToSwapTo))
				return item;

			Item result = exAccessorySlot[accSlotToSwapTo].Clone();
			exAccessorySlot[accSlotToSwapTo] = item.Clone();

			SoundEngine.PlaySound(7);
			Recipe.FindRecipes();
			success = true;
			return result;
		}

		public static void VanillaLastMinuteFixesMirror(Player newPlayer) {
			for (int i = 0; i < moddedAccSlots.Count; i++) {
				int type = exAccessorySlot[i].type;
				if (type == 908 || type == 4874 || type == 5000)
					newPlayer.lavaMax += 420;

				if (type == 906 || type == 4038)
					newPlayer.lavaMax += 420;

				if (newPlayer.wingsLogic == 0 && exAccessorySlot[i].wingSlot >= 0)
					newPlayer.wingsLogic = exAccessorySlot[i].wingSlot;

				if (type == 158 || type == 396 || type == 1250 || type == 1251 || type == 1252)
					newPlayer.noFallDmg = true;

				newPlayer.lavaTime = newPlayer.lavaMax;
			}
		}

		private static ItemLoader.HookList HookUpdateArmorSet = ItemLoader.AddHook<Action<Player, string>>(g => g.UpdateArmorSet);
		//at end of Terraria.Player.UpdateArmorSets call ItemLoader.UpdateArmorSet(this, this.armor[0], this.armor[1], this.armor[2])
		/// <summary>
		/// If the head's ModItem.IsArmorSet returns true, calls the head's ModItem.UpdateArmorSet. This is then repeated for the body, then the legs. Then for each GlobalItem, if GlobalItem.IsArmorSet returns a non-empty string, calls GlobalItem.UpdateArmorSet with that string.
		/// </summary>
		public static void UpdateArmorSet(Player player, Item head, Item body, Item legs) {
			if (head.ModItem != null && head.ModItem.IsArmorSet(head, body, legs))
				head.ModItem.UpdateArmorSet(player);

			if (body.ModItem != null && body.ModItem.IsArmorSet(head, body, legs))
				body.ModItem.UpdateArmorSet(player);

			if (legs.ModItem != null && legs.ModItem.IsArmorSet(head, body, legs))
				legs.ModItem.UpdateArmorSet(player);

			foreach (GlobalItem globalItem in HookUpdateArmorSet.arr) {
				string set = globalItem.IsArmorSet(head, body, legs);
				if (!string.IsNullOrEmpty(set))
					globalItem.UpdateArmorSet(player, set);
			}
		}

		private static ItemLoader.HookList HookUpdateEquip = ItemLoader.AddHook<Action<Item, Player>>(g => g.UpdateEquip);
		/// <summary>
		/// Hook at the end of Player.VanillaUpdateEquip can be called from modded slots for modded equipments
		/// </summary>
		public static void UpdateEquip(Item item, Player player) {
			if (item.IsAir)
				return;

			item.ModItem?.UpdateEquip(player);

			foreach (var g in HookUpdateEquip.arr)
				g.Instance(item).UpdateEquip(item, player);
		}

		private static ItemLoader.HookList HookUpdateAccessory = ItemLoader.AddHook<Action<Item, Player, bool>>(g => g.UpdateAccessory);
		/// <summary>
		/// Hook at the end of Player.ApplyEquipFunctional can be called from modded slots for modded equipments
		/// </summary>
		public static void UpdateAccessory(Item item, Player player, bool hideVisual) {
			if (item.IsAir)
				return;

			item.ModItem?.UpdateAccessory(player, hideVisual);

			foreach (var g in HookUpdateAccessory.arr)
				g.Instance(item).UpdateAccessory(item, player, hideVisual);
		}

		private static ItemLoader.HookList HookUpdateVanity = ItemLoader.AddHook<Action<Item, Player>>(g => g.UpdateVanity);
		/// <summary>
		/// Hook at the end of Player.ApplyEquipVanity can be called from modded slots for modded equipments
		/// </summary>
		public static void UpdateVanity(Item item, Player player) {
			if (item.IsAir)
				return;

			item.ModItem?.UpdateVanity(player);

			foreach (var g in HookUpdateVanity.arr)
				g.Instance(item).UpdateVanity(item, player);
		}

		private static ItemLoader.HookList HookPreUpdateVanitySet = ItemLoader.AddHook<Action<Player, string>>(g => g.PreUpdateVanitySet);
		//in Terraria.Player.PlayerFrame after setting armor effects fields call this
		/// <summary>
		/// If the player's head texture's IsVanitySet returns true, calls the equipment texture's PreUpdateVanitySet. This is then repeated for the player's body, then the legs. Then for each GlobalItem, if GlobalItem.IsVanitySet returns a non-empty string, calls GlobalItem.PreUpdateVanitySet, using player.head, player.body, and player.legs.
		/// </summary>
		public static void PreUpdateVanitySet(Player player) {
			EquipTexture headTexture = EquipLoader.GetEquipTexture(EquipType.Head, player.head);
			EquipTexture bodyTexture = EquipLoader.GetEquipTexture(EquipType.Body, player.body);
			EquipTexture legTexture = EquipLoader.GetEquipTexture(EquipType.Legs, player.legs);

			if (headTexture != null && headTexture.IsVanitySet(player.head, player.body, player.legs))
				headTexture.PreUpdateVanitySet(player);

			if (bodyTexture != null && bodyTexture.IsVanitySet(player.head, player.body, player.legs))
				bodyTexture.PreUpdateVanitySet(player);

			if (legTexture != null && legTexture.IsVanitySet(player.head, player.body, player.legs))
				legTexture.PreUpdateVanitySet(player);

			foreach (GlobalItem globalItem in HookPreUpdateVanitySet.arr) {
				string set = globalItem.IsVanitySet(player.head, player.body, player.legs);
				if (!string.IsNullOrEmpty(set))
					globalItem.PreUpdateVanitySet(player, set);
			}
		}

		private static ItemLoader.HookList HookUpdateVanitySet = ItemLoader.AddHook<Action<Player, string>>(g => g.UpdateVanitySet);
		//in Terraria.Player.PlayerFrame after armor sets creating dust call this
		/// <summary>
		/// If the player's head texture's IsVanitySet returns true, calls the equipment texture's UpdateVanitySet. This is then repeated for the player's body, then the legs. Then for each GlobalItem, if GlobalItem.IsVanitySet returns a non-empty string, calls GlobalItem.UpdateVanitySet, using player.head, player.body, and player.legs.
		/// </summary>
		public static void UpdateVanitySet(Player player) {
			EquipTexture headTexture = EquipLoader.GetEquipTexture(EquipType.Head, player.head);
			EquipTexture bodyTexture = EquipLoader.GetEquipTexture(EquipType.Body, player.body);
			EquipTexture legTexture = EquipLoader.GetEquipTexture(EquipType.Legs, player.legs);

			if (headTexture != null && headTexture.IsVanitySet(player.head, player.body, player.legs))
				headTexture.UpdateVanitySet(player);

			if (bodyTexture != null && bodyTexture.IsVanitySet(player.head, player.body, player.legs))
				bodyTexture.UpdateVanitySet(player);

			if (legTexture != null && legTexture.IsVanitySet(player.head, player.body, player.legs))
				legTexture.UpdateVanitySet(player);

			foreach (GlobalItem globalItem in HookUpdateVanitySet.arr) {
				string set = globalItem.IsVanitySet(player.head, player.body, player.legs);
				if (!string.IsNullOrEmpty(set))
					globalItem.UpdateVanitySet(player, set);
			}
		}

		private static ItemLoader.HookList HookArmorSetShadows = ItemLoader.AddHook<Action<Player, string>>(g => g.ArmorSetShadows);
		//in Terraria.Main.DrawPlayers after armor combinations setting flags call
		//  ItemLoader.ArmorSetShadows(player);
		/// <summary>
		/// If the player's head texture's IsVanitySet returns true, calls the equipment texture's ArmorSetShadows. This is then repeated for the player's body, then the legs. Then for each GlobalItem, if GlobalItem.IsVanitySet returns a non-empty string, calls GlobalItem.ArmorSetShadows, using player.head, player.body, and player.legs.
		/// </summary>
		public static void ArmorSetShadows(Player player) {
			EquipTexture headTexture = EquipLoader.GetEquipTexture(EquipType.Head, player.head);
			EquipTexture bodyTexture = EquipLoader.GetEquipTexture(EquipType.Body, player.body);
			EquipTexture legTexture = EquipLoader.GetEquipTexture(EquipType.Legs, player.legs);

			if (headTexture != null && headTexture.IsVanitySet(player.head, player.body, player.legs))
				headTexture.ArmorSetShadows(player);

			if (bodyTexture != null && bodyTexture.IsVanitySet(player.head, player.body, player.legs))
				bodyTexture.ArmorSetShadows(player);

			if (legTexture != null && legTexture.IsVanitySet(player.head, player.body, player.legs))
				legTexture.ArmorSetShadows(player);

			foreach (GlobalItem globalItem in HookArmorSetShadows.arr) {
				string set = globalItem.IsVanitySet(player.head, player.body, player.legs);
				if (!string.IsNullOrEmpty(set))
					globalItem.ArmorSetShadows(player, set);
			}
		}

		private delegate void DelegateSetMatch(int armorSlot, int type, bool male, ref int equipSlot, ref bool robes);
		private static ItemLoader.HookList HookSetMatch = ItemLoader.AddHook<DelegateSetMatch>(g => g.SetMatch);
		/// <summary>
		/// Calls EquipTexture.SetMatch, then all GlobalItem.SetMatch hooks.
		/// </summary>   
		public static void SetMatch(int armorSlot, int type, bool male, ref int equipSlot, ref bool robes) {
			EquipTexture texture = EquipLoader.GetEquipTexture((EquipType)armorSlot, type);
			texture?.SetMatch(male, ref equipSlot, ref robes);

			foreach (var g in HookSetMatch.arr)
				g.SetMatch(armorSlot, type, male, ref equipSlot, ref robes);
		}

		private static ItemLoader.HookList HookCanRightClick = ItemLoader.AddHook<Func<Item, bool>>(g => g.CanRightClick);
		//in Terraria.UI.ItemSlot.RightClick in end of item-opening if/else chain before final else
		//  make else if(ItemLoader.CanRightClick(inv[slot]))
		/// <summary>
		/// Calls ModItem.CanRightClick, then all GlobalItem.CanRightClick hooks, until one of the returns true. If one of the returns true, returns Main.mouseRight. Otherwise, returns false.
		/// </summary>
		public static bool CanRightClick(Item item) {
			if (item.IsAir || !Main.mouseRight)
				return false;

			if (item.ModItem != null && item.ModItem.CanRightClick())
				return true;

			foreach (var g in HookCanRightClick.arr)
				if (g.Instance(item).CanRightClick(item))
					return true;

			return false;
		}

		private static ItemLoader.HookList HookRightClick = ItemLoader.AddHook<Action<Item, Player>>(g => g.RightClick);
		//in Terraria.UI.ItemSlot in block from CanRightClick call ItemLoader.RightClick(inv[slot], player)
		/// <summary>
		/// If Main.mouseRightRelease is true, the following steps are taken:
		/// 1. Call ModItem.RightClick
		/// 2. Calls all GlobalItem.RightClick hooks
		/// 3. Call ItemLoader.ConsumeItem, and if it returns true, decrements the item's stack
		/// 4. Sets the item's type to 0 if the item's stack is 0
		/// 5. Plays the item-grabbing sound
		/// 6. Sets Main.stackSplit to 30
		/// 7. Sets Main.mouseRightRelease to false
		/// 8. Calls Recipe.FindRecipes.
		/// </summary>
		public static void RightClick(Item item, Player player) {
			if (!Main.mouseRightRelease)
				return;

			item.ModItem?.RightClick(player);

			foreach (var g in HookRightClick.arr)
				g.Instance(item).RightClick(item, player);

			if (ItemLoader.ConsumeItem(item, player) && --item.stack == 0)
				item.SetDefaults();

			SoundEngine.PlaySound(7);
			Main.stackSplit = 30;
			Main.mouseRightRelease = false;
			Recipe.FindRecipes();
		}

		/// <summary>s
		/// Returns the wing item that the player is functionally using. If player.wingsLogic has been modified, so no equipped wing can be found to match what the player is using, this creates a new Item object to return.
		/// </summary>
		public static Item GetWing(Player player) {
			//TODO: Try and rework away to get value from Player? instead.

			// If wings are present in accessory slots (slots 3 through 10, where 0,1,2 are armor), then return wings
			Item item = null;

			for (int k = 3; k < 10; k++) {
				if (player.armor[k].wingSlot == player.wingsLogic) {
					item = player.armor[k];
				}
			}

			// If wings are present in modded accessory slots (slots 0 through N), then return wings
			for (int k = 0; k < EquipLoader.moddedAccSlots.Count; k++) {
				if (EquipLoader.exAccessorySlot[k].wingSlot == player.wingsLogic) {
					item = player.armor[k];
				}
			}

			if (item != null) {
				return item;
			}
			return null;
		}

		private delegate void DelegateVerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend);
		private static ItemLoader.HookList HookVerticalWingSpeeds = ItemLoader.AddHook<DelegateVerticalWingSpeeds>(g => g.VerticalWingSpeeds);
		//in Terraria.Player.WingMovement after if statements that set num1-5
		//  call ItemLoader.VerticalWingSpeeds(this, ref num2, ref num5, ref num4, ref num3, ref num)
		/// <summary>
		/// If the player is using wings, this uses the result of GetWing, and calls ModItem.VerticalWingSpeeds then all GlobalItem.VerticalWingSpeeds hooks.
		/// </summary>
		public static void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
			ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
			Item item = GetWing(player);
			if (item == null) {
				EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Wings, player.wingsLogic);
				texture?.VerticalWingSpeeds(
					player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier,
					ref maxAscentMultiplier, ref constantAscend);
				return;
			}

			item.ModItem?.VerticalWingSpeeds(player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier,
				ref maxAscentMultiplier, ref constantAscend);

			foreach (var g in HookVerticalWingSpeeds.arr)
				g.Instance(item).VerticalWingSpeeds(item, player, ref ascentWhenFalling, ref ascentWhenRising,
					ref maxCanAscendMultiplier, ref maxAscentMultiplier, ref constantAscend);
		}

		private delegate void DelegateHorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration);
		private static ItemLoader.HookList HookHorizontalWingSpeeds = ItemLoader.AddHook<DelegateHorizontalWingSpeeds>(g => g.HorizontalWingSpeeds);
		//in Terraria.Player.Update after wingsLogic if statements modifying accRunSpeed and runAcceleration
		//  call ItemLoader.HorizontalWingSpeeds(this)
		/// <summary>
		/// If the player is using wings, this uses the result of GetWing, and calls ModItem.HorizontalWingSpeeds then all GlobalItem.HorizontalWingSpeeds hooks.
		/// </summary>
		public static void HorizontalWingSpeeds(Player player) {
			Item item = GetWing(player);
			if (item == null) {
				EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Wings, player.wingsLogic);
				texture?.HorizontalWingSpeeds(player, ref player.accRunSpeed, ref player.runAcceleration);
				return;
			}

			item.ModItem?.HorizontalWingSpeeds(player, ref player.accRunSpeed, ref player.runAcceleration);

			foreach (var g in HookHorizontalWingSpeeds.arr)
				g.Instance(item).HorizontalWingSpeeds(item, player, ref player.accRunSpeed, ref player.runAcceleration);
		}

		private static ItemLoader.HookList HookWingUpdate = ItemLoader.AddHook<Func<int, Player, bool, bool>>(g => g.WingUpdate);
		/// <summary>
		/// If wings can be seen on the player, calls the player's wing's equipment texture's WingUpdate and all GlobalItem.WingUpdate hooks.
		/// </summary>
		public static bool WingUpdate(Player player, bool inUse) {
			if (player.wings <= 0)
				return false;

			EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Wings, player.wings);
			bool? retVal = texture?.WingUpdate(player, inUse);

			foreach (var g in HookWingUpdate.arr)
				retVal |= g.WingUpdate(player.wings, player, inUse);

			return retVal ?? false;
		}
	}
}
