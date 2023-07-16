using ExampleMod.Content.Biomes;
using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.NPCs
{
	// For info on ModNPC stuff see PartyZombie.cs
	public class NPCShimmerShowcase : ModNPC
	{
		public override string Texture => $"Terraria/Images/NPC_{NPCID.Zombie}";

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Zombie];

			// So on the proceeding lines we set four possible shimmer results with conditions in the following in the priority order:
			// 1: if the npc is on the left side of the world then spawn 3 skeletons and 30 example items, then shoot bullets from each skeleton
			// 2: if Plantera has been defeated then spawn 10 example items
			// 3: if an early game boss has been defeated then spawn the bride
			// 4: if all other conditions fail, transform into a skeleton

			// Here we set up a shimmer transformation for the npc where if the NPC is on the left half of the world, it spawns three skeletons and 30 example items
			CreateShimmerTransformation()
				.AddCanShimmerCallBack(new ModShimmer.CanShimmerCallBack((ModShimmer transformation, Entity target) => target.Center.X <= Main.maxTilesX * 8))
				.AddCanShimmerCallBack(new ModShimmer.CanShimmerCallBack((ModShimmer transformation, Entity target) => true))
				.AddModItemResult<ExampleItem>(30)
				.AddResult(new ModShimmerResult(ModShimmerTypeID.NPC, NPCID.Skeleton, 3))
				.AddOnShimmerCallBack(new ModShimmer.OnShimmerCallBack(OnShimmerCallBack))
				.Register();

			// Here we set up a shimmer transformation for the npc where if Plantera has been killed, it spawns 20 example items
			CreateShimmerTransformation()
				.AddCondition(Condition.DownedPlantera)
				.AddModItemResult<ExampleItem>(20)
				.Register();

			// Here we set up a shimmer transformation for the npc where if an early game boss has been killed, it spawns one the bride
			CreateShimmerTransformation()
				.AddCondition(Condition.DownedEarlygameBoss)
				.AddResult(new ModShimmerResult(ModShimmerTypeID.NPC, NPCID.TheBride, 1))
				.Register();

			NPCID.Sets.ShimmerTransformToNPC[NPC.type] = NPCID.Skeleton; // Sets a basic npc transformation, this uses the vanilla method, overrides by ModShimmer unless all conditions fall through
			NPCID.Sets.IgnoreNPCSpawnedFromStatue[NPC.type] = true;
		}

		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Zombie);
			NPC.lifeMax = 200;
			AIType = NPCID.Zombie;
			AnimationType = NPCID.Zombie;
			Banner = Item.NPCtoBanner(NPCID.Zombie);
			BannerItem = Item.BannerToItem(Banner);
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
			=> Main.ItemDropsDB.GetRulesForNPCID(NPCID.Zombie, false).ForEach((IItemDropRule zombieDropRule) => npcLoot.Add(zombieDropRule));

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
			=> bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				new FlavorTextBestiaryInfoElement("This zombie looks like a normal zombie but interacts with shimmer differently based on different conditions."),
				new BestiaryPortraitBackgroundProviderPreferenceInfoElement(ModContent.GetInstance<ExampleSurfaceBiome>().ModBiomeBestiaryInfoElement),
			});

		// This is static and not an override, it is used to create an instance of ModShimmer.OnShimmerCallBack, this is a delegate, delegates are essentially a reference to a method and as such need to be static
		public static void OnShimmerCallBack(ModShimmer transformation, Entity origin, List<Entity> spawnedEntities)
			=> spawnedEntities.ForEach((Entity entity)
				=> {
					Projectile p = Projectile.NewProjectileDirect(entity.GetSource_Misc("Shimmer"), entity.position, entity.velocity + Vector2.UnitY * -2, ProjectileID.Bullet, 20, 1);
					p.friendly = false;
					p.hostile = true;
				});
	}
}