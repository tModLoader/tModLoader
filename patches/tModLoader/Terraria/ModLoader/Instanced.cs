using System.Runtime.InteropServices;

namespace Terraria.ModLoader
{
	/// <summary> The purpose of this struct is to micro-optimize lookups of GlobalX indices by providing these associations without additional retrievals from the heap. </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal readonly struct Instanced<T>
	{
		public readonly short index;
		public readonly T instance;

		public Instanced(short index, T instance) {
			this.index = index;
			this.instance = instance;
		}

		public override string ToString() => $"{nameof(index)}: {index}, {nameof(instance)}: {instance?.GetType().Name ?? "null"}";
	}
}
