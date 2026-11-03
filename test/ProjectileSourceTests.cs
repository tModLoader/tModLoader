using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Utilities;

namespace Terraria.ModLoader;

[TestClass]
[DoNotParallelize]
public class ProjectileSourceTests
{
	[ClassInitialize]
	public static void ClassInit(TestContext context)
	{
		Program.SavePath ??= ".";
		Main.dedServ = true;
	}

	[DataTestMethod]
	[DataRow(0, 7)]
	[DataRow(3, 7)]
	[DataRow(3, 0)]
	public void ParentProjectileArmorPenetrationIsInheritedOnce(int childBase, int parentBonus)
	{
		var parent = new Projectile {
			ArmorPenetration = parentBonus,
			OriginalArmorPenetration = 4,
			CritChance = 11,
			OriginalCritChance = 2
		};
		var child = new Projectile { ArmorPenetration = childBase, CritChance = 3 };

		child.ApplyStatsFromSource(new EntitySource_Parent(parent));

		Assert.AreEqual(childBase + parentBonus, child.ArmorPenetration);
		Assert.AreEqual(childBase + 4, child.OriginalArmorPenetration);
		Assert.AreEqual(14, child.CritChance);
		Assert.AreEqual(5, child.OriginalCritChance);
		Assert.AreEqual(parentBonus, parent.ArmorPenetration);
	}

	[TestMethod]
	public void ArmorPenetrationDoesNotDoubleAcrossProjectileGenerations()
	{
		var parent = new Projectile { ArmorPenetration = 7, OriginalArmorPenetration = 4 };
		var child = new Projectile();
		var grandchild = new Projectile();

		child.ApplyStatsFromSource(new EntitySource_Parent(parent));
		grandchild.ApplyStatsFromSource(new EntitySource_Parent(child));

		Assert.AreEqual(7, child.ArmorPenetration);
		Assert.AreEqual(7, grandchild.ArmorPenetration);
		Assert.AreEqual(4, grandchild.OriginalArmorPenetration);
	}

	[TestMethod]
	public void PlayerSourceAddsPlayerBonusOnce()
	{
		var player = new Player();
		player.GetArmorPenetration(DamageClass.Generic) = 5;
		var projectile = new Projectile { ArmorPenetration = 3, DamageType = DamageClass.Generic };

		projectile.ApplyStatsFromSource(new EntitySource_Parent(player));

		Assert.AreEqual(8, projectile.ArmorPenetration);
		Assert.AreEqual(3, projectile.OriginalArmorPenetration);
	}

	[DataTestMethod]
	[DataRow(4, 0)]
	[DataRow(0, 5)]
	[DataRow(4, 5)]
	public void ItemSourceRetainsDistinctVanillaAndModdedBonuses(int vanillaBonus, int moddedBonus)
	{
		var player = new Player();
		player.GetArmorPenetration(DamageClass.Generic) = 3;
		var item = new Item {
			armorPenetration = vanillaBonus,
			ArmorPenetration = moddedBonus,
			DamageType = DamageClass.Generic
		};
		var projectile = new Projectile { ArmorPenetration = 2 };

		projectile.ApplyStatsFromSource(new EntitySource_ItemUse(player, item));

		Assert.AreEqual(2 + vanillaBonus + moddedBonus + 3, projectile.ArmorPenetration);
		Assert.AreEqual(2 + vanillaBonus + moddedBonus, projectile.OriginalArmorPenetration, "The snapshot must retain item bonuses without player bonuses.");
	}

	[TestMethod]
	public void NewProjectileInheritsSourceStatsOnce()
	{
		WithProjectileWorld(() => {
			var parent = new Projectile { ArmorPenetration = 7, OriginalArmorPenetration = 4, CritChance = 11, OriginalCritChance = 2 };

			var child = Projectile.NewProjectileDirect(new EntitySource_Parent(parent), Vector2.Zero, Vector2.Zero, ProjectileID.Bullet, 10, 0f, 0);

			Assert.AreEqual(7, child.ArmorPenetration);
			Assert.AreEqual(4, child.OriginalArmorPenetration);
			Assert.AreEqual(11, child.CritChance);
			Assert.AreEqual(2, child.OriginalCritChance);
		});
	}

	[TestMethod]
	public void NewProjectileModifierRunsAfterSourceStats()
	{
		WithProjectileWorld(() => {
			var parent = new Projectile { ArmorPenetration = 7 };
			int observedArmorPenetration = -1;

			var child = Projectile.NewProjectileDirect(new EntitySource_Parent(parent), Vector2.Zero, Vector2.Zero, ProjectileID.Bullet, 10, 0f, 0, modifer: projectile => {
				observedArmorPenetration = projectile.ArmorPenetration;
				projectile.ArmorPenetration = 2;
			});

			Assert.AreEqual(7, observedArmorPenetration);
			Assert.AreEqual(2, child.ArmorPenetration, "Source stats must not be applied again after the modifier.");
		});
	}

	[DataTestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public void GemStaffSnapshotIncludesFinalizedArmorPenetration(bool hasParent)
	{
		WithProjectileWorld(() => {
			IEntitySource source = hasParent
				? new EntitySource_Parent(new Projectile { ArmorPenetration = 7, OriginalArmorPenetration = 4 })
				: new EntitySource_Misc("test");
			var features = new Projectile.GemStaffFeatures { ArPenSpread = true };

			var projectile = Projectile.NewProjectileDirect(source, Vector2.Zero, Vector2.Zero, ProjectileID.AmethystBolt, 10, 0f, 0, ai0: features.Bits);

			Assert.AreEqual(hasParent ? 11 : 4, projectile.ArmorPenetration);
			Assert.AreEqual(hasParent ? 8 : 4, projectile.OriginalArmorPenetration);
		});
	}

	[DataTestMethod]
	[DataRow(0, 80)]
	[DataRow(20, 90)]
	[DataRow(40, 100)]
	[DataRow(60, 100)]
	public void InheritedArmorPenetrationReducesDefenseInActualHit(int armorPenetration, int expectedDamage)
	{
		WithProjectileWorld(() => {
			var npcs = Main.npc;
			var combatText = Main.combatText;
			var random = Main.rand;
			int variation = Main.DefaultDamageVariationPercent;
			var tileCutContext = DelegateMethods.tilecut_0;
			var tileCutIgnore = DelegateMethods.tileCutIgnore;
			// Damage records a source even for NPCs without a damage tracker.
			var nextHitSourceField = typeof(NPCDamageTracker).GetField("_nextHitSource", BindingFlags.Static | BindingFlags.NonPublic);
			Assert.IsNotNull(nextHitSourceField);
			var nextHitSource = nextHitSourceField.GetValue(null);
			try {
				Main.npc = new NPC[Main.maxNPCs + 1];
				for (int i = 0; i < Main.npc.Length; i++)
					Main.npc[i] = new NPC();
				// Suppress floating text without requiring font assets in the headless world.
				Main.combatText = new CombatText[100];
				for (int i = 0; i < Main.combatText.Length; i++)
					Main.combatText[i] = new CombatText { active = true };
				Main.rand = new UnifiedRandom(123);
				Main.DefaultDamageVariationPercent = 0;
				var target = new NPC {
					type = NPCID.BlueSlime,
					whoAmI = 0,
					active = true,
					life = 1000,
					lifeMax = 1000,
					defense = 40,
					width = 40,
					height = 40,
					knockBackResist = 0f
				};
				Main.npc[0] = target;
				var parent = new Projectile { ArmorPenetration = armorPenetration };
				var projectile = Projectile.NewProjectileDirect(new EntitySource_Parent(parent), Vector2.Zero, Vector2.Zero, ProjectileID.Bullet, 100, 1f, 0);
				projectile.position = target.position;
				// Keep projectile death effects outside this hit calculation test.
				projectile.penetrate = -1;

				projectile.Damage();

				Assert.AreEqual(expectedDamage, 1000 - target.life);
				Assert.AreEqual(armorPenetration, projectile.ArmorPenetration);
			}
			finally {
				Main.npc = npcs;
				Main.combatText = combatText;
				Main.rand = random;
				Main.DefaultDamageVariationPercent = variation;
				DelegateMethods.tilecut_0 = tileCutContext;
				DelegateMethods.tileCutIgnore = tileCutIgnore;
				nextHitSourceField.SetValue(null, nextHitSource);
			}
		});
	}

	private static void WithProjectileWorld(Action action)
	{
		var projectiles = Main.projectile;
		var player = Main.player[0];
		int myPlayer = Main.myPlayer;
		int netMode = Main.netMode;
		int tilesX = Main.maxTilesX;
		int tilesY = Main.maxTilesY;
		try {
			Main.projectile = new Projectile[1001];
			for (int i = 0; i < Main.projectile.Length; i++)
				Main.projectile[i] = new Projectile();
			Main.netMode = 0;
			Main.myPlayer = 0;
			Main.player[0] = new Player();
			// No world is loaded: keep the spawn collision check outside the tile array.
			Main.maxTilesX = Main.maxTilesY = 0;
			action();
		}
		finally {
			Main.projectile = projectiles;
			Main.player[0] = player;
			Main.myPlayer = myPlayer;
			Main.netMode = netMode;
			Main.maxTilesX = tilesX;
			Main.maxTilesY = tilesY;
		}
	}
}