using log4net;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Terraria.ModLoader.Engine;

internal class TerrariaSteamClient
{
	private static ILog Logger { get; } = LogManager.GetLogger("TerrariaSteamClient");

	private const int LatestTerrariaBuildID = 9653812; // Currently v1.4.4.4. Update this when any Terraria update changes any asset. Also update InitTMLContentManager with a newly added file
	private static AnonymousPipeServerStream serverPipe;

	private static string MsgInitFailed = "init_failed";
	private static string MsgInitSuccess = "init_success";
	private static string MsgFamilyShared = "family_shared";
	private static string MsgNotInstalled = "not_installed";
	private static string MsgInstallOutOfDate = "install_out_of_date";
	private static string MsgGrant = "grant:";
	private static string MsgAck = "acknowledged";
	private static string MsgShutdown = "shutdown";

	public enum LaunchResult
	{
		ErrClientProcDied,
		ErrSteamInitFailed,
		ErrNotInstalled,
		ErrInstallOutOfDate,
		Ok
	}

	internal static LaunchResult Launch()
	{
		if (Environment.GetEnvironmentVariable("SteamClientLaunch") != "1") {
			Logger.Debug("Disabled. Launched outside steam client.");
			return LaunchResult.Ok;
		}

		serverPipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
		var tMLName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
		var proc = new Process() {
			StartInfo = {
				FileName = Environment.ProcessPath,
				Arguments = $"{tMLName} -terrariasteamclient {serverPipe.GetClientHandleAsString()}",
				UseShellExecute = false,
				RedirectStandardOutput = true
			}
		};

		// clear steam env vars
		foreach (var k in ((IEnumerable<string>)proc.StartInfo.EnvironmentVariables.Keys).ToArray()) {
			if (k.StartsWith("steam", StringComparison.InvariantCultureIgnoreCase)) {
				proc.StartInfo.EnvironmentVariables.Remove(k);
			}
		}

		proc.Start();

		while (true) {
			var line = proc.StandardOutput.ReadLine()?.Trim();
			if (line == null) {
				if (proc.HasExited)
					return LaunchResult.ErrClientProcDied;

				continue;
			}

			Logger.Debug("Recv: " + line);

			if (line == MsgInitFailed)
				return LaunchResult.ErrSteamInitFailed;

			if (line == MsgNotInstalled)
				return LaunchResult.ErrNotInstalled;

			if (line == MsgInstallOutOfDate)
				return LaunchResult.ErrInstallOutOfDate;

			if (line == MsgInitSuccess)
				break;

			// Workaround for #881 family shared
			if (line == MsgFamilyShared) {
				Social.Steam.SteamedWraps.FamilyShared = true;
			}
		}

		SendCmd(MsgAck);

		// may need to put a Thread.Sleep here if there are issues with the steam current game reporting Terraria rather than tModLoader
		return LaunchResult.Ok;
	}

	private static void SendCmd(string cmd)
	{
		if (serverPipe == null)
			return;

		Logger.Debug("Send: " + cmd);
		using var sw = new StreamWriter(serverPipe, leaveOpen: true);
		sw.WriteLine(cmd);
	}

	internal static void Run()
	{
		Logging.Init(Logging.LogFile.TerrariaSteamClient);
		Logger.InfoFormat("Working Directory: {0}", Path.GetFullPath(Directory.GetCurrentDirectory()));
		Logger.InfoFormat("Args: {0}", string.Join(' ', Environment.GetCommandLineArgs()));
		Logger.Info("Setting steam app id to " + Steam.TerrariaAppId_t);
		Steam.SetAppId(Steam.TerrariaAppId_t);

		bool steamInit = false;
		try {
			using var clientPipe = new AnonymousPipeClientStream(PipeDirection.In, Program.LaunchParameters["-terrariasteamclient"]);
			using var sr = new StreamReader(clientPipe, leaveOpen: true);
			var Recv = () => {
				var s = sr.ReadLine()?.Trim();
				if (s == null)
					throw new EndOfStreamException();

				Logger.Debug("Recv: " + s);
				return s;
			};
			var Send = (string s) => {
				Logger.Debug("Send: " + s);
				Console.WriteLine(s);
			};


			Logger.Info("SteamAPI.Init()");
			steamInit = SteamAPI.Init();
			if (!steamInit) {
				Logger.Fatal("SteamAPI.Init() failed");
				Send(MsgInitFailed);
				return;
			}

			if (!SteamApps.BIsAppInstalled(Steam.TerrariaAppId_t)) {
				Logger.Fatal($"SteamApps.BIsAppInstalled({Steam.TerrariaAppId_t}): false");
				Send(MsgNotInstalled);
				SteamShutdown();
				return;
			}

			int TerrariaBuildID = SteamApps.GetAppBuildId();
			Logger.Info("Terraria BuildID: " + TerrariaBuildID);
			if (TerrariaBuildID < LatestTerrariaBuildID) { 
				Logger.Fatal("Terraria is out of date, you need to update Terraria in Steam.");
				Send(MsgInstallOutOfDate);
				SteamShutdown();
				return;
			}

			// Unfortunately, Valve doesn't support tModLoader for Family-shared Terraria, which has lead to this workaround.
			// Does not support Steam Overlay or Steam multiplayer as such.
			if (SteamApps.BIsSubscribedFromFamilySharing()) {
				Logger.Info("Terraria is installed via Family Share. Re-pathing tModLoader required");
				Send(MsgFamilyShared);
			}

			Send(MsgInitSuccess);

			while (Recv() != MsgAck) { }

			// message loop
			while (true) {
				Thread.Sleep(250);

				var nextCMD = Recv();
				if (nextCMD == MsgShutdown) // graceful shutdown
					break;

				if (nextCMD.StartsWith(MsgGrant)) {
					string achievement = nextCMD.Substring(MsgGrant.Length);

					SteamUserStats.GetAchievement(achievement, out bool pbAchieved);
					if (!pbAchieved)
						SteamUserStats.SetAchievement(achievement);
				}
			}
		}
		catch (EndOfStreamException) {
			Logger.Info("The connection to tML was closed unexpectedly. Look in client.log or server.log for details");
		}
		catch (Exception ex) {
			Logger.Fatal("Unhandled error", ex);
		}

		if (steamInit)
			SteamShutdown();
	}

	private static void SteamShutdown()
	{
		try {
			Logger.Info("SteamAPI.Shutdown()");
			SteamAPI.Shutdown();
		}
		catch (Exception ex) {
			Logger.Error("Error shutting down SteamAPI", ex);
		}
	}

	internal static void Shutdown()
	{
		try {
			SendCmd(MsgShutdown);
		} catch { }
	}
}
