using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;

namespace tModPorter.Rewriters;

public class RenameRewriter : BaseRewriter
{
	private static List<(string type, string from, string to)> memberRenames = new();
	private static List<(string from, string to)> typeRenames = new();

	public static void RenameInstanceField(string type, string from, string to) => memberRenames.Add((type, from, to));
	public static void RenameStaticField(string type, string from, string to) => memberRenames.Add((type, from, to));
	public static void RenameMethod(string type, string from, string to) => memberRenames.Add((type, from, to));
	public static void RenameType(string from, string to) => typeRenames.Add((from, to));

	public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node) {
		return node.Parent switch {
			MemberAccessExpressionSyntax memberAccess when node == memberAccess.Name && MemberReferenceInvalid(memberAccess, out _) =>
				Refactor(node, model.GetTypeInfo(memberAccess.Expression).Type),

			MemberBindingExpressionSyntax memberBinding when MemberReferenceInvalid(memberBinding, out var op) && op.ChildOperations.First() is IConditionalAccessInstanceOperation target =>
				Refactor(node, target.Type),

			// getting the operation for errored identifiers as part of the lhs of an initializer expression is difficult directly, need to go via the assignment
			AssignmentExpressionSyntax assignment when assignment.Parent is InitializerExpressionSyntax && model.GetOperation(assignment) is IAssignmentOperation { Target: IInvalidOperation, Parent: var parent } =>
				Refactor(node, parent.Type),

			_ when model.GetOperation(node) is IInvalidOperation =>
				RefactorSpeculative(node),

			_ when model.GetSymbolInfo(node).Symbol == null =>
				RefactorType(node),

			_ => node,
		};
	}

	public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node) {
		var sym = model.GetDeclaredSymbol(node);
		node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);
		if (sym.IsOverride && sym.OverriddenMethod == null) {
			node = node.WithIdentifier(Refactor(node.Identifier, sym.ContainingType));
		}

		return node;
	}

	private bool MemberReferenceInvalid(SyntaxNode memberRefExpr, out IInvalidOperation op) {
		// for MemberAccessExpressionSyntax and MemberBindingExpressionSyntax which are the target of InvocationExpressionSyntax, there is no operation for the member access, only the invocation
		// We check that all the arguments for the invocation are valid as a way of determining that it's the member access which is causing the failure (though this may fail if they rely on generic type inference I guess?)
		// If only there was a way to get 'member not found diagnostics...'
		// A different option would be to find the target type of the invoke/member access and see if the named member is missing
		if (memberRefExpr.Parent is InvocationExpressionSyntax invoke && memberRefExpr == invoke.Expression) {
			return (op = model.GetOperation(invoke) as IInvalidOperation) != null && invoke.ArgumentList.Arguments.All(arg => model.GetOperation(arg) is not IInvalidOperation);
		}

		return (op = model.GetOperation(memberRefExpr) as IInvalidOperation) != null;
	}

	private static IdentifierNameSyntax Refactor(IdentifierNameSyntax nameSyntax, ITypeSymbol instType) =>
		nameSyntax.WithIdentifier(Refactor(nameSyntax.Identifier, instType));

	private static SyntaxToken Refactor(SyntaxToken nameToken, ITypeSymbol instType) {
		if (instType == null)
			return nameToken;

		foreach (var (type, from, to) in memberRenames) {
			if (from != nameToken.Text || !instType.InheritsFrom(type))
				continue;

			return nameToken.WithText(to);
		}

		return nameToken;
	}

	private IdentifierNameSyntax RefactorSpeculative(IdentifierNameSyntax nameSyntax) {
		var nameToken = nameSyntax.Identifier;

		foreach (var (type, from, to) in memberRenames) {
			if (from != nameToken.Text)
				continue;

			var repl = nameSyntax.WithIdentifier(nameToken.WithText(to));
			var speculate = model.GetSpeculativeSymbolInfo(nameSyntax.SpanStart, repl, SpeculativeBindingOption.BindAsExpression);
			if (speculate.Symbol?.ContainingType?.ToString() == type)
				return repl;
		}

		return nameSyntax;
	}

	private SyntaxNode RefactorType(IdentifierNameSyntax nameSyntax) {
		var nameToken = nameSyntax.Identifier;

		foreach (var (from, to) in typeRenames) {
			if (!from.EndsWith(nameToken.Text))
				continue;

			var qualifier = from[..^nameToken.Text.Length];
			if (qualifier[^1] != '.' || !to.StartsWith(qualifier))
				continue;

			var repl = nameSyntax.WithIdentifier(nameToken.WithText(to[qualifier.Length..]));
			var speculate = model.GetSpeculativeSymbolInfo(nameSyntax.SpanStart, repl, SpeculativeBindingOption.BindAsTypeOrNamespace);
			if (speculate.Symbol?.ToString() == to)
				return repl;
		}

		return nameSyntax;
	}
}

