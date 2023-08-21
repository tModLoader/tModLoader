using Microsoft.CodeAnalysis;

namespace tModLoader.SourceGenerators.Tests;

public class Adapter<TIncrementalGenerator> : ISourceGenerator, IIncrementalGenerator
	where TIncrementalGenerator : IIncrementalGenerator, new() {
	private readonly TIncrementalGenerator _internalGenerator;

	public Adapter() {
		_internalGenerator = new TIncrementalGenerator();
	}

	public void Execute(GeneratorExecutionContext context) {
		throw new NotImplementedException();
	}

	public void Initialize(GeneratorInitializationContext context) {
		throw new NotImplementedException();
	}

	public void Initialize(IncrementalGeneratorInitializationContext context) {
		_internalGenerator.Initialize(context);
	}
}