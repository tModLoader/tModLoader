using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class UnloadedInfo
	{
		internal readonly string modName;
		internal readonly string name;
		internal readonly ushort fallbackType;

		public UnloadedInfo(string modName, string name, ushort fallbackType) {
			this.modName = modName;
			this.name = name;
			this.fallbackType = fallbackType;
		}

		public TagCompound Save() => new TagCompound {
			["mod"] = modName,
			["name"] = name,
			["fallbackType"] = fallbackType
		};

		public override bool Equals(object obj) {
			UnloadedInfo other = obj as UnloadedInfo;
			if (other == null) {
				return false;
			}
			if (modName != other.modName || name != other.name) {
				return false;
			}
			return true;
		}

		public override int GetHashCode() {
			int hash = name.GetHashCode() + modName.GetHashCode();
			return hash;
		}
	}
}
