﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace Terraria
{
	public partial class Player
	{
		internal IList<string> usedMods;
		internal ModPlayer[] modPlayers = Array.Empty<ModPlayer>();
		public Item equippedWings = null;

		public HashSet<int> NearbyModTorch { get; private set; } = new HashSet<int>();

		// Get

		/// <summary> Gets the instance of the specified ModPlayer type. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="IndexOutOfRangeException"/>
		public T GetModPlayer<T>() where T : ModPlayer
			=> GetModPlayer(ModContent.GetInstance<T>());

		/// <summary> Gets the local instance of the type of the specified ModPlayer instance. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="IndexOutOfRangeException"/>
		/// <exception cref="NullReferenceException"/>
		public T GetModPlayer<T>(T baseInstance) where T : ModPlayer
			=> modPlayers[baseInstance.index] as T ?? throw new KeyNotFoundException($"Instance of '{typeof(T).Name}' does not exist on the current player.");

		/*
		// TryGet

		/// <summary> Gets the instance of the specified ModPlayer type. </summary>
		public bool TryGetModPlayer<T>(out T result) where T : ModPlayer
			=> TryGetModPlayer(ModContent.GetInstance<T>(), out result);

		/// <summary> Safely attempts to get the local instance of the type of the specified ModPlayer instance. </summary>
		/// <returns> Whether or not the requested instance has been found. </returns>
		public bool TryGetModPlayer<T>(T baseInstance, out T result) where T : ModPlayer {
			if (baseInstance == null || baseInstance.index < 0 || baseInstance.index >= modPlayers.Length) {
				result = default;

				return false;
			}

			result = modPlayers[baseInstance.index] as T;

			return result != null;
		}
		*/

		// Damage Classes

		private DamageClassData[] damageData;

		internal void ResetDamageClassData() {
			damageData = new DamageClassData[DamageClassLoader.DamageClassCount];

			for (int i = 0; i < damageData.Length; i++) {
				damageData[i] = new DamageClassData(StatModifier.One, 0, StatModifier.One);
				DamageClassLoader.DamageClasses[i].SetDefaultStats(this);
			}
		}


		/// <summary>
		/// Gets the crit modifier for this damage type on this player.
		/// This returns a reference, and as such, you can freely modify this method's return value with operators.
		/// </summary> 
		public ref int GetCritChance<T>() where T : DamageClass => ref GetCritChance(ModContent.GetInstance<T>());

		/// <summary>
		/// Gets the damage modifier for this damage type on this player.
		/// This returns a reference, and as such, you can freely modify this method's return value with operators.
		/// </summary>
		public ref StatModifier GetDamage<T>() where T : DamageClass => ref GetDamage(ModContent.GetInstance<T>());

		/// <summary>
		/// Gets the knockback modifier for this damage type on this player.
		/// This returns a reference, and as such, you can freely modify this method's return value with operators.
		/// </summary>
		public ref StatModifier GetKnockback<T>() where T : DamageClass => ref GetKnockback(ModContent.GetInstance<T>());

		/// <summary>
		/// Gets the crit modifier for this damage type on this player.
		/// This returns a reference, and as such, you can freely modify this method's return value with operators.
		/// </summary>
		public ref int GetCritChance(DamageClass damageClass) => ref damageData[damageClass.Type].critChance;

		/// <summary>
		/// Gets the damage modifier for this damage type on this player.
		/// This returns a reference, and as such, you can freely modify this method's return value with operators.
		/// </summary>
		public ref StatModifier GetDamage(DamageClass damageClass) => ref damageData[damageClass.Type].damage;

		/// <summary>
		/// Gets the knockback modifier for this damage type on this player.
		/// This returns a reference, and as such, you can freely modify this method's return value with operators.
		/// </summary>
		public ref StatModifier GetKnockback(DamageClass damageClass) => ref damageData[damageClass.Type].knockback;

		/// <summary>
		/// Container for current SceneEffect client properties such as: Backgrounds, music, and water styling
		/// </summary>
		public SceneEffectLoader.SceneEffectInstance CurrentSceneEffect { get; set; } = new SceneEffectLoader.SceneEffectInstance();

		/// <summary>
		/// Stores whether or not the player is in a modbiome using boolean bits.
		/// </summary>
		internal BitArray modBiomeFlags = new BitArray(0);

		/// <summary> 
		/// Determines if the player is in specified ModBiome. This will throw exceptions on failure. 
		/// </summary>
		/// <exception cref="IndexOutOfRangeException"/>
		/// <exception cref="NullReferenceException"/>
		public bool InModBiome(ModBiome baseInstance) => modBiomeFlags[baseInstance.ZeroIndexType];

		/// <summary>
		/// The zone property storing if the player is in the purity/forest biome. Updated in <see cref="UpdateBiomes"/>
		/// </summary>
		public bool ZonePurity { get; set; } = false;

		/// <summary>
		/// Calculates whether or not the player is in the purity/forest biome.
		/// </summary>
		public bool InZonePurity() {
			bool one = ZoneBeach || ZoneCorrupt || ZoneCrimson || ZoneDesert || ZoneDungeon || ZoneGemCave;
			bool two = ZoneGlowshroom || ZoneGranite || ZoneGraveyard || ZoneHallow || ZoneHive || ZoneJungle;
			bool three = ZoneLihzhardTemple || ZoneMarble || ZoneMeteor || ZoneSnow || ZoneUnderworldHeight;
			bool four = modBiomeFlags.Cast<bool>().Contains(true);
			return !(one || two || three || four);
		}

		/// <summary>
		/// Invoked at the end of loading vanilla player data from files to fix stuff that isn't initialized coming out of load.
		/// Corrects the player.lavaMax time, wingsLogic, and no fall dmg to be accurate for the provided items in accessory slots.
		/// </summary>
		internal static void LoadPlayerLastMinuteFixes(Item item, Player newPlayer) {
			int type = item.type;
			if (type == 908 || type == 4874 || type == 5000)
				newPlayer.lavaMax += 420;

			if (type == 906 || type == 4038)
				newPlayer.lavaMax += 420;

			if (newPlayer.wingsLogic == 0 && item.wingSlot >= 0) {
				newPlayer.wingsLogic = item.wingSlot;
				newPlayer.equippedWings = item;
			}

			if (type == 158 || type == 396 || type == 1250 || type == 1251 || type == 1252)
				newPlayer.noFallDmg = true;

			newPlayer.lavaTime = newPlayer.lavaMax;
		}

		/// <summary>
		/// Invoked in UpdateVisibleAccessories. Runs common code for both modded slots and vanilla slots based on provided Items.
		/// </summary>
		internal void UpdateVisibleAccessories(Item item, Item vItem, bool invisible, int slot = -1, bool modded = false) {
			if (eocDash > 0 && shield == -1 && item.shieldSlot != -1) {
				shield = item.shieldSlot;
				if (cShieldFallback != -1)
					cShield = cShieldFallback;
			}

			if (shieldRaised && shield == -1 && item.shieldSlot != -1) {
				shield = item.shieldSlot;
				if (cShieldFallback != -1)
					cShield = cShieldFallback;
			}

			if (ItemIsVisuallyIncompatible(item))
				return;

			if (item.wingSlot > 0) {
				if (invisible && (velocity.Y == 0f || mount.Active))
					return;

				wings = item.wingSlot;
			}

			if (!invisible)
				UpdateVisibleAccessory(slot, item);

			if (!ItemIsVisuallyIncompatible(vItem))
				UpdateVisibleAccessory(slot, vItem);
		}
	}
}
