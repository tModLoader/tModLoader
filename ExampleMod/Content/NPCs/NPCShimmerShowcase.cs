using ExampleMod.Content.Biomes;
using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using System;
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
		//1: if the npc is on the left side of the world then transform into 5 - 10 frogs and 30 explosive bunnies (item), then shoot bullets from each skeleton
		//2: if Plantera has been defeated then transform into 20 example items
		//3: if an early game boss has been defeated then transform into the bride
		//4: if all other conditions fail, transform into a skeleton

		CreateShimmerTransformation()
			// A shimmer callback applies to the one transformation, whereas ModNPC.CanShimmer applies to every transformation this NPC does
			.AddCanShimmerCallBack((ShimmerTransformation transformation, IModShimmerable target) => target.Center.X <= Main.maxTilesX * 8)
			// Results
			.AddItemResult(ItemID.ExplosiveBunny, 30)
			.AddNPCResult(NPCID.Frog)
			.AddModifyShimmerCallBack((ShimmerTransformation transformation, IModShimmerable source) => transformation.Results[^1] = transformation.Results[^1] with { Count = Main.rand.Next(5, 11) }) // Spawn 5 - 10 frogs inclusive
			.AddModifyShimmerCallBack(ModifyShimmer_GildFrogs) // Applies the gold critter chance to the frogs in the transformation
			.AddOnShimmerCallBack(OnShimmer_SpawnFriendlyBullets)
			.DisableChainedShimmers() // Ensures the results must leave shimmer and become fully opaque before they can shimmer. By default shimmers chain into each other
			.Register();

		CreateShimmerTransformation()
			.AddCondition(Condition.DownedPlantera)
			.AddItemResult<ExampleItem>(20)
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
	// method. While it does not need to be static it should as, as any modification to ModNPC.NPC is instance based (affecting the instance created for SetStaticDefaults), use "origin" for modifying the spawner
	public static void OnShimmer_SpawnFriendlyBullets(ShimmerTransformation transformation, IModShimmerable origin, IEnumerable<IModShimmerable> spawnedShimmerables) {
		foreach (IModShimmerable spawnedShimmerable in spawnedShimmerables) {
			Projectile p = Projectile.NewProjectileDirect(spawnedShimmerable.GetSource_Misc("Shimmer"), spawnedShimmerable.Center, spawnedShimmerable.Velocity + Vector2.UnitY * -2, ProjectileID.Bullet, 20, 1);
			p.friendly = true;
			p.hostile = false;
		};
		// Here we show one way to spawn projectiles, we do it after the transformation because we need the list of spawned IModShimmerables. See
		// ExampleComplexCustomShimmerable for how to utilise projectiles in shimmer better
	}


	// Checks for NPCID.Frog and rolls for golden critters, then replaces the frog with the golden version if needed
	public static void ModifyShimmer_GildFrogs(ShimmerTransformation transformation, IModShimmerable target) {
		Player closestPlayer = Main.player[Player.FindClosest(target.Center, 1, 1)];
		int rollGoldCritters(int rollCount) {
			int successes = 0;
			for (int i = 0; i < rollCount; i++)
				if (closestPlayer.RollLuck(NPC.goldCritterChance) == 0)
					successes++;
			return successes;
		}
		int changeFrogCount = transformation.Results.Sum(result => result.IsNPCResult(NPCID.Frog) ? rollGoldCritters(result.Count) : 0);
		if (changeFrogCount > 0) {
			transformation.AddNPCResult(NPCID.GoldFrog, changeFrogCount);

			while (changeFrogCount > 0) {
				int index = transformation.Results.FindIndex(result => result.IsNPCResult(NPCID.Frog));
				if (index < 0)
					return;

				int removeFrogCount = Math.Min(transformation.Results[index].Count, changeFrogCount);
				if (removeFrogCount >= transformation.Results[index].Count)
					transformation.Results.RemoveAt(index);
				else
					transformation.Results[index] = transformation.Results[index] with { Count = transformation.Results[index].Count - removeFrogCount };

				changeFrogCount -= removeFrogCount;
			}
		}
	}
}