using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class UnloadedInfo
	{
		internal readonly string modName;
		internal readonly string name;
		internal readonly ushort fallbackType;
		internal readonly TagCompound customData;

		public UnloadedInfo(string modName, string name, ushort fallbackType, TagCompound customData = null) {
			this.modName = modName;
			this.name = name;
			this.fallbackType = fallbackType;
			this.customData = customData;
		}

		public TagCompound Save() => new TagCompound {
			["mod"] = modName,
			["name"] = name,
			["fallbackType"] = fallbackType,
			["customData"] = customData,
		};
	}
}
