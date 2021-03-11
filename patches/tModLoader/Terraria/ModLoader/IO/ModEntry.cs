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
		public ushort unloadedIndex;

		internal virtual void SetData<B>(B block) where B : ModBlockType {
			type = block.Type;
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
			unloadedType = tag.Get<string>("uType");
			unloadedIndex = tag.Get<ushort>("uValue");
		}

		public virtual TagCompound SerializeData() {
			return new TagCompound {
				["value"] = type,
				["mod"] = modName,
				["name"] = name,
				["fallbackID"] = vanillaReplacementType,
				["uType"] = unloadedType,
				["uValue"] = unloadedIndex
			};
		}
	}
}
