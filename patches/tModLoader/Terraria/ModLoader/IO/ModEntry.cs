using System;

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

		public ModEntry(TagCompound tag) {
			type = tag.Get<ushort>("value");
			modName = tag.Get<string>("mod");
			name = tag.Get<string>("name");
			vanillaReplacementType = tag.Get<ushort>("fallbackID");
			unloadedType = tag.Get<string>("uType");
		}

		public ModEntry(ModBlockType block) {
			type = block.Type;
			modName = block.Mod.Name;
			name = block.Name;
			vanillaReplacementType = block.vanillaFallbackOnModDeletion;
			throw new NotImplementedException();// unloadedType = ?
			loadedType = type;
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
	}
}
