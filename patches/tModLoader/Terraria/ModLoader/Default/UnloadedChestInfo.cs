using Terraria.ModLoader.IO;
using System.Collections.Generic;
using Terraria.ID;

namespace Terraria.ModLoader.Default
{
	internal class UnloadedChestInfo : UnloadedInfo 
	{
		internal byte ChestStyle; 

		public UnloadedChestInfo(string modName, string name, byte chestStyle = 0,ushort fallbackType = TileID.Containers):base(modName, name,fallbackType) {
			this.modName = modName;
			this.name = name;
			this.ChestStyle = chestStyle;
			this.FallbackType = fallbackType;

		}

		public new TagCompound Save() {
			var tag = new TagCompound {
				["mod"] = modName,
				["name"] = name,
				["style"] = ChestStyle,
				["fallbackType"] = FallbackType,
			};
			return tag;
		}
	}
}
