using System;
using System.Collections.Generic;
using Terraria.GameContent.UI;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Terraria.IO;

public partial class WorldFileData
{
	internal IList<string> usedMods;
	internal string modPack;
	internal Dictionary<string, Version> modVersionsDuringWorldGen; // Note: "ModLoader" entry also present, can be used to know tML version
	/// <summary>
	/// Retrieves the version that the specified mod was at when the world was generated. <see langword="false"/> will be returned for mods that were not enabled when the world was generated.<para/>
	/// The feature tracking which mods were used to generate a world was added in v2023.8, so modders should first check <see cref="WorldGenModsRecorded"/> to see if the mods used to generate the world were recorded at all.
	/// </summary>
	public bool TryGetModVersionGeneratedWith(string mod, out Version modVersion) => modVersionsDuringWorldGen.TryGetValue(mod, out modVersion);
	/// <summary>
	/// If <see langword="true"/>, the mods used to generate this world have been saved and their version can be retrieved using <see cref="TryGetModVersionGeneratedWith(string, out Version)"/>.<para/>
	/// If <see langword="false"/>, this world was generated before the feature tracking mods used to generate a world was added (v2023.8) and modders can't determine if a specific mod was enabled when the world was generated.
	/// </summary>
	public bool WorldGenModsRecorded => modVersionsDuringWorldGen != null;

	internal Dictionary<string, TagCompound> ModHeaders { get; set; } = new Dictionary<string, TagCompound>();

	public bool TryGetHeaderData<T>(out TagCompound data) where T : ModSystem => TryGetHeaderData(ModContent.GetInstance<T>(), out data);
	public bool TryGetHeaderData(ModSystem system, out TagCompound data) => ModHeaders.TryGetValue(system.FullName, out data);

	/// <summary> Contains error messages from ModSystem.SaveWorldData from a previous world save retrieved from the .twld during load or the latest autosave. Will be shown in various places to warn the user. Maps ModSystem.FullName.MethodName to exception string.</summary>
	internal Dictionary<string, string> ModSaveErrors { get; set; } = new Dictionary<string, string>();
}
