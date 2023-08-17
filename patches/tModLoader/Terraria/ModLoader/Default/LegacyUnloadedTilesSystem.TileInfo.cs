using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default;

internal partial class LegacyUnloadedTilesSystem
{
	private struct TileInfo
	{
		public static readonly TileInfo Invalid = new TileInfo("UnknownMod", "UnknownTile");

		public readonly string modName;
		public readonly string name;
		public readonly bool frameImportant;
		public readonly short frameX;
		public readonly short frameY;

		public TileInfo(string modName, string name)
		{
			this.modName = modName;
			this.name = name;

			frameImportant = false;
			frameX = -1;
			frameY = -1;
		}

		public TileInfo(string modName, string name, short frameX, short frameY)
		{
			this.modName = modName;
			this.name = name;
			this.frameX = frameX;
			this.frameY = frameY;

			frameImportant = true;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is TileInfo other)) {
				return false;
			}

			if (modName != other.modName || name != other.name || frameImportant != other.frameImportant) {
				return false;
			}

			return !frameImportant || (frameX == other.frameX && frameY == other.frameY);
		}

		public override int GetHashCode()
		{
			int hash = name.GetHashCode() + modName.GetHashCode();

			if (frameImportant) {
				hash += frameX + frameY;
			}

			return hash;
		}

		public TagCompound Save()
		{
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