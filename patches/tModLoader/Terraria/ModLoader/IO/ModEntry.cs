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

		protected ModEntry(ModBlockType block) {
			type = loadedType = block.Type;
			modName = block.Mod.Name;
			name = block.Name;
			vanillaReplacementType = block.VanillaFallbackOnModDeletion;
			unloadedType = GetUnloadedType(block.Type);
		}

		protected abstract string GetUnloadedType(ushort type);

		protected ModEntry(TagCompound tag) {
			type = tag.Get<ushort>("value");
			modName = tag.Get<string>("mod");
			name = tag.Get<string>("name");
			vanillaReplacementType = tag.Get<ushort>("fallbackID");
			unloadedType = tag.Get<string>("uType") ?? DefaultUnloadedType;
		}

		public bool IsUnloaded => loadedType != type;

		public abstract string DefaultUnloadedType { get; }

		public virtual TagCompound SerializeData() {
			return new TagCompound {
				["value"] = type,
				["mod"] = modName,
				["name"] = name,
				["fallbackID"] = vanillaReplacementType,
				["uType"] = unloadedType
			};
		}
	}
}
