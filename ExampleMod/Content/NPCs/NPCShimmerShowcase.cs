using ExampleMod.Content.Biomes;
using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace ExampleMod.Content.NPCs;

// For info on ModNPC stuff see PartyZombie.cs
public class NPCShimmerShowcase : ModNPC
{
	public override string Texture => $"Terraria/Images/NPC_{NPCID.Zombie}";
	public override void SetStaticDefaults() {
		Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Zombie];

		//So on the proceeding lines we set four possible shimmer results with conditions in the following in the priority order:
		//1: if the npc is on the left side of the world then transform into 3 skeletons and 30 explosive bunnies, then shoot bullets from each skeleton
		//2: if Plantera has been defeated then transform into 10 example items
		//3: if an early game boss has been defeated then transform into the bride
		//4: if all other conditions fail, transform into a skeleton

		CreateShimmerTransformation()
			// A shimmer callback applies to the one transformation, whereas ModNPC.CanShimmer applies to every transformation this NPC does
			.AddCanShimmerCallBack((ShimmerTransformation transformation, IModShimmerable target) => target.Center.X <= Main.maxTilesX * 8)
			.AddItemResult(ItemID.ExplosiveBunny, 30)
			.AddNPCResult(NPCID.Skeleton, 3)
			.AddOnShimmerCallBack(OnShimmerCallBack)
			.Register();

		CreateShimmerTransformation()
			.AddCondition(Condition.DownedPlantera)
			.AddModItemResult<ExampleItem>(20)
			.Register();

		CreateShimmerTransformation()
			.AddCondition(Condition.DownedEarlygameBoss)
			.AddNPCResult(NPCID.TheBride, 1)
			.Register();

		// Sets a basic npc transformation, this uses the vanilla method which is overridden by ShimmerTransformation unless all conditions fall through
		NPCID.Sets.ShimmerTransformToNPC[NPC.type] = NPCID.Skeleton;
		// In vanilla an NPC spawned from a statue will despawn in shimmer, he we disable that and allow it to shimmer as normal, this NPC does not have a statue, but this would be used if it did
		NPCID.Sets.ShimmerIgnoreNPCSpawnedFromStatue[NPC.type] = true;
	}

	public override void SetDefaults() {
		NPC.CloneDefaults(NPCID.Zombie);
		NPC.lifeMax = 200;
		AIType = NPCID.Zombie;
		AnimationType = NPCID.Zombie;
		Banner = Item.NPCtoBanner(NPCID.Zombie);
		BannerItem = Item.BannerToItem(Banner);
	}

	public override void ModifyNPCLoot(NPCLoot npcLoot) {
		Main.ItemDropsDB.GetRulesForNPCID(NPCID.Zombie, false).ForEach((IItemDropRule zombieDropRule) => npcLoot.Add(zombieDropRule));
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
		bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				new FlavorTextBestiaryInfoElement("This zombie looks like a normal zombie but interacts with shimmer differently based on different conditions."),
				new BestiaryPortraitBackgroundProviderPreferenceInfoElement(ModContent.GetInstance<ExampleSurfaceBiome>().ModBiomeBestiaryInfoElement),
			});
	}

	public override float SpawnChance(NPCSpawnInfo spawnInfo) {
		return SpawnCondition.OverworldNightMonster.Chance * 0.5f;
	}

	// This is static and not an override, it is used earlier to pass as a ShimmerTransformation.OnShimmerCallBack, OnShimmerCallBack is a delegate, a reference to a method.
	// While it does not need to be static it should as, as any modification to ModNPC.NPC is instance based, use "origin"
	public static void OnShimmerCallBack(ShimmerTransformation transformation, IModShimmerable origin, List<IModShimmerable> spawnedShimmerables) {
		spawnedShimmerables.ForEach((IModShimmerable spawnedShimmerable)
			=> {
				Projectile p = Projectile.NewProjectileDirect(spawnedShimmerable.GetSource_ForShimmer(), spawnedShimmerable.Center, spawnedShimmerable.ShimmerVelocity + Vector2.UnitY * -2, ProjectileID.Bullet, 20, 1);
				p.friendly = false;
				p.hostile = true;
			});
	}
}