using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using tModCodeAssist.IDType;

namespace tModCodeAssist.CodeFixers;

[TestClass]
public sealed class BadIDTypeUnitTest : CodeFixerUnitTest<BadIDTypeUnitTest.Test, IDTypeDiagnosticAnalyzer, IDTypeCodeFixer>
{
	public sealed class Test : BaseTest
	{
		protected sealed override DiagnosticDescriptor GetDefaultDiagnostic(DiagnosticAnalyzer[] analyzers)
		{
			return IDTypeDiagnosticAnalyzer.BadRule;
		}
	}

	[TestMethod]
	public async Task Test_AddTile()
	{
		await TestMissingInRegularAndScriptAsync(
			"""
			using Terraria.ID;
			using Terraria.ModLoader;
			
			namespace MyMod;
			
			public class A : ModItem {
				public void Foo() {
					CreateRecipe()
						.AddTile([|ItemID.IronPickaxe|]);
				}
			}
			""");
	}

	[TestMethod]
	public async Task Test_SetItemType()
	{
		await TestMissingInRegularAndScriptAsync(
			"""
			using Terraria;
			using Terraria.ID;
			
			var item = new Item();
			item.type = [|TileID.WorkBenches|];
			""",
			OutputKind.ConsoleApplication);
	}

	[TestMethod]
	public async Task Test_MethodParams()
	{
		await TestMissingInRegularAndScriptAsync(
			"""
			using Terraria.ID;
			using Terraria.ModLoader.Annotations;
			
			Foo([|TileID.WorkBenches|], [|TileID.WorkBenches|], [|TileID.WorkBenches|]);
			
			void Foo([IDType(IDTypeAttribute.Item)] params int[] a) {
			}
			""",
			OutputKind.ConsoleApplication);
	}

	[TestMethod]
	public async Task Test_NestedIDSet()
	{
		await TestMissingInRegularAndScriptAsync(
			"""
			using Terraria.ID;
			using Terraria.ModLoader.Annotations;
			
			Foo([|TileID.WorkBenches|]);
			
			void Foo([IDType(IDTypeAttribute.Armor_Head)] int a) {
			}
			""",
			OutputKind.ConsoleApplication);
	}

	[TestMethod]
	public async Task Test_MagicEquals()
	{
		await TestMissingInRegularAndScriptAsync(
			"""
			using Terraria.ID;
			using Terraria.ModLoader.Annotations;
			
			var a = new A();
			bool areSame = [|a.type == TileID.WorkBenches|];
			
			class A {
				[IDType(IDTypeAttribute.Armor_Head)]
				public int type;
			}
			""",
			OutputKind.ConsoleApplication);
	}

	[TestMethod]
	public async Task Test_SwitchLabel()
	{
		await TestMissingInRegularAndScriptAsync(
			"""
			using Terraria;
			using Terraria.ID;
			
			var item = new Item();
			switch (item.type) {
				case TileID.WorkBenches:
					break;
			}
			""",
			OutputKind.ConsoleApplication);
	}
}
