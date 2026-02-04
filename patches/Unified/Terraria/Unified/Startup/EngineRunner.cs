using System;
using System.IO;
using Microsoft.Extensions.Logging;
using ReLogic.OS;
using Terraria.Initializers;
using Terraria.Localization;
using Terraria.Social;
using Terraria.Utilities;

namespace Terraria.Unified.Startup;

public interface IEngineRunner
{
	void Run();
}

internal sealed class EngineRunner(ILogger<EngineRunner> logger, IPreJitPolicy policy) : IEngineRunner
{
	void IEngineRunner.Run()
	{
		var culture = GameCulture.DefaultCulture;
		logger.LogInformation("Setting to culture: {0}", culture.CultureInfo);
		LanguageManager.Instance.SetLanguage(GameCulture.DefaultCulture);

		logger.LogDebug("Registering engine load events...");
		if (Platform.IsOSX) {
			Main.OnEngineLoad += delegate {
				Main.instance.IsMouseVisible = false;
			};
		}
		else if (Platform.IsWindows) {
			Main.OnEngineLoad += delegate {
				Platform.Get<IMouseNotifier>()?.AddMouseHandler(delegate (bool connected) {
					if (connected) {
						Main.instance.IsMouseVisible = true;
						Main.instance.ReHideCursor = true;
					}
				});
			};
		}

		try {
			logger.LogDebug("Initializing social API...");
			SocialAPI.Initialize();

			logger.LogDebug("Initializing game engine...");
			using var main = new Main();

			logger.LogDebug("Initializing legacy localization maps...");
			Lang.InitializeLegacyLocalization();

			logger.LogDebug("Loading launch parameters...");
			LaunchInitializer.LoadParameters(main);

			Main.OnEnginePreload += StartForceLoad;

			logger.LogInformation("Entering main game/server loop...");
			if (Main.dedServ)
				main.DedServ();
			else
				main.Run();
		}
		catch (Exception e) {
			DisplayException(e);
		}
	}

	private void StartForceLoad()
	{
		logger.LogDebug("Beginning pre-JIT force load...");

		policy.InitializeAssemblies();
	}

	private void DisplayException(Exception e)
	{
		try {
			var errorMessage = e.ToString();

			logger?.LogCritical(e, "Unhandled engine exception");

			if (WorldGen.isGeneratingOrLoadingWorld) {
				try {
					var genText = $"Error occurred while creating world - Seed: {Main.ActiveWorldFileData.SeedText} Width: {Main.maxTilesX}, Height: {Main.maxTilesY}, Evil: {WorldGen.WorldGenParam_Evil}, IsExpert: {Main.expertMode}";
					errorMessage = genText + '\n' + errorMessage;
					logger?.LogCritical(genText);
				}
				catch {
				}
			}

			if (Main.dedServ) {
				Console.WriteLine(Language.GetTextValue("Error.ServerCrash"), DateTime.Now, errorMessage);
			}

			MessageBox.Show(errorMessage, "Terraria: Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
		catch {
		}
	}
}
