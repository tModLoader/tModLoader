using System;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default;

public class UnloadedGlobalItem : GlobalItem
{
	[CloneByReference] // safe to share between clones, because it cannot be changed after creation/load
	internal IList<TagCompound> data = new List<TagCompound>();

	[CloneByReference]
	public string ModPrefixMod { get; internal set; } = null;
	[CloneByReference]
	public string ModPrefixName { get; internal set; } = null;

	public override bool InstancePerEntity => true;

	public override void SaveData(Item item, TagCompound tag)
	{
		throw new NotSupportedException("UnloadedGlobalItem data is meant to be flattened and saved transparently via ItemIO");
	}

	// Unloaded globals aren't meant to save themselves, but we keep this to let us unpack legacy unloaedd globals items which did save themselves
	public override void LoadData(Item item, TagCompound tag)
	{
		if (tag.ContainsKey("modData")) {
			ItemIO.LoadGlobals(item, tag.GetList<TagCompound>("modData"));
		}
	}
}
