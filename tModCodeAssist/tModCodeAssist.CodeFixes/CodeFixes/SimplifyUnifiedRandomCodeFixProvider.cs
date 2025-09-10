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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Diagnostics.SimplifyUnifiedRandom)), Shared]
public sealed class SimplifyUnifiedRandomCodeFixProvider() : AbstractCodeFixProvider(nameof(Diagnostics.SimplifyUnifiedRandom))
{
	public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	protected override Task RegisterAsync(CodeFixContext context, Parameters parameters)
	{
		var root = parameters.Root;
		if (root?.FindNode(context.Span, getInnermostNodeForTie: true).FirstAncestorOrSelf<BinaryExpressionSyntax>() is { } expressionSyntax) {
			string title = Resources.SimplifyUnifiedRandomTitle;
			const string titleKey = nameof(Resources.SimplifyUnifiedRandomTitle);

			var properties = SimplifyUnifiedRandomAnalyzer.Properties.FromImmutable(parameters.Diagnostic.Properties);
			properties.Deconstruct(out bool isLeftConstant);

			context.RegisterCodeFix(CodeAction.Create(
				string.Format(title),
				cancellationToken => Simplify(context.Document, root, expressionSyntax, isLeftConstant, cancellationToken),
				titleKey
			), parameters.Diagnostic);
		}

		return Task.CompletedTask;
	}

	private static async Task<Document> Simplify(
		Document document,
		SyntaxNode root,
		BinaryExpressionSyntax expressionSyntax,
		bool isLeftConstant,
		CancellationToken cancellationToken)
	{
		var simplification = await Simplify(
			document,
			expressionSyntax,
			isLeftConstant,
			cancellationToken);

		return await Simplifier.ReduceAsync(document.WithSyntaxRoot(root.ReplaceNode(expressionSyntax, simplification)), cancellationToken: cancellationToken);
	}

	private static async Task<SyntaxNode> Simplify(
		Document document,
		BinaryExpressionSyntax expressionSyntax,
		bool isLeftConstant,
		CancellationToken cancellationToken)
	{
		var compilation = await document.Project.GetCompilationAsync(cancellationToken);
		if (compilation == null)
			return expressionSyntax;

		var utilsTypeSymbol = compilation.GetTypeByMetadataName("Terraria.Utils");
		if (utilsTypeSymbol == null)
			return expressionSyntax;

		if (expressionSyntax.IsKind(SyntaxKind.EqualsExpression) || expressionSyntax.IsKind(SyntaxKind.NotEqualsExpression)) {
			var invocationExpressionSyntax = (InvocationExpressionSyntax)(isLeftConstant ? expressionSyntax.Right : expressionSyntax.Left);
			if (invocationExpressionSyntax.Expression is not MemberAccessExpressionSyntax { Expression: var instanceSyntax })
				return expressionSyntax;

			var syntaxGenerator = SyntaxGenerator.GetGenerator(document);

			return syntaxGenerator.InvocationExpression(
				syntaxGenerator.MemberAccessExpression(
					syntaxGenerator.TypeExpression(utilsTypeSymbol),
					"NextBool"
				),
				[
					instanceSyntax,
					invocationExpressionSyntax.ArgumentList.Arguments[0]
				]
			).WithAdditionalAnnotations(Simplifier.Annotation);
		}
		else {
			return expressionSyntax;
		}
	}
}
