using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace tModLoader.Analyzers.Tests;

public static partial class Verifier
{
	public static partial class Analyzer<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new()
	{
		public static partial class CodeFixer<TCodeFix> where TCodeFix : CodeFixProvider, new();
	}
}
