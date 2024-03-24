using System;

namespace tModLoader.Analyzers.SourceGenerator;

public readonly struct EquatableString(string value) : IEquatable<EquatableString>
{
	private readonly string value = value;

	public int Length => value?.Length ?? 0;

	public char this[int index] => value[index];

	public override bool Equals(object obj) => obj is EquatableString equatableString && Equals(equatableString);

	public bool Equals(EquatableString other)
	{
		if (value == null)
			return other.value == null;

		return value.AsSpan().SequenceEqual(other.value.AsSpan());
	}

	public override int GetHashCode()
	{
		if (this.value is not string value)
			return 0;

		var hashCode = new HashCode();

		foreach (char character in value) {
			hashCode.Add(character);
		}

		return hashCode.ToHashCode();
	}

	public override string ToString() => value ?? string.Empty;

	public static implicit operator EquatableString(string str) => new EquatableString(str);
	public static implicit operator string(EquatableString str) => str.value;

	public static bool operator ==(EquatableString left, EquatableString right) => left.Equals(right);
	public static bool operator !=(EquatableString left, EquatableString right) => !left.Equals(right);
}
