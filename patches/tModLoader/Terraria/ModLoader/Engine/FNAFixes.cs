using SDL2;
using System;

namespace Terraria.ModLoader.Engine;

internal static class FNAFixes
{
	internal static void Init()
	{
		if (OperatingSystem.IsWindows()) {
			// FNA sets this to "1" on Windows. Terraria does not want this. See #2020
			SDL.SDL_SetHint(SDL.SDL_HINT_VIDEO_MINIMIZE_ON_FOCUS_LOSS, "0");

			// only directx is supported. See #2926
			var videoDriver = Environment.GetEnvironmentVariable("SDL_VIDEODRIVER");
			if (videoDriver != null && videoDriver != "directx") {
				Logging.FNA.Debug("Cleared SDL_VIDEODRIVER=" + videoDriver);
				Environment.SetEnvironmentVariable("SDL_VIDEODRIVER", null);
			}
		}
	}
}
