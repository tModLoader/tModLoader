using System;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terraria;

public partial class Player
{
	public struct HurtModifiers
	{
		/// <summary>
		/// The source of the strike. <br/>
		/// Use <see cref="PlayerDeathReason.TryGetCausingEntity"/> to get the source of the strike (only safe to do when the target is the local player).
		/// </summary>
		public PlayerDeathReason DamageSource { get; init; } = default;

		/// <summary>
		/// Whether or not this strike came from another player. <br/>
		/// Note that PvP support in Terraria is rudimentary and inconsistent, so careful research and testing may be required.
		/// </summary>
		public bool PvP { get; init; } = default;

		/// <summary>
		/// The <see cref="ImmunityCooldownID"/> of the strike
		/// </summary>
		public int CooldownCounter { get; init; } = ImmunityCooldownID.General;

		/// <summary>
		/// Whether or not this strike was dodgeable.
		/// </summary>
		public bool Dodgeable { get; init; } = true;

		/// <summary>
		/// The direction to apply knockback. If 0, no knockback will be applied. <br/>
		/// Could potentially be used for directional resistances. <br/>
		/// Can be overridden by <see cref="HitDirectionOverride"/>
		/// </summary>
		public int HitDirection { get; init; } = default;

		/// <summary>
		/// Use this to enhance or scale the base damage of the NPC/projectile/hit. <br/>
		/// <br/>
		/// Not used by vanilla due to lack of proper pvp support. <br/>
		/// Use cases are similar to <see cref="NPC.HitModifiers.SourceDamage"/> <br/>
		/// </summary>
		public StatModifier SourceDamage = new();

		/// <summary>
		/// Use this to reduce damage from certain sources before applying defense. <br/>
		/// Used by vanilla for coldResist and banner damage reduction.
		/// </summary>
		public MultipliableFloat IncomingDamageMultiplier = new();

		/// <summary>
		/// Applied to the final damage result. <br/>
		/// Used by <see cref="Player.endurance" /> to reduce overall incoming damage. <br/>
		/// <br/>
		/// Multiply to grant damage reduction buffs (eg *0.9f for -10% damage taken). <br/>
		/// Add to <see cref="StatModifier.Base"/> to deal damage which ignores armor, but still respects scaling damage reduction like endurance or paladins shield. <br/>
		/// Adding to <see cref="StatModifier.Flat"/> will ignore all reductions or increases to deal unconditional damage. Not recommended due to potential compatibility issues with accessories like paladin's shield, use <see cref="StatModifier.Base"/> instead.
		/// </summary>
		public StatModifier FinalDamage = new();

		/// <summary>
		/// Flat defense reduction. Applies after <see cref="ScalingArmorPenetration"/>. <br/>
		/// <br/>
		/// Consider supplying armorPenetration as an argument to <see cref="Player.Hurt(PlayerDeathReason, int, int, bool, bool, int, bool, float, float, float)"/> instead if possible.<br/>
		/// </summary>
		public AddableFloat ArmorPenetration = new();

		/// <summary>
		/// Used to ignore a fraction of player defense. Applies before flat <see cref="ArmorPenetration"/>. <br/>
		/// <br/>
		/// At 1f, the attack will completely ignore all defense.
		/// </summary>
		public AddableFloat ScalingArmorPenetration = new();

		/// <summary>
		/// Modifiers to apply to the knockback.
		/// Add to <see cref="StatModifier.Base"/> to increase the knockback of the strike.
		/// Multiply to decrease or increase overall knockback susceptibility.
		/// </summary>
		public StatModifier Knockback = new();

		/// <summary>
		/// Overrides the direction to apply knockback. <br/>
		/// Will not affect <see cref="HitDirection"/>, only the final <see cref="HurtInfo.HitDirection"/><br/>
		/// If set by multiple mods, only the last override will apply. <br/>
		/// Intended for use by attacks which want to hit the player towards the source of the attack.
		/// </summary>
		public int? HitDirectionOverride { private get; set; } = default;

		/// <summary>
		/// Use this to reduce the effectiveness of <see cref="Player.noKnockback"/> (cobalt shield accessory). <br/>
		/// Eg, *0.8f to reduce knockback to 20% when cobalt shield is equipped. <br/>
		/// Defaults to 1f (knockback immunity is 100% effective by default). <br/>
		/// Used by vanilla for the ogre's launching attack. <br/>
		/// </summary>
		public MultipliableFloat KnockbackImmunityEffectiveness = new();

		private bool _dustDisabled = default;
		/// <summary>
		/// Prevents dust from spawning
		/// </summary>
		public void DisableDust() => _dustDisabled = true;

		private bool _soundDisabled = default;
		/// <summary>
		/// Prevents the hurt sound from playing
		/// </summary>
		public void DisableSound() => _soundDisabled = true;

		public HurtModifiers() { }

		public float GetDamage(float baseDamage, float defense, float defenseEffectiveness)
		{
			float damage = SourceDamage.ApplyTo(baseDamage) * IncomingDamageMultiplier.Value;
			float armorPenetration = defense * Math.Clamp(ScalingArmorPenetration.Value, 0, 1) + ArmorPenetration.Value;
			defense = Math.Max(defense - armorPenetration, 0);

			float damageReduction = defense * defenseEffectiveness;
			damage = Math.Max(damage - damageReduction, 1);

			return Math.Max((int)FinalDamage.ApplyTo(damage), 1);
		}

		public float GetKnockback(float baseKnockback, bool knockbackImmune)
		{
			float knockback = Math.Max(Knockback.ApplyTo(baseKnockback), 0f);
			if (knockbackImmune)
				knockback *= (1 - Math.Clamp(KnockbackImmunityEffectiveness.Value, 0, 1));

			return knockback;
		}

		public HurtInfo ToHurtInfo(int damage, int defense, float defenseEffectiveness, float knockback, bool knockbackImmune) => new() {
			DamageSource = DamageSource,
			PvP = PvP,
			CooldownCounter = CooldownCounter,
			Dodgeable = Dodgeable,
			HitDirection = HitDirectionOverride ?? HitDirection,
			SourceDamage = (int)SourceDamage.ApplyTo(damage),
			Damage = (int)GetDamage(damage, defense, defenseEffectiveness),
			Knockback = GetKnockback(knockback, knockbackImmune),
		};
	}

	public struct HurtInfo
	{
		/// <summary>
		/// <inheritdoc cref="HurtModifiers.DamageSource"/>
		/// </summary>
		public PlayerDeathReason DamageSource;

		/// <summary>
		/// <inheritdoc cref="HurtModifiers.PvP"/>
		/// </summary>
		public bool PvP;

		/// <summary>
		/// <inheritdoc cref="HurtModifiers.CooldownCounter"/>
		/// </summary>
		public int CooldownCounter;

		/// <summary>
		/// <inheritdoc cref="HurtModifiers.Dodgeable"/>
		/// </summary>
		public bool Dodgeable;

		/// <summary>
		/// The amount of damage 'dealt' to the player, before incoming damage multipliers, armor, damage reduction.<br/>
		/// Use this to trigger effects which scale based on how 'hard' the player was hit rather than how much life was lost.<br/>
		/// <br/>
		/// Using this instead of <see cref="Damage"/> can prevent diminishing returns damage mitigation, when adding beneficial effects like retaliatory damage.
		/// </summary>
		public int SourceDamage;

		/// <summary>
		/// The amount of damage received by the player. How much life the player will lose. <br/>
		/// Is NOT capped at the player's current life.
		/// </summary>
		public int Damage;

		/// <summary>
		/// <inheritdoc cref="HurtModifiers.HitDirection"/>
		/// </summary>
		public int HitDirection;

		/// <summary>
		/// The amount of knockback to apply. Should always be >= 0.
		/// </summary>
		public float Knockback;

		/// <summary>
		/// If true, dust will not spawn
		/// </summary>
		public bool DustDisabled;

		/// <summary>
		/// If true, sound will not play
		/// </summary>
		public bool SoundDisabled;
	}

	public struct DefenseStat
	{
		public static DefenseStat Default = new();

		public int Positive { get; private set; } = 0;
		public int Negative { get; private set; } = 0;

		public AddableFloat AdditiveBonus = new();
		public MultipliableFloat FinalMultiplier = new();

		public DefenseStat() { }

		public static DefenseStat operator +(DefenseStat stat, int add) => add < 0 ? stat - (-add) : stat with { Positive = stat.Positive + add };
		public static DefenseStat operator -(DefenseStat stat, int sub) => sub < 0 ? stat + (-sub) : stat with { Negative = stat.Negative + sub };

		public static DefenseStat operator ++(DefenseStat stat) => stat+1;
		public static DefenseStat operator --(DefenseStat stat) => stat-1;

		public static DefenseStat operator *(DefenseStat stat, float mult) => stat with { FinalMultiplier = stat.FinalMultiplier * mult};
		public static DefenseStat operator /(DefenseStat stat, float div) => stat with { FinalMultiplier = stat.FinalMultiplier / div};

		public static implicit operator int(DefenseStat stat) => Math.Max((int)Math.Round((stat.Positive * (1 + stat.AdditiveBonus.Value) - stat.Negative) * stat.FinalMultiplier.Value), 0);

		public override string ToString() => ((int)this).ToString();
	}
}