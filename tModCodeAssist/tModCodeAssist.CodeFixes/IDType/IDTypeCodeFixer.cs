using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using static tModCodeAssist.Constants;
using MyResources = tModCodeAssist.IDType.IDTypeResources;

namespace tModCodeAssist.IDType;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IDTypeCodeFixer)), Shared]
public sealed class IDTypeCodeFixer : AbstractCodeFixProvider
{
	public IDTypeCodeFixer() : base(DiagnosticIDs.IDType)
	{
	}

	protected sealed override void Register(CodeFixContext context, in Parameters parameters)
	{
		var root = parameters.Root;
		var diagnostic = parameters.Diagnostic;
		var diagnosticSpan = parameters.DiagnosticSpan;

		var token = root.FindToken(diagnosticSpan.Start);
		var parent = token.Parent;

		string title = MyResources.Title;
		const string key = nameof(MyResources.Title);

		context.RegisterCodeFix(
			CodeAction.Create(
				title: title,
				equivalenceKey: key,
				createChangedSolution: c => ChangeMagicNumber(context.Document, diagnostic, parent, c)),
			diagnostic);
	}

	private async Task<Solution> ChangeMagicNumber(Document document, Diagnostic diagnostic, SyntaxNode syntaxNode, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken);
		var generator = SyntaxGenerator.GetGenerator(document);

		var semantic = await document.GetSemanticModelAsync(cancellationToken);
		var idSymbol = semantic.Compilation.GetTypeByMetadataName(diagnostic.Properties[IDTypeDiagnosticAnalyzer.IDFullyQualifiedNameProperty]);

		var newRoot = root.ReplaceNode(
			syntaxNode,
			generator.MemberAccessExpression(
				generator.TypeExpression(idSymbol),
				diagnostic.Properties[IDTypeDiagnosticAnalyzer.IDLiteralProperty]
			)
		);

		return document.WithSyntaxRoot(newRoot).Project.Solution;
	}
}
