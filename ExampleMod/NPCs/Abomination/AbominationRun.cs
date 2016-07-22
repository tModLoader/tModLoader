using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.NPCs.Abomination
{
	//ported from my tAPI mod because I'm lazy
	public class AbominationRun : ModNPC
	{
		public override bool Autoload(ref string name, ref string texture, ref string[] altTextures)
		{
			texture = "ExampleMod/NPCs/Abomination/Abomination";
			return mod.Properties.Autoload;
		}

		public override void AutoloadHead(ref string headTexture, ref string bossHeadTexture)
		{
			bossHeadTexture = "ExampleMod/NPCs/Abomination/Abomination_Head_Boss";
		}

		public override void SetDefaults()
		{
			npc.name = "Injured Abomination";
			npc.displayName = "The Abomination";
			npc.aiStyle = -1;
			npc.lifeMax = 40000;
			npc.damage = 100;
			npc.defense = 55;
			npc.knockBackResist = 0f;
			npc.dontTakeDamage = true;
			npc.width = 100;
			npc.height = 100;
			Main.npcFrameCount[npc.type] = 2;
			npc.value = Item.buyPrice(0, 20, 0, 0);
			npc.npcSlots = 15f;
			npc.boss = true;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.soundHit = 1;
			npc.soundKilled = 1;
			npc.buffImmune[24] = true;
			music = MusicID.Boss2;
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
		{
			npc.lifeMax = (int)(npc.lifeMax * 0.625f * bossLifeScale);
			npc.damage = (int)(npc.damage * 0.6f);
		}

		public override void AI()
		{
			if (npc.localAI[0] == 0f)
			{
				Main.PlaySound(15, (int)npc.position.X, (int)npc.position.Y, 0);
				npc.localAI[0] = 1f;
			}
			npc.velocity.Y += 1f;
			if (npc.timeLeft > 10)
			{
				npc.timeLeft = 10;
			}
		}

		public override void OnHitPlayer(Player player, int damage, bool crit)
		{
			if (Main.expertMode || Main.rand.Next(2) == 0)
			{
				player.AddBuff(BuffID.OnFire, 600, true);
			}
		}
	}
}