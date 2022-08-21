using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Common.GlobalNPCs
{
	// Here is a class dedicated to showcasing Send/ReceiveExtraAI()
	public class ExampleNPCNetSync : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		private bool differentBehavior;

		// This reduces how many NPCs actually have this GlobalNPC
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			return entity.type == NPCID.Sharkron2;
		}

		// Although this runs on both client and server, only the session that spawned the NPC knows its source
		// As such, the check demonstrated below will always be false client-side and the code will never run!
		public override void OnSpawn(NPC npc, IEntitySource source) {

			// When spawned by a Cthulunado during a Blood Moon
			if (source is EntitySource_Parent parent
				&& parent.Entity is Projectile projectile
				&& projectile.type == ProjectileID.Cthulunado
				&& Main.bloodMoon) {
				differentBehavior = true;
			}
		}

		// Because this GlobalNPC only applies to Sharkrons, this data is not attached to all NPC sync packets
		public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter) {
			bitWriter.WriteBit(differentBehavior);
		}

		// Make sure you always read exactly as much data as you sent!
		public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader) {
			differentBehavior = bitReader.ReadBit();
		}

		public override void AI(NPC npc) {
			if (differentBehavior) {
				npc.scale *= 1.0025f;
				if (npc.scale > 3f) {
					npc.scale = 3f;
				}
			}
		}
	}
}