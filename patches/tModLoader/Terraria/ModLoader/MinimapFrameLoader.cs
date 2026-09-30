using System.Collections.Generic;
using ReLogic.Content;

namespace Terraria.ModLoader;

/// <summary>
/// Manages minimap borders supplied by mods.
/// </summary>
public static class MinimapFrameLoader
{
	internal static readonly IList<ModMinimapFrame> ModdedFrames = new List<ModMinimapFrame>();

	internal static void Add(ModMinimapFrame frame)
	{
		ModdedFrames.Add(frame);
	}

	internal static void AddModdedFrames(AssetRequestMode mode)
	{
		if (Main.dedServ)
			return;

		Main.MinimapFrameManagerInstance.AddModdedFrames(mode);
	}

	internal static void Unload()
	{
		if (!Main.dedServ)
			Main.MinimapFrameManagerInstance.ResetToVanilla();

		ModdedFrames.Clear();
	}
}