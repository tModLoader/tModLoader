using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace tModCodeAssist;

public abstract class AbstractCodeFixProvider : CodeFixProvider
{
	protected record struct Parameters(
		in SyntaxNode Root,
		in Diagnostic Diagnostic
	)
	{
		public readonly TextSpan DiagnosticSpan => Diagnostic.Location.SourceSpan;
	}

	public override ImmutableArray<string> FixableDiagnosticIds { get; }

	protected AbstractCodeFixProvider(string diagnosticId)
	{
		FixableDiagnosticIds = ImmutableArray.Create(diagnosticId);
	}

	public override FixAllProvider GetFixAllProvider()
	{
		return WellKnownFixAllProviders.BatchFixer;
	}

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		var diagnostic = context.Diagnostics.First();

		var parameters = new Parameters(root, diagnostic);

		Register(context, in parameters);
		await RegisterAsync(context, parameters);
	}

	protected virtual void Register(CodeFixContext context, in Parameters parameters)
	{
	}

	protected virtual Task RegisterAsync(CodeFixContext context, Parameters parameters)
	{
		return Task.CompletedTask;
	}
}
