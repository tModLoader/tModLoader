using System.Collections.Generic;

namespace Terraria.ModLoader
{
	//TODO: When .NET Core arrives, remove this and use ReadOnlySpan<T>
	public ref struct RefReadOnlyArray<T>
	{
		internal readonly T[] array;

		public int Count => array.Length;

		public T this[int index] => array[index];

		public RefReadOnlyArray(T[] array) {
			this.array = array;
		}

		public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)array).GetEnumerator();

		public static implicit operator RefReadOnlyArray<T>(T[] array) => new RefReadOnlyArray<T>(array);
	}
}
