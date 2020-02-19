using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class MysteryTileInfo
	{
		public readonly string modName;
		public readonly string name;
		public readonly bool frameImportant;
		public readonly short frameX;
		public readonly short frameY;

		public MysteryTileInfo(string modName, string name) {
			this.modName = modName;
			this.name = name;
			this.frameImportant = false;
			this.frameX = -1;
			this.frameY = -1;
		}

		public MysteryTileInfo(string modName, string name, short frameX, short frameY) {
			this.modName = modName;
			this.name = name;
			this.frameImportant = true;
			this.frameX = frameX;
			this.frameY = frameY;
		}

		public override bool Equals(object obj) {
			MysteryTileInfo other = obj as MysteryTileInfo;
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
			};
			if (frameImportant) {
				tag.Set("frameX", frameX);
				tag.Set("frameY", frameY);
			}
			return tag;
		}
	}
}
