using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default;

public class UnloadedTileEntity : ModTileEntity
{
	private string modName;
	private string tileEntityName;
	private TagCompound data;

	internal void SetData(TagCompound tag)
	{
		modName = tag.GetString("mod");
		tileEntityName = tag.GetString("name");

		if (tag.ContainsKey("data")) {
			data = tag.GetCompound("data");
		}
	}

	public override bool IsTileValidForEntity(int i, int j)
	{
		Tile tile = Main.tile[i, j];
		return tile.HasTile && TileLoader.GetTile(tile.TileType) is UnloadedTile;
	}

	public override void SaveData(TagCompound tag)
	{
		tag["mod"] = modName;
		tag["name"] = tileEntityName;

		if (data?.Count > 0) {
			tag["data"] = data;
		}
	}

	public override void LoadData(TagCompound tag)
	{
		SetData(tag);
	}

	internal bool TryRestore(out ModTileEntity newEntity)
	{
		newEntity = null;

		if (ModContent.TryFind(modName, tileEntityName, out ModTileEntity tileEntity)) {
			newEntity = ModTileEntity.ConstructFromBase(tileEntity);
			newEntity.type = (byte)tileEntity.Type;
			newEntity.Position = Position;

			if (data?.Count > 0) {
				newEntity.LoadData(data);
			}
		}

		return newEntity != null;
	}
}
