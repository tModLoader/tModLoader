using System;
using Terraria.ModLoader;

namespace Terraria;

public partial class NPC
{
	/// <summary>
	/// Represents a damage calculation in the process of being calculated for damage to be applied to an NPC. The final damage calculation will be present in the resulting <see cref="HitInfo"/> provided to the OnHit hooks.
	/// </summary>
	public struct HitModifiers
	{
		/// <summary>
		/// The DamageType of the hit.
		/// </summary>
		public DamageClass DamageType { get; init; } = DamageClass.Default;

		/// <summary>
		/// The direction to apply knockback. If 0, no knockback will be applied. <br/>
		/// Could potentially be used for directional resistances. <br/>
		/// Can be overridden by <see cref="HitDirectionOverride"/>
		/// </summary>
		public int HitDirection { get; init; } = default;

		/// <summary>
		/// If true, no amount of damage can get through the defense of this NPC, damage will be reduced to 1. <br/>
		/// <see cref="CritDamage"/> will still apply, but only Additive and Multiplicative. Maximum crit damage will be capped at 4. <br/>
		/// </summary>
		public bool SuperArmor { get; init; } = false;

		/// <summary>
		/// Use this to enhance or scale the base damage of the item/projectile/hit. This damage modifier will apply to <see cref="HitInfo.SourceDamage"/> and be transferred to on-hit effects. <br/>
		/// <br/>
		/// For effects which apply to all damage dealt by the player, or a specific damage type, consider using <see cref="Player.GetDamage"/> instead. <br/>
		/// For effects which apply to all damage dealt by an item, consider using <see cref="GlobalItem.ModifyWeaponDamage"/> instead. <br/>
		/// <br/>
		/// Used by vanilla for weapons with unique scaling such as jousting lance, ham bat, breaker blade. And for accessories which enhance a projectile (strong bees)
		/// </summary>
		public StatModifier SourceDamage = new();

		/// <summary>
		/// Use this to add bonus damage to the hit, but not to on-hit effects. <br/>
		/// <br/>
		/// Used by vanilla for most summon tag damage.
		/// </summary>
		public AddableFloat FlatBonusDamage = new();

		/// <summary>
		/// Use this to add bonus <br/>
		/// Used by vanilla for melee parry buff (+4f) and some summon tag damage.
		/// </summary>
		public AddableFloat ScalingBonusDamage = new();

		/// <summary>
		/// Not recommended for modded use due to difficulty balancing around defense, consider multiplying <see cref="FinalDamage"/> instead. <br/>
		/// Used by vanilla for banners, cultist projectile resistances, extra damage for stakes against vampires etc.
		/// </summary>
		public MultipliableFloat TargetDamageMultiplier = new();

		/// <summary>
		/// The defense of the receiver, including any temporary modifiers (buffs/debuffs). <br/>
		/// <br/>
		/// Increase <see cref="StatModifier.Base"/> to add extra defense. <br/>
		/// Add for scaling buffs (eg +0.1f for +10% defense). <br/>
		/// Multiply for debuffs (eg *0.9f for -10% defense). <br/>
		/// Decrease <see cref="StatModifier.Flat"/> to provide flat debuffs like ichor or betsys curse <br/>
		/// </summary>
		public StatModifier Defense = new();

		/// <summary>
		/// Flat defense reduction. Applies after <see cref="ScalingArmorPenetration"/>. <br/>
		/// Add to give bonus flat armor penetration. <br/>
		/// Do not subtract or multiply, consider altering <see cref="Defense"/> or <see cref="ScalingArmorPenetration"/> instead.
		/// <br/>
		/// Used by the <see cref="Projectile.ArmorPenetration"/>, <see cref="Item.ArmorPenetration"/> and <see cref="Player.GetTotalArmorPenetration"/> stats.
		/// </summary>
		public AddableFloat ArmorPenetration = new();

		/// <summary>
		/// Used to ignore a fraction of enemy armor. Applies before flat <see cref="ArmorPenetration"/>. <br/>
		/// Recommend only additive buffs, no multiplication or subtraction. <br/>
		/// <br/>
		/// At 1f, the attack will completely ignore all defense.
		/// </summary>
		public AddableFloat ScalingArmorPenetration = new();

		/// <summary>
		/// The conversion ratio between defense and damage reduction. Defaults to 0.5 for NPCs. Depends on difficulty for players. <br/>
		/// Increase to make defense more effective and armor penetration more important. <br/>
		/// <br/>
		/// Recommend only multiplication, no addition or subtraction. <br/>
		/// Not recommended to for buffs/debuffs. Use for gamemode tweaks, or if an enemy revolves very heavily around armor penetration.
		/// </summary>
		public MultipliableFloat DefenseEffectiveness = MultipliableFloat.One * 0.5f;

		/// <summary>
		/// Applied to the final damage (after defense) result when the hit is a crit. Defaults to +1f additive (+100% damage). <br/>
		///  <br/>
		/// Add to give hits extra crit damage (eg +0.1f for 10% extra crit damage (total +110% or 2.1 times base). <br/>
		/// Add to <see cref="StatModifier.Flat"/> to give crits extra flat damage. Use with caution as this extra damage will not be reduced by armor. <br/>
		/// Multiplication not recommended for buffs. Could be used to decrease the effectiveness of crits on an enemy without disabling completely. <br/>
		/// Use of <see cref="StatModifier.Base"/> also not recommended. <br/>
		/// </summary>
		public StatModifier CritDamage = new(2f, 1f);

		/// <summary>
		/// Applied to damage after defense and before <see cref="FinalDamage"/> when the hit is _not_ a crit. <br/>
		/// Effectively a compliment for <see cref="CritDamage"/>
		/// </summary>
		public StatModifier NonCritDamage = new();

		/// <summary>
		/// Applied to the final damage result. <br/>
		/// Used by <see cref="NPC.takenDamageMultiplier"/> to make enemies extra susceptible/resistant to damage. <br/>
		/// <br/>
		/// Multiply to make your enemy more susceptible or resistant to damage. <br/>
		/// Add to give 'bonus' post-mitigation damage. <br/>
		/// Add to <see cref="StatModifier.Base"/> to deal damage which ignores armor, but still respects scaling damage reductions or increases. <br/>
		/// Adding to <see cref="StatModifier.Flat"/> will ignore all reductions or increases to deal unconditional damage. Not recommended due to potential compatibility issues with enemy or player damage reduction effects, use <see cref="StatModifier.Base"/> instead.
		/// </summary>
		public StatModifier FinalDamage = new();

		/// <summary>
		/// Multiply to adjust the damage variation of the hit. <br/>
		/// Multiply by 0 to disable damage variation.<br/>
		/// Default damage variation is 15%, so maximum scale is ~6.67 <br/>
		/// Only affects hits where damage variation is enabled (which is most projectile/item/NPC damage)
		/// </summary>
		public MultipliableFloat DamageVariationScale = new();

		private int _damageLimit = int.MaxValue;
		/// <summary>
		/// Sets an inclusive upper bound on the final damage of the hit. <br/>
		/// Can be set by multiple mods, in which case the lowest limit will be used. <br/>
		/// Cannot be set to less than 1
		/// </summary>
		public void SetMaxDamage(int limit) => _damageLimit = Math.Min(_damageLimit, Math.Max(limit, 1));

		private bool? _critOverride = default;

		/// <summary>
		/// Disables <see cref="CritDamage"/> calculations, and clears <see cref="HitInfo.Crit"/> flag from the resulting hit.
		/// </summary>
		public void DisableCrit() => _critOverride = false;

		/// <summary>
		/// Sets the hit to be a crit. Does nothing if <see cref="DisableCrit"/> has been called
		/// </summary>
		public void SetCrit() => _critOverride ??= true;

		private bool _knockbackDisabled = false;

		/// <summary>
		/// Sets the hit to have no knockback, regardless of <see cref="Knockback"/> values. Set automatically for NPC with <see cref="NPC.knockBackResist"/> values of 0 for consistency.
		/// </summary>
		public void DisableKnockback() => _knockbackDisabled = true;

		/// <summary>
		/// Used by <see cref="NPC.onFire2"/> buff (additive) and <see cref="NPC.knockBackResist"/> (multiplicative) <br/>
		/// <br/>
		/// Recommend using <see cref="GlobalItem.ModifyWeaponKnockback"/> or <see cref="Player.GetKnockback"/> instead where possible.<br/>
		/// <br/>
		/// Add for knockback buffs. <br/>
		/// Multiply for knockback resistances. <br/>
		/// Subtraction not recommended. <br/>
		/// <br/>
		/// Knockback falloff still applies after this, so high knockback has diminishing returns. <br/>
		/// </summary>
		public StatModifier Knockback = new();

		/// <summary>
		/// Overrides the direction to apply knockback. <br/>
		/// Will not affect <see cref="HitDirection"/>, only the final <see cref="HitInfo.HitDirection"/><br/>
		/// If set by multiple mods, only the last override will apply. <br/>
		/// Intended for use by flails, or other projectiles which need to hit the NPC away from the player, even when striking from behind.
		/// </summary>
		public int? HitDirectionOverride { private get; set; } = default;

		private bool _instantKill = default;
		/// <summary>
		/// Set to make the hit instantly kill the target, dealing as much damage as necessary. <br/>
		/// Combat text will not be shown.
		/// </summary>
		public void SetInstantKill() => _instantKill = true;

		private bool _combatTextHidden = default;
		/// <summary>
		/// Set to hide the damage number popup for this hit.
		/// </summary>
		public void HideCombatText() => _combatTextHidden = true;

		public delegate void HitInfoModifier(ref HitInfo info);
		/// <summary>
		/// Use with caution and consider other alternatives first.<br/>
		/// Can be used to register a callback to freely modify the <see cref="HitInfo"/> produced by <see cref="ToHitInfo"/> before it is returned<br/>
		/// If multiple mods register different callbacks which modify the hit info in different ways the results could be a mess!
		/// </summary>
		public event HitInfoModifier ModifyHitInfo = null;

		public HitModifiers() { }

		public readonly int GetDamage(float baseDamage, bool crit, bool damageVariation = false, float luck = 0f)
		{
			crit = _critOverride ?? crit;
			if (SuperArmor) {
				float dmg = 1;
				if (crit)
					dmg *= CritDamage.Additive * CritDamage.Multiplicative;

				return Math.Clamp((int)dmg, 1, Math.Min(_damageLimit, 4));
			}

			float damage = SourceDamage.ApplyTo(baseDamage);
			damage += FlatBonusDamage.Value + ScalingBonusDamage.Value * damage;
			damage *= TargetDamageMultiplier.Value;

			int variationPercent = Utils.Clamp((int)Math.Round(Main.DefaultDamageVariationPercent * DamageVariationScale.Value), 0, 100);
			if (damageVariation && variationPercent > 0)
				damage = Main.DamageVar(damage, variationPercent, luck);

			float defense = Math.Max(Defense.ApplyTo(0), 0);
			float armorPenetration = defense * Math.Clamp(ScalingArmorPenetration.Value, 0, 1) + ArmorPenetration.Value;
			defense = Math.Max(defense - armorPenetration, 0);

			float damageReduction = defense * DefenseEffectiveness.Value;
			damage = Math.Max(damage - damageReduction, 1);

			damage = (crit ? CritDamage : NonCritDamage).ApplyTo(damage);

			return Math.Clamp((int)FinalDamage.ApplyTo(damage), 1, _damageLimit);
		}

		public readonly float GetKnockback(float baseKnockback) => _knockbackDisabled ? 0 : Math.Max(Knockback.ApplyTo(baseKnockback), 0);

		public HitInfo ToHitInfo(float baseDamage, bool crit, float baseKnockback, bool damageVariation = false, float luck = 0f)
		{
			var hitInfo = new HitInfo() {
				DamageType = DamageType ?? DamageClass.Default, // just in case
				SourceDamage = Math.Max((int)SourceDamage.ApplyTo(baseDamage), 1),
				Damage = _instantKill ? 1 : GetDamage(baseDamage, crit, damageVariation, luck),
				Crit = _critOverride ?? crit,
				Knockback = GetKnockback(baseKnockback),
				HitDirection = HitDirectionOverride ?? HitDirection,
				InstantKill = _instantKill,
				HideCombatText = _combatTextHidden
			};

			// Good for one use only just to be safe. Structs can be copied so this doesn't prevent misuse, but one can hope.
			ModifyHitInfo?.Invoke(ref hitInfo);
			ModifyHitInfo = null;
			return hitInfo;
		}
	}

	/// <summary>
	/// Represents a finalized damage calculation for damage about to be applied to an NPC. This is the result of all modifications done previously in a <see cref="HitModifiers"/> provided to the ModifyHit hooks.
	/// </summary>
	public struct HitInfo
	{
		/// <summary>
		/// The DamageType of the hit.
		/// </summary>
		public DamageClass DamageType = DamageClass.Default;

		private int _sourceDamage = 1;
		/// <summary>
		/// The amount of damage 'dealt' to the NPC, before incoming damage multipliers, armor, critical strikes etc.<br/>
		/// Use this to trigger effects which scale based on damage dealt, and also deal damage.<br/>
		/// Cannot be set to less than 1.<br/>
		/// <br/>
		/// Using this instead of <see cref="Damage"/> can prevent diminishing returns from NPC defense, double crits, or excessively strong effects if the NPC has a vulnerability to the weapon/projectile (like vampires and stakes).
		/// <br/>
		/// Used by vanilla for dryad ward retaliation, and many sword on-hit projectiles like volcano and beekeeper
		/// </summary>
		public int SourceDamage {
			readonly get => _sourceDamage;
			set => _sourceDamage = Math.Max(value, 1);
		}

		private int _damage = 1;
		/// <summary>
		/// The amount of damage received by the NPC. How much life the NPC will lose. <br/>
		/// Is NOT capped at the NPC's current life. <br/>
		/// Will be 1 if <see cref="InstantKill"/> is set. <br/>
		/// Cannot be set to less than 1.
		/// </summary>
		public int Damage {
			readonly get => _damage;
			set => _damage = Math.Max(value, 1);
		}

		/// <summary>
		/// Whether or not the hit is a crit
		/// </summary>
		public bool Crit = false;

		/// <summary>
		/// The direction to apply knockback in.
		/// </summary>
		public int HitDirection = 0;

		/// <summary>
		/// The amount of knockback to apply. Should always be >= 0. <br/>
		/// Note that <see cref="NPC.StrikeNPC(HitInfo, bool, bool)"/> has a staggered knockback falloff, and that critical strikes automatically get extra 40% knockback in excess of this value.
		/// </summary>
		public float Knockback = 0;

		/// <summary>
		/// If true, as much damage as necessary will be dealt, and damage number popups will not be shown for this hit. <br/>
		/// Has no effect if the NPC is <see cref="NPC.immortal"/>
		/// </summary>
		public bool InstantKill = false;

		/// <summary>
		/// If true, damage number popups will not be shown for this hit.
		/// </summary>
		public bool HideCombatText = false;

		public HitInfo() { }
	}
}
