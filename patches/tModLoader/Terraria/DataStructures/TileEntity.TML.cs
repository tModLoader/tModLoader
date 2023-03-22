using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Terraria.DataStructures;

public partial class TileEntity
{
	/// <summary>
	/// Allows you to save custom data for this tile entity.
	/// <br/>
	/// <br/><b>NOTE:</b> The provided tag is always empty by default, and is provided as an argument only for the sake of convenience and optimization.
	/// <br/><b>NOTE:</b> Try to only save data that isn't default values.
	/// </summary>
	/// <param name="tag"> The TagCompound to save data into. Note that this is always empty by default, and is provided as an argument only for the sake of convenience and optimization. </param>
	public virtual void SaveData(TagCompound tag) { }

	/// <summary>
	/// Allows you to load custom data that you have saved for this tile entity.
	/// <br/><b>Try to write defensive loading code that won't crash if something's missing.</b>
	/// </summary>
	/// <param name="tag"> The TagCompound to load data from. </param>
	public virtual void LoadData(TagCompound tag) { }

	/// <summary>
	/// Allows you to send custom data for this tile entity between client and server, which will be handled in <see cref="NetReceive"/>.
	/// <br/>Called while sending tile data (!lightSend) and when <see cref="MessageID.TileEntitySharing"/> is sent (lightSend).
	/// <br/>Only called on the server.
	/// </summary>
	/// <param name="writer">The writer.</param>
	public virtual void NetSend(BinaryWriter writer) => WriteExtraData(writer, true);

	/// <summary>
	/// Receives custom data sent in the <see cref="NetSend"/> hook.
	/// <br/>Called while receiving tile data (!lightReceive) and when <see cref="MessageID.TileEntitySharing"/> is received (lightReceive).
	/// <br/>Only called on the client.
	/// </summary>
	/// <param name="reader">The reader.</param>
	public virtual void NetReceive(BinaryReader reader) => ReadExtraData(reader, true);
}
