using Terraria.ModLoader.IO;
using System.Collections.Generic;

namespace Terraria.ModLoader.Default
{
	internal class UnloadedChestInfo : UnloadedInfo 
	{
		public readonly short frameX;
		public readonly short frameY;
		public readonly short chestIndex;
		public readonly short MaxChests = 8000; //As of Vanilla 1.4.0.1

		public UnloadedChestInfo(string modName, string name, short frameX, short frameY, short chestIndex):base(modName, name) {
			this.modName = modName;
			this.name = name;
			this.frameX = frameX;
			this.frameY = frameY;
			this.chestIndex = chestIndex;
		}

		public new TagCompound Save() {
			var tag = new TagCompound {
				["mod"] = modName,
				["name"] = name,
				["frameX"] = frameX,
				["frameY"] = frameY,
				["chestIndex"] = chestIndex,
			};
			return tag;
		}
	}
}
