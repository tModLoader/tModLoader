using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis;

namespace tModCodeAssist.Test.CodeFixers;

public abstract class CodeFixerUnitTest<TTest, TAnalyzer, TCodeFixProvider>
	where TTest : CSharpCodeFixVerifier<TAnalyzer, TCodeFixProvider>.Test, new()
	where TAnalyzer : DiagnosticAnalyzer, new()
	where TCodeFixProvider : CodeFixProvider, new()
{
	public abstract class BaseTest : CSharpCodeFixVerifier<TAnalyzer, TCodeFixProvider>.Test
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

	protected static async Task TestInRegularAndScript1Async(string testCode, string fixedCode, OutputKind outputKind = OutputKind.ConsoleApplication)
	{
		testCode = testCode.ReplaceLineEndings();
		fixedCode = fixedCode.ReplaceLineEndings();

		await new TTest() {
			TestCode = testCode,
			FixedCode = fixedCode,
			CodeActionValidationMode = CodeActionValidationMode.None,
			TestState =
			{
				OutputKind = outputKind,
			},
		}.RunAsync();
	}

	protected static async Task TestMissingInRegularAndScriptAsync(string testCode, OutputKind outputKind = OutputKind.ConsoleApplication)
	{
		testCode = testCode.ReplaceLineEndings();

		await new TTest() {
			TestCode = testCode,
			FixedCode = testCode,
			TestState =
			{
				OutputKind = outputKind,
			},
		}.RunAsync();
	}
}
