using System.IO;
using Terraria.ModLoader.IO;

namespace Terraria.DataStructures
{
	public partial class TileEntity
	{
		/// <summary>
		/// Allows you to save custom data for this tile entity.
		/// </summary>
		/// <returns></returns>
		public virtual TagCompound Save() => null;

		/// <summary>
		/// Allows you to load the custom data you have saved for this tile entity.
		/// </summary>
		public virtual void Load(TagCompound tag) { }

		/// <summary>
		/// Allows you to send custom data for this tile entity between client and server. This is called on the server while sending tile data (!lightSend) and when a MessageID.TileEntitySharing message is sent (lightSend)
		/// </summary>
		/// <param name="writer">The writer.</param>
		public virtual void NetSend(BinaryWriter writer) => WriteExtraData(writer, true);

		/// <summary>
		/// Receives the data sent in the NetSend hook. Called on MP Client when receiving tile data (!lightReceive) and when a MessageID.TileEntitySharing message is sent (lightReceive)
		/// </summary>
		/// <param name="reader">The reader.</param>
		public virtual void NetReceive(BinaryReader reader) => ReadExtraData(reader, true);
	}
}
