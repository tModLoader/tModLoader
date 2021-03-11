using System;
using Terraria.ID;

namespace Terraria.ModLoader.IO
{
	public class TileEntry : ModEntry
	{
		public static Func<TagCompound, TileEntry> DESERIALIZER = tag => DeserializeTag(tag);

		public static TileEntry DeserializeTag(TagCompound tag) {
			var entry = new TileEntry();
			entry.LoadData(tag);
			return entry;
		}

		public bool frameImportant;

		internal override void SetData<T>(T block) {
			base.SetData<T>(block);
			this.frameImportant = Main.tileFrameImportant[block.Type];
		}

		internal override void LoadData(TagCompound tag) {
			base.LoadData(tag);
			frameImportant = tag.GetBool("framed");
		}

		public override TagCompound SerializeData() {
			var tag = base.SerializeData();
			tag["framed"] = frameImportant;
			return tag;
		}
	}
}
