using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = tModCodeAssist.Tests.Verifier.Analyzer<tModCodeAssist.Analyzers.ChangeMagicNumberToIDAnalyzer>.CodeFixer<tModCodeAssist.CodeFixes.ChangeMagicNumberToIDCodeFixProvider>;

namespace tModCodeAssist.Tests.CodeFixes;

[TestClass]
public sealed class ChangeMagicNumberToIDUnitTest
{
	[TestMethod]
	public async Task Test_Assignment()
	{
		await VerifyCS.Run(
			"""
			using Terraria;

			var item = new Item();
			item.createTile = [|42|];
			item.type = [|42|];
			item.useStyle = [|4|];
			item.shoot = [|42|];
			item.rare = [|4|];
			item.useTime = 69;
			""",
			"""
			using Terraria;
			using Terraria.ID;
			
			var item = new Item();
			item.createTile = TileID.HangingLanterns;
			item.type = ItemID.Shuriken;
			item.useStyle = ItemUseStyleID.HoldUp;
			item.shoot = ProjectileID.SandBallGun;
			item.rare = ItemRarityID.LightRed;
			item.useTime = 69;
			""");
	}

	[TestMethod]
	public async Task Test_Binary()
	{
		await VerifyCS.Run(
			"""
			using Terraria;

			_ = new Item().type == [|1|];
			""",
			"""
			using Terraria;
			using Terraria.ID;
			
			_ = new Item().type == ItemID.IronPickaxe;
			""");
	}

	[TestMethod]
	public async Task Test_Invocation()
	{
		await VerifyCS.Run(
			"""
			using Microsoft.Xna.Framework;
			using Terraria;
			using Terraria.ID;

			Recipe.Create([|420|]);
			
			Projectile.NewProjectile(Main.LocalPlayer.GetSource_FromThis(), Main.LocalPlayer.Top, new Vector2(0, -Main.rand.NextFloat(2f, 4f)).RotatedByRandom(0.3f), [|60|], 0, 0, Main.myPlayer);
			""",
			"""
			using Microsoft.Xna.Framework;
			using Terraria;
			using Terraria.ID;
			
			Recipe.Create(ItemID.CobaltBrickWall);
			
			Projectile.NewProjectile(Main.LocalPlayer.GetSource_FromThis(), Main.LocalPlayer.Top, new Vector2(0, -Main.rand.NextFloat(2f, 4f)).RotatedByRandom(0.3f), ProjectileID.MythrilDrill, 0, 0, Main.myPlayer);
			""");
	}
}
