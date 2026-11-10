using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace tModPorter.Rewriters;

public class EventHandlerRewriter : BaseRewriter
{
	private static List<(string type, string name)> appendedParameters = new();

	public static void AppendEventHandlerParameters(string type, string name) => appendedParameters.Add((type, name));

	public override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node) {
		var newNode = (AssignmentExpressionSyntax)base.VisitAssignmentExpression(node);

		if (!node.IsKind(SyntaxKind.AddAssignmentExpression) && !node.IsKind(SyntaxKind.SubtractAssignmentExpression))
			return newNode;

		string name = node.Left switch {
			MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
			SimpleNameSyntax simpleName => simpleName.Identifier.Text,
			_ => null
		};
		if (!appendedParameters.Any(e => e.name == name))
			return newNode;

		if (model.GetSymbolInfo(node.Left).Symbol is not IEventSymbol ev || ev.Type is not INamedTypeSymbol { DelegateInvokeMethod: { } invoke })
			return newNode;

		if (!appendedParameters.Any(e => e.name == ev.Name && ev.ContainingType.InheritsFrom(e.type)))
			return newNode;

		var parameters = invoke.Parameters;
		ExpressionSyntax right = newNode.Right switch {
			SimpleLambdaExpressionSyntax lambda when parameters.Length > 1 =>
				ParenthesizedLambdaExpression(lambda.Modifiers, AppendParameters(ParameterList(SingletonSeparatedList(lambda.Parameter.WithoutTrivia())), parameters).WithTrailingTrivia(Space), lambda.ArrowToken, lambda.Block, lambda.ExpressionBody)
					.WithTriviaFrom(lambda),
			ParenthesizedLambdaExpressionSyntax lambda when lambda.ParameterList.Parameters.Count < parameters.Length =>
				lambda.WithParameterList(AppendParameters(lambda.ParameterList, parameters)),
			// delegate { } without a parameter list is compatible with any signature
			AnonymousMethodExpressionSyntax { ParameterList: { } paramList } anon when paramList.Parameters.Count < parameters.Length =>
				anon.WithParameterList(AppendParameters(paramList, parameters)),
			// method groups and delegate typed variables can't be rewritten, comment unless they already match the new signature
			IdentifierNameSyntax or MemberAccessExpressionSyntax when !model.ClassifyConversion(node.Right, ev.Type).Exists =>
				newNode.Right.WithBlockComment($"Suggestion: Handler parameters are now ({string.Join(", ", parameters.Select(p => p.Type.Name + " " + p.Name))})"),
			_ => newNode.Right
		};

		return newNode.WithRight(right);
	}

	private ParameterListSyntax AppendParameters(ParameterListSyntax list, IReadOnlyList<IParameterSymbol> parameters) {
		// keep the existing style: typed parameters if the handler already declares types, otherwise implicit
		bool typed = list.Parameters.Any(p => p.Type != null);
		var existing = list.Parameters;
		foreach (var p in parameters.Skip(existing.Count)) {
			var param = typed ? Parameter(p) : SyntaxFactory.Parameter(Identifier(p.Name));
			existing = existing.Add(param);
		}

		// SeparatedList.Add inserts a bare comma, add the usual space after it
		var separators = existing.GetSeparators().Select(s => s.WithTrailingTrivia(Space));
		return list.WithParameters(SeparatedList(existing, separators));
	}
}
