using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace tModCodeAssist;

public abstract class AbstractDiagnosticAnalyzer(params DiagnosticDescriptor[] descriptors) : DiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = descriptors.ToImmutableArray();

	public virtual GeneratedCodeAnalysisFlags GeneratedCodeAnalysisFlags => GeneratedCodeAnalysisFlags.None;

	public sealed override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags);

		InitializeWorker(context);
	}

	protected abstract void InitializeWorker(AnalysisContext context);
}
