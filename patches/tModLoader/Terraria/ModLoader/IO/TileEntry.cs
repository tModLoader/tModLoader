using System;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader.IO
{
	public class TileEntry : ModEntry
	{
		public static Func<TagCompound, TileEntry> DESERIALIZER = tag => DeserializeTag<TileEntry>(tag);

		public bool frameImportant;

		public override string DefaultUnloadedType => ModContent.GetInstance<UnloadedSolidTile>().FullName;

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
