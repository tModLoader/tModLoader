using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = tModCodeAssist.Tests.Verifier.Analyzer<tModCodeAssist.Analyzers.SimplifyUnifiedRandomAnalyzer>.CodeFixer<tModCodeAssist.CodeFixes.SimplifyUnifiedRandomCodeFixProvider>;

namespace tModCodeAssist.Tests.CodeFixes;

[TestClass]
public sealed class SimplifyUnifiedRandomUnitTest
{
	[TestMethod]
	public async Task Test_Next2NextBool()
	{
		await VerifyCS.Run(
			"""
			using Terraria;

			_ = [|Main.rand.Next(4) == 0|];
			_ = [|Main.rand.Next(4) == 2|];
			_ = Main.rand.Next(4) == 4;
			_ = -1 == Main.rand.Next(4);
			_ = 5 == Main.rand.Next(4);
			_ = [|3 == Main.rand.Next(4)|];
			""",
			"""
			using Terraria;

			_ = Main.rand.NextBool(4);
			_ = Main.rand.NextBool(4);
			_ = Main.rand.Next(4) == 4;
			_ = -1 == Main.rand.Next(4);
			_ = 5 == Main.rand.Next(4);
			_ = Main.rand.NextBool(4);
			""");
	}
}
