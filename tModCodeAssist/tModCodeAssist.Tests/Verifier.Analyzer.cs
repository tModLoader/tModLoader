using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace tModCodeAssist.Tests;

partial class Verifier
{
	partial class Analyzer<TAnalyzer>
	{
		public sealed class Test : CSharpAnalyzerVerifier<TAnalyzer>.Test
		{
			public Test(string testCode, IEnumerable<DiagnosticResult> expected) : base()
			{
				ReferenceAssemblies = MetadataReferences.Net80;

				TestCode = testCode.ReplaceLineEndings();

				TestState.OutputKind = OutputKind.ConsoleApplication;

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

		public static DiagnosticResult Diagnostic() => CSharpAnalyzerVerifier<TAnalyzer>.Diagnostic();

		public static Test Run(string testCode, params DiagnosticResult[] expected)
		{
			return new Test(testCode, expected);
		}
	}
}
