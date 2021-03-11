using System;
using Terraria.ID;

namespace Terraria.ModLoader.IO
{
	public class TileEntry : ModEntry
	{
		public static Func<TagCompound, TileEntry> DESERIALIZER = tag => new TileEntry(tag);

		public bool frameImportant;

		public TileEntry(TagCompound tag) : base(tag) {
			frameImportant = tag.Get<bool>("framed");
		}

		public TileEntry(ModTile tile) : base(tile) {
			frameImportant = TileID.Sets.FrameImportant[tile.Type];
		}

		public override TagCompound SerializeData() {
			var tag = base.SerializeData();
			tag["frameImportant"] = frameImportant;
			return tag;
		}
	}
}
