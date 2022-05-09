using System.Runtime.InteropServices;

namespace Terraria.ModLoader
{
	/// <summary> The purpose of this struct is to micro-optimize lookups of GlobalX indices by providing these associations without additional retrievals from the heap. </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public readonly struct Instanced<T> where T : GlobalType
	{
		public readonly ushort Index;
		public readonly T Instance;

		public Instanced(ushort index, T instance) {
			Index = index;
			Instance = instance;
		}

		public override string ToString()
			=> $"{nameof(Index)}: {Index}, {nameof(Instance)}: {Instance?.GetType().Name ?? "null"}";
	}
}
