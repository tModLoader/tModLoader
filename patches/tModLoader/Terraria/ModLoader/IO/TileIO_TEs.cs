using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader.IO;

internal static partial class TileIO
{
	//*********** Tile Entities Save, Load, and Placeholder Implementations ***********************************//
	internal static List<TagCompound> SaveTileEntities()
	{
		var list = new List<TagCompound>();

		var saveData = new TagCompound();

		foreach (KeyValuePair<int, TileEntity> pair in TileEntity.ByID) {
			var tileEntity = pair.Value;
			var modTileEntity = tileEntity as ModTileEntity;

			tileEntity.SaveData(saveData);

			var tag = new TagCompound {
				["mod"] = modTileEntity?.Mod.Name ?? "Terraria",
				["name"] = modTileEntity?.Name ?? tileEntity.GetType().Name,
				["X"] = tileEntity.Position.X,
				["Y"] = tileEntity.Position.Y
			};

			if (saveData.Count != 0) {
				tag["data"] = saveData;
				saveData = new TagCompound();
			}

			list.Add(tag);
		}

		return list;
	}

	internal static void LoadTileEntities(IList<TagCompound> list)
	{
		foreach (TagCompound tag in list) {
			string modName = tag.GetString("mod");
			string name = tag.GetString("name");
			var point = new Point16(tag.GetShort("X"), tag.GetShort("Y"));

			ModTileEntity baseModTileEntity = null;
			TileEntity tileEntity;
			bool foundTE = true;

			//If the TE is modded
			if (modName != "Terraria") {
				//Try finding its type, defaulting to pending.
				if (!ModContent.TryFind(modName, name, out baseModTileEntity)) {
					foundTE = false;
					baseModTileEntity = ModContent.GetInstance<UnloadedTileEntity>();
				}

				tileEntity = ModTileEntity.ConstructFromBase(baseModTileEntity);
				tileEntity.type = (byte)baseModTileEntity.Type;
				tileEntity.Position = point;

				if (!foundTE) {
					(tileEntity as UnloadedTileEntity)?.SetData(tag);
				}
			}
			//Otherwise, if the TE is vanilla, try to find its existing instance for the current coordinate.
			else if (!TileEntity.ByPosition.TryGetValue(point, out tileEntity)) {
				//Do not create an PendingTileEntity on failure to do so.
				continue;
			}

			//Load TE data.
			if (tag.ContainsKey("data")) {
				try {
					if (foundTE) {
						tileEntity.LoadData(tag.GetCompound("data"));
					}
					if (tileEntity is UnloadedTileEntity unloadedTE && unloadedTE.TryRestore(out var restoredTE)) {
						tileEntity = restoredTE;
					}
				}
				catch (Exception e) {
					throw new CustomModDataException((tileEntity as ModTileEntity)?.Mod, $"Error in reading {name} tile entity data for {modName}", e);
				}
			}

			//Check mods' TEs for being valid. If they are, register them to TE collections.
			if (baseModTileEntity != null && tileEntity.IsTileValidForEntity(tileEntity.Position.X, tileEntity.Position.Y)) {
				tileEntity.ID = TileEntity.AssignNewID();
				TileEntity.ByID[tileEntity.ID] = tileEntity;

				if (TileEntity.ByPosition.TryGetValue(tileEntity.Position, out TileEntity other)) {
					TileEntity.ByID.Remove(other.ID);
				}

				TileEntity.ByPosition[tileEntity.Position] = tileEntity;
			}
		}
	}
}
