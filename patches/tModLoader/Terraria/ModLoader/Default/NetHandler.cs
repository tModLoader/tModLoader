using System.IO;
using Terraria.ID;

namespace Terraria.ModLoader.Default;

internal abstract class NetHandler
{
	internal byte HandlerType { get; set; }
	public abstract void HandlePacket(BinaryReader r, int fromWho);

	protected NetHandler(byte handlerType)
	{
		HandlerType = handlerType;
	}

	protected ModPacket GetPacket(byte packetType, int fromWho)
	{
		var p = ModContent.GetInstance<ModLoaderMod>().GetPacket();
		p.Write(HandlerType);
		p.Write(packetType);
		if (Main.netMode == NetmodeID.Server) {
			p.Write((byte)fromWho);
		}
		return p;
	}
}
