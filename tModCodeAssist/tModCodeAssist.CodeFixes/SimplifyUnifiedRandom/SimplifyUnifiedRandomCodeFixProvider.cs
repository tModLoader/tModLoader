using System.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace tModCodeAssist.SimplifyUnifiedRandom;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SimplifyUnifiedRandom)), Shared]
public sealed class SimplifyUnifiedRandomCodeFixProvider() : AbstractCodeFixProvider(SimplifyUnifiedRandomAnalyzer.Id)
{
	protected override Task RegisterAsync(CodeFixContext context, Parameters parameters)
	{
		int spanStart = parameters.DiagnosticSpan.Start;

		var operation = parameters.Root.FindToken(spanStart).Parent.FirstAncestorOrSelf<BinaryExpressionSyntax>();

		Debug.Assert(operation != null);

		bool negate = parameters.Diagnostic.Properties.ContainsKey(SimplifyUnifiedRandomAnalyzer.NegateParameter);

		string title = Resources.SimplifyUnifiedRandomTitle;
		const string titleKey = nameof(Resources.SimplifyUnifiedRandomTitle);

		context.RegisterCodeFix(
			CodeAction.Create(
				title,
				token => SimplifyAsync(context.Document, operation, negate, token),
				titleKey),
			parameters.Diagnostic);

		return Task.CompletedTask;
	}

	private static async Task<Document> SimplifyAsync(Document document, BinaryExpressionSyntax operation, bool negate, CancellationToken cancellationToken)
	{
		var newRoot = await document.GetSyntaxRootAsync(cancellationToken);
		var generator = SyntaxGenerator.GetGenerator(document.Project);

		var oldInvocationExpression = (InvocationExpressionSyntax)operation.Left;
		var oldMemberAccessExpression = (MemberAccessExpressionSyntax)oldInvocationExpression.Expression;

		// Replace `Next` with `NextBool` for the sake of simplicity of this code.
		var newMemberAccessExpression = oldMemberAccessExpression.WithName(SyntaxFactory.IdentifierName("NextBool"));

		var newOperation = generator.InvocationExpression(newMemberAccessExpression, oldInvocationExpression.ArgumentList.Arguments[0]);
		if (negate)
			newOperation = generator.LogicalNotExpression(newOperation);

		newRoot = newRoot.ReplaceNode(operation, newOperation);

		return document.WithSyntaxRoot(newRoot);
	}
}
