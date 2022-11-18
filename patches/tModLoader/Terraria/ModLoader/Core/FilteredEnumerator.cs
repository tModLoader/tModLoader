using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Terraria.ModLoader.Core;

// split into two classes, because the span version is slow. Keep an eye on https://github.com/dotnet/runtime/issues/68797 and check disassembly periodically
// or https://github.com/dotnet/runtime/issues/9977
// and my issue https://github.com/dotnet/runtime/issues/71510
public ref struct FilteredArrayEnumerator<T>
{
	// T[] current produces much better code-gen than `ReadOnlySpan<T>`
	private readonly T[] arr;
	private readonly int[] inds;

	private T current;
	private int i;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FilteredArrayEnumerator(T[] arr, int[] inds)
	{
		this.arr = arr;
		this.inds = inds;
		current = default;
		i = 0;
	}

	public T Current => current;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool MoveNext()
	{
		if (i >= inds.Length)
			return false;

		current = arr[inds[i++]];
		return true;
	}

	public FilteredArrayEnumerator<T> GetEnumerator() => this;
}

public ref struct FilteredSpanEnumerator<T>
{
	private readonly ReadOnlySpan<T> arr;
	private readonly int[] inds;

	private T current;
	private int i;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FilteredSpanEnumerator(ReadOnlySpan<T> arr, int[] inds)
	{
		this.arr = arr;
		this.inds = inds;
		current = default;
		i = 0;
	}

	public T Current => current;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool MoveNext()
	{
		if (i >= inds.Length)
			return false;

		current = arr[inds[i++]];
		return true;
	}

	public FilteredSpanEnumerator<T> GetEnumerator() => this;
}
