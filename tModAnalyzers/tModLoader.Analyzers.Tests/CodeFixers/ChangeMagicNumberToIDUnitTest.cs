using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = tModLoader.Analyzers.Tests.Verifier.Analyzer<tModLoader.Analyzers.ChangeMagicNumberToID.ChangeMagicNumberToIDAnalyzer>.CodeFixer<tModLoader.Analyzers.ChangeMagicNumberToID.ChangeMagicNumberToIDCodeFixProvider>;

namespace tModLoader.Analyzers.Tests.CodeFixers;

[TestClass]
public sealed class ChangeMagicNumberToIDUnitTest
{
	[TestMethod]
	public async Task Test_Assignment()
	{
		await VerifyCS.Run(
			"""
			using Terraria;

			new Item().type = [|1|];
			""",
			"""
			using Terraria;
			using Terraria.ID;
			
			new Item().type = ItemID.IronPickaxe;
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
	public async Task Test_SwitchCase()
	{
		await VerifyCS.Run(
			"""
			using Terraria;

			switch (new Item().type) {
				case [|1|]:
					break;
			}
			""",
			"""
			using Terraria;
			using Terraria.ID;
			
			switch (new Item().type) {
				case ItemID.IronPickaxe:
					break;
			}
			""");
	}

	[TestMethod]
	public async Task Test_Parameter()
	{
		await VerifyCS.Run(
			"""
			using Terraria;

			_ = new Item([|1|]);
			""",
			"""
			using Terraria;
			using Terraria.ID;
			
			_ = new Item(ItemID.IronPickaxe);
			""");
	}

	[TestMethod]
	public async Task Test_Return()
	{
		await VerifyCS.Run(
			"""
			using Terraria;
			using Terraria.ID;
			using Terraria.ModLoader.Annotations;

			_ = Foo() == [|1|];

			[return: IDType<ItemID>]
			int Foo() => ItemID.None;
			""",
			"""
			using Terraria;
			using Terraria.ID;
			using Terraria.ModLoader.Annotations;
			
			_ = Foo() == ItemID.IronPickaxe;

			[return: IDType<ItemID>]
			int Foo() => ItemID.None;
			""");
	}
}
