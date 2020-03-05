using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.NPCs
{
	// This NPC is simply an exhibition of the DrawBehind method. The npc cycles between all the available "layers" that a ModNPC can be drawn at. Spawn this NPC with something like Cheat Sheet or Hero's Mod to view the effect.
	class DrawBehindNPC : ModNPC
	{
		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[npc.type] = 6;
		}

		public override void SetDefaults()
		{
			npc.width = 30;
			npc.height = 40;
			npc.aiStyle = -1;
			npc.damage = 0;
			npc.defense = 2;
			npc.lifeMax = 100;
			npc.HitSound = SoundID.NPCHit2;
			npc.DeathSound = SoundID.NPCDeath2;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.knockBackResist = .2f;
		}

		public override void FindFrame(int frameHeight)
		{
			npc.frame.Y = (int)(npc.ai[0] / 40) * frameHeight;
		}

		public override void AI()
		{
			npc.ai[0] = (npc.ai[0] + 1) % 240;

			// These are the defaults
			npc.hide = false;
			npc.behindTiles = false;

			switch ((int)(npc.ai[0] / 40)) {
				case 0:
				case 1:
				case 4:
				case 5:
					npc.hide = true;
					break;
				case 2:
					npc.behindTiles = true;
					break;
				case 3:
					break;
			}
		}

		public override void DrawBehind(int index)
		{
			// The 6 available positions are as follows:  
			switch ((int)(npc.ai[0] / 40)) {
				case 0: // Behind tiles and walls
					Main.instance.DrawCacheNPCsMoonMoon.Add(index);
					break;
				case 1: // Behind non solid tiles, but in front of walls
					Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
					break;
				case 2: // Behind tiles, but in front of non solid tiles
				case 3: // Normal (in front of tiles)
					break;
				case 4: // In front of all normal NPC
					Main.instance.DrawCacheNPCProjectiles.Add(index);
					break;
				case 5: // In front of Players
					Main.instance.DrawCacheNPCsOverPlayers.Add(index);
					break;
			}
		}
	}
}
