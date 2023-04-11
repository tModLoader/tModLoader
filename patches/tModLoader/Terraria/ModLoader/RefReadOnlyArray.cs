using System;

namespace Terraria.ModLoader;

//The reason why this is used over ReadOnlySpan<T> is a slight performance difference that so far seems like a JIT bug.
public ref struct RefReadOnlyArray<T>
{
	internal readonly T[] array;

	public int Length => array.Length;

	public T this[int index] => array[index];

	public RefReadOnlyArray(T[] array)
	{
		this.array = array;
	}

	public ReadOnlySpan<T>.Enumerator GetEnumerator() => ((ReadOnlySpan<T>)array).GetEnumerator();

	public static implicit operator RefReadOnlyArray<T>(T[] array) => new RefReadOnlyArray<T>(array);

	public static implicit operator ReadOnlySpan<T>(RefReadOnlyArray<T> readOnlyArray) => readOnlyArray.array;
}