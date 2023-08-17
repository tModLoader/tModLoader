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
			writer.Write7BitEncodedInt(Value1);
			writer.Write7BitEncodedInt(Value2);
			writer.Write7BitEncodedInt(Value3());
			writer.Write7BitEncodedInt(4);
		}

		public override void NetReceive(BinaryReader reader) {
			int value1 = reader.Read7BitEncodedInt();
			int value2 = reader.Read7BitEncodedInt();
			int value3 = reader.Read7BitEncodedInt();
			int value4 = reader.Read7BitEncodedInt();
		}
	}
}