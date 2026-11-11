using System;
using System.Runtime.CompilerServices;

namespace Terraria.ModLoader.Core;

public ref struct EntityGlobalsEnumerator<TGlobal> where TGlobal : GlobalType<TGlobal>
{
	private readonly TGlobal[] baseGlobals;
	private readonly TGlobal[] entityGlobals;
	private int i;
	private TGlobal current;

	public EntityGlobalsEnumerator(TGlobal[] baseGlobals, TGlobal[] entityGlobals)
	{
		this.baseGlobals = entityGlobals == null ? Array.Empty<TGlobal>() : baseGlobals;
		this.entityGlobals = entityGlobals;
		i = 0;
		current = null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public EntityGlobalsEnumerator(IEntityWithGlobals<TGlobal> entity) : this(GlobalTypeLookups<TGlobal>.GetGlobalsForType(entity.Type), entity) { }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public EntityGlobalsEnumerator(TGlobal[] baseGlobals, IEntityWithGlobals<TGlobal> entity) : this(baseGlobals, entity.EntityGlobals.array) { }

	public TGlobal Current => current;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool MoveNext()
	{
		while (i < baseGlobals.Length) {
			current = baseGlobals[i++];
			var slot = current.PerEntityIndex;
			if (slot < 0)
				return true;

			current = entityGlobals[slot];
			if (current != null)
				return true;
		}
		return false;
	}

	public EntityGlobalsEnumerator<TGlobal> GetEnumerator() => this;
}