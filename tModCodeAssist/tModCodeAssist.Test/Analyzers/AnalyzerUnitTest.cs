using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace tModCodeAssist.Test.Analyzers;

public abstract class AnalyzerUnitTest<TTest, TAnalyzer>
	where TTest : CSharpAnalyzerVerifier<TAnalyzer>.Test, new()
	where TAnalyzer : DiagnosticAnalyzer, new()
{
	public abstract class BaseTest : CSharpAnalyzerVerifier<TAnalyzer>.Test
	{
		public BaseTest()
		{
			ReferenceAssemblies = ReferenceAssemblies.Net.Net60;
		}

		protected abstract override DiagnosticDescriptor GetDefaultDiagnostic(DiagnosticAnalyzer[] analyzers);

		protected override Project ApplyCompilationOptions(Project project)
		{
			return base.ApplyCompilationOptions(project)
				.AddMetadataReference(MetadataReferences.TmlReference);
		}
	}

	protected static async Task TestInRegularAndScript1Async(string testCode, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary, params DiagnosticResult[] expected)
	{
		testCode = testCode.ReplaceLineEndings();

		var test = new TTest {
			TestCode = testCode,
			TestState = { OutputKind = outputKind }
		};

		test.ExpectedDiagnostics.AddRange(expected);

		await test.RunAsync();
	}

	protected static async Task TestMissingInRegularAndScriptAsync(string testCode, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary)
	{
		testCode = testCode.ReplaceLineEndings();

		var test = new TTest {
			TestCode = testCode,
			TestState = { OutputKind = outputKind }
		};

		await test.RunAsync();
	}
}
