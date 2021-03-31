using System.Runtime.InteropServices;

namespace Terraria.ModLoader
{
	/// <summary> The purpose of this struct is to micro-optimize lookups of GlobalX indices by providing these associations without additional retrievals from the heap. </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public readonly struct Instanced<T> where T : GlobalType
	{
		public readonly ushort index;
		public readonly T instance;

		public Instanced(ushort index, T instance) {
			this.index = index;
			this.instance = instance;
		}

		public override string ToString() => $"{nameof(index)}: {index}, {nameof(instance)}: {instance?.GetType().Name ?? "null"}";
	}
}
