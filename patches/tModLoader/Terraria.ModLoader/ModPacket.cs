using System;
using System.IO;

namespace Terraria.ModLoader
{
	public sealed class ModPacket : BinaryWriter
	{
		private byte[] buf;
		private ushort len;

		internal ModPacket(byte messageID, int capacity = 256) : base(new MemoryStream(capacity)) {
			Write((ushort)0);
			Write(messageID);
		}

		public void Send(int toClient = -1, int ignoreClient = -1) {
			Finish();

			if (Main.netMode == 1)
				Netplay.Connection.Socket.AsyncSend(buf, 0, len, SendCallback);
			else if (toClient != -1)
				Netplay.Clients[toClient].Socket.AsyncSend(buf, 0, len, SendCallback);
			else
				for (int i = 0; i < 256; i++)
					if (i != ignoreClient && Netplay.Clients[i].IsConnected())
						Netplay.Clients[i].Socket.AsyncSend(buf, 0, len, SendCallback);
		}

		private void SendCallback(object state) {}

		private void Finish() {
			if (buf != null)
				return;

			if (OutStream.Position > ushort.MaxValue)
				throw new Exception("Packet too large " + OutStream.Position + " > " + ushort.MaxValue);

			len = (ushort)OutStream.Position;
			Seek(0, SeekOrigin.Begin);
			Write(len);
			Close();
			buf = ((MemoryStream) OutStream).GetBuffer();
		}
	}
}
