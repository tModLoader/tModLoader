using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public class UnloadedTileEntity : ModTileEntity
	{
		private string modName;
		private string tileEntityName;
		private IReadOnlyTagCompound data;

		internal void SetData(IReadOnlyTagCompound tag) {
			modName = tag.GetString("mod");
			tileEntityName = tag.GetString("name");

			if (tag.ContainsKey("data")) {
				data = tag.GetCompound("data");
			}
		}

		public override bool ValidTile(int i, int j) {
			Tile tile = Main.tile[i, j];
			return tile.active() && TileLoader.GetTile(type) is UnloadedTile;
		}

		public override void SaveData(TagCompound tag) {
			tag["mod"] = modName;
			tag["name"] = tileEntityName;

			if (data?.Count > 0) {
				tag["data"] = data;
			}
		}

		public override void LoadData(IReadOnlyTagCompound tag) {
			SetData(tag);
		}

		internal void TryRestore(ref ModTileEntity newEntity) {
			if (ModContent.TryFind(modName, tileEntityName, out ModTileEntity tileEntity)) {
				newEntity = ModTileEntity.ConstructFromBase(tileEntity);
				newEntity.type = (byte)tileEntity.Type;
				newEntity.Position = Position;

				if (data?.Count > 0) {
					newEntity.LoadData(data);
				}
			}
		}
	}
}
