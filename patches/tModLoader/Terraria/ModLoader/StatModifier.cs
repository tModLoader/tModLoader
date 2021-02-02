namespace Terraria.ModLoader
{
	public struct StatModifier
	{
		public static readonly StatModifier One = new StatModifier(1f, 1f);

		public float Additive { get; private set; }
		public float Multiplicative { get; private set; }

		public StatModifier(float additive, float multiplicative = 1) {
			Additive = additive;
			Multiplicative = multiplicative;
		}

		public override bool Equals(object obj) {
			if (!(obj is StatModifier))
				return false;

			var m = (StatModifier)obj;
			return Additive == m.Additive &&
				   Multiplicative == m.Multiplicative;
		}

		public override int GetHashCode() {
			int hashCode = 1713062080;
			hashCode = hashCode * -1521134295 + Additive.GetHashCode();
			hashCode = hashCode * -1521134295 + Multiplicative.GetHashCode();
			return hashCode;
		}

		public static StatModifier operator +(StatModifier m, float add) =>
			new StatModifier(m.Additive + add, m.Multiplicative);

		public static StatModifier operator -(StatModifier m, float sub) =>
			new StatModifier(m.Additive - sub, m.Multiplicative);

		public static StatModifier operator *(StatModifier m, float mul) =>
			new StatModifier(m.Additive, m.Multiplicative * mul);

		public static StatModifier operator /(StatModifier m, float div) =>
			new StatModifier(m.Additive, m.Multiplicative / div);

		public static StatModifier operator +(float add, StatModifier m) => m + add;
		public static StatModifier operator *(float mul, StatModifier m) => m * mul;

		public static bool operator ==(StatModifier m1, StatModifier m2) =>
			m1.Additive == m2.Additive && m1.Multiplicative == m2.Multiplicative;

		public static bool operator !=(StatModifier m1, StatModifier m2) =>
			m1.Additive != m2.Additive || m1.Multiplicative != m2.Multiplicative;

		public static implicit operator float(StatModifier m) =>
			m.Additive * m.Multiplicative;

		public static implicit operator int(StatModifier m) =>
			(int)(m.Additive * m.Multiplicative);

		public StatModifier CombineWith(StatModifier m) =>
			new StatModifier(Additive + m.Additive - 1, Multiplicative * m.Multiplicative);

		public StatModifier Scale(float scale) =>
			new StatModifier(1 + (Additive - 1) * scale, 1 + (Multiplicative - 1) * scale);
	}
}
