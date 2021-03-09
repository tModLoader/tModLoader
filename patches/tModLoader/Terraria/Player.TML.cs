using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.ModLoader;

namespace Terraria
{
	public partial class Player
	{
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

		public void tMLUpdateManaRegen() {
			if (nebulaLevelMana > 0) {
				int num = 6;
				nebulaManaCounter += nebulaLevelMana;
				if (nebulaManaCounter >= num) {
					nebulaManaCounter -= num;
					statMana++;
					if (statMana >= statManaMax2)
						statMana = statManaMax2;
				}
			}
			else {
				nebulaManaCounter = 0;
			}

			CheckManaRegenDelay();

			manaRegenCount += manaRegen;
			while (manaRegenCount >= 120) {
				bool flag = false;
				manaRegenCount -= 120;
				if (statMana < statManaMax2) {
					statMana++;
					flag = true;
				}


				if (statMana < statManaMax2)
					continue;

				if (whoAmI == Main.myPlayer && flag) {
					SoundEngine.PlaySound(25);
					for (int i = 0; i < 5; i++) {
						int num3 = Dust.NewDust(position, width, height, 45, 0f, 0f, 255, default(Color), (float)Main.rand.Next(20, 26) * 0.1f);
						Main.dust[num3].noLight = true;
						Main.dust[num3].noGravity = true;
						Main.dust[num3].velocity *= 0.5f;
					}
				}

				statMana = statManaMax2;
			}

			while (manaRegenCount <= -120) {
				manaRegenCount -= 120;
				if (statMana > 0) {
					statMana--;
				}
			}
		}

		public void CheckManaRegenDelay() {
			if (manaRegenDelay == 0) {
				SetManaRegen();
				return;
			}

			manaRegenDelay--;
			manaRegenDelay -= manaRegenDelayBonus;
			if ((velocity.X == 0f && velocity.Y == 0f) || grappling[0] >= 0 || manaRegenBuff)
				manaRegenDelay--;

			if (manaRegenBuff && manaRegenDelay > 20) {
				manaRegenDelay = 20;
				return;
			}

			if (manaRegenDelay <= 0) {
				manaRegenDelay = 0;
				SetManaRegen();
			}
		}

		public void SetManaRegen() {
			manaRegen = statManaMax2 / 7 + 1;

			float delta = manaRegen;
			foreach (var entry in RegenEffect.effects) {
				ProcessManaEffects(this, entry.mana, ref delta);
			}
			manaRegen = (int)Math.Round(delta);
		}

		internal static void ProcessManaEffects(Player player, RegenEffect.ByStatStruct entry, ref float delta) {
			if (!entry.isActive(player))
				return;

			float val = entry.deltaRegenPer120Frames(player);
			bool prevStateDebuff = delta < 0;
			delta += val;

			if ((prevStateDebuff || val < 0) && !entry.allowPositiveRegenWhileDebuffed) {
				delta = Math.Min(delta, 0);
			}

			if (entry.additionalEffects != null)
				entry.additionalEffects(player, delta);
		}

		public class RegenEffect
		{
			public delegate float DeltaRegenPer120Frames(Player player);
			public delegate void AdditionalEffects(Player player, float regen);

			public struct CombinationFlags
			{
				public bool overrideCondition;
				public bool useANDForConditionElseOR;
				public bool overrideDelta;
				public bool useANDForForcePositiveRegenElseOR;
				public bool overrideAdditionalEffects;

				public static CombinationFlags Create(bool b0, bool b1, bool b2, bool b3, bool b4) {
					return new CombinationFlags { overrideCondition = b0, useANDForConditionElseOR = b1, overrideDelta = b2, useANDForForcePositiveRegenElseOR = b3, overrideAdditionalEffects = b4 };
				}
			};

			public struct ByStatStruct
			{
				public Predicate<Player> isActive;
				public DeltaRegenPer120Frames deltaRegenPer120Frames;
				public bool allowPositiveRegenWhileDebuffed;
				public AdditionalEffects additionalEffects;

				public static ByStatStruct Create(Predicate<Player> isActive, DeltaRegenPer120Frames delta, bool forcePositiveRegen = false, AdditionalEffects additionalEffects = null) {
					return new ByStatStruct { isActive = isActive, deltaRegenPer120Frames = delta, allowPositiveRegenWhileDebuffed = forcePositiveRegen, additionalEffects = additionalEffects };
				}

				public static readonly ByStatStruct nullStruct = ByStatStruct.Create(_ => false, _ => 0);

				internal static void Combine(ByStatStruct origin, ByStatStruct combine, CombinationFlags flags) {
					if (flags.overrideCondition) {
						origin.isActive = combine.isActive;
					}
					else if (flags.useANDForConditionElseOR) {
						origin.isActive = (obj) => (combine.isActive(obj) && origin.isActive(obj));
					}
					else {
						origin.isActive = (obj) => (combine.isActive(obj) || origin.isActive(obj));
					}
					
					if (flags.overrideDelta) {
						origin.deltaRegenPer120Frames = combine.deltaRegenPer120Frames;
					} else {
						origin.deltaRegenPer120Frames = (obj) => origin.deltaRegenPer120Frames(obj) + combine.deltaRegenPer120Frames(obj);
					}

					origin.allowPositiveRegenWhileDebuffed = combine.allowPositiveRegenWhileDebuffed;

					if (flags.overrideAdditionalEffects) {
						origin.additionalEffects = combine.additionalEffects;
					} else {
						origin.additionalEffects = (obj, delta) => { origin.additionalEffects(obj, delta); combine.additionalEffects(obj, delta); };
					}
				}
			}

			public string name;
			public ByStatStruct mana;

			public RegenEffect(string name, ByStatStruct mana) {
				this.name = name;
				this.mana = mana;
			}

			#region VanillaEffects

			public readonly static RegenEffect manaRegenBonus1 = new RegenEffect("manaRegenBonus1", ByStatStruct.Create(_ => true, (obj) => obj.manaRegenBonus));
			public readonly static RegenEffect manaRegenBonus2 = new RegenEffect("manaRegenBonus2", ByStatStruct.Create((obj) => (obj.velocity.X == 0f && obj.velocity.Y == 0f) || obj.grappling[0] >= 0 || obj.manaRegenBuff, (obj) => obj.statManaMax2 / 2));
			public readonly static RegenEffect manaRegenBonus3 = new RegenEffect("manaRegenBonus3", ByStatStruct.Create((obj) => true, (obj) => 0, additionalEffects: (obj, delta) => delta *= (int)(obj.manaRegenBuff ? 1.15f : 1.15f * ((float)obj.statMana / (float)obj.statManaMax2 * 0.8f + 0.2f))));

			internal static void RegisterVanilla() {
				effectsDict.Clear(); effects.Clear();
				Register(manaRegenBonus3);
				Register(manaRegenBonus2);
				Register(manaRegenBonus1);
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

			public struct ModifyRegenEffectStruct
			{
				public string targetEffect;
				public ByStatStruct modifyWith;
				public CombinationFlags flags;
			}

			//TODO: different logging and not an exception please.
			internal static void ModifyEffect(ModifyRegenEffectStruct r) {
				if (effectsDict.TryGetValue(r.targetEffect, out short index)) {
					ByStatStruct.Combine(effects[index].mana, r.modifyWith, r.flags);
				}
				else {
					throw new ArgumentOutOfRangeException(r.targetEffect, "Mana regen effect targeted to modify does not exist!");
				}
			}
		}
	}
}
