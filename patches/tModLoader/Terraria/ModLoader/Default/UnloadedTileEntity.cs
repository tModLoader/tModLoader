using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public class UnloadedTileEntity : ModTileEntity
	{
		private string modName;
		private string tileEntityName;
		private TagCompound data;

		internal void SetData(TagCompound tag) {
			modName = tag.GetString("mod");
			tileEntityName = tag.GetString("name");
			data = tag.GetCompound("data");
		}

		public override bool ValidTile(int i, int j) {
			Tile tile = Main.tile[i, j];
			return tile.active() && (tile.type == Mod.TileType("UnloadedTile") || tile.type == Mod.TileType("PendingUnloadedTile"));
		}

		public override TagCompound Save() {
			return new TagCompound {
				["mod"] = modName,
				["name"] = tileEntityName,
				["data"] = data
			};
		}

		public override void Load(TagCompound tag) {
			SetData(tag);
		}

		internal void TryRestore(ref ModTileEntity newEntity) {
			if (ModContent.TryGet<ModTileEntity>(modName, tileEntityName, out var tileEntity)) {
				newEntity = ModTileEntity.ConstructFromBase(tileEntity);
				newEntity.type = (byte)tileEntity.Type;
				newEntity.Position = Position;
				newEntity.Load(data);
			}
		}
	}
}
