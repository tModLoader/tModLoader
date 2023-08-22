using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Terraria.IO;

public partial class WorldFileData
{
	internal Dictionary<string, TagCompound> ModHeaders { get; set; } = new Dictionary<string, TagCompound>();

	public bool TryGetHeaderData<T>(out TagCompound data) where T : ModSystem => TryGetHeaderData(ModContent.GetInstance<T>(), out data);
	public bool TryGetHeaderData(ModSystem system, out TagCompound data) => ModHeaders.TryGetValue(system.FullName, out data);
}
