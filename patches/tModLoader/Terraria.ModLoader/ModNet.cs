using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
	public class ModNet
	{
		private class ModHeader
		{
			public string name;
			public Version version;
			public byte[] hash;
			public bool signed;
			public string path;

			public ModHeader(string name, Version version, byte[] hash, bool signed)
			{
				this.name = name;
				this.version = version;
				this.hash = hash;
				this.signed = signed;
				path = Path.Combine(ModLoader.ModPath, name + ".tmod");
			}

			public bool Matches(TmodFile mod) => name == mod.name && version == mod.version && hash.SequenceEqual(mod.hash);
			public override string ToString() => name + " v" + version;
		}

		public static bool AllowVanillaClients { get; internal set; }
		internal static bool downloadModsFromServers = true;
		internal static bool onlyDownloadSignedMods = false;

		internal static bool[] isModdedClient = new bool[256];

		private static Mod[] netMods;

		public static bool IsModdedClient(int i) => isModdedClient[i];

		public static Mod GetMod(int netID) =>
			netID >= 0 && netID < netMods.Length ? netMods[netID] : null;

		private static Queue<ModHeader> downloadQueue = new Queue<ModHeader>();
		private static ModHeader downloadingMod;
		private static FileStream downloadingFile;
		private static long downloadingLength;

		internal static void AssignNetIDs()
		{
			netMods = ModLoader.LoadedMods.Where(mod => mod.Side != ModSide.Server).ToArray();
			for (short i = 0; i < netMods.Length; i++)
				netMods[i].netID = i;
		}

		internal static void Unload()
		{
			netMods = null;
		}

		internal static void SyncMods(int clientIndex)
		{
			var p = new ModPacket(MessageID.SyncMods);
			p.Write(AllowVanillaClients);

			var syncMods = ModLoader.LoadedMods.Where(mod => mod.Side == ModSide.Both).ToArray();
			p.Write(syncMods.Length);
			foreach (var mod in syncMods)
			{
				p.Write(mod.Name);
				p.Write(mod.Version.ToString());
				p.Write(mod.File.hash);
				p.Write(mod.File.ValidModBrowserSignature);
			}

			p.Send(clientIndex);
		}

		internal static void SyncClientMods(BinaryReader reader)
		{
			AllowVanillaClients = reader.ReadBoolean();

			Main.statusText = "Syncing Mods";
			var clientMods = ModLoader.LoadedMods;
			var modFiles = ModLoader.FindMods();
			var needsReload = false;
			downloadQueue.Clear();
			var syncSet = new HashSet<string>();
			var blockedList = new List<ModHeader>();

			int n = reader.ReadInt32();
			for (int i = 0; i < n; i++)
			{
				var header = new ModHeader(reader.ReadString(), new Version(reader.ReadString()), reader.ReadBytes(20), reader.ReadBoolean());
				syncSet.Add(header.name);

				var clientMod = clientMods.SingleOrDefault(m => m.Name == header.name);
				if (clientMod != null)
				{
					if (header.Matches(clientMod.File))
						continue;

					header.path = clientMod.File.path;
				}
				else
				{
					var disabledVersions = modFiles.Where(m => m.name == header.name).ToArray();
					var matching = disabledVersions.FirstOrDefault(header.Matches);
					if (matching != null)
					{
						ModLoader.EnableMod(matching);
						needsReload = true;
						continue;
					}

					if (disabledVersions.Length > 0)
						header.path = disabledVersions[0].path;
				}

				if (downloadModsFromServers && (header.signed || !onlyDownloadSignedMods))
					downloadQueue.Enqueue(header);
				else
					blockedList.Add(header);
			}

			foreach (var mod in clientMods)
				if (mod.Side == ModSide.Both && !syncSet.Contains(mod.Name))
				{
					ModLoader.DisableMod(mod.File);
					needsReload = true;
				}

			if (blockedList.Count > 0)
			{
				var msg = "The following mods are installed on the server but cannot be downloaded ";
				msg += downloadModsFromServers
					? "because you only accept mods signed by the mod browser"
					: "because you have disabled automatic mod downloading";
				msg += ".\nYou will need to change your settings or acquire the mods from the server owner.\n";
				foreach (var mod in blockedList)
					msg += "\n    " + mod;

				ErrorLogger.LogMissingMods(msg);
				return;
			}

			if (downloadQueue.Count > 0)
				DownloadNextMod();
			else
				OnModsDownloaded(needsReload);
		}

		private static void DownloadNextMod()
		{
			downloadingMod = downloadQueue.Dequeue();
			downloadingFile = null;

			var p = new ModPacket(MessageID.ModFile);
			p.Write(downloadingMod.name);
			p.Send();
		}

		private const int CHUNK_SIZE = 16384;
		internal static void SendMod(string modName, int toClient)
		{
			var mod = ModLoader.GetMod(modName);
			var path = mod.File.path;
			var fs = new FileStream(path, FileMode.Open);
			{//send file length
				var p = new ModPacket(MessageID.ModFile);
				p.Write(mod.DisplayName);
				p.Write(fs.Length);
				p.Send(toClient);
			}

			var buf = new byte[CHUNK_SIZE];
			int count;
			while ((count = fs.Read(buf, 0, buf.Length)) > 0)
			{
				var p = new ModPacket(MessageID.ModFile, CHUNK_SIZE + 3);
				p.Write(buf, 0, count);
				p.Send(toClient);
			}

			fs.Close();
		}

		internal static void ReceiveMod(BinaryReader reader)
		{
			if (downloadingMod == null)
				return;

			try
			{
				if (downloadingFile == null)
				{
					Interface.downloadMod.SetDownloading(reader.ReadString());
					Interface.downloadMod.SetCancel(() =>
					{
						downloadingFile?.Close();
						downloadingMod = null;
						Netplay.disconnect = true;
						Main.menuMode = 0;
					});
					Main.menuMode = Interface.downloadModID;

					downloadingLength = reader.ReadInt64();
					downloadingFile = new FileStream(downloadingMod.path, FileMode.Create);
					return;
				}

				var bytes = reader.ReadBytes((int)Math.Min(downloadingLength - downloadingFile.Position, CHUNK_SIZE));
				downloadingFile.Write(bytes, 0, bytes.Length);
				Interface.downloadMod.SetProgress(downloadingFile.Position, downloadingLength);

				if (downloadingFile.Position == downloadingLength)
				{
					downloadingFile.Close();
					var mod = new TmodFile(downloadingMod.path);
					mod.Read();
					var ex = mod.ValidMod();
					if (ex != null)
						throw ex;

					if (!downloadingMod.Matches(mod))
						throw new Exception("Hash mismatch");

					if (downloadingMod.signed && !mod.ValidModBrowserSignature)
						throw new Exception("Mod was not signed by the Mod Browser");

					ModLoader.EnableMod(mod);

					if (downloadQueue.Count > 0)
						DownloadNextMod();
					else
						OnModsDownloaded(true);
				}
			}
			catch (Exception e)
			{
				try
				{
					downloadingFile?.Close();
				}
				catch { }

				File.Delete(downloadingMod.path);
				ErrorLogger.LogException(e, "An error occured while downloading " + downloadingMod.name);
				downloadingMod = null;
			}
		}

		private static void OnModsDownloaded(bool needsReload)
		{
			if (needsReload)
			{
				ModLoader.PostLoad = NetReload;
				ModLoader.Reload();
				return;
			}

			downloadingMod = null;
			netMods = null;
			foreach (var mod in ModLoader.LoadedMods)
				mod.netID = -1;

			new ModPacket(MessageID.SyncMods).Send();
		}

		private static void NetReload()
		{
			Main.ActivePlayerFileData = Player.GetFileData(Main.ActivePlayerFileData.Path, Main.ActivePlayerFileData.IsCloudSave);
			Main.ActivePlayerFileData.SetAsActive();
			//from Netplay.ClientLoopSetup
			Main.player[Main.myPlayer].hostile = false;
			Main.clientPlayer = (Player)Main.player[Main.myPlayer].clientClone();

			Main.menuMode = 10;
			OnModsDownloaded(false);
		}

		internal static void SendNetIDs(int toClient)
		{
			var p = new ModPacket(MessageID.ModPacket);
			p.Write((short)-1);
			p.Write(netMods.Length);
			foreach (var mod in netMods)
				p.Write(mod.Name);

			ItemLoader.WriteNetGlobalOrder(p);
			WorldHooks.WriteNetWorldOrder(p);

			p.Send(toClient);
		}

		private static void ReadNetIDs(BinaryReader reader)
		{
			var mods = ModLoader.LoadedMods;
			var list = new List<Mod>();
			var n = reader.ReadInt32();
			for (short i = 0; i < n; i++)
			{
				var name = reader.ReadString();
				var mod = mods.SingleOrDefault(m => m.Name == name);
				list.Add(mod);
				if (mod != null) //nosync mod that doesn't exist on the client
					mod.netID = i;
			}
			netMods = list.ToArray();

			ItemLoader.ReadNetGlobalOrder(reader);
			WorldHooks.ReadNetWorldOrder(reader);
		}

		internal static void HandleModPacket(BinaryReader reader, int whoAmI)
		{
			var id = reader.ReadInt16();
			if (id < 0)
				ReadNetIDs(reader);
			else
				GetMod(id)?.HandlePacket(reader, whoAmI);
		}

		internal static bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber)
		{
			if (netMods == null)
			{
				return false;
			}

			bool hijacked = false;
			long readerPos = reader.BaseStream.Position;
			long biggestReaderPos = readerPos;
			foreach (var mod in netMods)
			{
				if (mod.HijackGetData(ref messageType, ref reader, playerNumber))
				{
					hijacked = true;
					biggestReaderPos = Math.Max(reader.BaseStream.Position, biggestReaderPos);
				}
				reader.BaseStream.Position = readerPos;
			}
			if (hijacked)
			{
				reader.BaseStream.Position = biggestReaderPos;
			}
			return hijacked;
		}

		internal static bool HijackSendData(int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
		{
			bool hijacked = false;
			foreach (Mod mod in ModLoader.mods.Values)
			{
				hijacked |= mod.HijackSendData(whoAmI, msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);
			}
			return hijacked;
		}
	}
}
