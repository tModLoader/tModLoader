﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace tModCodeAssist;

public abstract class AbstractDiagnosticAnalyzer : DiagnosticAnalyzer
{
	protected virtual GeneratedCodeAnalysisFlags GeneratedCodeAnalysisFlags => GeneratedCodeAnalysisFlags.None;

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

	protected AbstractDiagnosticAnalyzer(params DiagnosticDescriptor[] diagnosticDescriptors)
	{
		SupportedDiagnostics = ImmutableArray.Create(diagnosticDescriptors);
	}

	public sealed override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags);
		context.EnableConcurrentExecution();

		InitializeWorker(context);
	}

	protected abstract void InitializeWorker(AnalysisContext context);
}
