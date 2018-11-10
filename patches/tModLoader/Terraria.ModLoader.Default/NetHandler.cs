using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Terraria.ID;
using Terraria.ModLoader.Default.Developer;

namespace Terraria.ModLoader.Default
{
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
			var p = ModLoaderMod.Instance.GetPacket();
			p.Write(HandlerType);
			p.Write(packetType);
			if (Main.netMode == NetmodeID.Server)
			{
				p.Write((byte)fromWho);
			}
			return p;
		}
	}

	internal class ModNetHandler
	{
		// When a lot of handlers are added, it might be wise to automate
		// creation of them

		public const byte AndromedonType = 1;

		internal static AndromedonNetHandler Andromedon = new AndromedonNetHandler(AndromedonType);

		public static void HandlePacket(BinaryReader r, int fromWho)
		{
			switch (r.ReadByte())
			{
				case AndromedonType:
					Andromedon.HandlePacket(r, fromWho);
					break;
			}
		}
	}
}
