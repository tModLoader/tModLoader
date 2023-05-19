using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default;

[LegacyName("UnloadedWorld")]
public class UnloadedSystem : ModSystem
{
	internal IList<TagCompound> data;
	internal IList<TagCompound> unloadedNPCs;
	internal IList<TagCompound> unloadedKillCounts;
	internal IList<TagCompound> unloadedBestiaryKills;
	internal IList<TagCompound> unloadedBestiarySights;
	internal IList<TagCompound> unloadedBestiaryChats;

	public override void ClearWorld()
	{
		data = new List<TagCompound>();
		unloadedNPCs = new List<TagCompound>();
		unloadedKillCounts = new List<TagCompound>();
		unloadedBestiaryKills = new List<TagCompound>();
		unloadedBestiarySights = new List<TagCompound>();
		unloadedBestiaryChats = new List<TagCompound>();

		TileIO.ClearWorld();
	}

	public override void Unload()
	{
		TileIO.ResetUnloadedTypes();
	}

	public override void SaveWorldData(TagCompound tag)
	{
		tag["list"] = data;
		tag["unloadedNPCs"] = unloadedNPCs;
		tag["unloadedKillCounts"] = unloadedKillCounts;
		tag["unloadedBestiaryKills"] = unloadedBestiaryKills;
		tag["unloadedBestiarySights"] = unloadedBestiarySights;
		tag["unloadedBestiaryChats"] = unloadedBestiaryChats;
	}

	public override void LoadWorldData(TagCompound tag)
	{
		WorldIO.LoadModData(tag.GetList<TagCompound>("list"));
		WorldIO.LoadNPCs(tag.GetList<TagCompound>("unloadedNPCs"));
		WorldIO.LoadNPCKillCounts(tag.GetList<TagCompound>("unloadedKillCounts"));
		WorldIO.LoadNPCBestiaryKills(tag.GetList<TagCompound>("unloadedBestiaryKills"));
		WorldIO.LoadNPCBestiarySights(tag.GetList<TagCompound>("unloadedBestiarySights"));
		WorldIO.LoadNPCBestiaryChats(tag.GetList<TagCompound>("unloadedBestiaryChats"));
	}
}
