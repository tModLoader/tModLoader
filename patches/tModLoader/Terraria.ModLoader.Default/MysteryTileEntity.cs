using System;
using System.IO;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public class MysteryTileEntity : ModTileEntity
	{
		private string modName;
		private string tileEntityName;
		private TagCompound data;

		internal void SetData(TagCompound tag)
		{
			modName = tag.GetString("mod");
			tileEntityName = tag.GetString("name");
			data = tag.GetCompound("data");
		}

		public override bool ValidTile(int i, int j)
		{
			return Main.tile[i, j].active() && Main.tile[i, j].type == mod.TileType("MysteryTile");
		}

		public override TagCompound Save()
		{
			return new TagCompound
			{
				["mod"] = modName,
				["name"] = tileEntityName,
				["data"] = data
			};
		}

		public override void Load(TagCompound tag)
		{
			SetData(tag);
		}

		internal void TryRestore(ref ModTileEntity newEntity)
		{
			Mod mod = ModLoader.GetMod(modName);
			ModTileEntity tileEntity = mod?.GetTileEntity(tileEntityName);
			if (tileEntity != null)
			{
				newEntity = ModTileEntity.ConstructFromBase(tileEntity);
				newEntity.type = (byte)tileEntity.Type;
				newEntity.Position = Position;
				newEntity.Load(data);
			}
		}
	}
}
