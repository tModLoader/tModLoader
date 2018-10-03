using Terraria.ModLoader;
using System.IO;

namespace UnloadFail
{
	class UnloadFail : Mod
	{
		public MemoryStream stream;
		
		public override void Load() {
			stream = new MemoryStream();
		}
		
		public override void Unload() {
			stream.Dispose();
		}
	}
}
