namespace Terraria.ModLoader
{
	public struct StatModifier
	{
		public static readonly StatModifier Default = new StatModifier(1f, 1f, 0);

		public float Additive { get; private set; }
		public float Multiplicative { get; private set; }
		public float Flat { get; set; } // this can be set separately in order to not have to make 15 new methods

		public StatModifier(float additive, float multiplicative = 1, float flat = 0) {
			Additive = additive;
			Multiplicative = multiplicative;
			Flat = flat;
		}

		public override bool Equals(object obj) {
			if (!(obj is StatModifier))
				return false;

			var m = (StatModifier)obj;
			return Additive == m.Additive &&
				   Multiplicative == m.Multiplicative &&
				   Flat == m.Flat;
		}

		public override int GetHashCode() {
			int hashCode = 1713062080;
			hashCode = hashCode * -1521134295 + Additive.GetHashCode();
			hashCode = hashCode * -1521134295 + Multiplicative.GetHashCode();
			return hashCode;
		}

		/// <summary>
		/// By using the add operator, the supplied additive modifier is combined with the existing modifiers. For example, adding 0.12f would be equivalent to a typical 12% damage boost. For 99% of effects used in the game, this approach is used.
		/// </summary>
		/// <param name="m"></param>
		/// <param name="add">The additive modifier to add, where 0.01f is equivalent to 1%</param>
		/// <returns></returns>
		public static StatModifier operator +(StatModifier m, float add)
			=> new StatModifier(m.Additive + add, m.Multiplicative, m.Flat);

		/// <summary>
		/// By using the subtract operator, the supplied subtractive modifier is combined with the existing modifiers. For example, subtracting 0.12f would be equivalent to a typical 12% damage decrease. For 99% of effects used in the game, this approach is used.
		/// </summary>
		/// <param name="m"></param>
		/// <param name="sub">The additive modifier to subtract, where 0.01f is equivalent to 1%</param>
		/// <returns></returns>
		public static StatModifier operator -(StatModifier m, float sub)
			=> new StatModifier(m.Additive - sub, m.Multiplicative, m.Flat);

		/// <summary>
		/// The multiply operator applies a multiplicative effect to the resulting multiplicative modifier. This effect is very rarely used, typical effects use the add operator.
		/// </summary>
		/// <param name="m"></param>
		/// <param name="mul">The factor by which the multiplicative modifier is scaled</param>
		/// <returns></returns>
		public static StatModifier operator *(StatModifier m, float mul)
			=> new StatModifier(m.Additive, m.Multiplicative * mul, m.Flat);

		public static StatModifier operator /(StatModifier m, float div)
			=> new StatModifier(m.Additive, m.Multiplicative / div, m.Flat);

		public static StatModifier operator +(float add, StatModifier m)
			=> m + add;

		public static StatModifier operator *(float mul, StatModifier m)
			=> m * mul;

		public static bool operator ==(StatModifier m1, StatModifier m2)
			=> m1.Additive == m2.Additive && m1.Multiplicative == m2.Multiplicative && m1.Flat == m2.Flat;

		public static bool operator !=(StatModifier m1, StatModifier m2)
			=> m1.Additive != m2.Additive || m1.Multiplicative != m2.Multiplicative || m1.Flat != m2.Flat;

		public static implicit operator float(StatModifier m)
			=> m.Additive * m.Multiplicative;

		public static explicit operator int(StatModifier m)
			=> (int)(float)m;

		public StatModifier CombineWith(StatModifier m)
			=> new StatModifier(Additive + m.Additive - 1, Multiplicative * m.Multiplicative, Flat + m.Flat);

		public StatModifier Scale(float scale)
			=> new StatModifier(1 + (Additive - 1) * scale, 1 + (Multiplicative - 1) * scale, Flat * scale);
	}
}
