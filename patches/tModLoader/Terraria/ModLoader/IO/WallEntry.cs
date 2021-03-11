using System;

namespace Terraria.ModLoader.IO
{
	internal class WallEntry : ModEntry
	{
		public static Func<TagCompound, WallEntry> DESERIALIZER = tag => new WallEntry(tag);

		public WallEntry(TagCompound tag) : base(tag) { }
		public WallEntry(ModWall wall) : base(wall) { }
	}
}
