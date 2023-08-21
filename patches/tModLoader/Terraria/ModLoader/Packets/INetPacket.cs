using System.IO;
using Terraria.ID;

namespace Terraria.ModLoader.Packets;

public interface INetPacket {
	void HandlePacket();

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

	/// <summary>
	/// Causes all clients and the server to handle the results of this packet
	/// <br />
	/// Works for single-player or multiplayer
	/// </summary>
	public void HandleForAll() {
		switch (Main.netMode) {
			case NetmodeID.MultiplayerClient:
				Send(-1, Main.myPlayer);
				break;
			case NetmodeID.Server:
				Send();
				break;
		}

		HandlePacket();
	}
}
