using Terraria.ModLoader;
using Terraria.ID;
using System.IO;

namespace ServerHang
{
	class ServerHang : Mod
	{
		public override bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber)
		{
			while (messageType == MessageID.SpawnPlayer);
			return false;
		}
	}
}
