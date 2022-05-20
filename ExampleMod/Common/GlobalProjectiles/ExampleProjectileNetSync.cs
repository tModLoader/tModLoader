using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using System.IO;

namespace ExampleMod.Common.GlobalProjectiles
{
	// Here is a class dedicated to showcasing Send/ReceiveExtraAI()
	public class ExampleProjectileNetSync : GlobalProjectile
	{
		public override bool InstancePerEntity => true;
		private bool differentBehaviour;
		private float distance;

		// This reduces how many projectiles actually have this GlobalProjectile
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			return entity.type == ProjectileID.SharknadoBolt;
		}

		// Although this runs on both client and server, only the session that spawned the projectile knows its source
		// As such, the check demonstrated below will always be false client-side and the code will never run!
		public override void OnSpawn(Projectile projectile, IEntitySource source) {

			// When spawned by Duke Fishron during a Blood Moon
			if (source is EntitySource_Parent parent
				&& parent.Entity is NPC npc
				&& npc.type == NPCID.DukeFishron
				&& Main.bloodMoon) {

				differentBehaviour = true;
				distance = projectile.Distance(Main.player[npc.target].Center);
			}
		}

		// Because this GlobalProjectile only applies to typhoons, this data is not attached to all projectile sync packets
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			bitWriter.WriteBit(differentBehaviour);

			// This check further avoids sending distance when it wouldn't be necessary
			if (differentBehaviour) {
				binaryWriter.Write(distance);
			}
		}

		// Make sure you always read exactly as much data as you sent!
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			differentBehaviour = bitReader.ReadBit();

			if (differentBehaviour) {
				distance = binaryReader.ReadSingle();
			}
		}

		public override void AI(Projectile projectile) {
			if (differentBehaviour) {
				int p = Player.FindClosest(projectile.position, projectile.width, projectile.height);
				float currentDistance = p == -1 ? 0 : projectile.Distance(Main.player[p].Center);
				int dustType = DustID.GemSapphire;

				// Ends behaviour when in very close range
				if (currentDistance < distance / 4) {
					differentBehaviour = false;
					projectile.netUpdate = true;
				}
				// Move at normal speed but can speed back up
				else if (currentDistance < distance / 2) {
					projectile.extraUpdates = 0;
				}
				// Becomes faster when out of range
				else {
					projectile.extraUpdates = 1;
					dustType = DustID.GemRuby;
				}

				// Visually indicates this typhoon has special behaviour and which mode it is in
				int d = Dust.NewDust(projectile.position, projectile.width, projectile.height, dustType, Scale: 5f);
				Main.dust[d].noGravity = true;
			}
		}
	}
}
