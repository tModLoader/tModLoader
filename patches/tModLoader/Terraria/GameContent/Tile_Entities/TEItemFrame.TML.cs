using Terraria.ModLoader.IO;

namespace Terraria.GameContent.Tile_Entities
{
	public partial class TEItemFrame
	{
		public override TagCompound Save() => new TagCompound {
			{ "item", ItemIO.Save(item) }
		};

		public override void Load(TagCompound tag) => item = ItemIO.Load(tag.GetCompound("item"));
	}
}
