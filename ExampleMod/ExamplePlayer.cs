using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod
{
	public class ExamplePlayer : ModPlayer
	{
		private const int saveVersion = 0;
		public int score = 0;

		public override void SaveCustomData(BinaryWriter writer)
		{
			writer.Write(saveVersion);
			writer.Write(score);
		}

		public override void LoadCustomData(BinaryReader reader)
		{
			int loadVersion = reader.ReadInt32();
			score = reader.ReadInt32();
		}
	}
}
