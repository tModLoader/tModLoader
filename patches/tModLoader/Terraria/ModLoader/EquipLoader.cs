using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Initializers;
using Terraria.ModLoader.Core;

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

		public static readonly EquipType[] EquipTypes = (EquipType[])Enum.GetValues(typeof(EquipType));

		static EquipLoader() {
			foreach (EquipType type in EquipTypes) {
				nextEquip[type] = GetNumVanilla(type);
				equipTextures[type] = new Dictionary<int, EquipTexture>();
			}

			slotToId[EquipType.Head] = new Dictionary<int, int>();
			slotToId[EquipType.BodyLegacy] = new Dictionary<int, int>();
			slotToId[EquipType.Body] = new Dictionary<int, int>();
			slotToId[EquipType.Legs] = new Dictionary<int, int>();
		}

		internal static int ReserveEquipID(EquipType type)
		{
			if (type == EquipType.BodyLegacy || type == EquipType.Body)
			{
				nextEquip[EquipType.Body]++;
				return nextEquip[EquipType.BodyLegacy]++;
			}
			return nextEquip[type]++;
		}

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
			Array.Resize(ref TextureAssets.ArmorBody, nextEquip[EquipType.BodyLegacy]);
			Array.Resize(ref TextureAssets.ArmorBodyComposite, nextEquip[EquipType.Body]);
			Array.Resize(ref TextureAssets.FemaleBody, nextEquip[EquipType.BodyLegacy]);
			Array.Resize(ref TextureAssets.ArmorArm, nextEquip[EquipType.BodyLegacy]);
			Array.Resize(ref TextureAssets.ArmorLeg, nextEquip[EquipType.Legs]);
			Array.Resize(ref TextureAssets.AccHandsOn, nextEquip[EquipType.HandsOnLegacy]);
			Array.Resize(ref TextureAssets.AccHandsOnComposite, nextEquip[EquipType.HandsOn]);
			Array.Resize(ref TextureAssets.AccHandsOff, nextEquip[EquipType.HandsOffLegacy]);
			Array.Resize(ref TextureAssets.AccHandsOffComposite, nextEquip[EquipType.HandsOff]);
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
					EquipTexture equipTexture = entry.Value;
					string texture = equipTexture.Texture;
					
					GetTextureArray(type)[slot] = ModContent.GetTexture(texture);

					if (type == EquipType.BodyLegacy) {
						TextureAssets.FemaleBody[slot] = ModContent.TextureExists(texture + "_Female") ? ModContent.GetTexture(texture + "_Female") : ModContent.GetTexture(texture);
						TextureAssets.ArmorArm[slot] = ModContent.GetTexture(texture + "_Arms");
					}
					
					if (type == EquipType.Body) {
						ArmorIDs.Body.Sets.UsesNewFramingCode[slot] = true;
					}
					else if (type == EquipType.HandsOn) {
						ArmorIDs.HandOn.Sets.UsesNewFramingCode[slot] = true;
					}
					else if (type == EquipType.HandsOff) {
						ArmorIDs.HandOff.Sets.UsesNewFramingCode[slot] = true;
					}
				}
			}

			Array.Resize(ref Item.headType, nextEquip[EquipType.Head]);
			
			foreach (var entry in slotToId[EquipType.Head]) {
				Item.headType[entry.Key] = entry.Value;
			}

			Array.Resize(ref Item.bodyType, nextEquip[EquipType.BodyLegacy]);

			foreach (var entry in slotToId[EquipType.BodyLegacy]) {
				Item.bodyType[entry.Key] = entry.Value;
			}
			
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
			slotToId[EquipType.BodyLegacy].Clear();
			slotToId[EquipType.Body].Clear();
			slotToId[EquipType.Legs].Clear();
		}

		internal static int GetNumVanilla(EquipType type) {
			switch (type) {
				case EquipType.Head:
					return Main.numArmorHead;
				case EquipType.BodyLegacy:
				case EquipType.Body:
					return Main.numArmorBody;
				case EquipType.Legs:
					return Main.numArmorLegs;
				case EquipType.HandsOnLegacy:
				case EquipType.HandsOn:
					return Main.numAccHandsOn;
				case EquipType.HandsOffLegacy:
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
				case EquipType.BodyLegacy:
					return TextureAssets.ArmorBody;
				case EquipType.Body:
					return TextureAssets.ArmorBodyComposite;
				case EquipType.Legs:
					return TextureAssets.ArmorLeg;
				case EquipType.HandsOnLegacy:
				case EquipType.HandsOn:
					return TextureAssets.AccHandsOn;
				case EquipType.HandsOffLegacy:
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
					case EquipType.BodyLegacy:
					case EquipType.Body:
						item.bodySlot = slot;
						break;
					case EquipType.Legs:
						item.legSlot = slot;
						break;
					case EquipType.HandsOnLegacy:
					case EquipType.HandsOn:
						item.handOnSlot = (sbyte)slot;
						break;
					case EquipType.HandsOffLegacy:
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
				case EquipType.BodyLegacy:
				case EquipType.Body:
					return player.body;
				case EquipType.Legs:
					return player.legs;
				case EquipType.HandsOnLegacy:
				case EquipType.HandsOn:
					return player.handon;
				case EquipType.HandsOffLegacy:
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
	}
}
