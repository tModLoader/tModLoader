using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class UnloadedWallInfo : UnloadedInfo 
	{
		public UnloadedWallInfo(string modName, string name, ushort fallbackType = 0) :base(modName, name, fallbackType) {
			this.modName = modName;
			this.name = name;
			this.fallbackType = fallbackType;
		}
	}
}
