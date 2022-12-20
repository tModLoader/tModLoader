using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Terraria.ModLoader {
	/// <summary>
	/// Represents a part of a whole or, more generally, any number of equal parts.
	/// </summary>
	[DebuggerDisplay("{Numerator}/{Denominator}")]
	public struct Fraction
		: IEquatable<float>, IEquatable<double>, IEquatable<decimal>, IEquatable<Fraction>,
		IComparable, IComparable<float>, IComparable<double>, IComparable<decimal>, IComparable<Fraction> {
		#region Private Variables
		// Should there be SingleFraction that will have numerator and denominator that are float? (maybe IFraction interface as well)
		private int numerator;
		private int denominator;
		#endregion

		#region Public Properties
		/// <summary>
		/// The lower part.
		/// </summary>
		public int Numerator {
			get {
				return numerator;
			}
			set {
				numerator = value;
			}
		}

		/// <summary>
		/// The bottom part. Cannot be zero.
		/// </summary>
		public int Denominator {
			get {
				return denominator;
			}
			set {
				if (value == 0) {
					throw new DivideByZeroException();
				}
				denominator = value;
			}
		}
		#endregion

		#region Public Static Read-onlys
		/// <summary>
		/// Represents 0/1, or 0%.
		/// </summary>
		public static readonly Fraction Zero = new(0, 1);
		/// <summary>
		/// Represents 1/1, or 100%.
		/// </summary>
		public static readonly Fraction One = new(1, 1);
		/// <summary>
		/// Represents 1/2, or 50%.
		/// </summary>
		public static readonly Fraction OneHalf = new(1, 2);
		/// <summary>
		/// Represents 1/3, or 33%.
		/// </summary>
		public static readonly Fraction OneThird = new(1, 3);
		/// <summary>
		/// Represents 2/3, or 66%.
		/// </summary>
		public static readonly Fraction TwoThirds = new(2, 3);
		/// <summary>
		/// Represents 1/4, or 25%.
		/// </summary>
		public static readonly Fraction OneQuarter = new(1, 4);
		/// <summary>
		/// <inheritdoc cref="OneHalf"/>
		/// </summary>
		public static readonly Fraction TwoQuarters = OneHalf;
		/// <summary>
		/// Represents 3/4, or 75%.
		/// </summary>
		public static readonly Fraction ThreeQuarters = new(3, 4);
		/// <summary>
		/// Represents 1/5, or 20%.
		/// </summary>
		public static readonly Fraction OneFifth = new(1, 5);
		/// <summary>
		/// Represents 2/5, or 40%.
		/// </summary>
		public static readonly Fraction TwoFifths = new(2, 5);
		/// <summary>
		/// Represents 3/5, or 60%.
		/// </summary>
		public static readonly Fraction ThreeFifths = new(3, 5);
		/// <summary>
		/// Represents 4/5, or 80%.
		/// </summary>
		public static readonly Fraction FourFifths = new(4, 5);
		#endregion

		#region Public Constructors
		public Fraction(float chance) : this(Convert.ToDecimal(chance)) {
		}

		public Fraction(double chance) : this(Convert.ToDecimal(chance)) {
		}

		public Fraction(decimal chance) {
			if (chance == decimal.Zero) {
				numerator = Zero.denominator;
				denominator = Zero.numerator;
				return;
			}

			int tries = 0;
			do {
				chance *= 10.0m;
				tries++;
			}
			while (tries < 15 && chance != (int)chance);
			int num = (int)chance;
			int den = (int)Math.Pow(10, tries);

			numerator = num;
			denominator = den;

			Normalize();
		}

		public Fraction(int numerator, int denominator) {
			this.numerator = numerator;
			this.denominator = denominator;
		}
		#endregion

		#region Normalizing
		/// <summary>
		/// Simplifies fraction. For example, from 5/100 to 1/20.
		/// </summary>
		/// <exception cref="ArithmeticException">Thrown if numerator and denominator are greater than <code>int.MaxValue / 2</code> or if numerator and denominator are lesser than <code>-int.MaxValue / 2</code>.</exception>
		public void Normalize() {
			bool numeratorIsNegative = numerator < 0;
			bool denominatorIsNegative = denominator < 0;
			if (numeratorIsNegative) {
				numerator *= -1;
			}
			if (denominatorIsNegative) {
				denominator *= -1;
			}

			if (numerator > int.MaxValue / 2 && denominator > int.MaxValue / 2) {
				throw new ArithmeticException($"Numerator or denominator are greater than {int.MaxValue / 2} or lesser than {-int.MaxValue / 2}.");
			}

			numerator += denominator;
			Reduce(GCD(numerator, denominator));
			Reduce(Math.Sign(denominator));
			numerator %= denominator;

			if (numeratorIsNegative) {
				numerator *= -1;
			}
			if (denominatorIsNegative) {
				denominator *= -1;
			}
		}

		private void Reduce(int x) {
			numerator /= x;
			denominator /= x;
		}

		private static int GCD(int a, int b) {
			while (b != 0) {
				int t = b;
				b = a % b;
				a = t;
			}
			return a;
		}
		#endregion

		#region Percentages
		[Pure]
		public float ToPercentageSingle() {
			return numerator / (float)denominator;
		}

		[Pure]
		public double ToPercentageDouble() {
			return numerator / (double)denominator;
		}

		[Pure]
		public decimal ToPercentageDecimal() {
			return numerator / (decimal)denominator;
		}
		#endregion

		#region Object methods
		public override string ToString() {
#if !NETSTANDARD2_0 && !NETFRAMEWORK
			Span<char> span = stackalloc char[1 + (2 * 20)];
			int pos = 0;

			bool formatted = numerator.TryFormat(span[pos..], out int charsWritten);
			Debug.Assert(formatted);
			pos += charsWritten;

			span[pos++] = '/';
			
			formatted = denominator.TryFormat(span[pos..], out charsWritten);
			Debug.Assert(formatted);
			pos += charsWritten;

			return new string(span[..pos]);
#else
			return $"{numerator}/{denominator}";
#endif
		}

		public override int GetHashCode() {
			return HashCode.Combine(numerator, denominator);
		}
		#endregion

		#region Equatable
		public override bool Equals([NotNullWhen(true)] object? obj) {
			return obj switch {
				Fraction other => Equals(other),
				float single => Equals(single),
				double dou => Equals(dou),
				decimal deci => Equals(deci),
				_ => false
			};
		}

		public bool Equals(Fraction other) {
			return other.numerator == numerator && other.denominator == denominator;
		}

		public bool Equals(float single) {
			return ToPercentageSingle() == single;
		}

		public bool Equals(double dou) {
			return ToPercentageDouble() == dou;
		}

		public bool Equals(decimal deci) {
			return ToPercentageDecimal() == deci;
		}
		#endregion

		#region Comparable
		public int CompareTo(object? obj) {
			return obj switch {
				Fraction other => CompareTo(other),
				float single => CompareTo(single),
				double dou => CompareTo(dou),
				decimal deci => CompareTo(deci),
				_ => 0
			};
		}

		public int CompareTo(Fraction other) {
			if (this < other) return -1;
			if (this > other) return 1;
			return 0;
		}

		public int CompareTo(float other) {
			return ToPercentageSingle().CompareTo(other);
		}

		public int CompareTo(double other) {
			return ToPercentageSingle().CompareTo(other);
		}

		public int CompareTo(decimal other) {
			return ToPercentageSingle().CompareTo(other);
		}
		#endregion

		#region Conversion operators
		public static implicit operator float(Fraction self) => self.ToPercentageSingle();
		public static implicit operator double(Fraction self) => self.ToPercentageDouble();
		public static implicit operator decimal(Fraction self) => self.ToPercentageDecimal();
		public static explicit operator Fraction(float chance) => new(chance);
		public static explicit operator Fraction(double chance) => new(chance);
		public static explicit operator Fraction(decimal chance) => new(chance);
		#endregion

		#region Arithmetic operators
		public static Fraction operator +(Fraction single) => single;
		public static Fraction operator +(Fraction left, Fraction right) => new(left.numerator * right.denominator + right.numerator * left.denominator, left.denominator * right.denominator);
		public static Fraction operator +(Fraction left, float right) => new(left.ToPercentageSingle() + right);
		public static Fraction operator +(Fraction left, double right) => new(left.ToPercentageDouble() + right);
		public static Fraction operator +(Fraction left, decimal right) => new(left.ToPercentageDecimal() + right);
		public static Fraction operator -(Fraction single) => new(-single.numerator, single.denominator);
		public static Fraction operator -(Fraction left, Fraction right) => left + (-right);
		public static Fraction operator -(Fraction left, float right) => new(left.ToPercentageSingle() - right);
		public static Fraction operator -(Fraction left, double right) => new(left.ToPercentageDouble() - right);
		public static Fraction operator -(Fraction left, decimal right) => new(left.ToPercentageDecimal() - right);
		public static Fraction operator *(Fraction left, Fraction right) => new(left.numerator * right.numerator, left.denominator * right.denominator);
		public static Fraction operator *(Fraction left, float right) => new(left.ToPercentageSingle() * right);
		public static Fraction operator *(Fraction left, double right) => new(left.ToPercentageDouble() * right);
		public static Fraction operator *(Fraction left, decimal right) => new(left.ToPercentageDecimal() * right);
		public static Fraction operator /(Fraction left, Fraction right) {
			if (right.numerator == 0) {
				throw new DivideByZeroException();
			}
			return new(left.numerator * right.denominator, left.denominator * right.numerator);
		}
		public static Fraction operator /(Fraction left, float right) => new(left.ToPercentageSingle() / right);
		public static Fraction operator /(Fraction left, double right) => new(left.ToPercentageDouble() / right);
		public static Fraction operator /(Fraction left, decimal right) => new(left.ToPercentageDecimal() / right);
		public static Fraction operator %(Fraction left, Fraction right) => new(left.numerator % right.numerator, left.denominator % right.denominator);
		public static Fraction operator %(Fraction left, float right) => new(left.ToPercentageSingle() % right);
		public static Fraction operator %(Fraction left, double right) => new(left.ToPercentageDouble() % right);
		public static Fraction operator %(Fraction left, decimal right) => new(left.ToPercentageDecimal() % right);
		#endregion

		#region Comparison operators
		public static bool operator >(Fraction left, Fraction right) => left.denominator > right.denominator && left.numerator > right.denominator;
		public static bool operator >(Fraction left, float right) => left.ToPercentageSingle() > right;
		public static bool operator >(Fraction left, double right) => left.ToPercentageDouble() > right;
		public static bool operator >(Fraction left, decimal right) => left.ToPercentageDecimal() > right;
		public static bool operator >=(Fraction left, Fraction right) => left.denominator >= right.denominator && left.numerator >= right.denominator;
		public static bool operator >=(Fraction left, float right) => left.ToPercentageSingle() >= right;
		public static bool operator >=(Fraction left, double right) => left.ToPercentageDouble() >= right;
		public static bool operator >=(Fraction left, decimal right) => left.ToPercentageDecimal() >= right;
		public static bool operator <(Fraction left, Fraction right) => !(left > right);
		public static bool operator <(Fraction left, float right) => !(left > right);
		public static bool operator <(Fraction left, double right) => !(left > right);
		public static bool operator <(Fraction left, decimal right) => !(left > right);
		public static bool operator <=(Fraction left, Fraction right) => !(left >= right);
		public static bool operator <=(Fraction left, float right) => !(left >= right);
		public static bool operator <=(Fraction left, double right) => !(left >= right);
		public static bool operator <=(Fraction left, decimal right) => !(left >= right);
		#endregion

		#region Bitwise operators
		public static Fraction operator ~(Fraction single) => new(single.denominator, single.numerator);
		//public static Fraction operator <<(Fraction left, Fraction other) => new(left.numerator << (int)other.numerator, left.numerator << (int)other.denominator);
		//public static Fraction operator >>(Fraction left, Fraction other) => new(left.numerator >> (int)other.numerator, left.numerator >> (int)other.denominator);
		//public static Fraction operator >>>(Fraction left, Fraction other) => new(left.numerator >>> (int)other.numerator, left.numerator >>> (int)other.denominator);
		public static Fraction operator &(Fraction left, Fraction other) => new(left.numerator & other.numerator, left.denominator & other.denominator);
		public static Fraction operator ^(Fraction left, Fraction other) => new(left.numerator ^ other.numerator, left.denominator ^ other.denominator);
		public static Fraction operator |(Fraction left, Fraction other) => new(left.numerator | other.numerator, left.denominator | other.denominator);
		#endregion

		#region Equality operators
		public static bool operator ==(Fraction left, Fraction right) => left.Equals(right);
		public static bool operator ==(Fraction left, float right) => left.Equals(right);
		public static bool operator ==(Fraction left, double right) => left.Equals(right);
		public static bool operator ==(Fraction left, decimal right) => left.Equals(right);
		public static bool operator !=(Fraction left, Fraction right) => !(left == right);
		public static bool operator !=(Fraction left, float right) => !(left == right);
		public static bool operator !=(Fraction left, double right) => !(left == right);
		public static bool operator !=(Fraction left, decimal right) => !(left == right);
		#endregion
	}
}
