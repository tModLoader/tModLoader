using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using tModCodeAssist.IDType;

namespace tModCodeAssist.CodeFixers;

[TestClass]
public sealed class IDTypeUnitTest : CodeFixerUnitTest<IDTypeUnitTest.Test, IDTypeDiagnosticAnalyzer, IDTypeCodeFixer>
{
	public sealed class Test : BaseTest
	{
		protected sealed override DiagnosticDescriptor GetDefaultDiagnostic(DiagnosticAnalyzer[] analyzers)
		{
			return IDTypeDiagnosticAnalyzer.Rule;
		}
	}

	[TestMethod]
	public async Task Test_AddTile()
	{
		await TestInRegularAndScript1Async(
			"""
			using Terraria.ModLoader;

			namespace MyMod;
			
			public class A : ModItem {
				public void Foo() {
					CreateRecipe()
						.AddTile([|1|]);
				}
			}
			""",
			"""
			using Terraria.ModLoader;
			
			namespace MyMod;
			
			public class A : ModItem {
				public void Foo() {
					CreateRecipe()
						.AddTile(Terraria.ID.TileID.Stone);
				}
			}
			""");
	}

	[TestMethod]
	public async Task Test_SetItemType()
	{
		await TestInRegularAndScript1Async(
			"""
			using Terraria;

			var item = new Item();
			item.type = [|1|];
			""",
			"""
			using Terraria;
			
			var item = new Item();
			item.type = Terraria.ID.ItemID.IronPickaxe;
			""",
			OutputKind.ConsoleApplication);
	}

	[TestMethod]
	public async Task Test_MethodParams()
	{
		await TestInRegularAndScript1Async(
			"""
			using Terraria.ModLoader.Annotations;

			Foo([|1|], [|2|], [|3|]);
			
			void Foo([IDType(IDTypeAttribute.Item)] params int[] a) {
			}
			""",
			"""
			using Terraria.ModLoader.Annotations;
			
			Foo(Terraria.ID.ItemID.IronPickaxe, Terraria.ID.ItemID.DirtBlock, Terraria.ID.ItemID.StoneBlock);
			
			void Foo([IDType(IDTypeAttribute.Item)] params int[] a) {
			}
			""",
			OutputKind.ConsoleApplication);
	}

	[TestMethod]
	public async Task Test_NestedIDSet()
	{
		await TestInRegularAndScript1Async(
			"""
			using Terraria.ModLoader.Annotations;

			Foo([|1|]);
			
			void Foo([IDType(IDTypeAttribute.Armor_Head)] int a) {
			}
			""",
			"""
			using Terraria.ModLoader.Annotations;
			
			Foo(Terraria.ID.ArmorIDs.Head.CopperHelmet);
			
			void Foo([IDType(IDTypeAttribute.Armor_Head)] int a) {
			}
			""",
			OutputKind.ConsoleApplication);
	}

	[TestMethod]
	public async Task Test_MagicEquals()
	{
		await TestInRegularAndScript1Async(
			"""
			using Terraria.ID;
			using Terraria.ModLoader.Annotations;
			
			var a = new A();
			_ = a.type == [|1|];

			class A {
				[IDType(IDTypeAttribute.Armor_Head)]
				public int type;
			}
			""",
			"""
			using Terraria.ID;
			using Terraria.ModLoader.Annotations;
			
			var a = new A();
			_ = a.type == ArmorIDs.Head.CopperHelmet;
			
			class A {
				[IDType(IDTypeAttribute.Armor_Head)]
				public int type;
			}
			""",
			OutputKind.ConsoleApplication);
	}

	[TestMethod]
	public async Task Test_ValidEquals()
	{
		await TestMissingInRegularAndScriptAsync(
			"""
			using Terraria;
			using Terraria.ID;
			
			_ = new Item().type == ItemID.IronPickaxe;
			""",
			OutputKind.ConsoleApplication);
	}

	[TestMethod]
	public async Task Test_ValidInequals()
	{
		await TestMissingInRegularAndScriptAsync(
			"""
			using Terraria;
			using Terraria.ID;
			
			_ = new Item().type != ItemID.IronPickaxe;
			""",
			OutputKind.ConsoleApplication);
	}

	[TestMethod]
	public async Task Test_SwitchLabel()
	{
		await TestInRegularAndScript1Async(
			"""
			using Terraria;

			switch (new Item().type) {
				case [|1|]:
					break;
			}
			""",
			"""
			using Terraria;
			
			switch (new Item().type) {
				case Terraria.ID.ItemID.IronPickaxe:
					break;
			}
			""",
			OutputKind.ConsoleApplication);
	}

	[TestMethod]
	public async Task Test_SwitchLabel2()
	{
		await TestInRegularAndScript1Async(
			"""
			using Terraria.ModLoader;

			namespace MyMod;
			
			public class A : ModItem {
				public void Foo() {
					switch (Item.type) {
						case [|1|]:
							break;
					}
				}
			}
			""",
			"""
			using Terraria.ModLoader;
			
			namespace MyMod;
			
			public class A : ModItem {
				public void Foo() {
					switch (Item.type) {
						case Terraria.ID.ItemID.IronPickaxe:
							break;
					}
				}
			}
			""");
	}
}
