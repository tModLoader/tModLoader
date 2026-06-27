using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using tModCodeAssist.Analyzers;

namespace tModCodeAssist.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Diagnostics.SimplifyLocalPlayer)), Shared]
public sealed class SimplifyLocalPlayerCodeFixProvider() : AbstractCodeFixProvider(nameof(Diagnostics.SimplifyLocalPlayer))
{
	public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	protected override Task RegisterAsync(CodeFixContext context, Parameters parameters)
	{
		var root = parameters.Root;
		if (root?.FindNode(context.Span, getInnermostNodeForTie: true).FirstAncestorOrSelf<ElementAccessExpressionSyntax>() is { } eaExpressionSyntax) {
			string title = Resources.SimplifyLocalPlayerTitle;
			const string titleKey = nameof(Resources.SimplifyLocalPlayerTitle);

			context.RegisterCodeFix(CodeAction.Create(
				string.Format(title),
				cancellationToken => Simplify(context.Document, root, eaExpressionSyntax, cancellationToken),
				titleKey
			), parameters.Diagnostic);
		}

		return Task.CompletedTask;
	}

	private static async Task<Document> Simplify(
		Document document,
		SyntaxNode root,
		ElementAccessExpressionSyntax expressionSyntax,
		CancellationToken cancellationToken)
	{
		var simplification = await Simplify(
			document,
			expressionSyntax,
			cancellationToken);

		return await Simplifier.ReduceAsync(document.WithSyntaxRoot(root.ReplaceNode(expressionSyntax, simplification)), cancellationToken: cancellationToken);
	}

	private static async Task<SyntaxNode> Simplify(
		Document document,
		ElementAccessExpressionSyntax expressionSyntax,
		CancellationToken cancellationToken)
	{
		var compilation = await document.Project.GetCompilationAsync(cancellationToken);
		if (compilation == null)
			return expressionSyntax;

		var mainTypeSymbol = compilation.GetTypeByMetadataName("Terraria.Main");
		if (mainTypeSymbol == null)
			return expressionSyntax;

		if (expressionSyntax.IsKind(SyntaxKind.ElementAccessExpression)) {
			var syntaxGenerator = SyntaxGenerator.GetGenerator(document);

			return syntaxGenerator.MemberAccessExpression(syntaxGenerator.TypeExpression(mainTypeSymbol), "LocalPlayer");
		}
		else {
			return expressionSyntax;
		}
	}
}
