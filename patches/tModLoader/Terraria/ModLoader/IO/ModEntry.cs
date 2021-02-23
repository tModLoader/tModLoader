using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class ModEntry
	{
		internal ushort id;
		internal string modName;
		internal string name;
		internal ushort fallbackID;
		internal string unloadedType;

		public ModEntry() {	}

		internal void SetData(ushort id, string modName, string name, ushort fallbackID, string unloadedType) {
			this.id = id;
			this.modName = modName;
			this.name = name;
			this.fallbackID = fallbackID;
			this.unloadedType = unloadedType;
		}

		internal virtual void LoadData(TagCompound tag) {
			id = tag.Get<ushort>("value");
			modName = tag.Get<string>("mod");
			name = tag.Get<string>("name");
			fallbackID = tag.Get<ushort>("fallbackID");
			unloadedType = tag.Get<string>("uType");
		}

		public virtual TagCompound Save() {
			return null;
		}

		public override bool Equals(object obj) {
			ModEntry other = obj as ModEntry;
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
