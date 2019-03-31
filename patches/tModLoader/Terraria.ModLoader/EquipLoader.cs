using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This serves as a central place to store equipment slots and their corresponding textures. You will use this to obtain the IDs for your equipment textures.
	/// </summary>
	public static class EquipLoader
	{
		//in Terraria.Main.DrawPlayer and Terraria.Main.DrawPlayerHead get rid of checks for slot too high (not necessary for loading)
		private static readonly IDictionary<EquipType, int> nextEquip = new Dictionary<EquipType, int>();

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

		internal static int ReserveEquipID(EquipType type) {
			int reserveID = nextEquip[type];
			nextEquip[type]++;
			return reserveID;
		}

		/// <summary>
		/// Gets the equipment texture for the specified equipment type and ID.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="slot"></param>
		/// <returns></returns>
		public static EquipTexture GetEquipTexture(EquipType type, int slot) {
			EquipTexture texture;
			return equipTextures[type].TryGetValue(slot, out texture) ? texture : null;
		}

		internal static void ResizeAndFillArrays() {
			Array.Resize(ref Main.armorHeadLoaded, nextEquip[EquipType.Head]);
			Array.Resize(ref Main.armorBodyLoaded, nextEquip[EquipType.Body]);
			Array.Resize(ref Main.armorLegsLoaded, nextEquip[EquipType.Legs]);
			Array.Resize(ref Main.accHandsOnLoaded, nextEquip[EquipType.HandsOn]);
			Array.Resize(ref Main.accHandsOffLoaded, nextEquip[EquipType.HandsOff]);
			Array.Resize(ref Main.accBackLoaded, nextEquip[EquipType.Back]);
			Array.Resize(ref Main.accFrontLoaded, nextEquip[EquipType.Front]);
			Array.Resize(ref Main.accShoesLoaded, nextEquip[EquipType.Shoes]);
			Array.Resize(ref Main.accWaistLoaded, nextEquip[EquipType.Waist]);
			Array.Resize(ref Main.wingsLoaded, nextEquip[EquipType.Wings]);
			Array.Resize(ref Main.accShieldLoaded, nextEquip[EquipType.Shield]);
			Array.Resize(ref Main.accNeckLoaded, nextEquip[EquipType.Neck]);
			Array.Resize(ref Main.accFaceLoaded, nextEquip[EquipType.Face]);
			Array.Resize(ref Main.accballoonLoaded, nextEquip[EquipType.Balloon]);
			foreach (EquipType type in EquipTypes) {
				for (int k = GetNumVanilla(type); k < nextEquip[type]; k++) {
					GetLoadedArray(type)[k] = true;
				}
			}
			Array.Resize(ref Main.armorHeadTexture, nextEquip[EquipType.Head]);
			Array.Resize(ref Main.armorBodyTexture, nextEquip[EquipType.Body]);
			Array.Resize(ref Main.femaleBodyTexture, nextEquip[EquipType.Body]);
			Array.Resize(ref Main.armorArmTexture, nextEquip[EquipType.Body]);
			Array.Resize(ref Main.armorLegTexture, nextEquip[EquipType.Legs]);
			Array.Resize(ref Main.accHandsOnTexture, nextEquip[EquipType.HandsOn]);
			Array.Resize(ref Main.accHandsOffTexture, nextEquip[EquipType.HandsOff]);
			Array.Resize(ref Main.accBackTexture, nextEquip[EquipType.Back]);
			Array.Resize(ref Main.accFrontTexture, nextEquip[EquipType.Front]);
			Array.Resize(ref Main.accShoesTexture, nextEquip[EquipType.Shoes]);
			Array.Resize(ref Main.accWaistTexture, nextEquip[EquipType.Waist]);
			Array.Resize(ref Main.wingsTexture, nextEquip[EquipType.Wings]);
			Array.Resize(ref Main.accShieldTexture, nextEquip[EquipType.Shield]);
			Array.Resize(ref Main.accNeckTexture, nextEquip[EquipType.Neck]);
			Array.Resize(ref Main.accFaceTexture, nextEquip[EquipType.Face]);
			Array.Resize(ref Main.accBalloonTexture, nextEquip[EquipType.Balloon]);
			foreach (EquipType type in EquipTypes) {
				foreach (var entry in equipTextures[type]) {
					int slot = entry.Key;
					EquipTexture texture = entry.Value;
					GetTextureArray(type)[slot] = ModContent.GetTexture(texture.Texture);
					if (type == EquipType.Body) {
						Main.femaleBodyTexture[slot] = ModContent.GetTexture(femaleTextures[slot]);
						Main.armorArmTexture[slot] = ModContent.GetTexture(armTextures[slot]);
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

		internal static bool[] GetLoadedArray(EquipType type) {
			switch (type) {
				case EquipType.Head:
					return Main.armorHeadLoaded;
				case EquipType.Body:
					return Main.armorBodyLoaded;
				case EquipType.Legs:
					return Main.armorLegsLoaded;
				case EquipType.HandsOn:
					return Main.accHandsOnLoaded;
				case EquipType.HandsOff:
					return Main.accHandsOffLoaded;
				case EquipType.Back:
					return Main.accBackLoaded;
				case EquipType.Front:
					return Main.accFrontLoaded;
				case EquipType.Shoes:
					return Main.accShoesLoaded;
				case EquipType.Waist:
					return Main.accWaistLoaded;
				case EquipType.Wings:
					return Main.wingsLoaded;
				case EquipType.Shield:
					return Main.accShieldLoaded;
				case EquipType.Neck:
					return Main.accNeckLoaded;
				case EquipType.Face:
					return Main.accFaceLoaded;
				case EquipType.Balloon:
					return Main.accballoonLoaded;
			}
			return null;
		}

		internal static Texture2D[] GetTextureArray(EquipType type) {
			switch (type) {
				case EquipType.Head:
					return Main.armorHeadTexture;
				case EquipType.Body:
					return Main.armorBodyTexture;
				case EquipType.Legs:
					return Main.armorLegTexture;
				case EquipType.HandsOn:
					return Main.accHandsOnTexture;
				case EquipType.HandsOff:
					return Main.accHandsOffTexture;
				case EquipType.Back:
					return Main.accBackTexture;
				case EquipType.Front:
					return Main.accFrontTexture;
				case EquipType.Shoes:
					return Main.accShoesTexture;
				case EquipType.Waist:
					return Main.accWaistTexture;
				case EquipType.Wings:
					return Main.wingsTexture;
				case EquipType.Shield:
					return Main.accShieldTexture;
				case EquipType.Neck:
					return Main.accNeckTexture;
				case EquipType.Face:
					return Main.accFaceTexture;
				case EquipType.Balloon:
					return Main.accBalloonTexture;
			}
			return null;
		}

		internal static void SetSlot(Item item) {
			IDictionary<EquipType, int> slots;
			if (!idToSlot.TryGetValue(item.type, out slots))
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
	}
}
