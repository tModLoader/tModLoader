using System.Runtime.InteropServices;

namespace HookListPerformance
{
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	struct Instanced<T>
    {
        public readonly T instance;
        public readonly short index;
        public Instanced(T instance, int index) {
            this.instance = instance;
            this.index = (short) index;
        }
	}

	struct InstancedUnpacked<T>
	{
		public readonly T instance;
		public readonly short index;
		public InstancedUnpacked(T instance, int index) {
			this.instance = instance;
			this.index = (short)index;
		}
	}
}