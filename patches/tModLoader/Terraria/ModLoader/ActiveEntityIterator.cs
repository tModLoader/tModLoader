using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Terraria.ModLoader;

// Do not touch without benchmarking.

public ref struct ActiveEntityIterator<T> where T : Entity
{
	private ref T entity;
	private ref T dummy;

	public ActiveEntityIterator() {
		Reset();
	}

	public T Current { get; private set; }

	public void Reset()
	{
		entity = ref MemoryMarshal.GetArrayDataReference(EntityArrays<T>.Array);
		dummy = ref Unsafe.Add(ref entity, EntityArrays<T>.Max);
	}

	public bool MoveNext() {
		while (Unsafe.IsAddressLessThan(ref entity, ref dummy)) {
			var currentEntity = entity;
			entity = ref Unsafe.Add(ref entity, 1);
			
			if (currentEntity.active) {
				Current = currentEntity;
				return true;
			}
		}
		
		return false;
	}
	
	public ActiveEntityIterator<T> GetEnumerator() {
		return this;
	}
}
