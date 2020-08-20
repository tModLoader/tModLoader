namespace Terraria.ModLoader
{
	public struct DamageModifier
	{
		public static readonly DamageModifier One = new DamageModifier(1f, 1f);

		public float additive;
		public float multiplicative;

		public DamageModifier(float additive, float multiplicative = 1) {
			this.additive = additive;
			this.multiplicative = multiplicative;
		}

		public static DamageModifier operator +(DamageModifier m, float add) =>
			new DamageModifier(m.additive + add, m.multiplicative);

		public static DamageModifier operator -(DamageModifier m, float sub) =>
			new DamageModifier(m.additive - sub, m.multiplicative);

		public static DamageModifier operator *(DamageModifier m, float mul) =>
			new DamageModifier(m.additive, m.multiplicative * mul);

		public static DamageModifier operator /(DamageModifier m, float div) =>
			new DamageModifier(m.additive, m.multiplicative / div);

		public static DamageModifier operator &(DamageModifier m1, DamageModifier m2) =>
			new DamageModifier(m1.additive + m2.additive - 1, m1.multiplicative * m2.multiplicative);

		public static explicit operator float(DamageModifier m) =>
			m.additive * m.multiplicative;

		public static DamageModifier operator +(float add, DamageModifier m) => m + add;
		public static DamageModifier operator *(float mul, DamageModifier m) => m * mul;

		public static bool operator ==(DamageModifier m1, DamageModifier m2) =>
			m1.additive == m2.additive && m1.multiplicative == m2.multiplicative;

		public static bool operator !=(DamageModifier m1, DamageModifier m2) =>
			m1.additive != m2.additive || m1.multiplicative != m2.multiplicative;

		public override bool Equals(object obj) {
			if (!(obj is DamageModifier))
				return false;

			var m = (DamageModifier)obj;
			return additive == m.additive &&
				   multiplicative == m.multiplicative;
		}

		public override int GetHashCode() {
			int hashCode = 1713062080;
			hashCode = hashCode * -1521134295 + additive.GetHashCode();
			hashCode = hashCode * -1521134295 + multiplicative.GetHashCode();
			return hashCode;
		}
	}
}
