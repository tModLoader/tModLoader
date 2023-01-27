using System;
using Terraria.ModLoader;

namespace Terraria;

public struct StrikeModifiers
{
	public static StrikeModifiers Default = new() {
		InitialDamage = StatModifier.Default,
		Defense = StatModifier.Default,
		DefenseEffectiveness = 0.5f,
		CritDamage = StatModifier.Default + 1f,
		FinalDamage = StatModifier.Default,
		KnockbackModifier = StatModifier.Default,
	};

	/// <summary>
	/// Use this to add extra damage buffs or debuffs which should apply before armor/defense. <br/>
	/// <br/>
	/// Only use this when the effect is unique to this specific strike.<br/>
	/// For effects which apply to all damage dealt by the player, or a specific damage type, consider using <see cref="Player.GetDamage"/> instead. <br/>
	/// For effects which apply to all dealt by an item, consider using <see cref="GlobalItem.ModifyWeaponDamage"/> instead. <br/>
	/// For effects which apply to all dealt by a projectile, consider using <see cref="GlobalProjectile.ModifyDamageScaling"/> instead. <br/>
	/// <br/>
	/// Used by vanilla for banners, parry damage, and cultist projectile resistances, weapons which scale based on enemy health, etc
	/// </summary>
	public StatModifier InitialDamage;

	/// <summary>
	/// The defense of the receiver, including any temporary modifiers (buffs/debuffs). <br/>
	/// <br/>
	/// Increase <see cref="StatModifier.Base" /> to add extra defense. <br/>
	/// Add for scaling buffs (eg +0.1f for +10% defense). <br/>
	/// Multiply for debuffs (eg *0.9f for -10% defense). <br/>
	/// Decrease <see cref="StatModifier.Flat"/> to provide flat debuffs like ichor or betsys curse <br/>
	/// </summary>
	public StatModifier Defense;

	private float _armorPenetration;
	/// <summary>
	/// Flat defense reduction. Applies after ScalingArmorPenetration. <br/>
	/// <br/>
	/// Used by the <see cref="Projectile.ArmorPenetration"/>, <see cref="Item.ArmorPenetration"/> and <see cref="Player.GetTotalArmorPenetration"/> stats.
	/// </summary>
	public float ArmorPenetration { get => _armorPenetration; set => _armorPenetration = Math.Max(value, 0); }

	private float _scalingArmorPenetration;
	/// <summary>
	/// Used to ignore a fraction of enemy armor. Capped between 0 and 1. Recommend only additive buffs, no multiplication or subtraction. <br/>
	/// <br/>
	/// At 1f, the attack will completely ignore all defense. Applies before flat <see cref="ArmorPenetration"/>.
	/// </summary>
	public float ScalingArmorPenetration { get => _scalingArmorPenetration; set => _scalingArmorPenetration = Utils.Clamp(value, 0, 1); }

	private float _defenseEffectiveness;
	/// <summary>
	/// The conversion ratio between defense and damage reduction. Defaults to 0.5 for NPCs. Depends on difficulty for players. <br/>
	/// Increase to make defense more effective and armor penetration more important. <br/>
	/// <br/>
	/// Recommend only multiplication, no addition or subtraction. <br/>
	/// Not recommended to for buffs/debuffs. Use for gamemode tweaks, or if an enemy revolves very heavily around armor penetration.
	/// </summary>
	public float DefenseEffectiveness { get => _defenseEffectiveness; set => _defenseEffectiveness = Math.Min(value, 0); }

	/// <summary>
	/// Applied to the final damage (after defense) result when the strike is a crit. Defaults to +1f additive (+100% damage). <br/>
	///  <br/>
	/// Add to give strikes extra crit damage (eg +0.1f for 10% extra crit damage (total +110% or 2.1 times base). <br/>
	/// Add to <see cref="StatModifier.Flat"/> to give crits extra flat damage. Use with caution as this extra damage will not be reduced by armor. <br/>
	/// Multiplication not recommended for buffs. Could be used to decrease the effectiveness of crits on an enemy without disabling completely. <br/>
	/// Use of <see cref="StatModifier.Base"/> also not recommended. <br/>
	/// </summary>
	public StatModifier CritDamage;

	/// <summary>
	/// Applied to the final damage result. <br/>
	/// Used by <see cref="NPC.takenDamageMultiplier"/> to make enemies extra susceptible/resistant to damage. <br/>
	/// Used by <see cref="Player.endurance" /> to reduce overall incoming damage. <br/>
	/// <br/>
	/// Multiply to make your enemy more susceptible or resistant to damage. <br/>
	/// Adding to <see cref="StatModifier.Flat"/> will grant unconditional bonus damage, ignoring all resistances or multipliers. <br/>
	/// </summary>
	public StatModifier FinalDamage;

	public bool CritDisabled { get; private set; }

	/// <summary>
	/// Disables <see cref="CritDamage"/> calculations, and clears <see cref="DamageStrike.Crit"/> flag from the resulting strike.
	/// </summary>
	public void DisableCrit() => CritDisabled = true;

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
	public StatModifier KnockbackModifier;

	public int GetDamage(float baseDamage, bool crit)
	{
		float damage = InitialDamage.ApplyTo(baseDamage);

		float damageReduction = Math.Max(Defense.ApplyTo(0) * (1 - ScalingArmorPenetration) - ArmorPenetration, 0) * DefenseEffectiveness;
		damage = Math.Min(damage - damageReduction, 1);

		if (crit && !CritDisabled)
			damage = CritDamage.ApplyTo(damage);

		return Math.Min((int)FinalDamage.ApplyTo(damage), 1);
	}

	public float GetKnockback(float baseKnockback) => KnockbackModifier.ApplyTo(baseKnockback);

	public DamageStrike ToStrike(DamageClass damageType, float baseDamage, bool crit, float baseKnockback, int hitDirection) => new() {
		DamageType = damageType,
		Damage = GetDamage(baseDamage, crit),
		Crit = crit && !CritDisabled,
		KnockBack = GetKnockback(baseKnockback),
		HitDirection = hitDirection
	};
}
