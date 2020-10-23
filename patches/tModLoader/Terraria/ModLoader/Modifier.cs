namespace Terraria.ModLoader
{
	public struct Modifier
	{
		public static readonly Modifier One = new Modifier(1f, 1f);

		public float additive;
		public float multiplicative;

		public Modifier(float additive, float multiplicative = 1) {
			this.additive = additive;
			this.multiplicative = multiplicative;
		}

		public override bool Equals(object obj) {
			if (!(obj is Modifier))
				return false;

			var m = (Modifier)obj;
			return additive == m.additive &&
				   multiplicative == m.multiplicative;
		}

		public override int GetHashCode() {
			int hashCode = 1713062080;
			hashCode = hashCode * -1521134295 + additive.GetHashCode();
			hashCode = hashCode * -1521134295 + multiplicative.GetHashCode();
			return hashCode;
		}

		public static Modifier operator +(Modifier m, float add) =>
			new Modifier(m.additive + add, m.multiplicative);

		public static Modifier operator -(Modifier m, float sub) =>
			new Modifier(m.additive - sub, m.multiplicative);

		public static Modifier operator *(Modifier m, float mul) =>
			new Modifier(m.additive, m.multiplicative * mul);

		public static Modifier operator /(Modifier m, float div) =>
			new Modifier(m.additive, m.multiplicative / div);

		public static Modifier operator &(Modifier m1, Modifier m2) =>
			new Modifier(m1.additive + m2.additive - 1, m1.multiplicative * m2.multiplicative);

		public static Modifier operator +(float add, Modifier m) => m + add;
		public static Modifier operator *(float mul, Modifier m) => m * mul;

		public static bool operator ==(Modifier m1, Modifier m2) =>
			m1.additive == m2.additive && m1.multiplicative == m2.multiplicative;

		public static bool operator !=(Modifier m1, Modifier m2) =>
			m1.additive != m2.additive || m1.multiplicative != m2.multiplicative;

		public static implicit operator float(Modifier m) =>
			m.additive * m.multiplicative;

		public static implicit operator int(Modifier m) =>
			(int)(m.additive * m.multiplicative);
	}
}
