using System;

namespace Terraria.ModLoader.IO
{
	internal class WallEntry : ModEntry
	{
		public static Func<TagCompound, WallEntry> DESERIALIZER = tag => DeserializeTag<WallEntry>(tag);
	}
}
