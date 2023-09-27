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
		}

		ConfigureDrivers();
	}

	private static void ConfigureDrivers()
	{
		// https://wiki.libsdl.org/SDL2/FAQUsingSDL
		// Hints in https://github.com/libsdl-org/SDL/blob/release-2.28.x/include/SDL_hints.h#L2384
		// Note that env var names change in SDL 3.x

		// Default driver order https://github.com/libsdl-org/SDL/blob/release-2.28.x/src/video/SDL_video.c#L67
		ConfigureDrivers("SDL_VIDEODRIVER", "-videodriver", "cocoa,x11,wayland,vivante,directfb,windows,winrt,haiku,uikit,android,ps2,psp,vita,n3ds,kmsdrm,riscos,rpi,nacl,emscripten,qnx,ngage,offscreen,dummy,evdev");

		// Default driver order https://github.com/libsdl-org/SDL/blob/release-2.28.x/src/audio/SDL_audio.c#L38
		ConfigureDrivers("SDL_AUDIODRIVER", "-audiodriver", "pulseaudio,alsa,sndio,netbsd,qsa,wasapi,directsound,haiku,coreaudio,aaudio,openslES,android,ps2,psp,vita,n3ds,emscripten,jack,pipewire,dsp,disk,dummy");
	}

	private static void ConfigureDrivers(string sdlEnvVar, string launchArg, string defaultDriverList)
	{
		if (Program.LaunchParameters.TryGetValue(launchArg, out var launchArgValue)) {
			Environment.SetEnvironmentVariable(sdlEnvVar, launchArgValue);
		}

		// Append the default driver list, in case of an old or unsupported env var left on the system (See #2926)
		if (Environment.GetEnvironmentVariable(sdlEnvVar) is string envVarValue) {
			Logging.FNA.Info($"Detected {sdlEnvVar}={envVarValue}. Appending default driver list as fallbacks.");
			Environment.SetEnvironmentVariable(sdlEnvVar, $"{envVarValue},{defaultDriverList}");
		}
	}
}
