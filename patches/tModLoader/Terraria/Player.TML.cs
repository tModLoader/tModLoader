using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
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

		bool shinyStoneFlag => (shinyStone && (double)Math.Abs(velocity.X) < 0.05 && (double)Math.Abs(velocity.Y) < 0.05 && itemAnimation == 0);

		public void tMLUpdateLifeRegen() {
			lifeRegenTime++;

			foreach (var entry in RegenEffect.effects) {
				RegenEffect.ProcessLifeEffects(this, entry.life);
			}

			lifeRegenCount += lifeRegen;
			bool flag = shinyStoneFlag;

			//TODO: Everything here downwards is copy paste vanilla, can probably be improved
			if (flag && lifeRegen > 0 && statLife < statLifeMax2) {
				lifeRegenCount++;
				if (flag && (Main.rand.Next(30000) < lifeRegenTime || Main.rand.Next(30) == 0)) {
					int num5 = Dust.NewDust(position, width, height, 55, 0f, 0f, 200, default(Color), 0.5f);
					Main.dust[num5].noGravity = true;
					Main.dust[num5].velocity *= 0.75f;
					Main.dust[num5].fadeIn = 1.3f;
					Vector2 vector = new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 101));
					vector.Normalize();
					vector *= (float)Main.rand.Next(50, 100) * 0.04f;
					Main.dust[num5].velocity = vector;
					vector.Normalize();
					vector *= 34f;
					Main.dust[num5].position = base.Center - vector;
				}
			}

			while (lifeRegenCount >= 120) {
				lifeRegenCount -= 120;
				if (statLife < statLifeMax2) {
					statLife++;
					if (crimsonRegen) {
						for (int i = 0; i < 10; i++) {
							int num6 = Dust.NewDust(position, width, height, 5, 0f, 0f, 175, default(Color), 1.75f);
							Main.dust[num6].noGravity = true;
							Main.dust[num6].velocity *= 0.75f;
							int num7 = Main.rand.Next(-40, 41);
							int num8 = Main.rand.Next(-40, 41);
							Main.dust[num6].position.X += num7;
							Main.dust[num6].position.Y += num8;
							Main.dust[num6].velocity.X = (float)(-num7) * 0.075f;
							Main.dust[num6].velocity.Y = (float)(-num8) * 0.075f;
						}
					}
				}

				if (statLife > statLifeMax2)
					statLife = statLifeMax2;
			}

			if (burned || suffocating || (tongued && Main.expertMode)) {
				while (lifeRegenCount <= -600) {
					lifeRegenCount += 600;
					statLife -= 5;
					CombatText.NewText(new Rectangle((int)position.X, (int)position.Y, width, height), CombatText.LifeRegen, 5, dramatic: false, dot: true);
					if (statLife <= 0 && whoAmI == Main.myPlayer) {
						if (suffocating)
							KillMe(PlayerDeathReason.ByOther(7), 10.0, 0);
						else
							KillMe(PlayerDeathReason.ByOther(8), 10.0, 0);
					}
				}

				return;
			}

			while (lifeRegenCount <= -120) {
				if (lifeRegenCount <= -480) {
					lifeRegenCount += 480;
					statLife -= 4;
					CombatText.NewText(new Rectangle((int)position.X, (int)position.Y, width, height), CombatText.LifeRegen, 4, dramatic: false, dot: true);
				}
				else if (lifeRegenCount <= -360) {
					lifeRegenCount += 360;
					statLife -= 3;
					CombatText.NewText(new Rectangle((int)position.X, (int)position.Y, width, height), CombatText.LifeRegen, 3, dramatic: false, dot: true);
				}
				else if (lifeRegenCount <= -240) {
					lifeRegenCount += 240;
					statLife -= 2;
					CombatText.NewText(new Rectangle((int)position.X, (int)position.Y, width, height), CombatText.LifeRegen, 2, dramatic: false, dot: true);
				}
				else {
					lifeRegenCount += 120;
					statLife--;
					CombatText.NewText(new Rectangle((int)position.X, (int)position.Y, width, height), CombatText.LifeRegen, 1, dramatic: false, dot: true);
				}

				if (statLife <= 0 && whoAmI == Main.myPlayer) {
					if (poisoned || venom)
						KillMe(PlayerDeathReason.ByOther(9), 10.0, 0);
					else if (electrified)
						KillMe(PlayerDeathReason.ByOther(10), 10.0, 0);
					else
						KillMe(PlayerDeathReason.ByOther(8), 10.0, 0);
				}
			}
		}

		public int LifeRegenTimeBonuses() {
			float num2 = 0f;

			lifeRegenTime = shinyStoneFlag ? lifeRegenTime : Math.Min(3600, lifeRegenTime);

			int num3 = (int)Math.Floor((float)(lifeRegenTime / 300));
			num2 += Math.Min(num3, 30);

			if (sitting.isSitting || sleeping.isSleeping) {
				lifeRegenTime += 10;
				num2 *= 1.5f;
			}

			num2 = ((velocity.X != 0f && grappling[0] <= 0) ? (num2 * 0.5f) : (num2 * 1.25f));

			if (crimsonRegen)
				num2 *= 1.5f;

			if (shinyStone)
				num2 *= 1.1f;

			if (whoAmI == Main.myPlayer && Main.SceneMetrics.HasCampfire)
				num2 *= 1.1f;

			if (Main.expertMode && !wellFed)
				num2 = ((!shinyStone) ? (num2 / 2f) : (num2 * 0.75f));

			if (rabid)
				num2 = ((!shinyStone) ? (num2 / 2f) : (num2 * 0.75f));

			float num4 = (float)statLifeMax2 / 400f * 0.85f + 0.15f;

			return (int)(num2 * num4);
		}

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
				if (statMana < statManaMax2 && manaRegen > 0) {
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

			foreach (var entry in RegenEffect.effects) {
				RegenEffect.ProcessManaEffects(this, entry.mana);
			}
		}

		public class RegenEffect {
			public delegate int DeltaRegenPer120Frames(Player obj);
			public delegate void AdditionalEffects(Player obj);

			public struct ByStatStruct {
				public Predicate<Player> isActive;
				public DeltaRegenPer120Frames deltaRegenPer120Frames;
				public bool forcePositiveRegen;
				public AdditionalEffects additionalEffects;

				public static ByStatStruct Create(Predicate<Player> isActive, DeltaRegenPer120Frames delta, bool forcePositiveRegen = false, AdditionalEffects additionalEffects = null) {
					return new ByStatStruct { isActive = isActive, deltaRegenPer120Frames = delta, forcePositiveRegen = forcePositiveRegen , additionalEffects = additionalEffects};
				}

				public static readonly ByStatStruct nullStruct = ByStatStruct.Create(_ => false, _ => 0);
			}

			public string name;
			public ByStatStruct life;
			public ByStatStruct mana;

			public RegenEffect(string name, ByStatStruct life, ByStatStruct mana) {
				this.name = name;
				this.life = life;
				this.mana = mana;
			}

			#region VanillaEffects 

			public static List<RegenEffect> effects = new List<RegenEffect>() {
				new RegenEffect("poison", ByStatStruct.Create((obj) => obj.poisoned, _ => -4), ByStatStruct.nullStruct),
				new RegenEffect("venom", ByStatStruct.Create((obj) => obj.venom, _ => -30), ByStatStruct.nullStruct),
				new RegenEffect("onFire1", ByStatStruct.Create((obj) => obj.onFire, _ => -8), ByStatStruct.nullStruct),
				new RegenEffect("onFire3", ByStatStruct.Create((obj) => obj.onFire3, _ => -8), ByStatStruct.nullStruct),
				new RegenEffect("onFrostBurn", ByStatStruct.Create((obj) => obj.onFrostBurn, _ => -16), ByStatStruct.nullStruct),
				new RegenEffect("onFrostBurn2", ByStatStruct.Create((obj) => obj.onFrostBurn2, _ => -16), ByStatStruct.nullStruct),
				new RegenEffect("onFire2", ByStatStruct.Create((obj) => obj.onFire2, _ => -24), ByStatStruct.nullStruct),
				new RegenEffect("burned", ByStatStruct.Create((obj) => obj.burned, _ => -60, additionalEffects: (obj) => obj.moveSpeed *= 0.5f), ByStatStruct.nullStruct),
				new RegenEffect("suffocating", ByStatStruct.Create((obj) => obj.suffocating, _ => -40), ByStatStruct.nullStruct),
				new RegenEffect("electrified", ByStatStruct.Create((obj) => obj.electrified, (obj) => (obj.controlLeft || obj.controlRight) ? -40 : -8), ByStatStruct.nullStruct),
				new RegenEffect("tongued", ByStatStruct.Create((obj) => obj.tongued && Main.expertMode, _ => -100), ByStatStruct.nullStruct),
				new RegenEffect("antiDebuffHoney", ByStatStruct.Create((obj) => obj.honey && obj.lifeRegen < 0, _ => 4), ByStatStruct.nullStruct),
				new RegenEffect("nebulaLevelLife", ByStatStruct.Create((obj) => obj.nebulaLevelLife > 0 && obj.lifeRegen < 0, (obj) => 0 - obj.lifeRegen), ByStatStruct.nullStruct),
				new RegenEffect("antiDebuffShinyStone", ByStatStruct.Create((obj) => obj.shinyStoneFlag && obj.lifeRegen < 0, _ => 0, additionalEffects: (obj) => obj.lifeRegen /= 2), ByStatStruct.nullStruct),
				new RegenEffect("crimsonRegen", ByStatStruct.Create((obj) => obj.crimsonRegen, _ => 0, additionalEffects: (obj) => obj.lifeRegenTime++), ByStatStruct.nullStruct),
				new RegenEffect("soulDrain", ByStatStruct.Create((obj) => obj.soulDrain > 0, (obj) => (5 + obj.soulDrain) / 2, true, (obj) => obj.lifeRegenTime += (5 + obj.soulDrain) / 2 + 2), ByStatStruct.nullStruct),
				new RegenEffect("shinyStone", ByStatStruct.Create((obj) => obj.shinyStoneFlag, _ => 4, true, (obj) => obj.lifeRegenTime += (obj.lifeRegenTime > 90 && obj.lifeRegenTime < 1800) ? 1804 : 4 ), ByStatStruct.nullStruct),
				new RegenEffect("honey", ByStatStruct.Create((obj) => obj.honey, _ => 2, additionalEffects: (obj) => obj.lifeRegenTime += 2), ByStatStruct.nullStruct),
				new RegenEffect("campfire", ByStatStruct.Create((obj) => obj.whoAmI == Main.myPlayer && Main.SceneMetrics.HasCampfire, _ => 1, true), ByStatStruct.nullStruct),
				new RegenEffect("heartLantern", ByStatStruct.Create((obj) => obj.whoAmI == Main.myPlayer && Main.SceneMetrics.HasHeartLantern, _ => 2, true), ByStatStruct.nullStruct),
				new RegenEffect("bleed", ByStatStruct.Create((obj) => obj.bleed, _ => 0, additionalEffects: (obj) => obj.lifeRegenTime = 0), ByStatStruct.nullStruct),
				new RegenEffect("naturalRegen", ByStatStruct.Create((obj) => true, (obj) => obj.LifeRegenTimeBonuses(), true), ByStatStruct.nullStruct),
				new RegenEffect("palladiumRegen", ByStatStruct.Create((obj) => obj.palladiumRegen, _ => 4, true), ByStatStruct.nullStruct),
				new RegenEffect("manaRegenBonus1", ByStatStruct.nullStruct, ByStatStruct.Create(_ => true, (obj) => obj.manaRegenBonus)),
				new RegenEffect("manaRegenBonus2", ByStatStruct.nullStruct, ByStatStruct.Create((obj) => (obj.velocity.X == 0f && obj.velocity.Y == 0f) || obj.grappling[0] >= 0 || obj.manaRegenBuff, (obj) => obj.statManaMax2 / 2)),
				new RegenEffect("manaRegenBonus3", ByStatStruct.nullStruct, ByStatStruct.Create((obj)=> true, (obj) => 0, additionalEffects: (obj) => obj.manaRegen *= (int)(obj.manaRegenBuff ? 1.15f : 1.15f * ((float)obj.statMana / (float)obj.statManaMax2 * 0.8f + 0.2f)))),
			};

			#endregion

			internal static void ProcessLifeEffects(Player player, ByStatStruct entry) {
				if (!entry.isActive(player))
					return;

				int val = entry.deltaRegenPer120Frames(player);
				bool prevStateDebuff = player.lifeRegen <= 0;
				player.lifeRegen += val;

				if (val >= 0) {
					if (prevStateDebuff && !entry.forcePositiveRegen) {
						player.lifeRegen = Math.Min(player.lifeRegen, 0);
					}
				}
				else {
					player.lifeRegenTime = 0;
					player.lifeRegen = Math.Min(player.lifeRegen, 0);
				}

				if (entry.additionalEffects != null)
					entry.additionalEffects(player);
			}

			internal static void ProcessManaEffects(Player player, ByStatStruct entry) {
				if (!entry.isActive(player))
					return;

				int val = entry.deltaRegenPer120Frames(player);
				bool prevStateDebuff = player.manaRegen < 0;

				if (val >= 0) {
					player.manaRegen += val;
					if (prevStateDebuff && !entry.forcePositiveRegen) {
						player.manaRegen = Math.Min(player.manaRegen, 0);
					}
				}
				else {
					player.manaRegenDelay = (int)player.maxRegenDelay;
				}

				if (entry.additionalEffects != null)
					entry.additionalEffects(player);
			}
		}
	}
}
