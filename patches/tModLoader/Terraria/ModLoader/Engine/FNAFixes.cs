using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SDL3;
using Terraria.Utilities;

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

		TryLinkOrCopyAgilitySDK();
		EnableHighDPI();
		ConfigureDrivers();
	}

	private static void TryLinkOrCopyAgilitySDK()
	{
		if (!OperatingSystem.IsWindows()) return;

		var sdkDir = MonoLaunch.NativesDir;
		var processDir = Path.GetDirectoryName(Environment.ProcessPath);
		if (Path.GetRelativePath(processDir, sdkDir) is string relPath && relPath != sdkDir) {
			Logging.FNA.Info($"Relative path to Agility SDK: \"{relPath}\"");
			SDL.SDL_SetHintWithPriority("FNA3D_SDL_AGILITY_SDK_PATH", relPath, SDL.SDL_HintPriority.SDL_HINT_OVERRIDE);
			return;
		}

		var sdkPath = Path.Combine(MonoLaunch.NativesDir, "D3D12Core.dll");
		var targetPath = Path.Combine(processDir, "D3D12", "D3D12Core.dll");
		if (File.Exists(targetPath) && File.ReadAllBytes(targetPath).SequenceEqual(File.ReadAllBytes(sdkPath))) return;

		try {
			using var _ = new Logging.QuietExceptionHandle();
			Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
			File.Copy(sdkPath, targetPath, overwrite: true);
		} catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
			Logging.FNA.Warn($"Failed to install Agility SDK to \"{targetPath}\"");

			// For developers of tModLoader itself running system dotnet, we'll need elevated permissions to copy the file to the destination.
			if (Debugger.IsAttached) {
				try {
					ElevatedCopy(sdkPath, targetPath);
				}
				catch { }
			}
		}
	}

	private static void ElevatedCopy(string source, string destination)
	{
		DialogResult result = MessageBox.Show($"""
			tModLoader would like to copy a file to "{destination}", which requires elevated permissions.

			Press "OK" to close this window and then "Yes" when asked "Do you want to allow this app to make changes to your device?" to proceed.

			You should only see this message if you are developing tModLoader itself on Windows 10 or earlier.
			""", "Elevated file copy", buttons: MessageBoxButtons.OKCancel, icon: MessageBoxIcon.Error);

		if(result != DialogResult.OK) {
			Logging.FNA.Warn($"User opted out of elevated copy to \"{destination}\".");
			return;
		}
		Logging.FNA.Warn($"Attempting elevated copy to \"{destination}\".");

		ProcessStartInfo processInfo = new ProcessStartInfo {
			FileName = "cmd.exe",
			// /c carries out the command and then terminates
			Arguments = $"/c xcopy \"{source}\" \"{destination}*\"", // * needed so xcopy treats destination as file instead of asking. xcopy instead copy since we need to create the subfolder as well.
			Verb = "runas", // This triggers the UAC prompt
			UseShellExecute = true, // Required for 'runas'
			WindowStyle = ProcessWindowStyle.Hidden
		};

		try {
			Process.Start(processInfo);
		}
		catch (System.ComponentModel.Win32Exception) {
			Logging.FNA.Warn($"User cancelled the UAC prompt");
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
