using Terraria.ModLoader.IO;
using System.Collections.Generic;

namespace Terraria.ModLoader.Default
{
	internal class UnloadedChestInfo : UnloadedInfo 
	{
		internal byte ChestStyle; 

		public UnloadedChestInfo(string modName, string name, byte chestStyle = 0):base(modName, name) {
			this.modName = modName;
			this.name = name;
			this.ChestStyle = chestStyle;
		}

		public new TagCompound Save() {
			var tag = new TagCompound {
				["mod"] = modName,
				["name"] = name,
				["style"] = ChestStyle,
			};
			return tag;
		}
	}
}
