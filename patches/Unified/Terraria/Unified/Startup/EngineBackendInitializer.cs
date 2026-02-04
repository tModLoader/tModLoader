using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.OS;

namespace Terraria.Unified.Startup;

public interface IEngineBackendInitializer
{
	void Initialize();
}

internal sealed class EngineBackendInitializer(ILogger<EngineBackendInitializer> logger) : IEngineBackendInitializer
{
	void IEngineBackendInitializer.Initialize()
	{
		LogLibraryVersions();
	}

	private void LogLibraryVersions()
	{
		var terrariaVersion = typeof(Program).Assembly.GetName().Version;
		var fnaVersion = typeof(SpriteBatch).Assembly.GetName().Version;
		uint fna3dVersion = FNA3D.FNA3D_LinkedVersion();
		uint faudioVersion = FAudio.FAudioLinkedVersion();

		logger.LogInformation("Terraria v{0}", terrariaVersion);
		logger.LogInformation("Terraria Unified v{0}", Main.UnifiedVersion);
		logger.LogInformation("FNA v{0}", fnaVersion);
		logger.LogInformation("FNA3D v{0}", fna3dVersion / 10000 + "." + fna3dVersion / 100 % 100 + "." + fna3dVersion % 100);
		logger.LogInformation("FAudio v{0}", faudioVersion / 10000 + "." + faudioVersion / 100 % 100 + "." + faudioVersion % 100);
		logger.LogInformation("SDL v{0}", SDL3.SDL.SDL_GetVersion() + "." + SDL3.SDL.SDL_GetRevision());

		logger.LogInformation("Using save path: {0}", GameLaunch.SavePath);
		logger.LogInformation("Running as server: {0}", Main.dedServ);

		// Remove bad environment variables that might be set by other app installs
		if (Main.dedServ) {
			Environment.SetEnvironmentVariable("FNA_PLATFORM_BACKEND", "NONE");
		}
		else {
			Environment.SetEnvironmentVariable("FNA_PLATFORM_BACKEND", "SDL3");
		}

		ThreadPool.SetMinThreads(8, 8);
		Platform.Get<IWindowService>().SetQuickEditEnabled(enabled: false);
	}
}
