using SDL2;
using System;
using System.Linq;

namespace Terraria.ModLoader.Engine;

internal static class FNAFixes
{
	internal static void Init()
	{
		if (OperatingSystem.IsWindows()) {
			// FNA sets this to "1" on Windows. Terraria does not want this. See #2020
			SDL.SDL_SetHint(SDL.SDL_HINT_VIDEO_MINIMIZE_ON_FOCUS_LOSS, "0" );
		}

		ConfigureDrivers();
	}

	private static void ConfigureDrivers()
	{
		// https://wiki.libsdl.org/SDL2/FAQUsingSDL
		// Hints in https://github.com/libsdl-org/SDL/blob/release-2.28.x/include/SDL_hints.h#L2384
		// Note that env var names change in SDL 3.x

		ConfigureDrivers("SDL_VIDEODRIVER", "-videodriver", SDL.SDL_GetNumVideoDrivers(), SDL.SDL_GetVideoDriver);
		ConfigureDrivers("SDL_AUDIODRIVER", "-audiodriver", SDL.SDL_GetNumAudioDrivers(), SDL.SDL_GetAudioDriver);
	}

	private static void ConfigureDrivers(string sdlHintName, string launchArg, int numDrivers, Func<int, string> getDriver)
	{
		var drivers = Enumerable.Range(0, numDrivers).Select(getDriver).Where(n => n != null).ToArray();
		var defaultDriverString = string.Join(",", drivers);

		if (Program.LaunchParameters.TryGetValue(launchArg, out var launchArgValue)) {
			Environment.SetEnvironmentVariable(sdlHintName, launchArgValue);
		}

		// Append the default driver list, in case of an old or unsupported env var left on the system (See #2926)
		if (Environment.GetEnvironmentVariable(sdlHintName) is string envVarValue) {
			Logging.FNA.Info($"Detected {sdlHintName}={envVarValue}. Appending default driver list as fallback: {defaultDriverString}");
			SDL.SDL_SetHintWithPriority(sdlHintName, $"{envVarValue},{defaultDriverString}", SDL.SDL_HintPriority.SDL_HINT_OVERRIDE);
		}
	}
}
