using System.Collections.Generic;
using System.IO;

namespace Terraria.ModLoader.Default
{
	class MysteryTilesWorld : ModWorld
	{
		private List<MysteryTileInfo> infos = new List<MysteryTileInfo>();
		internal List<MysteryTileInfo> pendingInfos = new List<MysteryTileInfo>();

		public override void Initialize()
		{
			infos.Clear();
			pendingInfos.Clear();
		}

		public override void SaveCustomData(BinaryWriter writer)
		{
			writer.Write(infos.Count);
			foreach (MysteryTileInfo info in infos)
			{
				if (info == null)
				{
					writer.Write("");
				}
				else
				{
					writer.Write(info.modName);
					writer.Write(info.name);
					writer.Write(info.frameImportant);
					if (info.frameImportant)
					{
						writer.Write(info.frameX);
						writer.Write(info.frameY);
					}
				}
			}
		}

		public override void LoadCustomData(BinaryReader reader)
		{
			int count = reader.ReadInt32();
			List<ushort> canRestore = new List<ushort>();
			bool canRestoreFlag = false;
			for (int k = 0; k < count; k++)
			{
				MysteryTileInfo info;
				string modName = reader.ReadString();
				if (modName.Length == 0)
				{
					infos.Add(null);
					canRestore.Add(0);
				}
				else
				{
					string name = reader.ReadString();
					bool frameImportant = reader.ReadBoolean();
					if (frameImportant)
					{
						info = new MysteryTileInfo(modName, name, reader.ReadInt16(), reader.ReadInt16());
					}
					else
					{
						info = new MysteryTileInfo(modName, name);
					}
					infos.Add(info);
					Mod mod = ModLoader.GetMod(modName);
					ushort type = (ushort)(mod == null ? 0 : mod.TileType(name));
					canRestore.Add(type);
					if (type != 0)
					{
						canRestoreFlag = true;
					}
				}
			}
			if (canRestoreFlag)
			{
				RestoreTiles(canRestore);
				for (int k = 0; k < count; k++)
				{
					if (canRestore[k] > 0)
					{
						infos[k] = null;
					}
				}
			}
			if (pendingInfos.Count > 0)
			{
				ConfirmPendingInfo();
			}
		}

		private void RestoreTiles(List<ushort> canRestore)
		{
			ushort mysteryType = (ushort)ModLoader.GetMod("ModLoader").TileType("MysteryTile");
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = 0; y < Main.maxTilesY; y++)
				{
					if (Main.tile[x, y].type == mysteryType)
					{
						Tile tile = Main.tile[x, y];
						MysteryTileFrame frame = new MysteryTileFrame(tile.frameX, tile.frameY);
						int frameID = frame.FrameID;
						if (canRestore[frameID] > 0)
						{
							MysteryTileInfo info = infos[frameID];
							tile.type = canRestore[frameID];
							tile.frameX = info.frameX;
							tile.frameY = info.frameY;
						}
					}
				}
			}
		}

		private void ConfirmPendingInfo()
		{
			List<int> truePendingID = new List<int>();
			int nextID = 0;
			for (int k = 0; k < pendingInfos.Count; k++)
			{
				while (nextID < infos.Count && infos[nextID] != null)
				{
					nextID++;
				}
				if (nextID == infos.Count)
				{
					infos.Add(pendingInfos[k]);
				}
				else
				{
					infos[nextID] = pendingInfos[k];
				}
				truePendingID.Add(nextID);
			}
			ushort pendingType = (ushort)ModLoader.GetMod("ModLoader").TileType("PendingMysteryTile");
			ushort mysteryType = (ushort)ModLoader.GetMod("ModLoader").TileType("MysteryTile");
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = 0; y < Main.maxTilesY; y++)
				{
					if (Main.tile[x, y].type == pendingType)
					{
						Tile tile = Main.tile[x, y];
						MysteryTileFrame frame = new MysteryTileFrame(tile.frameX, tile.frameY);
						frame = new MysteryTileFrame(truePendingID[frame.FrameID]);
						tile.type = mysteryType;
						tile.frameX = frame.FrameX;
						tile.frameY = frame.FrameY;
					}
				}
			}
		}
	}

	class MysteryTileInfo
	{
		public readonly string modName;
		public readonly string name;
		public readonly bool frameImportant;
		public readonly short frameX;
		public readonly short frameY;

		public MysteryTileInfo(string modName, string name)
		{
			this.modName = modName;
			this.name = name;
			this.frameImportant = false;
			this.frameX = -1;
			this.frameY = -1;
		}

		public MysteryTileInfo(string modName, string name, short frameX, short frameY)
		{
			this.modName = modName;
			this.name = name;
			this.frameImportant = true;
			this.frameX = frameX;
			this.frameY = frameY;
		}

		public override bool Equals(object obj)
		{
			MysteryTileInfo other = obj as MysteryTileInfo;
			if (other == null)
			{
				return false;
			}
			if (modName != other.modName || name != other.name || frameImportant != other.frameImportant)
			{
				return false;
			}
			return !frameImportant || (frameX == other.frameX && frameY == other.frameY);
		}

		public override int GetHashCode()
		{
			int hash = name.GetHashCode() + modName.GetHashCode();
			if (frameImportant)
			{
				hash += frameX + frameY;
			}
			return hash;
		}
	}

	class MysteryTileFrame
	{
		private short frameX;
		private short frameY;

		public short FrameX => frameX;
		public short FrameY => frameY;

		public int FrameID
		{
			get
			{
				return frameY * (short.MaxValue + 1) + frameX;
			}
			set
			{
				frameX = (short)(value % (short.MaxValue + 1));
				frameY = (short)(value / (short.MaxValue + 1));
			}
		}

		public MysteryTileFrame(int value)
		{
			FrameID = value;
		}

		public MysteryTileFrame(short frameX, short frameY)
		{
			this.frameX = frameX;
			this.frameY = frameY;
		}
	}
}
