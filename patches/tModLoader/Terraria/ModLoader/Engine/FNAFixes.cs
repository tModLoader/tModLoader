using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SDL3;

namespace Terraria.ModLoader.Engine;

internal static class FNAFixes
{
	internal static void Init()
	{
		if (Main.dedServ)
			return;

		Environment.SetEnvironmentVariable("FNA_SOUNDEFFECT_UNCAPPED_PITCH", "1");

		if (OperatingSystem.IsWindows()) {
			// FNA sets this to "1" on Windows. Terraria does not want this. See #2020
			SDL.SDL_SetHintWithPriority(SDL.SDL_HINT_VIDEO_MINIMIZE_ON_FOCUS_LOSS, "0", SDL.SDL_HintPriority.SDL_HINT_OVERRIDE);
		}

		// Probably not needed anymore with SDL3, since we start/stop text input when focusing text boxes now
		/*
		if (Environment.GetEnvironmentVariable("SteamDeck") is string steamDeckValue && steamDeckValue == "1") {
			Logging.FNA.Info("SteamDeck detected, configuring keyboard input workaround.");
			SDL.SDL_SetHintWithPriority("SDL_ENABLE_SCREEN_KEYBOARD", "0", SDL.SDL_HintPriority.SDL_HINT_OVERRIDE);
			// SDL.SDL_HINT_ENABLE_SCREEN_KEYBOARD const only exists in SDL 2.28+, but FNA is currently targeting 2.26.0, so we use string directly.
			SDL.SDL_StartTextInput();
		}
		*/

		TryCopyAgilitySDK();
		EnableHighDPI();
		ConfigureDrivers();
	}

	private static void TryCopyAgilitySDK()
	{
		if (!OperatingSystem.IsWindows()) return;

		var dllPath = Path.Combine(MonoLaunch.NativesDir, "D3D12Core.dll");
		var targetPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "D3D12", "D3D12Core.dll");
		if (File.Exists(targetPath) && File.ReadAllBytes(targetPath).SequenceEqual(File.ReadAllBytes(dllPath))) return;

		try {
			using var _ = new Logging.QuietExceptionHandle();
			Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
			File.Copy(dllPath, targetPath, overwrite: true);
		} catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
			Logging.FNA.Warn($"Failed to install Agility SDK to \"{targetPath}\"");
		}
	}

	private static void EnableHighDPI()
	{
		// Don't know if this is needed, maybe it helps us get resolutions? - Chicken Bones, SDL3 update

		// High DPI is used by Windows for its 'SCALE' setting. This ensures that 200% SCALE does not break the game rendering
		// On OSX, it creates issues and it is unknown whether this is needed on Linux. -- Solxan, forward port PR#4951 during 1.4.5
		if (OperatingSystem.IsWindows()) {
			[DllImport("user32.dll")]
			static extern bool SetProcessDPIAware();
			SetProcessDPIAware();

			Environment.SetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI", "1");
		}
	}

	private static void ConfigureDrivers()
	{
		// https://github.com/libsdl-org/SDL/blob/main/docs/README-migration.md and https://wiki.libsdl.org/SDL3/EnvironmentVariables
		ConfigureDrivers("SDL_VIDEODRIVER", "SDL_VIDEO_DRIVER", "-videodriver", SDL.SDL_GetNumVideoDrivers(), SDL.SDL_GetVideoDriver);
		ConfigureDrivers("SDL_AUDIODRIVER", "SDL_AUDIO_DRIVER", "-audiodriver", SDL.SDL_GetNumAudioDrivers(), SDL.SDL_GetAudioDriver);
	}

	private static void ConfigureDrivers(string sdlHintName, string sdl3HintName, string launchArg, int numDrivers, Func<int, string> getDriver)
	{
		var drivers = Enumerable.Range(0, numDrivers).Select(getDriver).Where(n => n != null).ToArray();
		var defaultDriverString = string.Join(",", drivers);

		if (Program.LaunchParameters.TryGetValue(launchArg, out var launchArgValue)) {
			Environment.SetEnvironmentVariable(sdl3HintName, launchArgValue);
		}

		// Append the default driver list, in case of an old or unsupported env var left on the system (See #2926)
		if ((Environment.GetEnvironmentVariable(sdlHintName) ?? Environment.GetEnvironmentVariable(sdl3HintName)) is string envVarValue) {
			Logging.FNA.Info($"Detected {sdlHintName}={envVarValue}. Appending default driver list as fallback: {defaultDriverString}");
			SDL3.SDL.SDL_SetHintWithPriority(sdlHintName, $"{envVarValue},{defaultDriverString}", SDL3.SDL.SDL_HintPriority.SDL_HINT_OVERRIDE);
		}
	}
}
