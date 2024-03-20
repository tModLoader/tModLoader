using System;
using System.Runtime.CompilerServices;

namespace Terraria.ModLoader;

public readonly ref struct ActiveEntityIterator<T> where T : Entity
{
	private readonly Span<T> span;

	public ActiveEntityIterator() : this(EntityArrays<T>.Array.AsSpan(0, EntityArrays<T>.Max))
	{
	}

	public ActiveEntityIterator(Span<T> span)
	{
		this.span = span;
	}

	public readonly Enumerator GetEnumerator()
	{
		return new(span.GetEnumerator());
	}

	public ref struct Enumerator(Span<T>.Enumerator enumerator)
	{
		private Span<T>.Enumerator enumerator = enumerator;

		public readonly T Current => enumerator.Current;

		public bool MoveNext()
		{
			do {
				if (!enumerator.MoveNext())
					return false;
			} while (!enumerator.Current.active);

			return true;
		}
	}
}
