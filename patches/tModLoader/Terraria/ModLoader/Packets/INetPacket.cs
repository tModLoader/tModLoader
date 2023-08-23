using System.IO;

namespace Terraria.ModLoader.Packets;

public interface INetPacket {
	/// <summary>
	/// Serializes the information and then sends it using the same format as <see cref="ModPacket.Send"/>
	/// </summary>
	/// <param name="toClient">The toClient param of <see cref="ModPacket.Send"/></param>
	/// <param name="ignoreClient">The ignoreClient param of <see cref="ModPacket.Send"/></param>
	void Send(int toClient = -1, int ignoreClient = -1);

	/// <summary>
	/// Called when the packet is recieved
	///	</summary>
	///	<param name="reader">The BinaryReader read in <see cref="Mod.HandlePacket"/></param>
	///	<param name="sender">The int whoAmI in <see cref="Mod.HandlePacket"/></param>
	void Receive(BinaryReader reader, int sender);

	void SendToPlayer(int player);

	void SendToPlayers(params int[] players);

	void SendToAllPlayers(int ignoreClient = -1);

	void SendToServer();

	void SendToAll();
}
