using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = tModCodeAssist.Tests.Verifier.Analyzer<tModCodeAssist.Analyzers.ChangeMagicNumberToIDAnalyzer>;

namespace tModCodeAssist.Tests.Analyzers;

[TestClass]
public class BadIDTypeUnitTest
{
	[TestMethod]
	public async Task Test_Assignment()
	{
		await VerifyCS.Run(
			"""
			using Terraria;
			using Terraria.ID;

			var item = new Item();
			item.type = {|BadIDType:TileID.Dirt|};
			int a = 420;
			item.type = a;
			const int b = 420;
			item.type = b;

			Terraria.ModLoader.ModNPC modProjectile = null;
			modProjectile.AIType = {|BadIDType:NPCAIStyleID.DemonEye|};
			"""
			);
	}

	[TestMethod]
	public async Task Test_Binary()
	{
		await VerifyCS.Run(
			"""
			using Terraria;
			using Terraria.ID;

			_ = new Item().type == {|BadIDType:TileID.Dirt|};
			_ = Main.tile[10, 20].TileType == {|BadIDType:ItemID.GoldOre|}; // ref property

			// https://github.com/tModLoader/tModLoader/issues/4849
			_ = Dust.NewDust(default, 0, 0, DustID.Dirt) == Main.maxDust;
			"""
			);
	}

	[TestMethod]
	public async Task Test_Invocation()
	{
		await VerifyCS.Run(
			"""
			using Terraria;
			using Terraria.ID;

			var recipe = Recipe.Create(ItemID.CobaltBrickWall);
			recipe.AddIngredient({|BadIDType:TileID.Dirt|});
			recipe.AddRecipeGroup({|BadIDType:ItemID.Wood|});
			"""
			);
	}

	[TestMethod]
	public async Task Test_CaseSwitchLabel()
	{
		await VerifyCS.Run(
			"""
			using Terraria;
			using Terraria.ID;

			switch (new NPC().type) {
				case {|BadIDType:TileID.Dirt|}:
					break;
			}
			""");
	}

	[TestMethod]
	public async Task Test_ArrayIndexing()
	{
		await VerifyCS.Run(
			"""
			using Terraria;
			using Terraria.ID;

			ItemID.Sets.StaffMinionSlotsRequired[{|BadIDType:ProjectileID.Hornet|}] = 2f;
			""");
	}
}
