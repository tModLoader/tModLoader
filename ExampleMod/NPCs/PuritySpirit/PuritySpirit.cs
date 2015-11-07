using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.NPCs
{
	public class PuritySpirit : ModNPC
	{
		public override void SetDefaults()
		{
			npc.name = "PuritySpirit";
			npc.displayName = "Spirit of Purity";
			npc.aiStyle = -1;
			npc.lifeMax = 200000;
			npc.damage = 0;
			npc.defense = 70;
			npc.knockBackResist = 0f;
			npc.width = 80;
			npc.height = 80;
			npc.value = Item.buyPrice(0, 50, 0, 0);
			npc.npcSlots = 50f;
			npc.boss = true;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.soundHit = 1;
			npc.soundKilled = 0;
			npc.hide = true;
			for (int k = 0; k < npc.buffImmune.Length; k++)
			{
				npc.buffImmune[k] = true;
			}
			music = MusicID.Title;
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
		{
			npc.lifeMax = (int)(npc.lifeMax / Main.expertLife * 1.2f * bossLifeScale);
			npc.defense = 72;
		}

		public override bool Autoload(ref string name, ref string texture) //remove when ready for testing
		{
			return false;
		}

		private int difficulty
		{
			get
			{
				double strength = (double)npc.life / (double)npc.lifeMax;
				int difficulty = (int)(4.0 * (1.0 - strength));
				if (Main.expertMode)
				{
					difficulty++;
				}
				return difficulty;
			}
		}

		private int stage
		{
			get
			{
				return (int)npc.ai[0];
			}
			set
			{
				npc.ai[0] = value;
			}
		}

		private float attackTimer
		{
			get
			{
				return npc.ai[1];
			}
			set
			{
				npc.ai[1] = value;
			}
		}

		private float debuffTimer
		{
			get
			{
				return npc.ai[2];
			}
			set
			{
				npc.ai[2] = value;
			}
		}

		private int attackProgress
		{
			get
			{
				return (int)npc.ai[3];
			}
			set
			{
				npc.ai[3] = value;
			}
		}

		public override void AI()
		{
			if (Main.netMode == 1)
			{
				return;
			}
		}

		public void Initialize()
		{
		}

		public void FindPlayers()
		{
		}

		private void SetupCrystals(int radius)
		{
		}

		private void UltimateAttack()
		{
		}

		private void BeamAttack()
		{
		}

		private void SnakeAttack()
		{
		}

		private void LaserAttack()
		{
		}

		private void SphereAttack()
		{
		}

		private void GiveDebuffs()
		{
		}
	}
}
