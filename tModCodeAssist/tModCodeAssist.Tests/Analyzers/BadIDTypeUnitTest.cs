using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AnalyzerVerifier = tModCodeAssist.Tests.Verifier.Analyzer<tModCodeAssist.Analyzers.ChangeMagicNumberToIDAnalyzer>;

namespace tModCodeAssist.Tests.Analyzers;

[TestClass]
public class BadIDTypeUnitTest
{
	[TestMethod]
	public async Task Test_Assignment()
	{
		await AnalyzerVerifier.Run(
			"""
			using Terraria;
			using Terraria.ID;

			var item = new Item();
			item.type = {|BadIDType:TileID.Dirt|};
			int a = 420;
			item.type = a;
			const int b = 420;
			item.type = b;
			"""
			);
	}

	[TestMethod]
	public async Task Test_Binary()
	{
		await AnalyzerVerifier.Run(
			"""
			using Terraria;
			using Terraria.ID;

			_ = new Item().type == {|BadIDType:TileID.Dirt|};
			"""
			);
	}

	[TestMethod]
	public async Task Test_Invocation()
	{
		await AnalyzerVerifier.Run(
			"""
			using Terraria;
			using Terraria.ID;

			var recipe = Recipe.Create(ItemID.CobaltBrickWall);
			recipe.AddIngredient({|BadIDType:TileID.Dirt|});
			"""
			);
	}

	[TestMethod]
	public async Task Test_CaseSwitchLabel()
	{
		await AnalyzerVerifier.Run(
			"""
			using Terraria;
			using Terraria.ID;

			switch (new NPC().type) {
				case {|BadIDType:TileID.Dirt|}:
					break;
			}
			""");
	}
}
