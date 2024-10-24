using Terraria.ID;
using Terraria.ModLoader;

namespace Terraria;

public partial class Cloud
{
	public ModCloud ModCloud { get; private set; }

	internal static void SwapOutModdedClouds()
	{
		if (!Main.dedServ) {
			for (int i = 0; i < 200; i++) {
				Main.cloud[i].ModCloud = null;
				if (Main.cloud[i].type >= CloudID.Count)
					Main.cloud[i].type = Main.rand.Next(0, 22);
			}
		}
	}

	public override string ToString() => active ? $"type: {type}, scale: {scale}, ModCloud: {ModCloud?.FullName ?? "null"}" : "Inactive";
}
