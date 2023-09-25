using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using tModCodeAssist.DefaultResearchCount;

namespace tModCodeAssist.Test.Analyzers;

[TestClass]
public sealed class DefaultResearchCountUnitTest : AnalyzerUnitTest<DefaultResearchCountUnitTest.Test, DefaultResearchCountDiagnosticAnalyzer>
{
	public sealed class Test : BaseTest
	{
		protected sealed override DiagnosticDescriptor GetDefaultDiagnostic(DiagnosticAnalyzer[] analyzers)
		{
			return DefaultResearchCountDiagnosticAnalyzer.Rule;
		}
	}

	[TestMethod]
	public async Task Test_Hint()
	{
		await TestInRegularAndScript1Async(
			"""
			using Terraria;

			[|new Item().ResearchUnlockCount = 1|];
			""",
			OutputKind.ConsoleApplication);
	}

	[TestMethod]
	public async Task Test_NotThis()
	{
		await TestMissingInRegularAndScriptAsync(
			"""
			using Terraria;

			new Item().ResearchUnlockCount = 25;
			""",
			OutputKind.ConsoleApplication);
	}
}
