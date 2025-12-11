using System;
using System.IO;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader;

/// <summary>
/// Used to send data between the server and client. Syncing data is essential for keeping clients up to date with changes to the game state. ModPacket is used to sync arbitrary data, most commonly data corresponding to this mod. The <see href="https://github.com/tModLoader/tModLoader/wiki/intermediate-netcode">Intermediate netcode wiki page</see> explains more about this concept.
/// <para/> Initialize a ModPacket using the <see cref="Mod.GetPacket(int)"/> method.
/// <para/> This class inherits from BinaryWriter. This means that you can use all of its writing functions to send information between client and server. This class also comes with a <see cref="Send(int, int)"/> method that's used to actually send everything you've written between client and server.
/// <para/> ModPacket has all the same methods as BinaryWriter, and some additional ones.
/// </summary>
/// <seealso cref="System.IO.BinaryWriter" />
public sealed class ModPacket : BinaryWriter
{
	private byte[] buf;
	private ushort len;
	internal short netID = -1;

	internal ModPacket(byte messageID, int capacity = 256) : base(new MemoryStream(capacity))
	{
		Write((ushort)0);
		Write(messageID);
	}

	/// <summary>
	/// Sends all the information you've written to this ModPacket over the network to clients or the server. When called on a client, the data will be sent to the server and the optional parameters are ignored. When called on a server, the data will be sent to either all clients, all clients except a specific client, or just a specific client:
	/// <code>
	/// // Sends to all connected clients
	/// packet.Send();
	///
	/// // Sends to a specific client only
	/// packet.Send(toClient: somePlayer.whoAmI);
	///
	/// // Sends to all other clients except a specific client
	/// packet.Send(ignoreClient: somePlayer.whoAmI);
	/// </code>
	/// Typically if data is sent from a client to the server, the server will then need to relay this to all other clients to keep them in sync. This is when the <paramref name="ignoreClient"/> option will be used.
	/// </summary>
	public void Send(int toClient = -1, int ignoreClient = -1)
	{
		Finish();

		if (ModNet.DetailedLogging)
			ModNet.LogSend(toClient, ignoreClient, $"ModPacket.Send {ModNet.GetMod(netID)?.Name ?? "ModLoader"}({netID})", len);

		if (Main.netMode == 1) {
			Netplay.Connection.Socket.AsyncSend(buf, 0, len, SendCallback);

			if (netID >= 0)
				ModNet.ModNetDiagnosticsUI.CountSentMessage(netID, len);
		}
		else if (toClient != -1) {
			Netplay.Clients[toClient].Socket.AsyncSend(buf, 0, len, SendCallback);
		}
		else {
			for (int i = 0; i < 256; i++)
				if (i != ignoreClient && Netplay.Clients[i].IsConnected() && NetMessage.buffer[i].broadcast)
					Netplay.Clients[i].Socket.AsyncSend(buf, 0, len, SendCallback);
		}
	}

	private void SendCallback(object state) { }

	private void Finish()
	{
		if (buf != null)
			return;

		if (OutStream.Position > ushort.MaxValue)
			throw new Exception(Language.GetTextValue("tModLoader.MPPacketTooLarge", OutStream.Position, ushort.MaxValue));

		len = (ushort)OutStream.Position;
		Seek(0, SeekOrigin.Begin);
		Write(len);
		Close();
		buf = ((MemoryStream)OutStream).GetBuffer();
	}
}
