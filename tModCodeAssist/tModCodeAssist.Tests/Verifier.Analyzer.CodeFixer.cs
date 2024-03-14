using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.Text;

namespace tModCodeAssist.Tests;

partial class Verifier
{
	partial class Analyzer<TAnalyzer>
	{
		partial class CodeFixer<TCodeFix>
		{
			public sealed class Test : CSharpCodeFixVerifier<TAnalyzer, TCodeFix>.Test
			{
				public Test(string testCode, string fixedCode, IEnumerable<DiagnosticResult> expected) : base()
				{
					ReferenceAssemblies = ReferenceAssemblies.Net.Net60;

					TestCode = testCode.ReplaceLineEndings();
					TestState.OutputKind = OutputKind.ConsoleApplication;

					FixedCode = fixedCode.ReplaceLineEndings();

					ExpectedDiagnostics.AddRange(expected);
				}

				protected override Project ApplyCompilationOptions(Project project)
				{
					return base.ApplyCompilationOptions(project)
						.AddMetadataReference(MetadataReferences.TmlReference);
				}

				public Test WithAdditionalFiles(IEnumerable<(string fileName, SourceText content)> values)
				{
					TestState.AdditionalFiles.AddRange(values);
					return this;
				}

				public Test WithExpectedDiagnostic(params DiagnosticResult[] expected)
				{
					ExpectedDiagnostics.AddRange(expected);
					return this;
				}

				public TaskAwaiter GetAwaiter()
				{
					return RunAsync().GetAwaiter();
				}
			}

			public static DiagnosticResult Diagnostic() => CSharpCodeFixVerifier<TAnalyzer, TCodeFix>.Diagnostic();

			public static Test Run(string testCode, string fixedCode, params DiagnosticResult[] expected)
			{
				return new Test(testCode, fixedCode, expected);
			}
		}
	}
}
