namespace Terraria.ModLoader.IO;

internal abstract class ModBlockEntry : TagSerializable
{
	public ushort type;
	public string modName;
	public string name;
	public ushort vanillaReplacementType;
	public string unloadedType;
	public ushort loadedType;

	protected ModBlockEntry(ModBlockType block)
	{
		type = loadedType = block.Type;
		modName = block.Mod.Name;
		name = block.Name;
		vanillaReplacementType = block.VanillaFallbackOnModDeletion;
		unloadedType = GetUnloadedPlaceholder(block.Type).FullName;
	}

	protected virtual ModBlockType GetUnloadedPlaceholder(ushort type) => DefaultUnloadedPlaceholder;

	protected ModBlockEntry(TagCompound tag)
	{
		type = tag.Get<ushort>("value");
		modName = tag.Get<string>("mod");
		name = tag.Get<string>("name");
		vanillaReplacementType = tag.Get<ushort>("fallbackID");
		unloadedType = tag.Get<string>("uType");
	}

	public bool IsUnloaded => loadedType != type;

	public abstract ModBlockType DefaultUnloadedPlaceholder { get; }

	public virtual TagCompound SerializeData()
	{
		return new TagCompound {
			["value"] = type,
			["mod"] = modName,
			["name"] = name,
			["fallbackID"] = vanillaReplacementType,
			["uType"] = unloadedType
		};
	}
}
