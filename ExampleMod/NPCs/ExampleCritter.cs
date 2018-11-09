using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.NPCs
{
	/// <summary>
	/// This file shows off a critter npc. The unique thing about critters is how you can catch them with a bug net.  
	/// The important bits are: Main.npcCatchable, npc.catchItem, and item.makeNPC
	/// We will also show off adding an item to an existing RecipeGroup (see ExampleMod.AddRecipeGroups)
	/// </summary>
	class ExampleCritterNPC : ModNPC
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lava Snail");
			Main.npcFrameCount[npc.type] = Main.npcFrameCount[NPCID.GlowingSnail];
			Main.npcCatchable[npc.type] = true;
		}

		public override void SetDefaults()
		{
			//npc.width = 14;
			//npc.height = 14;
			//npc.aiStyle = 67;
			//npc.damage = 0;
			//npc.defense = 0;
			//npc.lifeMax = 5;
			//npc.HitSound = SoundID.NPCHit1;
			//npc.DeathSound = SoundID.NPCDeath1;
			//npc.npcSlots = 0.5f;
			//npc.noGravity = true;
			//npc.catchItem = 2007;

			npc.CloneDefaults(NPCID.GlowingSnail);
			npc.catchItem = (short)mod.ItemType<ExampleCritterItem>();
			npc.lavaImmune = true;
			//npc.aiStyle = 0;
			npc.friendly = true; // We have to add this and CanBeHitByItem/CanBeHitByProjectile because of reasons.
			aiType = NPCID.GlowingSnail;
			animationType = NPCID.GlowingSnail;
		}

		public override bool? CanBeHitByItem(Player player, Item item)
		{
			return true;
		}

		public override bool? CanBeHitByProjectile(Projectile projectile)
		{
			return true;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			return SpawnCondition.Underworld.Chance * 0.1f;
		}

		public override void HitEffect(int hitDirection, double damage)
		{
			if (npc.life <= 0)
			{
				for (int i = 0; i < 6; i++)
				{
					int dust = Dust.NewDust(npc.position, npc.width, npc.height, 200, 2 * hitDirection, -2f);
					if (Main.rand.NextBool(2))
					{
						Main.dust[dust].noGravity = true;
						Main.dust[dust].scale = 1.2f * npc.scale;
					}
					else
					{
						Main.dust[dust].scale = 0.7f * npc.scale;
					}
				}
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/LavaSnailHead"), npc.scale);
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/LavaSnailShell"), npc.scale);
			}
		}

		public override Color? GetAlpha(Color drawColor)
		{
			// GetAlpha gives our Lava Snail a red glow.
			drawColor.R = 255;
			// both these do the same in this situation, using these methods is useful.
			drawColor.G = Utils.Clamp<byte>(drawColor.G, 175, 255);
			drawColor.B = Math.Min(drawColor.B, (byte)75);
			drawColor.A = 255;
			return drawColor;
		}

		public override bool PreAI()
		{
			// Usually we can use npc.wet, but aiStyle 67 prevents wet from being set.
			if (Collision.WetCollision(npc.position, npc.width, npc.height)) //if (npc.wet)
			{
				// These 3 lines instantly kill the npc without showing damage numbers, dropping loot, or playing DeathSound. Use this for instant deaths
				npc.life = 0;
				npc.HitEffect();
				npc.active = false;
				Main.PlaySound(SoundID.NPCDeath16, npc.position); // plays a fizzle sound
			}
			return base.PreAI();
		}

		// TODO: Hooks for Collision_MoveSnailOnSlopes and npc.aiStyle = 67 problem
	}

	class ExampleCritterItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lava Snail");
		}

		public override void SetDefaults()
		{
			//item.useStyle = 1;
			//item.autoReuse = true;
			//item.useTurn = true;
			//item.useAnimation = 15;
			//item.useTime = 10;
			//item.maxStack = 999;
			//item.consumable = true;
			//item.width = 12;
			//item.height = 12;
			//item.makeNPC = 360;
			//item.noUseGraphic = true;
			//item.bait = 15;

			item.CloneDefaults(ItemID.GlowingSnail);
			item.bait = 17;
			item.makeNPC = (short)mod.NPCType<ExampleCritterNPC>();
		}
	}

	// ExampleCritterCage needed.

	// TODO: spawn from statue?
}
