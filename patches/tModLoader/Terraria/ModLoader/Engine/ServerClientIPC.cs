using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Engine;
internal class ServerClientIPC
{
	private enum IsWaiting
	{
		None,
		DisplayName,
		Version
	}

	private static ILog Logger { get; } = LogManager.GetLogger("ServerClientIPC");

	public static NamedPipeServerStream ServerPipe;
	public static NamedPipeClientStream ClientPipe;

	public const string MsgLoadStage = "load:";
	public const string MsgCurrentCount = "count:";
	public const string MsgSendDisplayName = "send_display_name";
	public const string MsgSendVersion = "send_version";
	public const string MsgCommitMod = "commit_mod";
	public const string MsgDone = "done_load";

	public static NamedPipeClientStream InitClient()
	{
		if (ClientPipe == null) {
			ClientPipe = new NamedPipeClientStream(".", "TML.ServerClientIPC", PipeDirection.Out);
		}
		return ClientPipe;
	}

	public static NamedPipeServerStream InitServer()
	{
		if (ServerPipe == null) {
			ServerPipe = new NamedPipeServerStream("TML.ServerClientIPC", PipeDirection.In);
		}
		return ServerPipe;
	}

	public static void ShutdownClient()
	{
		if (ClientPipe != null) {
			SendCmd(MsgDone);
			ClientPipe.Dispose();
			ClientPipe = null;
		}
	}

	public static void ShutdownServer()
	{
		if (ServerPipe != null) {
			ServerPipe.Dispose();
			ServerPipe = null;
		}
	}

	public static void SendCmd(string cmd)
	{
		if (ClientPipe == null)
			return;

		using var sw = new StreamWriter(ClientPipe, leaveOpen: true);
		sw.WriteLine(cmd);
	}

	public static void SendCurrentStage(string stageText, int modCount = -1)
	{
		SendCmd($"{MsgLoadStage}{stageText}:{modCount}");
	}

	public static void SendCurrentMod(int i, string displayName, Version version)
	{
		SendCmd($"{MsgCurrentCount}{i}");
		SendCmd(MsgSendDisplayName);
		SendCmd(displayName);
		SendCmd(MsgSendVersion);
		SendCmd(version.ToString());
		SendCmd(MsgCommitMod);
	}

	public static void Run(CancellationToken token)
	{
		Task.Run(() => {
			if (ServerPipe == null)
				return;

			try {
				using (StreamReader sr = new StreamReader(ServerPipe, leaveOpen: true)) {
					var Recv = () => {
						var s = sr.ReadLine().Trim();
						if (s == null) {
							throw new EndOfStreamException();
						}

						Logger.Debug($"Recieved \"{s}\"");

						return s;
					};

					ServerPipe.WaitForConnection();

					IsWaiting waiting = IsWaiting.None;
					string displayName = string.Empty;
					string version = string.Empty;
					int currentCount = 0;

					while (true) {
						var cmd = Recv();

						if (cmd == MsgDone)
							break;

						switch (waiting) {
							case IsWaiting.DisplayName:
								displayName = cmd;
								waiting = IsWaiting.None;
								break;
							case IsWaiting.Version:
								version = cmd;
								waiting = IsWaiting.None;
								break;
						}

						if (cmd.StartsWith(MsgLoadStage)) {
							string[] data = cmd.Substring(MsgLoadStage.Length).Split(':');
							if (data.Length != 2)
								throw new InvalidOperationException($"The command \"{cmd}\" is not valid");

							string stage = data[0];
							int modCount = int.Parse(data[1]);

							Interface.startServer.SetLoadStage(stage, modCount);
						}
						else if (cmd.StartsWith(MsgCurrentCount)) {
							currentCount = int.Parse(cmd.Substring(MsgCurrentCount.Length));
						}
						else if (cmd == MsgSendDisplayName) {
							waiting = IsWaiting.DisplayName;
							continue;
						}
						else if (cmd == MsgSendVersion) {
							waiting = IsWaiting.Version;
							continue;
						}
						else if (cmd == MsgCommitMod) {
							Interface.startServer.SetCurrentMod(currentCount, displayName, version);
							currentCount = 0;
							displayName = string.Empty;
							version = string.Empty;
						}
					}

					Netplay.SetRemoteIP("127.0.0.1");
					Main.autoPass = true;
					Netplay.StartTcpClient();
					Main.statusText = Lang.menu[8].Value;
				}
			}
			catch when (token.IsCancellationRequested) {
				// Silently catch any errors because the server pipe being closed by UIStartServer should be the cause of it
			}
		}, token);
	}
}
