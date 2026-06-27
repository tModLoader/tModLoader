using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace tModCodeAssist.Analyzers;

public abstract class AbstractDiagnosticAnalyzer(params DiagnosticDescriptor[] descriptors) : DiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = descriptors.ToImmutableArray();

	public virtual GeneratedCodeAnalysisFlags GeneratedCodeAnalysisFlags => GeneratedCodeAnalysisFlags.None;

	public sealed override void Initialize(AnalysisContext ctx)
	{
		ctx.EnableConcurrentExecution();
		ctx.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags);

		InitializeWorker(ctx);
	}

	protected abstract void InitializeWorker(AnalysisContext ctx);
}
