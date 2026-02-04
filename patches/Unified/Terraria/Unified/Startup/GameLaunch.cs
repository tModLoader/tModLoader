using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReLogic.OS;

namespace Terraria.Unified.Startup;

/// <summary>
///		Responsible for launching the game and managing its lifetime.
/// </summary>
public static class GameLaunch
{
	/// <summary>
	///		The host container and lifetime of the game.
	/// </summary>
	public static GameLifetime Instance {
		get => field ?? throw new InvalidOperationException("Cannot access game lifetime before the game has started");
		private set;
	}

	#region Program member contracts
	// The following members are from the original Program implementation and
	// are depended on by various parts of the game.

	/// <summary>
	///		Whether the game is running under XNA; always false.
	/// </summary>
	internal static bool IsXna => false;

	/// <summary>
	///		Whether the game is running under FNA; always true.
	/// </summary>
	internal static bool IsFna => true;

	/// <summary>
	///		Whether the game is running under Mono; always false.
	///		<br />
	///		It's technically possible for the game to be running using Mono,
	///		but any checks that use this do not matter.
	/// </summary>
	internal static bool IsMono => false;

	/// <summary>
	///		Parsed launch arguments.
	/// </summary>
	internal static Dictionary<string, string> LaunchParameters { get; private set; }

	/// <summary>
	///		Whether the main Terraria assembly has been fully manually JITed
	///		and has had its static members all initialized.
	/// </summary>
	internal static bool LoadedEverything => Instance.Host.Services.GetRequiredService<IPreJitPolicy>().FinishedLoading;

	/// <summary>
	///		The root directory in which game content is saved.  This is
	///		typically per-user and stored separately, but may be the game's
	///		root in cases where it isn't accessible.
	/// </summary>
	internal static string SavePath { get; private set; }
	#endregion

	internal static void StartGame(string[] args)
	{
		Thread.CurrentThread.Name = "Main Thread";

		args = Utils.ConvertMonoArgsToDotNet(args);
		LaunchParameters = Utils.ParseArguements(args);

		SavePath = LaunchParameters.TryGetValue("-savedirectory", out string savePath) ? savePath : Platform.Get<IPathService>().GetStoragePath("Terraria");
		Main.dedServ = LaunchParameters.ContainsKey("-server");

		var host = Host.CreateDefaultBuilder(args)
			.ConfigureLogging(logging => {
				Logging.Initialize(logging);

				try {
					Console.OutputEncoding = Encoding.UTF8;
					Console.InputEncoding = Platform.IsWindows ? Encoding.Unicode : Encoding.UTF8;
				}
				catch {
					// no-op
				}
			})
			.ConfigureServices(services => {
				services.AddSingleton<INativeLibraryResolver, NativeLibraryResolver>();
				services.AddSingleton<IEngineBackendInitializer, EngineBackendInitializer>();
				services.AddSingleton<IEngineRunner, EngineRunner>();
				services.AddSingleton<IPreJitPolicy, DefaultPreJitPolicy>();
			})
			.Build();

		var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
		Logging.RedirectConsole(loggerFactory);

		var logger = loggerFactory.CreateLogger("Terraria");
		logger.LogInformation("Using launch arguments:");
		foreach (var (key, value) in LaunchParameters) {
			if (string.IsNullOrEmpty(value)) {
				logger.LogInformation($"{0}", key);
			}
			else {
				logger.LogInformation($"{key}: {value}");
			}
		}

		Instance = new GameLifetime(host, logger);

		host.Services.GetRequiredService<INativeLibraryResolver>().Initialize();
		host.Services.GetRequiredService<IEngineBackendInitializer>().Initialize();
		host.Services.GetRequiredService<IEngineRunner>().Run();

		host.StopAsync().GetAwaiter().GetResult();
		host.Dispose();
	}
}
