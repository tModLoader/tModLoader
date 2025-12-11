using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using tModCodeAssist.Tests.Verifiers;

namespace tModCodeAssist.Tests;

public static class Verifier
{
	public static class Analyzer<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new()
	{
		public static partial class CodeFixer<TCodeFix> where TCodeFix : CodeFixProvider, new()
		{
			public sealed class Test : CSharpCodeFixVerifier<TAnalyzer, TCodeFix>.Test
			{
				public Test(string testCode, string fixedCode, IEnumerable<DiagnosticResult> expected) : base()
				{
					ReferenceAssemblies = ReferenceAssemblies.Net.Net80;

					TestCode = testCode.ReplaceLineEndings();
					TestState.OutputKind = OutputKind.ConsoleApplication;

					FixedCode = fixedCode.ReplaceLineEndings();

					MarkupOptions = MarkupOptions.UseFirstDescriptor;
					ExpectedDiagnostics.AddRange(expected);

					NumberOfFixAllIterations = 1;
					NumberOfFixAllInProjectIterations = 1;
					NumberOfFixAllInDocumentIterations = 1;
				}

				protected override Project ApplyCompilationOptions(Project project)
				{
					return base.ApplyCompilationOptions(project)
						.AddMetadataReference(MetadataReferences.TmlReference)
						.AddMetadataReference(MetadataReferences.FnaReference)
						.AddMetadataReference(MetadataReferences.ReLogicReference);
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

			public static Test Run([StringSyntax("C#-test")] string testCode, [StringSyntax("C#-test")] string fixedCode, params DiagnosticResult[] expected)
			{
				return new Test(testCode, fixedCode, expected);
			}
		}

		public sealed class Test : CSharpAnalyzerVerifier<TAnalyzer>.Test
		{
			public Test(string testCode, IEnumerable<DiagnosticResult> expected) : base()
			{
				ReferenceAssemblies = ReferenceAssemblies.Net.Net80;

				TestCode = testCode.ReplaceLineEndings();
				TestState.OutputKind = OutputKind.ConsoleApplication;

				MarkupOptions = MarkupOptions.UseFirstDescriptor;
				ExpectedDiagnostics.AddRange(expected);
			}

			protected override Project ApplyCompilationOptions(Project project)
			{
				return base.ApplyCompilationOptions(project)
					.AddMetadataReference(MetadataReferences.TmlReference)
					.AddMetadataReference(MetadataReferences.FnaReference);
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

		public static Test Run([StringSyntax("C#-test")] string testCode, params DiagnosticResult[] expected)
		{
			return new Test(testCode, expected);
		}
	}
}
