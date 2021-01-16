using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Common
{
	//Acts as a container for "downed boss" flags.
	//Set a flag like this in your bosses OnKill hook:
	//    NPC.SetEventFlagCleared(ref DownedBossSystem.downedMinionBoss, -1);
	public class DownedBossSystem : ModSystem
	{
		public static bool downedMinionBoss = false;

		public override void OnWorldLoad() {
			downedMinionBoss = false;
		}

		public override void OnWorldUnload() {
			downedMinionBoss = false;
		}

		public override TagCompound SaveWorldData() {
			var downed = new List<string>();
			if (downedMinionBoss) {
				downed.Add("downedMinionBoss");
			}

			return new TagCompound {
				["downed"] = downed,
			};
		}

		public override void LoadWorldData(TagCompound tag) {
			var downed = tag.GetList<string>("downed");
			downedMinionBoss = downed.Contains("downedMinionBoss");
		}

		public override void NetSend(BinaryWriter writer) {
			var flags = new BitsByte();
			flags[0] = downedMinionBoss;
			writer.Write(flags);

			/*
			Remember that Bytes/BitsByte only have 8 entries. If you have more than 8 flags you want to sync, use multiple BitsByte:
				This is wrong:
			flags[8] = downed9thBoss; // an index of 8 is nonsense. 
				This is correct:
			flags[7] = downed8thBoss;
			writer.Write(flags);
			BitsByte flags2 = new BitsByte(); // create another BitsByte
			flags2[0] = downed9thBoss; // start again from 0
			// up to 7 more flags here
			writer.Write(flags2); // write this byte
			*/

			//If you prefer, you can use the BitsByte constructor approach as well.
			//writer.Write(saveVersion);
			//BitsByte flags = new BitsByte(downedMinionBoss, downedOtherBoss);
			//writer.Write(flags);

			// This is another way to do the same thing, but with bitmasks and the bitwise OR assignment operator (the |=)
			// Note that 1 and 2 here are bit masks. The next values in the pattern are 4,8,16,32,64,128. If you require more than 8 flags, make another byte.
			//writer.Write(saveVersion);
			//byte flags = 0;
			//if (downedMinionBoss)
			//{
			//	flags |= 1;
			//}
			//if (downedOtherBoss)
			//{
			//	flags |= 2;
			//}
			//writer.Write(flags);
		}

		public override void NetReceive(BinaryReader reader) {
			BitsByte flags = reader.ReadByte();
			downedMinionBoss = flags[0];
			// As mentioned in NetSend, BitBytes can contain 8 values. If you have more, be sure to read the additional data:
			// BitsByte flags2 = reader.ReadByte();
			// downed9thBoss = flags[0];
		}
	}
}
