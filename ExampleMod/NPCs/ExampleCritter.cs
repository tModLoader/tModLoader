using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.NPCs
{
	/// <summary>
	/// This file shows off a critter npc. The unique thing about critters is how you can catch them with a bug net.  
	/// The important bits are: Main.npcCatchable, npc.catchItem, and item.makeNPC
	/// We will also show off adding an item to an existing RecipeGroup (see ExampleMod.AddRecipeGroups)
	/// </summary>
	internal class ExampleCritterNPC : ModNPC
	{
		public override bool Autoload(ref string name) {
			IL.Terraria.Wiring.HitWireSingle += HookStatue;
			return base.Autoload(ref name);
		}

		/// <summary>
		/// Change the following code sequence in Wiring.HitWireSingle
		/// num145 = Utils.SelectRandom(Main.rand, new short[5]
		/// {
		/// 	359,
		/// 	359,
		/// 	359,
		/// 	359,
		/// 	360,
		/// });
		/// 
		/// to 
		/// 
		/// var arr = new short[5]
		/// {
		/// 	359,
		/// 	359,
		/// 	359,
		/// 	359,
		/// 	360,
		/// }
		/// arr = arr.ToList().Add(id).ToArray();
		/// num145 = Utils.SelectRandom(Main.rand, arr);
		/// 
		/// </summary>
		/// <param name="il"></param>
		private void HookStatue(ILContext il) {
			// obtain a cursor positioned before the first instruction of the method
			// the cursor is used for navigating and modifying the il
			var c = new ILCursor(il);

			// the exact location for this hook is very complex to search for due to the hook instructions not being unique, and buried deep in control flow
			// switch statements are sometimes compiled to if-else chains, and debug builds litter the code with no-ops and redundant locals

			// in general you want to search using structure and function rather than numerical constants which may change across different versions or compile settings
			// using local variable indices is almost always a bad idea

			// we can search for
			// switch (*)
			//   case 56:
			//     Utils.SelectRandom *

			// in general you'd want to look for a specific switch variable, or perhaps the containing switch (type) { case 105:
			// but the generated IL is really variable and hard to match in this case

			// we'll just use the fact that there are no other switch statements with case 56, followed by a SelectRandom

			ILLabel[] targets = null;
			while (c.TryGotoNext(i => i.MatchSwitch(out targets))) {
				// some optimising compilers generate a sub so that all the switch cases start at 0
				// ldc.i4.s 51
				// sub
				// switch
				int offset = 0;
				if (c.Prev.MatchSub() && c.Prev.Previous.MatchLdcI4(out offset)) {
					;
				}

				// not enough switch instructions
				if (targets.Length < 56 - offset) {
					continue;
				}

				var target = targets[56 - offset];
				if (target == null) {
					continue;
				}

				// move the cursor to case 56:
				c.GotoLabel(target);
				// there's lots of extra checks we could add here to make sure we're at the right spot, such as not encountering any branching instructions
				c.GotoNext(i => i.MatchCall(typeof(Utils), nameof(Utils.SelectRandom)));

				// goto next positions us before the instruction we searched for, so we can insert our array modifying code right here
				c.EmitDelegate<Func<short[], short[]>>(arr => {
					// resize the array and add our custom snail
					Array.Resize(ref arr, arr.Length+1);
					arr[arr.Length-1] = (short)npc.type;
					return arr;
				});

				// hook applied successfully
				return;
			}

			// couldn't find the right place to insert
			throw new Exception("Hook location not found, switch(*) { case 56: ...");
		}

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Lava Snail");
			Main.npcFrameCount[npc.type] = Main.npcFrameCount[NPCID.GlowingSnail];
			Main.npcCatchable[npc.type] = true;
		}

		public override void SetDefaults() {
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
			npc.catchItem = (short)ItemType<ExampleCritterItem>();
			npc.lavaImmune = true;
			//npc.aiStyle = 0;
			npc.friendly = true; // We have to add this and CanBeHitByItem/CanBeHitByProjectile because of reasons.
			aiType = NPCID.GlowingSnail;
			animationType = NPCID.GlowingSnail;
		}

		public override bool? CanBeHitByItem(Player player, Item item) {
			return true;
		}

		public override bool? CanBeHitByProjectile(Projectile projectile) {
			return true;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return SpawnCondition.Underworld.Chance * 0.1f;
		}

		public override void HitEffect(int hitDirection, double damage) {
			if (npc.life <= 0) {
				for (int i = 0; i < 6; i++) {
					int dust = Dust.NewDust(npc.position, npc.width, npc.height, 200, 2 * hitDirection, -2f);
					if (Main.rand.NextBool(2)) {
						Main.dust[dust].noGravity = true;
						Main.dust[dust].scale = 1.2f * npc.scale;
					}
					else {
						Main.dust[dust].scale = 0.7f * npc.scale;
					}
				}
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/LavaSnailHead"), npc.scale);
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/LavaSnailShell"), npc.scale);
			}
		}

		public override Color? GetAlpha(Color drawColor) {
			// GetAlpha gives our Lava Snail a red glow.
			drawColor.R = 255;
			// both these do the same in this situation, using these methods is useful.
			drawColor.G = Utils.Clamp<byte>(drawColor.G, 175, 255);
			drawColor.B = Math.Min(drawColor.B, (byte)75);
			drawColor.A = 255;
			return drawColor;
		}

		public override bool PreAI() {
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

		public override void OnCatchNPC(Player player, Item item) {
			item.stack = 2;

			try {
				var npcCenter = npc.Center.ToTileCoordinates();
				if (!WorldGen.SolidTile(npcCenter.X, npcCenter.Y) && Main.tile[npcCenter.X, npcCenter.Y].liquid == 0) {
					Main.tile[npcCenter.X, npcCenter.Y].liquid = (byte)Main.rand.Next(50, 150);
					Main.tile[npcCenter.X, npcCenter.Y].lava(true);
					Main.tile[npcCenter.X, npcCenter.Y].honey(false);
					WorldGen.SquareTileFrame(npcCenter.X, npcCenter.Y, true);
				}
			}
			catch {
				return;
			}
		}

		// TODO: Hooks for Collision_MoveSnailOnSlopes and npc.aiStyle = 67 problem
	}

	internal class ExampleCritterItem : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Lava Snail");
		}

		public override void SetDefaults() {
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
			item.makeNPC = (short)NPCType<ExampleCritterNPC>();
		}
	}
}
