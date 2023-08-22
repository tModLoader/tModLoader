using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace tModLoader.SourceGenerators.Tests;

internal static class CSharpVerifierHelper
{
	/// <summary>
	/// By default, the compiler reports diagnostics for nullable reference types at
	/// <see cref="DiagnosticSeverity.Warning"/>, and the analyzer test framework defaults to only validating
	/// diagnostics at <see cref="DiagnosticSeverity.Error"/>. This map contains all compiler diagnostic IDs
	/// related to nullability mapped to <see cref="ReportDiagnostic.Error"/>, which is then used to enable all
	/// of these warnings for default validation during analyzer and code fix tests.
	/// </summary>
	internal static ImmutableDictionary<string, ReportDiagnostic> NullableWarnings { get; } = GetNullableWarningsFromCompiler();

	private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
	{
		string[] args = { "/warnaserror:nullable" };
		var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
		return commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;
	}
}

public static partial class tModLoaderSourceGeneratorVerifier<TSourceGenerator> where TSourceGenerator : IIncrementalGenerator, new()
{
	public class Test : CSharpSourceGeneratorTest<TSourceGenerator, XUnitVerifier>
	{
		private readonly string _identifier;
		private readonly string _testFile;
		private readonly string _testMethod;

		public Test([CallerFilePath] string testFile = null, [CallerMemberName] string testMethod = null) : this(String.Empty, testFile, testMethod)
		{
		}

		public Test(string identifier, [CallerFilePath] string testFile = null, [CallerMemberName] string testMethod = null)
		{
			_identifier = identifier;
			_testFile = testFile;
			_testMethod = testMethod;

			ReferenceAssemblies = ReferenceAssemblies.Net.Net60;
		}

		public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.Preview;

		private string ResourceName {
			get {
				if (string.IsNullOrEmpty(_identifier))
					return _testMethod ?? "";

				return $"{_testMethod}_{_identifier}";
			}
		}

		protected override CompilationOptions CreateCompilationOptions()
		{
			var compilationOptions = base.CreateCompilationOptions();
			return compilationOptions.WithSpecificDiagnosticOptions(
				compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
		}

		protected override ParseOptions CreateParseOptions()
		{
			return ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
		}

		protected override Project ApplyCompilationOptions(Project project)
		{
			return base.ApplyCompilationOptions(project).AddMetadataReference(Commons.TmlReference);
		}
	}
}
