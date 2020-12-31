using Terraria.ModLoader.IO;
using System.Collections.Generic;

namespace Terraria.ModLoader.Default
{
	internal class UnloadedChestInfo : UnloadedInfo 
	{
		public readonly short frameX;
		public readonly short frameY;
		//TODO: Future add if the chest at the location had been unlocked.  
		//TODO: Future record what was the Chest Style 

		public UnloadedChestInfo(string modName, string name):base(modName, name) {
			this.modName = modName;
			this.name = name;
		}

		public new TagCompound Save() {
			var tag = new TagCompound {
				["mod"] = modName,
				["name"] = name
			};
			return tag;
		}
	}
}
