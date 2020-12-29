using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class UnloadedTileInfo
	{
		public readonly string modName;
		public readonly string name;
		public readonly bool frameImportant;
		public readonly short frameX;
		public readonly short frameY;
		public readonly bool IsChest;
		public readonly bool IsSolid;
		public readonly short ChestIndx;

		public UnloadedTileInfo(string modName, string name, bool IsChest, bool IsSolid, short ChestIndx) {
			this.modName = modName;
			this.name = name;
			this.frameImportant = false;
			this.frameX = -1;
			this.frameY = -1;
			this.IsChest = IsChest;
			this.IsSolid = IsSolid;
			this.ChestIndx = ChestIndx;
		}

		public UnloadedTileInfo(string modName, string name, short frameX, short frameY, bool IsChest, bool IsSolid, short ChestIndx) {
			this.modName = modName;
			this.name = name;
			this.frameImportant = true;
			this.frameX = frameX;
			this.frameY = frameY;
			this.IsChest = IsChest;
			this.IsSolid = IsSolid;
			this.ChestIndx = ChestIndx;
		}

		public override bool Equals(object obj) {
			UnloadedTileInfo other = obj as UnloadedTileInfo;
			if (other == null) {
				return false;
			}
			if (modName != other.modName || name != other.name || frameImportant != other.frameImportant) {
				return false;
			}
			return !frameImportant || (frameX == other.frameX && frameY == other.frameY);
		}

		public override int GetHashCode() {
			int hash = name.GetHashCode() + modName.GetHashCode();
			if (frameImportant) {
				hash += frameX + frameY;
			}
			return hash;
		}

		public TagCompound Save() {
			var tag = new TagCompound {
				["mod"] = modName,
				["name"] = name,
				["IsChest"] = IsChest,
				["IsSolid"] = IsSolid,
				["ChestIndx"] = ChestIndx,
			};
			if (frameImportant) {
				tag.Set("frameX", frameX);
				tag.Set("frameY", frameY);
			}
			return tag;
		}
	}
}
