using System;
using System.Collections.Generic;

namespace Terraria.ModLoader {
	public class PlayerRegenEffects {
		public delegate float DelegateFloat(Player player);
		public delegate void AdditionalEffects(Player player, float regen);

		public struct CommonRegenStats
		{
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

			internal static void Combine(ref CommonRegenStats origin, CommonRegenStats combine, CombinationFlags flags) {
				origin.deltaRegenPer120Frames = FloatCombine(origin.deltaRegenPer120Frames, combine.deltaRegenPer120Frames, flags.overrideDelta);
				origin.additionalEffects = VoidCombine(origin.additionalEffects, combine.additionalEffects, flags.overrideAdditionalEffects);
				origin.allowPositiveRegenWhileDebuffed |= combine.allowPositiveRegenWhileDebuffed;
			}

			internal CommonRegenStats(CommonRegenStats o) {
				deltaRegenPer120Frames = o.deltaRegenPer120Frames;
				allowPositiveRegenWhileDebuffed = o.allowPositiveRegenWhileDebuffed;
				additionalEffects = o.additionalEffects;
			}
		}

		public struct ManaRegenDelayStats
		{
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

			internal static void Combine(ref ManaRegenDelayStats origin, ManaRegenDelayStats combine, CombinationFlags flags) {
				origin.maxDelayCap = FloatCombine(origin.maxDelayCap, combine.maxDelayCap, flags.overrideMaxDelayCap);
				origin.increaseMaxDelay = FloatCombine(origin.increaseMaxDelay, combine.increaseMaxDelay, flags.overrideIncreaseMaxDelay);
				origin.increaseDelaySpeed = FloatCombine(origin.increaseDelaySpeed, combine.increaseDelaySpeed, flags.overrideIncreaseDelaySpeed);
				origin.resetDelayToZero = BoolCombine(origin.resetDelayToZero, combine.resetDelayToZero, flags.overrideResetDelayToZero, flags.useANDForResetDelayElseOR);
			}

			internal ManaRegenDelayStats(ManaRegenDelayStats o) {
				maxDelayCap = o.maxDelayCap;
				increaseDelaySpeed = o.increaseDelaySpeed;
				increaseMaxDelay = o.increaseMaxDelay;
				resetDelayToZero = o.resetDelayToZero;
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
				return (p) => o(p) + c(p);
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

		private static AdditionalEffects VoidCombine(AdditionalEffects o, AdditionalEffects c, bool flag) {
			if (flag || o == null) {
				return c;
			}
			else if (c != null) {
				return o + c;
			}
			else {
				return o;
			}
		}

		public class RegenEffect
		{
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

			internal RegenEffect(RegenEffect o) {
				this.name = o.name;
				this.isActive = o.isActive;
				this.manaCommon = new CommonRegenStats(o.manaCommon);
				this.manaDelay = new ManaRegenDelayStats(o.manaDelay);
			}
		}

		#region VanillaEffects

		public readonly static RegenEffect vanillaManaRegenBonus = new RegenEffect("vanillaManaRegenBonus", (p) => true, CommonRegenStats.Create((p) => p.manaRegenBonus), ManaRegenDelayStats.Create(increaseDelaySpeed: (p) => p.manaRegenDelayBonus));
		public readonly static RegenEffect stillPlayerMana = new RegenEffect("stillPlayerMana", (p) => (p.velocity.X == 0f && p.velocity.Y == 0f) || p.grappling[0] >= 0 || p.manaRegenBuff, CommonRegenStats.Create((p) => p.statManaMax2 / 2f), ManaRegenDelayStats.Create(increaseDelaySpeed: _ => 1f));
		public readonly static RegenEffect manaRegenMaxStatScaleBonus = new RegenEffect("manaRegenMaxStatScaleBonus", (p) => true, CommonRegenStats.Create((p) => 0, (p, regen) => regen *= (int)(p.manaRegenBuff ? 1.15f : 1.15f * ((float)p.statMana / (float)p.statManaMax2 * 0.8f + 0.2f))));

		internal static void RegisterVanilla() {
			Register(new RegenEffect(stillPlayerMana));
			Register(new RegenEffect(manaRegenMaxStatScaleBonus));
			Register(new RegenEffect(vanillaManaRegenBonus));
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

		public struct ModifyRegenEffectStruct
		{
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
			CommonRegenStats.Combine(ref effect.manaCommon, r.modifyManaCommonWith, r.flags.manaRegenFlags);
			ManaRegenDelayStats.Combine(ref effect.manaDelay, r.modifyManaDelayWith, r.flags.manaDelayFlags);

			return true;
		}
	}
}
