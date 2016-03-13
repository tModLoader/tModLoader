using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.NPCs
{
	public class PartyZombie : ModNPC
	{
		public override void SetDefaults()
		{
			npc.name = "Party Zombie";
			npc.displayName = "Zombie";
			npc.width = 18;
			npc.height = 40;
			npc.damage = 14;
			npc.defense = 6;
			npc.lifeMax = 200;
			npc.soundHit = 1;
			npc.soundKilled = 2;
			npc.value = 60f;
			npc.knockBackResist = 0.5f;
			npc.aiStyle = 3;
			Main.npcFrameCount[npc.type] = Main.npcFrameCount[NPCID.Zombie];
			aiType = NPCID.Zombie;
			animationType = NPCID.Zombie;
		}

		public override float CanSpawn(NPCSpawnInfo spawnInfo)
		{
			return spawnInfo.spawnTileY < Main.rockLayer && !Main.dayTime ? 0.5f : 0f;
		}

		public override void HitEffect(int hitDirection, double damage)
		{
			for (int i = 0; i < 10; i++)
			{
				int dustType = Main.rand.Next(139, 143);
				int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
				Dust dust = Main.dust[dustIndex];
				dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
				dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
				dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
			}
		}
	}
}
