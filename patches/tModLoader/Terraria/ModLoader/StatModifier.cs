namespace Terraria.ModLoader
{
	public struct StatModifier
	{
		public static readonly StatModifier One = new StatModifier(1f, 1f);

		public float additive;
		public float multiplicative;

		public StatModifier(float additive, float multiplicative = 1) {
			this.additive = additive;
			this.multiplicative = multiplicative;
		}

		public override bool Equals(object obj) {
			if (!(obj is StatModifier))
				return false;

			var m = (StatModifier)obj;
			return additive == m.additive &&
				   multiplicative == m.multiplicative;
		}

		public override int GetHashCode() {
			int hashCode = 1713062080;
			hashCode = hashCode * -1521134295 + additive.GetHashCode();
			hashCode = hashCode * -1521134295 + multiplicative.GetHashCode();
			return hashCode;
		}

		public static StatModifier operator +(StatModifier m, float add) =>
			new StatModifier(m.additive + add, m.multiplicative);

		public static StatModifier operator -(StatModifier m, float sub) =>
			new StatModifier(m.additive - sub, m.multiplicative);

		public static StatModifier operator *(StatModifier m, float mul) =>
			new StatModifier(m.additive, m.multiplicative * mul);

		public static StatModifier operator /(StatModifier m, float div) =>
			new StatModifier(m.additive, m.multiplicative / div);

		public static StatModifier operator +(float add, StatModifier m) => m + add;
		public static StatModifier operator *(float mul, StatModifier m) => m * mul;

		public static bool operator ==(StatModifier m1, StatModifier m2) =>
			m1.additive == m2.additive && m1.multiplicative == m2.multiplicative;

		public static bool operator !=(StatModifier m1, StatModifier m2) =>
			m1.additive != m2.additive || m1.multiplicative != m2.multiplicative;

		public static implicit operator float(StatModifier m) =>
			m.additive * m.multiplicative;

		public static implicit operator int(StatModifier m) =>
			(int)(m.additive * m.multiplicative);

		public StatModifier CombineWith(StatModifier m) =>
			new StatModifier(additive + m.additive - 1, multiplicative * m.multiplicative);

		public StatModifier Scale(float scale) =>
			new StatModifier(1 + (additive - 1) * scale, 1 + (multiplicative - 1) * scale);
	}
}
