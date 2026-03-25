using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Terraria.ID;

public readonly struct BoolListSet
{
	private readonly List<int> values;
	private readonly bool[] set;

	public BoolListSet(int _size)
	{
		values = [];
		set = new bool[_size];
	}

	public bool this[int index] {
		get => set[index];
		set {
			if (value == set[index]) return;

			if (value)
				values.Add(index);
			else
				values.Remove(index);

			set[index] = value;
		}
	}

	public ReadOnlySpan<int> Values => CollectionsMarshal.AsSpan(values);

	public ReadOnlySpan<int>.Enumerator GetEnumerator() => Values.GetEnumerator();

	public static implicit operator ReadOnlySpan<int>(BoolListSet b) => b.Values;
}
