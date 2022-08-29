using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.Net;
using Terraria.UI;

namespace Terraria.ModLoader
{
	public static class ModNet
	{
		internal class ModHeader
		{
			public string name;
			public Version version;
			public byte[] hash;
			public bool signed;
			public string path;

			public ModHeader(string name, Version version, byte[] hash, bool signed) {
				this.name = name;
				this.version = version;
				this.hash = hash;
				this.signed = signed;
				path = Path.Combine(ModLoader.ModPath, name + ".tmod");
			}

			public bool Matches(TmodFile mod) => name == mod.Name && version == mod.Version && hash.SequenceEqual(mod.Hash);
			public override string ToString() => $"{name} v{version}[{string.Concat(hash[..4].Select(b => b.ToString("x2")))}]";
		}

		internal class NetConfig
		{
			public string modname;
			public string configname;
			public string json;

			public NetConfig(string modname, string configname, string json) {
				this.modname = modname;
				this.configname = configname;
				this.json = json;
			}

			public override string ToString() => $"{modname}:{configname} {json}";
		}

		public static bool AllowVanillaClients { get; internal set; }
		internal static bool downloadModsFromServers = true;
		internal static bool onlyDownloadSignedMods = false;

		internal static bool[] isModdedClient = new bool[256];

		private static Mod[] netMods;

		public static bool IsModdedClient(int i) => isModdedClient[i];

		public static Mod GetMod(int netID) =>
			netID >= 0 && netID < netMods.Length ? netMods[netID] : null;

		public static int NetModCount => netMods.Length;

		internal static bool ShouldDrawModNetDiagnosticsUI = false;
		internal static INetDiagnosticsUI ModNetDiagnosticsUI { get; private set; }

		private static Queue<ModHeader> downloadQueue = new Queue<ModHeader>();
		internal static List<NetConfig> pendingConfigs = new List<NetConfig>();
		private static ModHeader downloadingMod;
		private static FileStream downloadingFile;
		private static long downloadingLength;

		/// <summary>
		/// Update every time a change is pushed to stable which is incompatible between server and clients. Ignored if not updated each month.
		/// </summary>
		private static Version IncompatiblePatchVersion = new(2022, 1, 1, 1);
		private static Version? StableNetVersion { get; } = !BuildInfo.IsStable ? null : IncompatiblePatchVersion.MajorMinor() == BuildInfo.tMLVersion.MajorMinor() ? IncompatiblePatchVersion : BuildInfo.tMLVersion.MajorMinorBuild();
		internal static string NetVersionString { get; } = BuildInfo.versionedName + (StableNetVersion != null ? "!" + StableNetVersion : "");
		static ModNet() {
			if (Main.dedServ && StableNetVersion != null)
				Logging.tML.Debug($"Network compatibility version is {StableNetVersion}");
		}

		internal static bool IsClientCompatible(string clientVersion, out bool isModded, out string kickMsg) {
			kickMsg = null;
			isModded = clientVersion.StartsWith("tModLoader");
			if (AllowVanillaClients && clientVersion == "Terraria" + Main.curRelease)
				return true;

			if (clientVersion == NetVersionString)
				return true;

			var split = clientVersion.Split('!');
			if (StableNetVersion != null
					&& split.Length == 2
					&& Version.TryParse(split[1], out var netVer)
					&& netVer == StableNetVersion) {

				Logging.tML.Debug($"Client has {split[0]}, assuming net compatibility");
				return true;
			}

			kickMsg = isModded
				? $"You are on {split[0]}, server is on {BuildInfo.versionedName}"
				: "You cannot connect to a tModLoader Server with an unmodded client";
			return false;
		}

		internal static void AssignNetIDs() {
			netMods = ModLoader.Mods.Where(mod => mod.Side != ModSide.Server).ToArray();
			for (short i = 0; i < netMods.Length; i++)
				netMods[i].netID = i;
		}

		internal static void Unload() {
			netMods = null;
			if (!Main.dedServ && Main.netMode != 1) //disable vanilla client compatibility restrictions when reloading on a client
				AllowVanillaClients = false;
		}

		internal static void SyncMods(int clientIndex) {
			var p = new ModPacket(MessageID.SyncMods);
			p.Write(AllowVanillaClients);

			var syncMods = ModLoader.Mods.Where(mod => mod.Side == ModSide.Both).ToList();
			AddNoSyncDeps(syncMods);

			p.Write(syncMods.Count);
			foreach (Mod mod in syncMods) { // We only sync ServerSide configs for ModSide.Both. ModSide.Server can have
				p.Write(mod.Name);
				p.Write(mod.Version.ToString());
				p.Write(mod.File.Hash);
				p.Write(mod.File.ValidModBrowserSignature);
				SendServerConfigs(p, mod);
			}

			p.Send(clientIndex);
		}

		private static void AddNoSyncDeps(List<Mod> syncMods) {
			var queue = new Queue<Mod>(syncMods.Where(m => m.Side == ModSide.Both));
			while (queue.Count > 0) {
				foreach (Mod dep in AssemblyManager.GetDependencies(queue.Dequeue())) {
					if (dep.Side == ModSide.NoSync && !syncMods.Contains(dep)) {
						syncMods.Add(dep);
						queue.Enqueue(dep);
					}
				}
			}
		}

		private static void SendServerConfigs(ModPacket p, Mod mod) {
			if (!ConfigManager.Configs.TryGetValue(mod, out List<ModConfig> configs)) {
				p.Write(0);
				return;
			}

			ModConfig[] serverConfigs = configs.Where(x => x.Mode == ConfigScope.ServerSide).ToArray();
			p.Write(serverConfigs.Length);
			foreach (ModConfig config in serverConfigs) {
				string json = JsonConvert.SerializeObject(config, ConfigManager.serializerSettingsCompact);
				Logging.tML.Debug($"Sending Server Config {config.Mod.Name}:{config.Name} {json}");

				p.Write(config.Name);
				p.Write(json);
			}
		}

		internal static void SyncClientMods(BinaryReader reader) {
			if (!SyncClientMods(reader, out bool needsReload))
				return; //error syncing can't connect to server

			if (downloadQueue.Count > 0)
				DownloadNextMod();
			else
				OnModsDownloaded(needsReload);
		}

		// This method is split so that the local variables aren't held by the GC when reloading
		internal static bool SyncClientMods(BinaryReader reader, out bool needsReload) {
			AllowVanillaClients = reader.ReadBoolean();
			Logging.tML.Info($"Server reports AllowVanillaClients set to {AllowVanillaClients}");

			Main.statusText = Language.GetTextValue("tModLoader.MPSyncingMods");
			Mod[] clientMods = ModLoader.Mods;
			LocalMod[] modFiles = ModOrganizer.FindMods(); // TODO: find all versions of mods, regardless of if a local is present
			needsReload = false;
			downloadQueue.Clear();
			pendingConfigs.Clear();
			var syncList = new List<ModHeader>();
			var syncSet = new HashSet<string>();
			var blockedList = new List<ModHeader>();

			int n = reader.ReadInt32();
			for (int i = 0; i < n; i++) {
				var header = new ModHeader(reader.ReadString(), new Version(reader.ReadString()), reader.ReadBytes(20), reader.ReadBoolean());
				syncList.Add(header);
				syncSet.Add(header.name);

				int configCount = reader.ReadInt32();
				for (int c = 0; c < configCount; c++)
					pendingConfigs.Add(new NetConfig(header.name, reader.ReadString(), reader.ReadString()));

				Mod clientMod = clientMods.SingleOrDefault(m => m.Name == header.name);
				if (clientMod != null && header.Matches(clientMod.File))
					continue;

				needsReload = true;

				LocalMod[] localVersions = modFiles.Where(m => m.Name == header.name).ToArray();
				LocalMod matching = Array.Find(localVersions, mod => header.Matches(mod.modFile));
				if (matching != null) {
					matching.Enabled = true;
					continue;
				}

				// overwrite an existing version of the mod if there is one
				if (localVersions.Length > 0)
					header.path = localVersions[0].modFile.path;

				if (downloadModsFromServers && (header.signed || !onlyDownloadSignedMods))
					downloadQueue.Enqueue(header);
				else
					blockedList.Add(header);
			}

			Logging.tML.Debug($"Server mods: "+string.Join(", ", syncList));
			Logging.tML.Debug($"Download queue: "+string.Join(", ", downloadQueue));
			if (pendingConfigs.Any())
				Logging.tML.Debug($"Configs:\n\t\t" + string.Join("\n\t\t", pendingConfigs));


			foreach (Mod mod in clientMods)
				if (mod.Side == ModSide.Both && !syncSet.Contains(mod.Name)) {
					ModLoader.DisableMod(mod.Name);
					needsReload = true;
				}

			if (blockedList.Count > 0) {
				string msg = Language.GetTextValue("tModLoader.MPServerModsCantDownload");
				msg += downloadModsFromServers
					? Language.GetTextValue("tModLoader.MPServerModsCantDownloadReasonSigned")
					: Language.GetTextValue("tModLoader.MPServerModsCantDownloadReasonAutomaticDownloadDisabled");
				msg += ".\n" + Language.GetTextValue("tModLoader.MPServerModsCantDownloadChangeSettingsHint") + "\n";
				foreach (ModHeader mod in blockedList)
					msg += "\n    " + mod;

				Logging.tML.Warn(msg);
				Interface.errorMessage.Show(msg, 0);
				return false;
			}

			// ready to connect, apply configs. Config manager will apply the configs on reload automatically
			if (!needsReload) {
				foreach (NetConfig pendingConfig in pendingConfigs)
					JsonConvert.PopulateObject(pendingConfig.json, ConfigManager.GetConfig(pendingConfig), ConfigManager.serializerSettingsCompact);

				if (ConfigManager.AnyModNeedsReload()) {
					needsReload = true;
				}
				else {
					foreach (NetConfig pendingConfig in pendingConfigs)
						ConfigManager.GetConfig(pendingConfig).OnChanged();
				}
			}

			return true;
		}

		private static void DownloadNextMod() {
			downloadingMod = downloadQueue.Dequeue();
			downloadingFile = null;
			var p = new ModPacket(MessageID.ModFile);
			p.Write(downloadingMod.name);
			p.Send();
		}

		// Start sending the mod to the connecting client
		// First, send the initial mod name and length of the file stream
		// so the client knows what to expect
		internal const int CHUNK_SIZE = 16384;
		internal static void SendMod(string modName, int toClient) {
			Mod mod = ModLoader.GetMod(modName);
			if (mod.Side == ModSide.Server) // Prevent exposing server side mods to malicious clients
				return;
			string path = mod.File.path;
			FileStream fs = File.OpenRead(path);

			{
				//send file length
				var p = new ModPacket(MessageID.ModFile);
				p.Write(mod.DisplayName);
				p.Write(fs.Length);
				p.Send(toClient);
			}

			byte[] buf = new byte[CHUNK_SIZE];
			int count;

			while ((count = fs.Read(buf, 0, buf.Length)) > 0) {
				var p = new ModPacket(MessageID.ModFile, CHUNK_SIZE + 3);
				p.Write(buf, 0, count);
				p.Send(toClient);
			}

			fs.Close();
		}

		// Receive a mod when connecting to server
		internal static void ReceiveMod(BinaryReader reader) {
			if (downloadingMod == null)
				return;

			try {
				if (downloadingFile == null) {
					Interface.progress.Show(displayText: reader.ReadString(), cancel: CancelDownload);

					if (ModLoader.TryGetMod(downloadingMod.name, out var mod))
						mod.Close();

					downloadingLength = reader.ReadInt64();
					Logging.tML.Debug($"Downloading: {downloadingMod.name} {downloadingLength}bytes");
					downloadingFile = new FileStream(downloadingMod.path, FileMode.Create);
					return;
				}

				byte[] bytes = reader.ReadBytes((int)Math.Min(downloadingLength - downloadingFile.Position, CHUNK_SIZE));
				downloadingFile.Write(bytes, 0, bytes.Length);
				Interface.progress.Progress = downloadingFile.Position / (float)downloadingLength;

				if (downloadingFile.Position == downloadingLength) {
					downloadingFile.Close();

					var mod = new TmodFile(downloadingMod.path);

					using (mod.Open()) { }

					if (!downloadingMod.Matches(mod))
						throw new Exception(Language.GetTextValue("tModLoader.MPErrorModHashMismatch"));

					if (downloadingMod.signed && onlyDownloadSignedMods && !mod.ValidModBrowserSignature)
						throw new Exception(Language.GetTextValue("tModLoader.MPErrorModNotSigned"));

					ModLoader.EnableMod(mod.Name);

					if (downloadQueue.Count > 0)
						DownloadNextMod();
					else
						OnModsDownloaded(true);
				}
			}
			catch (Exception e) {
				try {
					downloadingFile?.Close();
					File.Delete(downloadingMod.path);
				}
				catch (Exception exc2) {
					Logging.tML.Error("Unknown error during mod sync", exc2);
				}

				string msg = Language.GetTextValue("tModLoader.MPErrorModDownloadError", downloadingMod.name);
				Logging.tML.Error(msg, e);
				Interface.errorMessage.Show(msg + e, 0);

				Netplay.Disconnect = true;
				downloadingMod = null;
			}
		}

		private static void CancelDownload() {
			try {
				downloadingFile?.Close();
				File.Delete(downloadingMod.path);
			}
			catch { }

			downloadingMod = null;
			Netplay.Disconnect = true;
		}

		private static void OnModsDownloaded(bool needsReload) {
			if (needsReload) {
				Main.netMode = 0;
				ModLoader.OnSuccessfulLoad = NetReload();
				ModLoader.Reload();
				return;
			}

			Main.netMode = 1;
			downloadingMod = null;
			netMods = null;
			foreach (Mod mod in ModLoader.Mods)
				mod.netID = -1;

			new ModPacket(MessageID.SyncMods).Send();
		}

		internal static bool NetReloadActive;
		internal static Action NetReload() {
			// Main.ActivePlayerFileData gets cleared during reload
			string path = Main.ActivePlayerFileData.Path;
			bool isCloudSave = Main.ActivePlayerFileData.IsCloudSave;
			NetReloadActive = true;
			return () => {
				NetReloadActive = false;
				// re-select the current player
				Player.GetFileData(path, isCloudSave).SetAsActive();
				//from Netplay.ClientLoopSetup
				Main.player[Main.myPlayer].hostile = false;
				Main.clientPlayer = (Player)Main.player[Main.myPlayer].clientClone();

				if (!Netplay.Connection.Socket.IsConnected()) {
					Main.menuMode = MenuID.Error;
					Logging.tML.Error("Disconnected from server during reload.");
					Main.statusText = "Disconnected from server during reload.";
				}
				else {
					Main.menuMode = MenuID.Status;
					Main.statusText = "Reload complete, joining...";
					OnModsDownloaded(false);
				}
			};
		}

		internal static void SendNetIDs(int toClient) {
			var p = new ModPacket(MessageID.ModPacket);
			p.Write(netMods.Length);
			foreach (Mod mod in netMods)
				p.Write(mod.Name);

			ItemLoader.WriteNetGlobalOrder(p);
			SystemLoader.WriteNetSystemOrder(p);
			p.Write(Player.MaxBuffs);

			p.Send(toClient);
		}

		private static void ReadNetIDs(BinaryReader reader) {
			Mod[] mods = ModLoader.Mods;
			var list = new List<Mod>();
			int n = reader.ReadInt32();

			for (short i = 0; i < n; i++) {
				string name = reader.ReadString();
				Mod mod = mods.SingleOrDefault(m => m.Name == name);

				list.Add(mod);

				if (mod != null) // NoSync mod that doesn't exist on the client
					mod.netID = i;
			}

			netMods = list.ToArray();
			SetModNetDiagnosticsUI(netMods.Where(mod => mod != null)); // When client receives netMods, exclude NoSync mods that aren't on the client, and assign a new UI

			ItemLoader.ReadNetGlobalOrder(reader);
			SystemLoader.ReadNetSystemOrder(reader);

			int serverMaxBuffs = reader.ReadInt32();

			if (serverMaxBuffs != Player.MaxBuffs) {
				Netplay.Disconnect = true;
				Main.statusText = $"The server expects Player.MaxBuffs of {serverMaxBuffs}\nbut this client reports {Player.MaxBuffs}.\nSome mod is behaving poorly.";
			}
		}

		// Some mods have expressed concern about read underflow exceptions conflicting with their ModPacket design, they can use reflection to set this bool as a bandaid until they fix their code.
		internal static bool ReadUnderflowBypass = false; // Remove by 0.11.7
		internal static void HandleModPacket(BinaryReader reader, int whoAmI, int length) {
			if (netMods == null) {
				ReadNetIDs(reader);
				return;
			}

			short id = NetModCount < 256 ? reader.ReadByte() : reader.ReadInt16();
			int start = (int)reader.BaseStream.Position;
			int actualLength = length - 1 - (NetModCount < 256 ? 1 : 2);
			try {
				ReadUnderflowBypass = false;
				GetMod(id)?.HandlePacket(reader, whoAmI);
				if (!ReadUnderflowBypass && reader.BaseStream.Position - start != actualLength) {
					throw new IOException($"Read underflow {reader.BaseStream.Position - start} of {actualLength} bytes caused by {GetMod(id).Name} in HandlePacket");
				}
			}
			catch { }

			if (Main.netMode == 1 && id >= 0)
				ModNetDiagnosticsUI.CountReadMessage(id, length);
		}

		internal static void SetModNetDiagnosticsUI(IEnumerable<Mod> mods) {
			// If called in ModContent.Load, just displays all loaded mods (similar to vanilla). It gets set to netMods in ModNet.ReadNetIDs
			ModNetDiagnosticsUI = Main.dedServ ? new EmptyDiagnosticsUI() : new UIModNetDiagnostics(mods);
		}

		internal static bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber) {
			if (netMods == null) {
				return false;
			}

			return SystemLoader.HijackGetData(ref messageType, ref reader, playerNumber);
		}

		internal static bool HijackSendData(int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
			=> SystemLoader.HijackSendData(whoAmI, msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);


		public static bool DetailedLogging = Program.LaunchParameters.ContainsKey("-detailednetlog");
		private static ILog NetLog { get; } = LogManager.GetLogger("Network");

		private static string Identifier(int whoAmI) {
			if (!Main.dedServ) return "";

			if (whoAmI >= 0 && whoAmI < 256) {
				var client = Netplay.Clients[whoAmI];
				return $"[{whoAmI}][{client.Socket?.GetRemoteAddress()?.GetFriendlyName()} ({client.Name})] ";
			}

			if (whoAmI == -1)
				return "[*] ";

			return $"[{whoAmI}] ";
		}

		private static string Identifier(RemoteAddress addr) {
			if (!Main.dedServ || addr == null) return "";

			if (Netplay.Clients.SingleOrDefault(c => c.Socket?.GetRemoteAddress() == addr) is RemoteClient client)
				return Identifier(client.Id);

			return $"[{addr.GetFriendlyName()}] ";
		}

		public static void Log(int whoAmI, string s) => Log(Identifier(whoAmI) + s);
		public static void Log(RemoteAddress addr, string s) => Log(Identifier(addr) + s);
		public static void Log(string s) => NetLog.Info(s);

		public static void Warn(int whoAmI, string s) => Warn(Identifier(whoAmI) + s);
		public static void Warn(RemoteAddress addr, string s) => Warn(Identifier(addr) + s);
		public static void Warn(string s) => NetLog.Warn(s);

		public static void Debug(int whoAmI, string s) => Debug(Identifier(whoAmI) + s);
		public static void Debug(RemoteAddress addr, string s) => Debug(Identifier(addr) + s);
		public static void Debug(string s) {
			if (DetailedLogging)
				NetLog.Info(s);
		}

		public static void Error(int whoAmI, string s, Exception e = null) => Error(Identifier(whoAmI) + s, e);
		public static void Error(RemoteAddress addr, string s, Exception e = null) => Error(Identifier(addr) + s, e);
		public static void Error(string s, Exception e = null) => NetLog.Error(s, e);

		public static void LogSend(int toClient, int ignoreClient, string s, int len) {
			if (!DetailedLogging)
				return;

			s += $", {len}";
			if (ignoreClient != -1)
				s += $", ignore: {ignoreClient}";

			Debug(toClient, s);
		}
	}
}
