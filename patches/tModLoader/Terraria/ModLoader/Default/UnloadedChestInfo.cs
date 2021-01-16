using Terraria.ModLoader.IO;
using System.Collections.Generic;
using Terraria.ID;

namespace Terraria.ModLoader.Default
{
	internal class UnloadedChestInfo : UnloadedInfo 
	{
		internal byte chestStyle; 

		public UnloadedChestInfo(string modName, string name, byte chestStyle = 0,ushort fallbackType = TileID.Containers):base(modName, name,fallbackType) {
			this.modName = modName;
			this.name = name;
			this.chestStyle = chestStyle;
			this.fallbackType = fallbackType;

		}

		public new TagCompound Save() {
			var tag = new TagCompound {
				["mod"] = modName,
				["name"] = name,
				["style"] = chestStyle,
				["fallbackType"] = fallbackType,
			};
			return tag;
		}
	}
}
