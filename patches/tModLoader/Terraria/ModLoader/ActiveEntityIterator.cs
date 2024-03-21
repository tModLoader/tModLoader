using System;
using System.Runtime.CompilerServices;

namespace Terraria.ModLoader;

public readonly ref struct ActiveEntityIterator<T> where T : Entity
{
	private readonly ReadOnlySpan<T> span;

	public ActiveEntityIterator(ReadOnlySpan<T> span) => this.span = span;

	public readonly Enumerator GetEnumerator() => new(span.GetEnumerator());

	public ref struct Enumerator
	{
		private ReadOnlySpan<T>.Enumerator enumerator;

		public Enumerator(ReadOnlySpan<T>.Enumerator enumerator) => this.enumerator = enumerator;

		public readonly T Current => enumerator.Current;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			while (enumerator.MoveNext()) {
				if (enumerator.Current.active)
					return true;
			}

			return false;
		}
	}
}