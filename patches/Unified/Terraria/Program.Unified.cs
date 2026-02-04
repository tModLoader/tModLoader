using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Terraria.Unified.Startup;

namespace Terraria;

/// <summary>
///		This is a rewritten implementation of the game's entrypoint to
///		facilitate our intended program lifetime, logging, etc.
/// </summary>
internal static class Program
{
	public static bool IsXna => GameLaunch.IsXna;

	public static bool IsFna => GameLaunch.IsFna;

	public static bool IsMono => GameLaunch.IsMono;

	/// <summary>
	///		Parsed launch arguments.
	/// </summary>
	public static Dictionary<string, string> LaunchParameters => GameLaunch.LaunchParameters;

	/// <summary>
	///		Whether the main Terraria assembly has been fully manually JITed
	///		and has had its static members all initialized.
	/// </summary>
	public static bool LoadedEverything => GameLaunch.LoadedEverything;

	/// <summary>
	///		The root directory in which game content is saved.  This is
	///		typically per-user and stored separately, but may be the game's
	///		root in cases where it isn't accessible.
	/// </summary>
	public static string SavePath => GameLaunch.SavePath;

	public static void Main(string[] args)
	{
		GameLaunch.StartGame(args);
	}
}
