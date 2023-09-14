using ExampleMod.Content.Biomes;
using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
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
			// Results
			.AddItemResult(ItemID.ExplosiveBunny, 30) // 0
													  //.AddNPCResult(NPCID.Skeleton, 3) // 1
			.AddNPCResult(NPCID.Frog) // 2
			.AddModifyShimmerCallBack((ShimmerTransformation transformation, IModShimmerable source) => transformation.Results[^1] = transformation.Results[^1] with { Count = Main.rand.Next(5, 11) }) // Spawn 5 - 10 frogs inclusive
			.AddModifyShimmerCallBack(ModifyShimmer_GildFrogs) // Applies the gold critter chance to the frogs in the transformation
			.AddOnShimmerCallBack(OnShimmer_SpawnFriendlyBullets)
			.DisableChainedShimmers()
			.Register();

		CreateShimmerTransformation()
			.AddCondition(Condition.DownedPlantera)
			.AddModItemResult<ExampleItem>(20)
			.Register();

		CreateShimmerTransformation()
			.AddCondition(Condition.DownedEarlygameBoss)
			.AddNPCResult(NPCID.TheBride)
			.Register();

		// Sets a basic npc transformation, this uses the vanilla method which is overridden by ShimmerTransformation unless all conditions fall through
		NPCID.Sets.ShimmerTransformToNPC[NPC.type] = NPCID.Skeleton;
		// In vanilla an NPC spawned from a statue will despawn in shimmer, he we disable that and allow it to shimmer as normal, this NPC does not have a statue, but
		// this would be used if it did
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

	// This is static and not an override, it is used earlier to pass as a ShimmerTransformation.OnShimmerCallBack, OnShimmerCallBack is a delegate, a reference to a
	// method. While it does not need to be static it should as, as any modification to ModNPC.NPC is instance based, use "origin"
	public static void OnShimmer_SpawnFriendlyBullets(ShimmerTransformation transformation, IModShimmerable origin, IEnumerable<IModShimmerable> spawnedShimmerables) {
		foreach (IModShimmerable spawnedShimmerable in spawnedShimmerables) {
			Projectile p = Projectile.NewProjectileDirect(spawnedShimmerable.GetSource_Misc("Shimmer"), spawnedShimmerable.Center, spawnedShimmerable.Velocity + Vector2.UnitY * -2, ProjectileID.Bullet, 20, 1);
			p.friendly = true;
			p.hostile = false;
		};
		// Here we show one way to spawn projectiles, we do it after the transformation because we need the list of spawned IModShimmerables. See
		// ExampleComplexCustomShimmerable for how to utilise projectiles in shimmer better
	}

	public static void ModifyShimmer_GildFrogs(ShimmerTransformation transformation, IModShimmerable target) {
		int sumGoodRolls(int times) {
			Player closestPlayer = Main.player[Player.FindClosest(target.Center, 1, 1)];

			int goodRolls = 0;
			for (int i = 0; i < times; i++)
				if (closestPlayer.RollLuck(NPC.goldCritterChance) == 0)
					goodRolls++;
			return goodRolls;
		}
		int changeFrogCount = transformation.Results.Sum(result => result.IsNPCResult(NPCID.Frog) ? sumGoodRolls(result.Count) : 0);

		if (changeFrogCount > 0) {
			for (int i = 0; i < changeFrogCount; i++) {
				int index = transformation.Results.FindIndex(result => result.IsNPCResult(NPCID.Frog));
				if (index < 0)
					break;

				if (transformation.Results[index].Count > 1)
					transformation.Results[index] = transformation.Results[index] with { Count = transformation.Results[index].Count - 1 };
				else
					transformation.Results.RemoveAt(index);
			}

			transformation.AddNPCResult(NPCID.GoldFrog, changeFrogCount);
		}
	}
}