using System;
using System.IO;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class inherits from BinaryWriter. This means that you can use all of its writing functions to send information between client and server. This class also comes with a Send method that's used to actually send everything you've written between client and server.
	/// 
	/// ModPacket has all the same methods as BinaryWriter, and some additional ones.
	/// </summary>
	/// <seealso cref="System.IO.BinaryWriter" />
	public sealed class ModPacket : BinaryWriter
	{
		private byte[] buf;
		private ushort len;
		internal short netID = -1;

		internal ModPacket(byte messageID, int capacity = 256) : base(new MemoryStream(capacity)) {
			Write((ushort)0);
			Write(messageID);
		}

		/// <summary>
		/// Sends all the information you've written between client and server. If the toClient parameter is non-negative, this packet will only be sent to the specified client. If the ignoreClient parameter is non-negative, this packet will not be sent to the specified client.
		/// </summary>
		public void Send(int toClient = -1, int ignoreClient = -1) {
			Finish();

			if (Main.netMode == 1) {
				Netplay.Connection.Socket.AsyncSend(buf, 0, len, SendCallback);
				Main.txMsg++;
				Main.txData += len;
				if (netID > 0) {
					ModNet.txMsgType[netID]++;
					ModNet.txDataType[netID] += len;
				}
			}
			else if (toClient != -1)
				Netplay.Clients[toClient].Socket.AsyncSend(buf, 0, len, SendCallback);
			else
				for (int i = 0; i < 256; i++)
					if (i != ignoreClient && Netplay.Clients[i].IsConnected() && NetMessage.buffer[i].broadcast)
						Netplay.Clients[i].Socket.AsyncSend(buf, 0, len, SendCallback);
		}

		private void SendCallback(object state) { }

		private void Finish() {
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
}
