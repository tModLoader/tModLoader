using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace Terraria
{
	public partial class Player {
		internal IList<string> usedMods;
		internal ModPlayer[] modPlayers = new ModPlayer[0];

		public int infoDisplayPage;
		public HashSet<int> nearbyModTorch = new HashSet<int>();

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

		public void CheckManaRegenDelay() {
			if (manaRegenDelay == 0) {
				SetManaRegen();
				return;
			}

			manaRegenDelay--;

			float delay = manaRegenDelay;
			foreach (var entry in PlayerRegenEffects.effects) {
				if (!entry.isActive(this))
					continue;
				if (!ProcessManaDelay(entry.manaDelay, ref delay))
					break;
			}
			manaRegenDelay = (int)Math.Round(delay);

			if (manaRegenBuff && manaRegenDelay > 20) {
				manaRegenDelay = 20;
				return;
			}

			if (manaRegenDelay <= 0) {
				manaRegenDelay = 0;
				SetManaRegen();
			}
		}

		internal bool ProcessManaDelay(PlayerRegenEffects.ManaRegenDelayStats entry, ref float delay) {
			if (entry.resetDelayToZero != null) {
				if (entry.resetDelayToZero(this)) {
					delay = 0;
					return false;
				}
			}

			if (entry.increaseDelaySpeed != null) {
				delay = Math.Min(delay - entry.increaseDelaySpeed(this), this.maxRegenDelay);
			}
			return true;
		}

		internal void RecalculateMaxRegenDelay() {
			float maxDelayCap = float.MaxValue;
			foreach (var entry in PlayerRegenEffects.effects) {
				if (!entry.isActive(this))
					continue;

				if (entry.manaDelay.maxDelayCap != null) {
					maxDelayCap = Math.Min(maxDelayCap, entry.manaDelay.maxDelayCap(this));
				}
				if (entry.manaDelay.increaseMaxDelay != null) {
					maxRegenDelay = Math.Min(maxDelayCap, maxRegenDelay + entry.manaDelay.increaseMaxDelay(this));
				}
			}
		}

		public float manaRegenPotencyMultiplier;

		public void SetManaRegen() {
			manaRegen = statManaMax2 / 7 + 1;
			manaRegenPotencyMultiplier = 1;

			float regen = manaRegen;
			foreach (var entry in PlayerRegenEffects.effects) {
				if (!entry.isActive(this))
					continue;
				ProcessCommonRegen(entry.manaCommon, ref regen);

				if (manaRegenPotencyMultiplier == 0) {
					break;
				}
			}
			manaRegen = (int)Math.Round(regen * manaRegenPotencyMultiplier);
		}

		internal void ProcessCommonRegen(PlayerRegenEffects.CommonRegenStats entry, ref float regen) {
			float val = entry.deltaRegenPer120Frames(this);
			bool prevStateDebuff = regen < 0;
			regen += val;

			if ((prevStateDebuff || val < 0) && !entry.allowPositiveRegenWhileDebuffed) {
				regen = Math.Min(regen, 0);
			}

			if (entry.additionalEffects != null)
				entry.additionalEffects(this, regen);
		}
	}
}
