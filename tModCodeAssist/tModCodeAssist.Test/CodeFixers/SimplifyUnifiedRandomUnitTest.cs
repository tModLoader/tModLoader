using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using tModCodeAssist.SimplifyUnifiedRandom;

namespace tModCodeAssist.Test.CodeFixers;

[TestClass]
public sealed class SimplifyUnifiedRandomUnitTest : CodeFixerUnitTest<SimplifyUnifiedRandomUnitTest.Test, SimplifyUnifiedRandomAnalyzer, SimplifyUnifiedRandomCodeFixProvider>
{
	public sealed class Test : BaseTest
	{
		protected override DiagnosticDescriptor GetDefaultDiagnostic(DiagnosticAnalyzer[] analyzers) => SimplifyUnifiedRandomAnalyzer.Rule;
	}

	[TestMethod]
	public async Task Test_Equality_Zero_Success()
	{
		await TestInRegularAndScript1Async(
			"""
			using Terraria;

			_ = [|Main.rand.Next(5) == 0|];
			""",
			"""
			using Terraria;
			
			_ = Main.rand.NextBool(5);
			""");
	}

	[TestMethod]
	public async Task Test_Equality_NonZero_Success()
	{
		await TestInRegularAndScript1Async(
			"""
			using Terraria;

			_ = [|Main.rand.Next(5) == 4|];
			""",
			"""
			using Terraria;
			
			_ = Main.rand.NextBool(5);
			""");
	}

	[TestMethod]
	public async Task Test_Inequality_Success()
	{
		await TestInRegularAndScript1Async(
			"""
			using Terraria;

			_ = [|Main.rand.Next(5) != 0|];
			""",
			"""
			using Terraria;
			
			_ = !Main.rand.NextBool(5);
			""");
	}
}
