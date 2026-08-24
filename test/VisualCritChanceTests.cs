using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader;

[TestClass]
public class VisualCritChanceTests
{
	[ClassInitialize]
	public static void ClassInitialize(TestContext context)
	{
		Program.SavePath = ".";
	}

	[TestMethod]
	public void RevolverCritBonusIsAppliedExactlyOnceToProjectiles()
	{
		Player player = new();
		Item revolver = new() {
			type = ItemID.Revolver,
			crit = 7,
			DamageType = DamageClass.Ranged
		};

		int baseCritChance = player.GetWeaponCrit(revolver);
		player.revolverCritChanceBonus = 6;

		int expectedCritChance = baseCritChance + player.revolverCritChanceBonus;
		Assert.AreEqual(expectedCritChance, player.GetWeaponCrit(revolver));

		Projectile projectile = new();
		projectile.ApplyStatsFromSource(new EntitySource_ItemUse(player, revolver));

		Assert.AreEqual(expectedCritChance, projectile.CritChance);
	}
}
