using System;
using System.Runtime.CompilerServices;

namespace Terraria.ModLoader;

public readonly ref struct ActiveItemIterator(ReadOnlySpan<WorldItem> span)
{
	private readonly ReadOnlySpan<WorldItem> span = span;

	public readonly Enumerator GetEnumerator() => new(span.GetEnumerator());

	public ref struct Enumerator(ReadOnlySpan<WorldItem>.Enumerator enumerator)
	{
		private ReadOnlySpan<WorldItem>.Enumerator enumerator = enumerator;

		public readonly WorldItem Current => enumerator.Current;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			while (enumerator.MoveNext()) {
				if (Current.active)
					return true;
			}

			return false;
		}
	}
}

public readonly ref struct ActiveNPCIterator(ReadOnlySpan<NPC> span)
{
	private readonly ReadOnlySpan<NPC> span = span;

	public readonly Enumerator GetEnumerator() => new(span.GetEnumerator());

	public ref struct Enumerator(ReadOnlySpan<NPC>.Enumerator enumerator)
	{
		private ReadOnlySpan<NPC>.Enumerator enumerator = enumerator;

		public readonly NPC Current => enumerator.Current;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			while (enumerator.MoveNext()) {
				if (Current.active)
					return true;
			}

			return false;
		}
	}
}

public readonly ref struct ActiveProjectileIterator(ReadOnlySpan<Projectile> span)
{
	private readonly ReadOnlySpan<Projectile> span = span;

	public readonly Enumerator GetEnumerator() => new(span.GetEnumerator());

	public ref struct Enumerator(ReadOnlySpan<Projectile>.Enumerator enumerator)
	{
		private ReadOnlySpan<Projectile>.Enumerator enumerator = enumerator;

		public readonly Projectile Current => enumerator.Current;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			while (enumerator.MoveNext()) {
				if (Current.active)
					return true;
			}

			return false;
		}
	}
}

public readonly ref struct ActivePlayerIterator(ReadOnlySpan<Player> span)
{
	private readonly ReadOnlySpan<Player> span = span;

	public readonly Enumerator GetEnumerator() => new(span.GetEnumerator());

	public ref struct Enumerator(ReadOnlySpan<Player>.Enumerator enumerator)
	{
		private ReadOnlySpan<Player>.Enumerator enumerator = enumerator;

		public readonly Player Current => enumerator.Current;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			while (enumerator.MoveNext()) {
				if (Current.active)
					return true;
			}

			return false;
		}
	}
}