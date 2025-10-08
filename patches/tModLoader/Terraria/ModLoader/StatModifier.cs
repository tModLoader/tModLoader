namespace Terraria.ModLoader;

public struct StatModifier
{
	public static readonly StatModifier Default = new();

	/// <summary>
	/// An offset to the base value of the stat. Directly applied to the base stat before multipliers are applied.
	/// </summary>
	public float Base = 0f;

	/// <summary>
	/// The combination of all additive multipliers. Starts at 1
	/// </summary>
	public float Additive { get; private set; } = 1f;

	/// <summary>
	/// The combination of all multiplicative multipliers. Starts at 1. Applies 'after' all additive bonuses have been accumulated.
	/// </summary>
	public float Multiplicative { get; private set; } = 1f;

	/// <summary>
	/// Increase to the final value of the stat. Directly added to the stat after multipliers are applied.
	/// </summary>
	public float Flat = 0f;

	public StatModifier() { }

	public StatModifier(float additive, float multiplicative, float flat = 0f, float @base = 0f)
	{
		Additive = additive;
		Multiplicative = multiplicative;
		Flat = flat;
		Base = @base;
	}

	public override bool Equals(object obj)
	{
		if (obj is not StatModifier m)
			return false;

		return this == m;
	}

	public override int GetHashCode()
	{
		int hashCode = 1713062080;
		hashCode = hashCode * -1521134295 + Additive.GetHashCode();
		hashCode = hashCode * -1521134295 + Multiplicative.GetHashCode();
		hashCode = hashCode * -1521134295 + Flat.GetHashCode();
		hashCode = hashCode * -1521134295 + Base.GetHashCode();
		return hashCode;
	}

	/// <summary>
	/// By using the add operator, the supplied additive modifier is combined with the existing modifiers. For example, adding 0.12f would be equivalent to a typical 12% damage boost. For 99% of effects used in the game, this approach is used.
	/// </summary>
	/// <param name="m"></param>
	/// <param name="add">The additive modifier to add, where 0.01f is equivalent to 1%</param>
	/// <returns></returns>
	public static StatModifier operator +(StatModifier m, float add)
		=> new StatModifier(m.Additive + add, m.Multiplicative, m.Flat, m.Base);

	/// <summary>
	/// By using the subtract operator, the supplied subtractive modifier is combined with the existing modifiers. For example, subtracting 0.12f would be equivalent to a typical 12% damage decrease. For 99% of effects used in the game, this approach is used.
	/// </summary>
	/// <param name="m"></param>
	/// <param name="sub">The additive modifier to subtract, where 0.01f is equivalent to 1%</param>
	/// <returns></returns>
	public static StatModifier operator -(StatModifier m, float sub)
		=> new StatModifier(m.Additive - sub, m.Multiplicative, m.Flat, m.Base);

	/// <summary>
	/// The multiply operator applies a multiplicative effect to the resulting multiplicative modifier. This effect is very rarely used, typical effects use the add operator.
	/// </summary>
	/// <param name="m"></param>
	/// <param name="mul">The factor by which the multiplicative modifier is scaled</param>
	/// <returns></returns>
	public static StatModifier operator *(StatModifier m, float mul)
		=> new StatModifier(m.Additive, m.Multiplicative * mul, m.Flat, m.Base);

	public static StatModifier operator /(StatModifier m, float div)
		=> new StatModifier(m.Additive, m.Multiplicative / div, m.Flat, m.Base);

	public static StatModifier operator +(float add, StatModifier m)
		=> m + add;

	public static StatModifier operator *(float mul, StatModifier m)
		=> m * mul;

	public static bool operator ==(StatModifier m1, StatModifier m2)
		=> m1.Additive == m2.Additive && m1.Multiplicative == m2.Multiplicative && m1.Flat == m2.Flat && m1.Base == m2.Base;

	public static bool operator !=(StatModifier m1, StatModifier m2)
		=> m1.Additive != m2.Additive || m1.Multiplicative != m2.Multiplicative || m1.Flat != m2.Flat || m1.Base != m2.Base;

	/// <summary>
	/// Use this to apply the modifiers of this <see cref="StatModifier"/> to the <paramref name="baseValue"/>. You should assign
	/// the value passed in to the return result. For example:
	/// <para><br><c>damage = CritDamage.ApplyTo(damage)</c></br></para>
	/// <br></br>could be used to apply a crit damage modifier to a base damage value
	/// <para/> Note that when using this to calculate the final damage of a <see cref="DamageClass"/> make sure to use <see cref="Player.GetTotalDamage(DamageClass)"/> not <see cref="Player.GetDamage(DamageClass)"/> to account for inherited damage modifiers such as Generic damage.
	/// </summary>
	/// <remarks>For help understanding the meanings of the applied values please make note of documentation for:
	/// <list type="bullet">
	/// <item><description><see cref="Base"/></description></item>
	/// <item><description><see cref="Additive"/></description></item>
	/// <item><description><see cref="Multiplicative"/></description></item>
	/// <item><description><see cref="Flat"/></description></item>
	/// </list>
	/// The order of operations of the modifiers are as follows:
	/// <list type="number">
	/// <item><description>The <paramref name="baseValue"/> is added to <see cref="Base"/></description></item>
	/// <item><description>That result is multiplied by <see cref="Additive"/></description></item>
	/// <item><description>The previous result is then multiplied by <see cref="Multiplicative"/></description></item>
	/// <item><description>Finally, <see cref="Flat"/> as added to the result of all previous calculations</description></item>
	/// </list>
	/// </remarks>
	/// <param name="baseValue">The starting value to apply modifiers to</param>
	/// <returns>The result of <paramref name="baseValue"/> after all modifiers are applied</returns>
	public float ApplyTo(float baseValue) =>
		(baseValue + Base) * Additive * Multiplicative + Flat;

	/// <summary>
	/// Combines the components of two StatModifiers. Typically used to apply the effects of ammo-specific StatModifier to the DamageClass StatModifier values.
	/// </summary>
	public StatModifier CombineWith(StatModifier m)
		=> new StatModifier(Additive + m.Additive - 1, Multiplicative * m.Multiplicative, Flat + m.Flat, Base + m.Base);

	/// <summary>
	/// Scales all components of this StatModifier for the purposes of applying damage class modifier inheritance.<para/>This is <b>NOT</b> intended for typical modding usage, if you are looking to increase this stat by some percentage, use the addition (<c>+</c>) operator.
	/// </summary>
	public StatModifier Scale(float scale)
		=> new StatModifier(1 + (Additive - 1) * scale, 1 + (Multiplicative - 1) * scale, Flat * scale, Base * scale);

	/// <summary>
	/// Calculates a base value from a provided adjusted value, undoing <see cref="ApplyTo(float)"/>.
	/// </summary>
	/// <param name="currentValue">The value to remove modifiers from</param>
	/// <returns></returns>
	public float Undo(float currentValue)
		=> ((currentValue - Flat) / (Multiplicative * Additive)) - Base;

	// public override string ToString() => $"(baseValue + Base) * Additive * Multiplicative + Flat => (100 + {Base}) * {Additive} * {Multiplicative} + {Flat} => {ApplyTo(100)}";
}
