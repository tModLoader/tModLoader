using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader.Utilities;
using ExampleMod.Content.Biomes;
using ExampleMod.Content.Items;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ExampleMod.Content.NPCs
{
	// for info on other ModNPC stuff see PartyZombie.cs
	public class NPCShimmerShowcase : ModNPC
	{
		public override string Texture => $"Terraria/Images/NPC_{NPCID.Zombie}";

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Zombie];

			// So on the proceeding lines we set four possible shimmer results with conditions in the following in the priority order:
			// 1: if the npc is on the left side of the world then spawn 3 skeletons and 30 exampleitems, then shoot bullets from each skeleton
			// 2: if Plantera has been defeated then spawn 10 example items
			// 3: if an early game boss has been defeated then spawn the bride
			// 4: if all other conditions fail, transform into a skeleton

			// Here we set up a shimmer transformation for the npc where if the NPC is on the left half of the world, it spawns three skeletons and 30 example items
			CreateShimmerTransformation()
				.AddCanShimmerCallBack(new ShimmerTransformation.CanShimmerCallBack((ShimmerTransformation transformation, Entity target) => target.Center.X <= Main.maxTilesX * 8))
				.AddResult(new ModShimmerResult(ModShimmerTypeID.Item, ModContent.ItemType<ExampleItem>(), 30))
				.AddResult(new ModShimmerResult(ModShimmerTypeID.NPC, NPCID.Skeleton, 3))
				.AddOnShimmerCallBack(new ShimmerTransformation.OnShimmerCallBack(OnShimmerCallBack))
				.Register();

			// Here we set up a shimmer transformation for the npc where if Plantera has been killed, it spawns 20 example items
			CreateShimmerTransformation()
				.AddCondition(Condition.DownedPlantera)
				.AddResult(new ModShimmerResult(ModShimmerTypeID.Item, ModContent.ItemType<ExampleItem>(), 20))
				.Register();

			// Here we set up a shimmer transformation for the npc where if an early game boss has been killed, it spawns one the bride
			CreateShimmerTransformation()
				.AddCondition(Condition.DownedEarlygameBoss)
				.AddResult(new ModShimmerResult(ModShimmerTypeID.NPC, NPCID.TheBride, 1))
				.Register();

			NPCID.Sets.ShimmerTransformToNPC[NPC.type] = NPCID.Skeleton; // Sets a basic npc transformation, this uses the vanilla method
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
			NPC.aiStyle = 3; // Fighter AI, important to choose the aiStyle that matches the NPCID that we want to mimic

			AIType = NPCID.Zombie; // Use vanilla zombie's type when executing AI code. (This also means it will try to despawn during daytime)
			AnimationType = NPCID.Zombie; // Use vanilla zombie's type when executing animation code. Important to also match Main.npcFrameCount[NPC.type] in SetStaticDefaults.
			Banner = Item.NPCtoBanner(NPCID.Zombie); // Makes this NPC get affected by the normal zombie banner.
			BannerItem = Item.BannerToItem(Banner); // Makes kills of this NPC go towards dropping the banner it's associated with.
			SpawnModBiomes = new int[1] { ModContent.GetInstance<ExampleSurfaceBiome>().Type }; // Associates this NPC with the ExampleSurfaceBiome in Bestiary
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) { // See PartyZombie.cs for info here
			var zombieDropRules = Main.ItemDropsDB.GetRulesForNPCID(NPCID.Zombie, false);
			foreach (var zombieDropRule in zombieDropRules) {
				npcLoot.Add(zombieDropRule);
			}
			npcLoot.Add(ItemDropRule.Common(ItemID.Bone, 1));
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return SpawnCondition.OverworldNightMonster.Chance * 0.05f;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				new FlavorTextBestiaryInfoElement("This zombie looks like a normal zombie but interacts with shimmer differently based on different conditions."),
				new BestiaryPortraitBackgroundProviderPreferenceInfoElement(ModContent.GetInstance<ExampleSurfaceBiome>().ModBiomeBestiaryInfoElement),
			});
		}

		public static void OnShimmerCallBack(ShimmerTransformation transformation, Entity origin, List<Entity> spawnedEntities) {
			foreach (Entity entity in spawnedEntities) {
				Projectile p = Projectile.NewProjectileDirect(entity.GetSource_Misc("Shimmer"), entity.position, entity.velocity + Vector2.UnitY * 10, ProjectileID.Bullet, 20, 1);
				p.friendly = false;
				p.hostile = true;
			}
		}
	}
}