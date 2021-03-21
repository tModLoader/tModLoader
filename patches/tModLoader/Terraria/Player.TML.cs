using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace Terraria
{
	public partial class Player {
		internal IList<string> usedMods;
		internal ModPlayer[] modPlayers = new ModPlayer[0];

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
			foreach (var entry in RegenEffect.effects) {
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

		internal bool ProcessManaDelay(RegenEffect.ManaRegenDelayStats entry, ref float delay) {
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
			foreach (var entry in RegenEffect.effects) {
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
			foreach (var entry in RegenEffect.effects) {
				if (!entry.isActive(this))
					continue;
				ProcessCommonRegen(entry.manaCommon, ref regen);

				if (manaRegenPotencyMultiplier == 0) {
					break;
				}
			}
			manaRegen = (int)Math.Round(regen * manaRegenPotencyMultiplier);
		}

		internal void ProcessCommonRegen(RegenEffect.CommonRegenStats entry, ref float regen) {
			float val = entry.deltaRegenPer120Frames(this);
			bool prevStateDebuff = regen < 0;
			regen += val;

			if ((prevStateDebuff || val < 0) && !entry.allowPositiveRegenWhileDebuffed) {
				regen = Math.Min(regen, 0);
			}

			if (entry.additionalEffects != null)
				entry.additionalEffects(this, regen);
		}

		public class RegenEffect
		{
			public delegate float DelegateFloat(Player player);
			public delegate void AdditionalEffects(Player player, float regen);

			public struct CommonRegenStats {
				public DelegateFloat deltaRegenPer120Frames;
				public bool allowPositiveRegenWhileDebuffed;
				public AdditionalEffects additionalEffects;

				public static CommonRegenStats Create(DelegateFloat delta = null, AdditionalEffects additionalEffects = null, bool forcePositiveRegen = false) {
					return new CommonRegenStats {
						deltaRegenPer120Frames = delta,
						additionalEffects = additionalEffects, 
						allowPositiveRegenWhileDebuffed = forcePositiveRegen 
					};
				}

				public struct CombinationFlags
				{
					public bool overrideDelta;
					public bool overrideAdditionalEffects;

					public static CombinationFlags Create(bool overrideDelta = false, bool overrideAdditionalEffects = false) {
						return new CombinationFlags { overrideDelta = overrideDelta, overrideAdditionalEffects = overrideAdditionalEffects };
					}
				};

				internal static void Combine(CommonRegenStats origin, CommonRegenStats combine, CombinationFlags flags) {
					origin.deltaRegenPer120Frames = FloatCombine(origin.deltaRegenPer120Frames, combine.deltaRegenPer120Frames, flags.overrideDelta);

					if (flags.overrideAdditionalEffects || origin.additionalEffects == null) {
						origin.additionalEffects = combine.additionalEffects;
					}
					else if (combine.additionalEffects != null) {
						origin.additionalEffects = (p, regen) => { origin.additionalEffects(p, regen); combine.additionalEffects(p, regen); };
					}

					origin.allowPositiveRegenWhileDebuffed = combine.allowPositiveRegenWhileDebuffed;
				}
			}

			public struct ManaRegenDelayStats {
				public DelegateFloat maxDelayCap;
				public DelegateFloat increaseMaxDelay;
				public DelegateFloat increaseDelaySpeed;
				public Predicate<Player> resetDelayToZero;

				public static ManaRegenDelayStats Create(DelegateFloat maxDelayCap = null, DelegateFloat increaseMaxDelay = null, DelegateFloat increaseDelaySpeed = null, Predicate<Player> resetDelayToZero = null) {
					return new ManaRegenDelayStats {
						maxDelayCap = maxDelayCap,
						increaseMaxDelay = increaseMaxDelay,
						increaseDelaySpeed = increaseDelaySpeed,
						resetDelayToZero = resetDelayToZero
					};
				}

				public struct CombinationFlags
				{
					public bool overrideMaxDelayCap;
					public bool overrideIncreaseMaxDelay;
					public bool overrideIncreaseDelaySpeed;
					public bool overrideResetDelayToZero;
					public bool useANDForResetDelayElseOR;

					public static CombinationFlags Create(bool overrideMaxDelayCap = false, bool overrideIncreaseMaxDelay = false, bool overrideIncreaseDelaySpeed = false, bool overrideResetDelayToZero = false, bool useANDForResetDelayElseOR = false) {
						return new CombinationFlags {
							overrideMaxDelayCap = overrideMaxDelayCap,
							overrideIncreaseMaxDelay = overrideIncreaseMaxDelay,
							overrideIncreaseDelaySpeed = overrideIncreaseDelaySpeed,
							overrideResetDelayToZero = overrideResetDelayToZero,
							useANDForResetDelayElseOR = useANDForResetDelayElseOR
						};
					}
				};

				internal static void Combine(ManaRegenDelayStats origin, ManaRegenDelayStats combine, CombinationFlags flags) {
					origin.maxDelayCap = FloatCombine(origin.maxDelayCap, combine.maxDelayCap, flags.overrideMaxDelayCap);
					origin.increaseMaxDelay = FloatCombine(origin.increaseMaxDelay, combine.increaseMaxDelay, flags.overrideIncreaseMaxDelay);
					origin.increaseDelaySpeed = FloatCombine(origin.increaseDelaySpeed, combine.increaseDelaySpeed, flags.overrideIncreaseDelaySpeed);
					origin.resetDelayToZero = BoolCombine(origin.resetDelayToZero, combine.resetDelayToZero, flags.overrideResetDelayToZero, flags.useANDForResetDelayElseOR);
				}
			}

			private static DelegateFloat FloatCombine(DelegateFloat o, DelegateFloat c, bool flag) {
				if (c == null) {
					return o;
				}
				
				if (flag || o == null) {
					return c;
				}
				else {
					return o + c; //TODO: this doesn't appear to work, but should. BoolCombine works fine. Confusion.
				}
			}

			private static Predicate<Player> BoolCombine(Predicate<Player> o, Predicate<Player> c, bool flag, bool hIsAnd) {
				if (c == null) {
					return o;
				}
				if (flag) {
					return c;
				}
				else {
					if (hIsAnd) {
						return (p) => (c(p) && o(p));
					}
					else {
						return (p) => (c(p) || o(p));
					}
				}
			}
			
			public string name;
			public Predicate<Player> isActive;
			public CommonRegenStats manaCommon;
			public ManaRegenDelayStats manaDelay;

			public RegenEffect(string name, Predicate<Player> isActive, CommonRegenStats manaCommon = default(CommonRegenStats), ManaRegenDelayStats manaDelay = default(ManaRegenDelayStats)) {
				this.name = name;
				this.isActive = isActive;
				this.manaCommon = manaCommon;
				this.manaDelay = manaDelay;
			}

			#region VanillaEffects

			public readonly static RegenEffect vanillaManaRegenBonus = new RegenEffect("vanillaManaRegenBonus", (p) => true, CommonRegenStats.Create((p) => p.manaRegenBonus), ManaRegenDelayStats.Create(increaseDelaySpeed: (p) => p.manaRegenDelayBonus));
			public readonly static RegenEffect stillPlayerMana = new RegenEffect("stillPlayerMana", (p) => (p.velocity.X == 0f && p.velocity.Y == 0f) || p.grappling[0] >= 0 || p.manaRegenBuff, CommonRegenStats.Create((p) => p.statManaMax2 / 2), ManaRegenDelayStats.Create(increaseDelaySpeed: _ => 1));
			public readonly static RegenEffect manaRegenMaxStatScaleBonus = new RegenEffect("manaRegenMaxStatScaleBonus", (p) => true, CommonRegenStats.Create((p) => 0, (p, regen) => regen *= (int)(p.manaRegenBuff ? 1.15f : 1.15f * ((float)p.statMana / (float)p.statManaMax2 * 0.8f + 0.2f))));

			internal static void RegisterVanilla() {
				Register(stillPlayerMana);
				Register(manaRegenMaxStatScaleBonus);
				Register(vanillaManaRegenBonus);
			}

			#endregion

			internal static Dictionary<string, short> effectsDict = new Dictionary<string, short>();
			internal static List<RegenEffect> effects = new List<RegenEffect>();

			internal static void Register(RegenEffect effect) {
				if (!effectsDict.ContainsKey(effect.name)) {
					effectsDict.Add(effect.name, (short)effects.Count);
					effects.Add(effect);
				}
			}

			internal static void RecalculateEffects() {
				effectsDict.Clear();
				effects.Clear();
				RegisterVanilla();
				BuffLoader.RegisterModManaRegenEffect();
				BuffLoader.ModifyManaRegenEffects();
			}

			public struct ModifyFlags
			{
				public bool overrideIsActive;
				public bool useANDForIsActiveElseOR;
				public CommonRegenStats.CombinationFlags manaRegenFlags;
				public ManaRegenDelayStats.CombinationFlags manaDelayFlags;

				public static ModifyFlags Create(bool overrideIsActive = false, bool useANDForIsActiveElseOR = false, CommonRegenStats.CombinationFlags manaRegenFlags = default(CommonRegenStats.CombinationFlags), ManaRegenDelayStats.CombinationFlags manaDelayFlags = default(ManaRegenDelayStats.CombinationFlags)) {
					return new ModifyFlags { overrideIsActive = overrideIsActive, useANDForIsActiveElseOR = useANDForIsActiveElseOR, manaRegenFlags = manaRegenFlags, manaDelayFlags = manaDelayFlags };
				}
			}

			public struct ModifyRegenEffectStruct {
				public string targetEffect;
				public Predicate<Player> isActive;
				public CommonRegenStats modifyManaCommonWith;
				public ManaRegenDelayStats modifyManaDelayWith;
				public ModifyFlags flags;

				public static ModifyRegenEffectStruct Create(string targetEffect, Predicate<Player> isActive = null, CommonRegenStats modifyManaCommonWith = default(CommonRegenStats), ManaRegenDelayStats modifyManaDelayWith = default(ManaRegenDelayStats), ModifyFlags flags = default(ModifyFlags)) {
					return new ModifyRegenEffectStruct {
						targetEffect = targetEffect,
						isActive = isActive,
						modifyManaCommonWith = modifyManaCommonWith,
						modifyManaDelayWith = modifyManaDelayWith,
						flags = flags
					};
				}
			}

			internal static bool ModifyEffect(ModifyRegenEffectStruct r) {
				if (!effectsDict.TryGetValue(r.targetEffect, out short index)) {
					return false;
				}

				var effect = effects[index];
				effect.isActive = BoolCombine(effect.isActive, r.isActive, r.flags.overrideIsActive, r.flags.useANDForIsActiveElseOR);
				CommonRegenStats.Combine(effect.manaCommon, r.modifyManaCommonWith, r.flags.manaRegenFlags);
				ManaRegenDelayStats.Combine(effect.manaDelay, r.modifyManaDelayWith, r.flags.manaDelayFlags);

				return true;
			}
		}
	}
}
