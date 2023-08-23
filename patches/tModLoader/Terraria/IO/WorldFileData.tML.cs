using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Terraria.IO;

public partial class WorldFileData
{
	internal IList<string> usedMods; // log this?
	internal string modPack;
	/// <summary>
	/// Contains the mods (and their version) that were enabled when this world was generated.<para/>
	/// The feature tracking which mods were used to generate a world was added in v2023.8, so if <see cref="modsGeneratedWith"/> is <see langword="null"/>, this world was generated before then and can't be used to determine if a specific mod was enabled when the world was generated.
	/// </summary>
	public IList<(string modName, Version modVersion)> modsGeneratedWith; // TODO: Log this when loading world.
	internal Dictionary<string, TagCompound> ModHeaders { get; set; } = new Dictionary<string, TagCompound>();

	public bool TryGetHeaderData<T>(out TagCompound data) where T : ModSystem => TryGetHeaderData(ModContent.GetInstance<T>(), out data);
	public bool TryGetHeaderData(ModSystem system, out TagCompound data) => ModHeaders.TryGetValue(system.FullName, out data);
}
