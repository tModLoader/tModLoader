using System.IO;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;

namespace ExampleMod
{
	public class ExampleWorld : ModWorld
	{
		private const int saveVersion = 0;
		public static bool downedAbomination = false;
		public static bool downedPuritySpirit = false;

		public override void Initialize()
		{
			downedAbomination = false;
			downedPuritySpirit = false;
		}

		public override void SaveCustomData(BinaryWriter writer)
		{
			writer.Write(saveVersion);
			byte flags = 0;
			if (downedAbomination)
			{
				flags |= 1;
			}
			if (downedPuritySpirit)
			{
				flags |= 2;
			}
			writer.Write(flags);
		}

		public override void LoadCustomData(BinaryReader reader)
		{
			reader.ReadInt32();
			byte flags = reader.ReadByte();
			downedAbomination = ((flags & 1) == 1);
			downedPuritySpirit = ((flags & 2) == 2);
		}

		public override void PostWorldGen()
		{
			for (int i = 0; i < Main.maxTilesX; i++)
			{
				Main.tile[i, Main.maxTilesY / 2].type = TileID.Chlorophyte;
			}
		}
	}
}
