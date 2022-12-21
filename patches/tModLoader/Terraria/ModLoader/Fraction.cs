using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Terraria.ModLoader {
	/// <summary>
	/// Represents a part of a whole or, more generally, any number of equal parts.
	/// </summary>
	[DebuggerDisplay("{Numerator}/{Denominator}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct Fraction
		: IConvertible, ISerializable, IDeserializationCallback,
		IEquatable<float>, IEquatable<double>, IEquatable<decimal>, IEquatable<Fraction>,
		IComparable, IComparable<float>, IComparable<double>, IComparable<decimal>, IComparable<Fraction> {
		#region Decimal Wrapper
		private readonly struct DecimalWrapper {
			private delegate uint GetXDelegate(decimal deci);
			private delegate int GetFlagsDelegate(decimal deci);

			private static readonly GetXDelegate getLowDelegate;
			private static readonly GetXDelegate getMidDelegate;
			private static readonly GetXDelegate getHighDelegate;
			private static readonly GetFlagsDelegate getFlagsDelegate;

			private readonly decimal m_value;

			public uint Low => getLowDelegate(m_value);
			public uint Mid => getMidDelegate(m_value);
			public uint High => getHighDelegate(m_value);
			public int Flags => getFlagsDelegate(m_value);

			static DecimalWrapper() {
				getLowDelegate = CreateGetDelegate<GetXDelegate>(nameof(Low));
				getMidDelegate = CreateGetDelegate<GetXDelegate>(nameof(Mid));
				getHighDelegate = CreateGetDelegate<GetXDelegate>(nameof(High));
				getFlagsDelegate = CreateFlagsDelegate();

				static T CreateGetDelegate<T>(string propertyName) where T : Delegate {
					ParameterExpression param = Expression.Parameter(typeof(decimal));
					Expression field = Expression.Call(param, typeof(decimal).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetMethod!);

					return Expression.Lambda<T>(field, param).Compile();
				}
				static GetFlagsDelegate CreateFlagsDelegate() {
					ParameterExpression param = Expression.Parameter(typeof(decimal));
					Expression field = Expression.Field(param, typeof(decimal), "_flags");

					return Expression.Lambda<GetFlagsDelegate>(field, param).Compile();
				}
			}

			public DecimalWrapper(decimal value) {
				m_value = value;
			}
		}
		#endregion

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
		/// <summary>
		/// <inheritdoc cref="Fraction(decimal)"/>
		/// </summary>
		/// <param name="chance">A fractional number.</param>
		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		public Fraction(float chance) : this(Convert.ToDecimal(chance)) {
		}

		/// <summary>
		/// <inheritdoc cref="Fraction(decimal)"/>
		/// </summary>
		/// <param name="chance">A fractional number.</param>
		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		public Fraction(double chance) : this(Convert.ToDecimal(chance)) {
		}

		/// <summary>
		/// Makes a fraction. Result chance will never be higher than 100%.
		/// </summary>
		/// <param name="chance">A fractional number.</param>
		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		public Fraction(decimal chance) {
			DecimalWrapper wrapper = new(chance);
			ulong numerator = (1 - (((ulong)wrapper.Flags >> 30) & 2)) *
				(((uint)(int)wrapper.High << 64)
					| (uint)(int)(wrapper.Mid << 32)
					| (uint)(int)wrapper.Low);
			ulong denominator = 1ul;
			for (ulong i = 0, c = ((ulong)wrapper.Flags >> 16) & 0xFF; i < c; i++) {
				denominator *= 10ul;
			}

			this.numerator = (int)numerator;
			this.denominator = (int)denominator;
			Normalize();
		}

		/// <summary>
		/// Makes a fraction using numerator and denominator, then simplifies the result.
		/// </summary>
		/// <param name="numerator">A numerator.</param>
		/// <param name="denominator">A denominator.</param>
		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		public Fraction(int numerator, int denominator) {
			this.numerator = numerator;
			this.denominator = denominator;
			Normalize();
		}
		#endregion

		#region Private Constructors
		private Fraction(SerializationInfo info, StreamingContext context) {
			if (info == null)
				throw new ArgumentNullException(nameof(info));

			numerator = info.GetInt32(nameof(numerator));
			denominator = info.GetInt32(nameof(denominator));
		}
		#endregion

		#region Normalizing
		/// <summary>
		/// Simplifies fraction. For example, from 5/100 to 1/20.
		/// </summary>
		/// <exception cref="ArithmeticException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		public void Normalize() {
			numerator += denominator;
			Reduce(GCD(numerator, denominator));
			Reduce(Math.Sign(denominator));
			numerator %= denominator;
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		private void Reduce(int x) {
			numerator /= x;
			denominator /= x;
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		private static int GCD(int a, int b) {
			while (b != 0) {
				(a, b) = (b, a % b);
			}
			return a;
		}
		#endregion

		#region Percentages
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		public float ToPercentageSingle() {
			return numerator / (float)denominator;
		}

		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		public double ToPercentageDouble() {
			return numerator / (double)denominator;
		}

		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
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

		#region Serialization
		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue(nameof(numerator), numerator);
			info.AddValue(nameof(denominator), denominator);
		}

		public void OnDeserialization(object? sender) {
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

		#region Conversion methods
		public TypeCode GetTypeCode() {
			return TypeCode.Int64 | TypeCode.UInt64 | TypeCode.Single | TypeCode.Double | TypeCode.Decimal;
		}

		public bool ToBoolean(IFormatProvider? provider) {
			return Convert.ToBoolean(numerator | denominator);
		}

		public byte ToByte(IFormatProvider? provider) {
			return Convert.ToByte(numerator | denominator);
		}

		public char ToChar(IFormatProvider? provider) {
			return Convert.ToChar(numerator | denominator);
		}

		public DateTime ToDateTime(IFormatProvider? provider) {
			return Convert.ToDateTime(numerator | denominator);
		}

		public decimal ToDecimal(IFormatProvider? provider) {
			return ToPercentageDecimal();
		}

		public double ToDouble(IFormatProvider? provider) {
			return ToPercentageDouble();
		}

		public short ToInt16(IFormatProvider? provider) {
			return Convert.ToInt16(numerator | denominator);
		}

		public int ToInt32(IFormatProvider? provider) {
			return Convert.ToInt32(numerator | denominator);
		}

		public long ToInt64(IFormatProvider? provider) {
			return Convert.ToInt64(numerator | denominator);
		}

		public sbyte ToSByte(IFormatProvider? provider) {
			return Convert.ToSByte(numerator | denominator);
		}

		public float ToSingle(IFormatProvider? provider) {
			return ToPercentageSingle();
		}

		public string ToString(IFormatProvider? provider) {
			return ToString();
		}

		public object ToType(Type conversionType, IFormatProvider? provider) {
			return Convert.ChangeType(this, conversionType, provider);
		}

		public ushort ToUInt16(IFormatProvider? provider) {
			return Convert.ToUInt16(numerator | denominator);
		}

		public uint ToUInt32(IFormatProvider? provider) {
			return Convert.ToUInt32(numerator | denominator);
		}

		public ulong ToUInt64(IFormatProvider? provider) {
			return Convert.ToUInt64(numerator | denominator);
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
		public static Fraction operator *(Fraction left, Fraction right) => new(left.ToPercentageDecimal() * right.ToPercentageDecimal());
		public static Fraction operator *(Fraction left, float right) => new(left.ToPercentageSingle() * right);
		public static Fraction operator *(Fraction left, double right) => new(left.ToPercentageDouble() * right);
		public static Fraction operator *(Fraction left, decimal right) => new(left.ToPercentageDecimal() * right);
		public static Fraction operator /(Fraction left, Fraction right) => new(left.numerator * right.denominator, left.denominator * right.numerator);
		public static Fraction operator /(Fraction left, float right) => new(left.ToPercentageSingle() / right);
		public static Fraction operator /(Fraction left, double right) => new(left.ToPercentageDouble() / right);
		public static Fraction operator /(Fraction left, decimal right) => new(left.ToPercentageDecimal() / right);
		public static Fraction operator %(Fraction left, Fraction right) => new(left.numerator % right.numerator, left.denominator % right.denominator);
		public static Fraction operator %(Fraction left, float right) => new(left.ToPercentageSingle() % right);
		public static Fraction operator %(Fraction left, double right) => new(left.ToPercentageDouble() % right);
		public static Fraction operator %(Fraction left, decimal right) => new(left.ToPercentageDecimal() % right);
		#endregion

		#region Comparison operators
		public static bool operator >(Fraction left, Fraction right) => left.CompareTo(right) > 0;
		public static bool operator >(Fraction left, float right) => left.CompareTo(right) > 0;
		public static bool operator >(Fraction left, double right) => left.CompareTo(right) > 0;
		public static bool operator >(Fraction left, decimal right) => left.CompareTo(right) > 0;
		public static bool operator >=(Fraction left, Fraction right) => left.CompareTo(right) >= 0;
		public static bool operator >=(Fraction left, float right) => left.CompareTo(right) >= 0;
		public static bool operator >=(Fraction left, double right) => left.CompareTo(right) >= 0;
		public static bool operator >=(Fraction left, decimal right) => left.CompareTo(right) >= 0;
		public static bool operator <(Fraction left, Fraction right) => left.CompareTo(right) < 0;
		public static bool operator <(Fraction left, float right) => left.CompareTo(right) < 0;
		public static bool operator <(Fraction left, double right) => left.CompareTo(right) < 0;
		public static bool operator <(Fraction left, decimal right) => left.CompareTo(right) < 0;
		public static bool operator <=(Fraction left, Fraction right) => left.CompareTo(right) <= 0;
		public static bool operator <=(Fraction left, float right) => left.CompareTo(right) <= 0;
		public static bool operator <=(Fraction left, double right) => left.CompareTo(right) <= 0;
		public static bool operator <=(Fraction left, decimal right) => left.CompareTo(right) <= 0;
		#endregion

		#region Bitwise operators
		public static Fraction operator ~(Fraction single) => new(~single.numerator, ~single.denominator);
		public static Fraction operator <<(Fraction left, Fraction other) => new(left.numerator << (int)other.numerator, left.numerator << (int)other.denominator);
		public static Fraction operator >>(Fraction left, Fraction other) => new(left.numerator >> (int)other.numerator, left.numerator >> (int)other.denominator);
		public static Fraction operator >>>(Fraction left, Fraction other) => new(left.numerator >>> (int)other.numerator, left.numerator >>> (int)other.denominator);
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
