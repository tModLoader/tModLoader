namespace Terraria.ModLoader.IO
{
	public abstract class ModEntry : TagSerializable
	{
		public ushort type;
		public string modName;
		public string name;
		public ushort vanillaReplacementType;
		public string unloadedType;
		public ushort loadedType;

		public bool IsUnloaded => loadedType != type;

		public abstract string DefaultUnloadedType { get; }

		internal virtual void SetData<B>(B block) where B : ModBlockType {
			type = loadedType = block.Type;
			modName = block.Mod.Name;
			name = block.Name;
			vanillaReplacementType = block.vanillaFallbackOnModDeletion;
			unloadedType = TileIO.GetUnloadedType<B>(block.Type);
		}

		internal virtual void LoadData(TagCompound tag) {
			type = tag.Get<ushort>("value");
			modName = tag.Get<string>("mod");
			name = tag.Get<string>("name");
			vanillaReplacementType = tag.Get<ushort>("fallbackID");
			unloadedType = tag.Get<string>("uType") ?? DefaultUnloadedType;
		}

		public virtual TagCompound SerializeData() {
			return new TagCompound {
				["value"] = type,
				["mod"] = modName,
				["name"] = name,
				["fallbackID"] = vanillaReplacementType,
				["uType"] = unloadedType
			};
		}

		public static T DeserializeTag<T>(TagCompound tag) where T : ModEntry, new() {
			var entry = new T();
			entry.LoadData(tag);
			return entry;
		}
	}
}
