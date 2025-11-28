using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;
using tModCodeAssist.Analyzers;

namespace tModCodeAssist.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Diagnostics.ChangeMagicNumberToID)), Shared]
public sealed class ChangeMagicNumberToIDCodeFixProvider() : AbstractCodeFixProvider(nameof(Diagnostics.ChangeMagicNumberToID))
{
	public override Microsoft.CodeAnalysis.CodeFixes.FixAllProvider GetFixAllProvider() => new FixAllProvider();

	private static SyntaxNode GetReportedNode(SyntaxNode root, TextSpan location)
	{
		return root?.FindNode(location, getInnermostNodeForTie: true).AncestorsAndSelf().First(node => {
			return node
				is PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.UnaryMinusExpression, Operand.RawKind: (int)SyntaxKind.NumericLiteralExpression }
				or { RawKind: (int)SyntaxKind.NumericLiteralExpression };
		});
	}

	protected override Task RegisterAsync(CodeFixContext context, Parameters parameters)
	{
		var root = parameters.Root;
		if (GetReportedNode(root, context.Span) is { } syntax) {
			string title = Resources.ChangeMagicNumberToIDTitle;
			const string titleKey = nameof(Resources.ChangeMagicNumberToIDTitle);

			if (!parameters.Diagnostic.Properties.ContainsKey("ShortIdType"))
				return Task.CompletedTask; // Conflict with Diagnostic registered by old tModLoader.CodeAssist

			var properties = ChangeMagicNumberToIDAnalyzer.Properties.FromImmutable(parameters.Diagnostic.Properties);
			var (shortIdType, fullIdType, name) = properties;

			context.RegisterCodeFix(CodeAction.Create(
				string.Format(title, properties.ShortIdType),
				cancellationToken => ReplaceMagicNumber(context.Document, root, syntax, fullIdType, name, cancellationToken),
				titleKey
			), parameters.Diagnostic);
		}

		return Task.CompletedTask;
	}

	private static async Task<Document> ReplaceMagicNumber(
		Document document,
		SyntaxNode root,
		SyntaxNode literalSyntax,
		string typeName,
		string constantName,
		CancellationToken cancellationToken)
	{
		var magicNumberReplacement = await ReplaceMagicNumber(
			document,
			literalSyntax,
			typeName,
			constantName,
			cancellationToken);

		return document.WithSyntaxRoot(root.ReplaceNode(literalSyntax, magicNumberReplacement));
	}

	private static async Task<SyntaxNode> ReplaceMagicNumber(
		Document document,
		SyntaxNode literalSyntax,
		string typeName,
		string constantName,
		CancellationToken cancellationToken)
	{
		var compilation = await document.Project.GetCompilationAsync(cancellationToken);
		if (compilation == null)
			return literalSyntax;

		var idClassTypeSymbol = compilation.GetTypeByMetadataName(typeName);
		if (idClassTypeSymbol == null)
			return literalSyntax;

		var syntaxGenerator = SyntaxGenerator.GetGenerator(document);
		var idClassExpression = syntaxGenerator.TypeExpression(idClassTypeSymbol)
			.WithAdditionalAnnotations(Simplifier.AddImportsAnnotation);

		return syntaxGenerator.MemberAccessExpression(idClassExpression, constantName).WithTriviaFrom(literalSyntax);
	}

	// TODO: consider taking this down once multiple names support is introduced
	private sealed class FixAllProvider : DocumentBasedFixAllProvider
	{
		protected override async Task<Document> FixAllAsync(FixAllContext fixAllContext, Document document, ImmutableArray<Diagnostic> diagnostics)
		{
			if (await document.GetSyntaxRootAsync(fixAllContext.CancellationToken).ConfigureAwait(false) is not SyntaxNode root)
				return document;

			var syntaxEditor = new SyntaxEditor(root, fixAllContext.Solution.Workspace.Services);
			foreach (var diagnostic in diagnostics) {
				var literalSyntax = GetReportedNode(root, diagnostic.Location.SourceSpan);

				if (!diagnostic.Properties.ContainsKey("ShortIdType"))
					continue; // Conflict with Diagnostic registered by old tModLoader.CodeAssist

				var properties = ChangeMagicNumberToIDAnalyzer.Properties.FromImmutable(diagnostic.Properties);
				var (_, fullIdType, name) = properties;

				var updated = await ReplaceMagicNumber(document, literalSyntax, fullIdType, name, fixAllContext.CancellationToken);
				syntaxEditor.ReplaceNode(literalSyntax, updated);
			}

			return document.WithSyntaxRoot(syntaxEditor.GetChangedRoot());
		}
	}
}
