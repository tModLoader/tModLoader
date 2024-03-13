using System.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;

namespace tModCodeAssist.ChangeMagicNumberToID;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ChangeMagicNumberToID)), Shared]
public sealed class ChangeMagicNumberToIDCodeFixProvider() : AbstractCodeFixProvider(ChangeMagicNumberToIDAnalyzer.Id)
{
	protected override Task RegisterAsync(CodeFixContext context, Parameters parameters)
	{
		int spanStart = parameters.DiagnosticSpan.Start;

		var operation = parameters.Root.FindToken(spanStart).Parent.FirstAncestorOrSelf<LiteralExpressionSyntax>();
		Debug.Assert(operation != null);

		string idClass = parameters.Diagnostic.Properties[ChangeMagicNumberToIDAnalyzer.IdClassParameter];
		string name = parameters.Diagnostic.Properties[ChangeMagicNumberToIDAnalyzer.NameParameter];

		string title = Resources.ChangeMagicNumberToIDTitle;
		const string titleKey = nameof(Resources.ChangeMagicNumberToIDTitle);

		context.RegisterCodeFix(
			CodeAction.Create(
				title,
				token => SimplifyAsync(context.Document, operation, idClass, name, token),
				titleKey),
			parameters.Diagnostic);

		return Task.CompletedTask;
	}

	private static async Task<Document> SimplifyAsync(Document document, LiteralExpressionSyntax operation, string idClass, string name, CancellationToken cancellationToken)
	{
		var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
		var compilation = await document.Project.GetCompilationAsync(cancellationToken);

		var generator = SyntaxGenerator.GetGenerator(document.Project);

		var idClassTypeSymbol = compilation.GetTypeByMetadataName(idClass);
		var idClassExpression = generator.TypeExpression(idClassTypeSymbol).WithAdditionalAnnotations(Simplifier.AddImportsAnnotation, Simplifier.SpecialTypeAnnotation);

		var newOperation = generator.MemberAccessExpression(idClassExpression, name);

		var newRoot = oldRoot.ReplaceNode(operation, newOperation);

		return document.WithSyntaxRoot(newRoot);
	}
}
