using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace tModCodeAssist;

public abstract class AbstractCodeFixProvider(string diagnosticId) : CodeFixProvider
{
	protected record struct Parameters(
		in SyntaxNode Root,
		in Diagnostic Diagnostic
	)
	{
		public readonly TextSpan DiagnosticSpan => Diagnostic.Location.SourceSpan;
	}

	public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(diagnosticId);

	public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		var diagnostic = context.Diagnostics[0];

		var parameters = new Parameters(root, diagnostic);

		await RegisterAsync(context, parameters);
	}

	protected abstract Task RegisterAsync(CodeFixContext context, Parameters parameters);
}
