using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.NPCs;

/// <summary>
/// This file shows off a critter npc. The unique thing about critters is how you can catch them with a bug net.
/// The important bits are: Main.npcCatchable, npc.catchItem, and item.makeNPC
/// We will also show off adding an item to an existing RecipeGroup (see ExampleMod.AddRecipeGroups)
/// </summary>
internal class ExampleCritterNPC : ModNPC
{
	private const int ClonedNPCID = NPCID.Frog;

	public override bool IsLoadingEnabled(Mod mod) {
		IL_Wiring.HitWireSingle += HookFrogStatue;
		return true;
	}

	/// <summary>
	/// Change the following code sequence in Wiring.HitWireSingle
	/// <code>
	///		case 61:
	///			num115 = 361;
	/// </code>
	/// to
	/// <code>
	///		case 61:
	///			num115 = Utils.SelectRandom(Main.rand, new int[2] {
	///				361,
	///				(our npc type)
	///			});
	/// </code>
	/// </summary>
	/// <param name="ilContext"></param>
	private void HookFrogStatue(ILContext ilContext) {
		try {
			// obtain a cursor positioned before the first instruction of the method
			// the cursor is used for navigating and modifying the il
			ILCursor ilCursor = new ILCursor(ilContext);

			// the exact location for this hook is very complex to search for due to the hook instructions not being unique, and buried deep in control flow
			// switch statements are sometimes compiled to if-else chains, and debug builds litter the code with no-ops and redundant locals

			// in general you want to search using structure and function rather than numerical constants which may change across different versions or compile settings
			// using local variable indices is almost always a bad idea

			// we can search for
			// switch (*)
			//   case 61:

			// in general you'd want to look for a specific switch variable, or perhaps the containing switch (type) { case 105:
			// but the generated IL is really variable and hard to match in this case

			// we'll just use the fact that there are no other switch statements with case 61

			ILLabel[] targets = null;
			while (ilCursor.TryGotoNext(i => i.MatchSwitch(out targets))) {
				// some optimising compilers generate a sub so that all the switch cases start at 0
				// ldc.i4.s 51
				// sub
				// switch
				int offset = 0;
				if (ilCursor.Prev.MatchSub() && ilCursor.Prev.Previous.MatchLdcI4(out offset)) {
					;
				}

				// get the label for case 61: if it exists
				int case61Index = 61 - offset;
				if (case61Index < 0 || case61Index >= targets.Length || targets[case61Index] is not ILLabel target) {
					continue;
				}

				// move the cursor to case 61:
				ilCursor.GotoLabel(target);
				ilCursor.Index++;
				// there's lots of extra checks we could add here to make sure we're at the right spot, such as not encountering any branching instructions
				//ilCursor.GotoNext(i => i.MatchCall(typeof(Utils), nameof(Utils.SelectRandom)));

				// goto next positions us before the instruction we searched for, so we can insert our array modifying code right here
				ilCursor.EmitDelegate((int originalAssign) => Main.rand.NextBool() ? originalAssign : NPC.type);

				// hook applied successfully
				return;
			}

			// couldn't find the right place to insert
			throw new Exception("Hook location not found, switch(*) { case 61: ...");
		}
		catch {
			MonoModHooks.DumpIL(ModContent.GetInstance<ExampleMod>(), ilContext);
			throw;
		}
	}

	public override void SetDefaults() {
		NPC.CloneDefaults(ClonedNPCID);
		NPC.catchItem = ItemType<ExampleCritterItem>();
		NPC.lavaImmune = true;
		NPC.friendly = true;
		AIType = ClonedNPCID;
		NPC.aiStyle = NPCAIStyleID.Passive;
		AnimationType = ClonedNPCID;
	}

	public override void SetStaticDefaults() {
		Main.npcFrameCount[Type] = Main.npcFrameCount[ClonedNPCID];
		NPCID.Sets.CountsAsCritter[NPC.type] = true;
		NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[NPC.type] = true;
		NPCID.Sets.TownCritter[NPC.type] = true;
		NPC.buffImmune[BuffID.Confused] = true;
	}

	public override float SpawnChance(NPCSpawnInfo spawnInfo) {
		return SpawnCondition.Underworld.Chance * 0.1f;
	}

	public override void HitEffect(NPC.HitInfo hit) {
		if (NPC.life <= 0) {
			for (int i = 0; i < 6; i++) {
				Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Worm, 2 * hit.HitDirection, -2f);
				if (Main.rand.NextBool(2)) {
					dust.noGravity = true;
					dust.scale = 1.2f * NPC.scale;
				}
				else {
					dust.scale = 0.7f * NPC.scale;
				}
			}
			//Gore.NewGore(NPC.position, NPC.velocity, Mod.GetGoreSlot("Gores/LavaSnailHead"), NPC.scale);
			//Gore.NewGore(NPC.position, NPC.velocity, Mod.GetGoreSlot("Gores/LavaSnailShell"), NPC.scale);
		}
	}

	public override Color? GetAlpha(Color drawColor) {
		// GetAlpha gives our Lava Snail a red glow.
		// both these do the same in this situation, using these methods is useful.
		return drawColor with {
			R = 255,
			G = Utils.Clamp<byte>(drawColor.G, 175, 255),
			B = Math.Min(drawColor.B, (byte)75),
			A = 255
		};
	}

	public override bool PreAI() {//TODO: Make the frog swim in lava instead of water
								  // Usually we can use npc.wet, but aiStyle 67 prevents wet from being set.
		if (Collision.WetCollision(NPC.position, NPC.width, NPC.height)) //if (npc.wet)
		{
			// These 3 lines instantly kill the npc without showing damage numbers, dropping loot, or playing DeathSound. Use this for instant deaths
			NPC.life = 0;
			NPC.HitEffect();
			NPC.active = false;
			SoundEngine.PlaySound(SoundID.NPCDeath16, NPC.position); // plays a fizzle sound
		}
		return base.PreAI();
	}

	public override void OnCaughtBy(Player player, Item item, bool failed) {
		//try {
		Point npcTile = NPC.Center.ToTileCoordinates();

		if (!WorldGen.SolidTile(npcTile.X, npcTile.Y)) { // Check if the tile the npc resides the most in is non solid
			Main.tile[npcTile].LiquidAmount = Main.tile[npcTile].Get<LiquidData>().LiquidType == LiquidID.Lava ? // Check if the tile has lava in it
				Math.Max((byte)Main.rand.Next(50, 150), Main.tile[npcTile].LiquidAmount) // If it does, then top up the amount
				: (byte)Main.rand.Next(50, 150); // If it doesn't, then overwrite the amount
			Main.tile[npcTile].Get<LiquidData>().LiquidType = LiquidID.Lava; // Set the liquid type to lava
			WorldGen.SquareTileFrame(npcTile.X, npcTile.Y, true); // Update the surrounding area in the tilemap
		}
		//}
		//catch {
		//	return;
		//} Is this still crash code in 1.4.4?
	}
}

internal class ExampleCritterItem : ModItem
{
	private const int ClonedItemID = ItemID.Frog;

	public override void SetDefaults() {
		//useStyle = 1;
		//autoReuse = true;
		//useTurn = true;
		//useAnimation = 15;
		//useTime = 10;
		//maxStack = CommonMaxStack;
		//consumable = true;
		//width = 12;
		//height = 12;
		//makeNPC = 361;
		//noUseGraphic = true;

		// Cloning ItemID.Frog sets the preceeding values
		Item.CloneDefaults(ClonedItemID);
		Item.makeNPC = NPCType<ExampleCritterNPC>();
		Item.value += Item.buyPrice(0, 0, 30, 0); // Make this critter worth slightly more than the frog
		Item.rare = ItemRarityID.Blue;
	}
}