using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

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
	/// <para/> Called while receiving tile data (!lightReceive) and when <see cref="MessageID.TileEntitySharing"/> is received (lightReceive).
	/// <para/> Note that this is called on a new instance that will replace the existing instance at the <see cref="Position"/>, if any. <see cref="ID"/> is not necessarily assigned yet when this is called.
	/// <para/> Only called on the client.
	/// </summary>
	/// <param name="reader">The reader.</param>
	public virtual void NetReceive(BinaryReader reader) => ReadExtraData(reader, true);

	/// <summary>
	/// Attempts to retrieve the TileEntity at the given coordinates of the specified Type (<typeparamref name="T"/>). Works with any provided coordinate belonging to the multitile. Note that this method assumes the TileEntity is placed in the top left corner of the multitile.
	/// </summary>
	/// <typeparam name="T">The type to get the entity as</typeparam>
	/// <param name="i">The tile X-coordinate</param>
	/// <param name="j">The tile Y-coordinate</param>
	/// <param name="entity">The found <typeparamref name="T"/> instance, if there was one.</param>
	/// <returns><see langword="true"/> if there was a <typeparamref name="T"/> instance, or <see langword="false"/> if there was no entity present OR the entity was not a <typeparamref name="T"/> instance.</returns>
	public static bool TryGet<T>(int i, int j, out T entity) where T : TileEntity
	{
		Point16 topLeft = TileObjectData.TopLeft(i, j);
		if (ByPosition.TryGetValue(topLeft, out var existing) && existing is T existingAsT) {
			entity = existingAsT;
			return true;
		}
		entity = null;
		return false;
	}

	/// <inheritdoc cref="TryGet{T}(int, int, out T)"/>
	public static bool TryGet<T>(Point16 point, out T entity) where T : TileEntity => TryGet<T>(point.X, point.Y, out entity);
}
