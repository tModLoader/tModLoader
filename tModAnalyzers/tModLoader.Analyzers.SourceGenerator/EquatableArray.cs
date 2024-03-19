#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace tModLoader.Analyzers.SourceGenerator;

public readonly struct EquatableArray<T>(ImmutableArray<T> array) : IEquatable<EquatableArray<T>>, IEnumerable<T>
	where T : IEquatable<T>
{
	private readonly T[]? array = Unsafe.As<ImmutableArray<T>, T[]?>(ref array);

	public ref readonly T this[int index] {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref AsImmutableArray().ItemRef(index);
	}

	public bool IsEmpty {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => AsImmutableArray().IsEmpty;
	}

	public bool Equals(EquatableArray<T> array)
	{
		return AsSpan().SequenceEqual(array.AsSpan());
	}

	public override bool Equals(object? obj)
	{
		return obj is EquatableArray<T> array && Equals(this, array);
	}

	public override int GetHashCode()
	{
		if (this.array is not T[] array) {
			return 0;
		}

		var hashCode = new HashCode();

		foreach (T item in array) {
			hashCode.Add(item);
		}

		return hashCode.ToHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ImmutableArray<T> AsImmutableArray()
	{
		return Unsafe.As<T[]?, ImmutableArray<T>>(ref Unsafe.AsRef(in array));
	}

	public ReadOnlySpan<T> AsSpan()
	{
		return AsImmutableArray().AsSpan();
	}

	public ImmutableArray<T>.Enumerator GetEnumerator()
	{
		return AsImmutableArray().GetEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return ((IEnumerable<T>)AsImmutableArray()).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)AsImmutableArray()).GetEnumerator();
	}

	public static implicit operator EquatableArray<T>(ImmutableArray<T> array)
	{
		return new EquatableArray<T>(array);
	}

	public static implicit operator ImmutableArray<T>(EquatableArray<T> array)
	{
		return array.AsImmutableArray();
	}

	public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right)
	{
		return !left.Equals(right);
	}
}
