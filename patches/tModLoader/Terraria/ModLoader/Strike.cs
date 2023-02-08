namespace Terraria.ModLoader;

public struct Strike
{
	/// <summary>
	/// The DamageType of the strike. Should not be null, but it may be a good idea to check just in-case a mod does something bad.
	/// </summary>
	public DamageClass DamageType;

	/// <summary>
	/// The amount of damage 'dealt' to the NPC, before incoming damage multipliers, armor, critical strikes etc.<br/>
	/// Use this to trigger effects which scale based on damage dealt, and also deal damage.<br/>
	/// <br/>
	/// Using this instead of <see cref="Damage"/> can prevent diminishing returns from NPC defense, double crits, or excessively strong effects if the NPC has a vulnerability to the weapon/projectile (like vampires and stakes).
	/// <br/>
	/// Used by vanilla for dryad ward retaliation, and many sword on-hit projectiles like volcano and beekeepr
	/// </summary>
	public int SourceDamage = 0;

	/// <summary>
	/// The amount of damage received by the NPC. How much life the NPC will lose. <br/>
	/// Is NOT capped at the NPC's current life. <br/>
	/// Will be 0 if <see cref="InstantKill"/> is set. <br/>
	/// </summary>
	public int Damage = 0;

	/// <summary>
	/// Whether or not the strike is a crit
	/// </summary>
	public bool Crit = false;

	/// <summary>
	/// The direction to apply knockback in.
	/// </summary>
	public int HitDirection = 0;

	/// <summary>
	/// The amount of knockback to apply. Should always be >= 0. <br/>
	/// Note that <see cref="NPC.StrikeNPC"/> has a staggered knockback falloff, and that critical strikes automatically get extra 40% knockback in excess of this value.
	/// </summary>
	public float KnockBack = 0;

	/// <summary>
	/// If true, as much damage as necessary will be dealt, and damage number popups will not be shown for this hit. <br/>
	/// Has no effect if the NPC is <see cref="NPC.immortal"/>
	/// </summary>
	public bool InstantKill = false;

	/// <summary>
	/// If true, damage number popups will not be shown for this hit.
	/// </summary>
	public bool HideCombatText = false;

	public Strike()
	{
		DamageType = DamageClass.Default;
	}

}
