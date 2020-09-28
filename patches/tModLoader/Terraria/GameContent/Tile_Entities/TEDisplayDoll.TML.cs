using Terraria.ModLoader.IO;

namespace Terraria.GameContent.Tile_Entities
{
	public partial class TEDisplayDoll
	{
		public override TagCompound Save() {
			return new TagCompound {
				{ "items", PlayerIO.SaveInventory(_items) },
				{ "dyes", PlayerIO.SaveInventory(_dyes) },
			};
		}

		public override void Load(TagCompound tag) {
			PlayerIO.LoadInventory(_items, tag.GetList<TagCompound>("items"));
			PlayerIO.LoadInventory(_dyes, tag.GetList<TagCompound>("dyes"));
		}
	}
}
