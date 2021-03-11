using System;
using Terraria.ID;

namespace Terraria.ModLoader.IO
{
	internal class WallEntry : ModEntry
	{
		public static Func<TagCompound, WallEntry> DESERIALIZER = tag => DeserializeTag(tag);

		public static WallEntry DeserializeTag(TagCompound tag) {
			var entry = new WallEntry();
			entry.LoadData(tag);
			return entry;
		}
	}
}
