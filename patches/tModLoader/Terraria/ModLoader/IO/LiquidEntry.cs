using System;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader.IO;
internal class LiquidEntry : TagSerializable
{
	public ushort type;
	public string modName;
	public string name;
	public ushort vanillaReplacementType;
	public string unloadedType;
	public ushort loadedType;

	public LiquidEntry(ModLiquid liquid)
	{
		type = loadedType = liquid.Type;
		modName = liquid.Mod.Name;
		name = liquid.Name;
		vanillaReplacementType = liquid.VanillaFallbackOnModDeletion;
		unloadedType = GetUnloadedPlaceholder(liquid.Type).FullName;
	}

	public static Func<TagCompound, LiquidEntry> DESERIALIZER = tag => new LiquidEntry(tag);

	public ModLiquid DefaultUnloadedPlaceholder => ModContent.GetInstance<UnloadedLiquid>();

	public ModLiquid GetUnloadedPlaceholder(ushort type) => DefaultUnloadedPlaceholder;

	protected LiquidEntry(TagCompound tag)
	{
		type = tag.Get<ushort>("value");
		modName = tag.Get<string>("mod");
		name = tag.Get<string>("name");
		vanillaReplacementType = tag.Get<ushort>("fallbackID");
		unloadedType = tag.Get<string>("uType");
	}

	public bool IsUnloaded => loadedType != type;

	public TagCompound SerializeData()
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
