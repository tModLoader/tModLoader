using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using ExampleMod.Content.Items;
using ExampleMod.Content.Buffs;

namespace ExampleMod.Content.NPCS
{
	// Party Zombie is a pretty basic clone of a vanilla NPC. To learn how to further adapt vanilla NPC behaviors, see https://github.com/tModLoader/tModLoader/wiki/Advanced-Vanilla-Code-Adaption#example-npc-npc-clone-with-modified-projectile-hoplite
	public class PartyZombie : ModNPC
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Zombie");

			Main.npcFrameCount[NPC.type] = Main.npcFrameCount[NPCID.Zombie];

			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers(0) { //Influences how the NPC looks in the Bestiary
				Velocity = 1f //Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, value);
		}

		public override void SetDefaults() {
			NPC.width = 18;
			NPC.height = 40;
			NPC.damage = 14;
			NPC.defense = 6;
			NPC.lifeMax = 200;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.value = 60f;
			NPC.knockBackResist = 0.5f;
			NPC.aiStyle = 3;

			AIType = NPCID.Zombie; // Use vanilla zombie's type when executing AI code.
			AnimationType = NPCID.Zombie; // Use vanilla zombie's type when executing animation code.
			Banner = Item.NPCtoBanner(NPCID.Zombie); // Makes this NPC get affected by the normal zombie banner.
			BannerItem = Item.BannerToItem(Banner); // Makes kills of this NPC go towards dropping the banner it's associated with.
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) { 
			npcLoot.Add(ItemDropRule.Common(ItemID.Shackle, 50)); //Drop shackles with a 1 out of 50 chance.
			npcLoot.Add(ItemDropRule.Common(ItemID.ZombieArm, 250)); //Drop zombie arm with a 1 out of 250 chance.	
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return SpawnCondition.OverworldNightMonster.Chance * 0.5f;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				// Sets the spawning conditions of this NPC that is listed in the bestiary.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,

				// Sets the description of this NPC that is listed in the bestiary.
				new FlavorTextBestiaryInfoElement("This type of zombie for some reason really likes to spread confetti around. Otherwise, it behaves just like a normal zombie.")
			});
		}

		public override void HitEffect(int hitDirection, double damage) {
			// Spawn confetti when this zombie is hit.
			for (int i = 0; i < 10; i++) {
				int dustType = Main.rand.Next(139, 143);
				var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType);

				dust.velocity.X += Main.rand.NextFloat(-0.05f, 0.05f);
				dust.velocity.Y += Main.rand.NextFloat(-0.05f, 0.05f);
				
				dust.scale *= 1f + Main.rand.NextFloat(-0.03f, 0.03f);
			}
		}

		public override void OnHitPlayer(Player target, int damage, bool crit) {
			//Here we can make things happen if this NPC hits a player via its hitbox (not projectiles it shoots, this is handled in the projectile code usually)
			//Common use is applying buffs/debuffs:

			int buffType = ModContent.BuffType<AnimatedBuff>();
			//Alternatively, you can use a vanilla buff: buffType = BuffID.Slow;

			int timeToAdd = 5 * 60; //This makes it 5 seconds, one second is 60 ticks
			target.AddBuff(buffType, timeToAdd);
		}
	}
}
