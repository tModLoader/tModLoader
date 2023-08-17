using System.IO;
using Terraria.ModLoader;

namespace tModPorter.Tests.TestData
{
	public class BinaryIOVarInt : ModSystem
	{
		public static int Value1 = 1;
		public static int Value2 => 2;
		public static int Value3() => 3;

		public override void NetSend(BinaryWriter writer) {
			writer.WriteVarInt(Value1);
			writer.WriteVarInt(Value2);
			writer.WriteVarInt(Value3());
			writer.WriteVarInt(4);
		}

		public override void NetReceive(BinaryReader reader) {
			int value1 = reader.ReadVarInt();
			int value2 = reader.ReadVarInt();
			int value3 = reader.ReadVarInt();
			int value4 = reader.ReadVarInt();
		}
	}
}