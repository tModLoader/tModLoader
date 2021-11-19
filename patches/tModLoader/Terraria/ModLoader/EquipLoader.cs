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
		internal static readonly Dictionary<EquipType, int> nextEquip = new Dictionary<EquipType, int>();

		internal static readonly Dictionary<EquipType, Dictionary<int, EquipTexture>> equipTextures = new();

		//list of equiptypes and slots registered for an item id. Used for SetDefaults
		internal static readonly Dictionary<int, Dictionary<EquipType, int>> idToSlot = new();

		//holds mappings of slot id -> item id for head/body/legs
		//used to populate Item.(head/body/leg)Type for Manequinns
		internal static readonly Dictionary<EquipType, Dictionary<int, int>> slotToId = new();

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

		internal static int ReserveEquipID(EquipType type)
			=> nextEquip[type]++;

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
			Array.Resize(ref TextureAssets.ArmorBodyComposite, nextEquip[EquipType.Body]);
			Array.Resize(ref TextureAssets.FemaleBody, nextEquip[EquipType.Body]);
			Array.Resize(ref TextureAssets.ArmorArm, nextEquip[EquipType.Body]);
			Array.Resize(ref TextureAssets.ArmorLeg, nextEquip[EquipType.Legs]);
			Array.Resize(ref TextureAssets.AccHandsOn, nextEquip[EquipType.HandsOn]);
			Array.Resize(ref TextureAssets.AccHandsOnComposite, nextEquip[EquipType.HandsOn]);
			Array.Resize(ref TextureAssets.AccHandsOff, nextEquip[EquipType.HandsOff]);
			Array.Resize(ref TextureAssets.AccHandsOffComposite, nextEquip[EquipType.HandsOff]);
			Array.Resize(ref TextureAssets.AccBack, nextEquip[EquipType.Back]);
			Array.Resize(ref TextureAssets.AccFront, nextEquip[EquipType.Front]);
			Array.Resize(ref TextureAssets.AccShoes, nextEquip[EquipType.Shoes]);
			Array.Resize(ref TextureAssets.AccWaist, nextEquip[EquipType.Waist]);
			Array.Resize(ref TextureAssets.Wings, nextEquip[EquipType.Wings]);
			Array.Resize(ref TextureAssets.AccShield, nextEquip[EquipType.Shield]);
			Array.Resize(ref TextureAssets.AccNeck, nextEquip[EquipType.Neck]);
			Array.Resize(ref TextureAssets.AccFace, nextEquip[EquipType.Face]);
			Array.Resize(ref TextureAssets.AccBeard, nextEquip[EquipType.Beard]);
			Array.Resize(ref TextureAssets.AccBalloon, nextEquip[EquipType.Balloon]);

			//Sets
			LoaderUtils.ResetStaticMembers(typeof(ArmorIDs), true);
			WingStatsInitializer.Load();

			foreach (EquipType type in EquipTypes) {
				foreach (var entry in equipTextures[type]) {
					int slot = entry.Key;
					EquipTexture equipTexture = entry.Value;
					string texture = equipTexture.Texture;
					
					GetTextureArray(type)[slot] = ModContent.Request<Texture2D>(texture);
					
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

			static void ResizeAndRegisterType(EquipType equipType, ref int[] typeArray) {
				Array.Resize(ref typeArray, nextEquip[equipType]);

				foreach (var entry in slotToId[equipType]) {
					typeArray[entry.Key] = entry.Value;
				}
			}

			ResizeAndRegisterType(EquipType.Head, ref Item.headType);
			ResizeAndRegisterType(EquipType.Body, ref Item.bodyType);
			ResizeAndRegisterType(EquipType.Legs, ref Item.legType);
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
		}

		internal static int GetNumVanilla(EquipType type)
			=> type switch {
				EquipType.Head => Main.numArmorHead,
				EquipType.Body => Main.numArmorBody,
				EquipType.Legs => Main.numArmorLegs,
				EquipType.HandsOn => Main.numAccHandsOn,
				EquipType.HandsOff => Main.numAccHandsOff,
				EquipType.Back => Main.numAccBack,
				EquipType.Front => Main.numAccFront,
				EquipType.Shoes => Main.numAccShoes,
				EquipType.Waist => Main.numAccWaist,
				EquipType.Wings => Main.maxWings,
				EquipType.Shield => Main.numAccShield,
				EquipType.Neck => Main.numAccNeck,
				EquipType.Face => Main.numAccFace,
				EquipType.Beard => Main.numAccBeard,
				EquipType.Balloon => Main.numAccBalloon,
				_ => 0,
			};

		internal static Asset<Texture2D>[] GetTextureArray(EquipType type)
			=> type switch {
				EquipType.Head => TextureAssets.ArmorHead,
				EquipType.Body => TextureAssets.ArmorBodyComposite,
				EquipType.Legs => TextureAssets.ArmorLeg,
				EquipType.HandsOn => TextureAssets.AccHandsOnComposite,
				EquipType.HandsOff => TextureAssets.AccHandsOffComposite,
				EquipType.Back => TextureAssets.AccBack,
				EquipType.Front => TextureAssets.AccFront,
				EquipType.Shoes => TextureAssets.AccShoes,
				EquipType.Waist => TextureAssets.AccWaist,
				EquipType.Wings => TextureAssets.Wings,
				EquipType.Shield => TextureAssets.AccShield,
				EquipType.Neck => TextureAssets.AccNeck,
				EquipType.Face => TextureAssets.AccFace,
				EquipType.Beard => TextureAssets.AccBeard,
				EquipType.Balloon => TextureAssets.AccBalloon,
				_ => null,
			};

		internal static void SetSlot(Item item) {
			if (!idToSlot.TryGetValue(item.type, out var slots))
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
					case EquipType.Beard:
						item.beardSlot = (sbyte)slot;
						break;
					case EquipType.Balloon:
						item.balloonSlot = (sbyte)slot;
						break;
				}
			}
		}

		internal static int GetPlayerEquip(Player player, EquipType type)
			=> type switch {
				EquipType.Head => player.head,
				EquipType.Body => player.body,
				EquipType.Legs => player.legs,
				EquipType.HandsOn => player.handon,
				EquipType.HandsOff => player.handoff,
				EquipType.Back => player.back,
				EquipType.Front => player.front,
				EquipType.Shoes => player.shoe,
				EquipType.Waist => player.waist,
				EquipType.Wings => player.wings,
				EquipType.Shield => player.shield,
				EquipType.Neck => player.neck,
				EquipType.Face => player.face,
				EquipType.Beard => player.beard,
				EquipType.Balloon => player.balloon,
				_ => 0,
			};

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