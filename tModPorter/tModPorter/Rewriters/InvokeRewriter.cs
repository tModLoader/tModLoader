using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static tModPorter.Rewriters.SimpleSyntaxFactory;

namespace tModPorter.Rewriters;

public class InvokeRewriter : BaseRewriter
{
	public delegate SyntaxNode RewriteInvoke(InvokeRewriter rw, InvocationExpressionSyntax invoke, SyntaxToken methodName);

	private static List<(string type, string name, bool isStatic, RewriteInvoke handler)> handlers = new();

	public static void RefactorInstanceMethodCall(string type, string name, RewriteInvoke handler) => handlers.Add((type, name, isStatic: false, handler));

	public static void RefactorStaticMethodCall(string type, string name, RewriteInvoke handler) => handlers.Add((type, name, isStatic: true, handler));

	public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node) {
		if (base.VisitInvocationExpression(node) is SyntaxNode newNode && newNode != node) // fix arguments and expression first
			return newNode;

		if (model.GetOperation(node) is not IInvalidOperation op || node.ArgumentList.Arguments.Any(arg => model.GetOperation(arg) is IInvalidOperation))
			return node;

		return node.Expression switch {
			MemberAccessExpressionSyntax memberAccess =>
				Refactor(node, memberAccess.Name, model.GetTypeInfo(memberAccess.Expression).Type),

			MemberBindingExpressionSyntax memberBinding when op.ChildOperations.First() is IConditionalAccessInstanceOperation target =>
				Refactor(node, memberBinding.Name, target.Type),

			IdentifierNameSyntax name => RefactorViaLookup(node, name),

			_ => node,
		};
	}

	private SyntaxNode Refactor(InvocationExpressionSyntax node, SimpleNameSyntax nameSyntax, ITypeSymbol targetType) {
		var nameToken = nameSyntax.Identifier;
		foreach (var (type, name, isStatic, handler) in handlers) {
			if (name != nameToken.Text || !targetType.InheritsFrom(type))
				continue;

			return handler(this, node, nameToken);
		}

		return node;
	}

	private SyntaxNode RefactorViaLookup(InvocationExpressionSyntax node, IdentifierNameSyntax nameSyntax) {
		// this won't work if all the static members of a type have been removed, but it's a nice way to handle both the static context from inheritance heirachy, and from using statements.
		INamedTypeSymbol[] _staticTypesWithMembersInCtx = null;
		INamedTypeSymbol[] GetStaticallyLocalTypes() => _staticTypesWithMembersInCtx ??= model
			.LookupStaticMembers(node.SpanStart)
			.OfType<IMethodSymbol>()
			.Select(m => m.ContainingType)
			.Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
			.ToArray();

		var enclosingMethod = model.GetEnclosingSymbol(node.SpanStart);
		var enclosingType = enclosingMethod.ContainingType;


		var nameToken = nameSyntax.Identifier;
		foreach (var (type, name, isStatic, handler) in handlers) {
			if (name != nameToken.Text)
				continue;

			if (isStatic ? !GetStaticallyLocalTypes().Any(t => t.InheritsFrom(type)) : enclosingMethod.IsStatic || !enclosingType.InheritsFrom(type))
				continue;

			return handler(this, node, nameToken);
		}

		return node;
	}

	#region Handlers
	public static RewriteInvoke AddComment(string comment) => (_, invoke, methodName) => {
		if (methodName.TrailingTrivia.Any(SyntaxKind.MultiLineCommentTrivia))
			return invoke;

		return invoke.ReplaceToken(methodName, methodName.WithBlockComment(comment));
	};

	private static ExpressionSyntax ConvertInvokeToMemberReference(InvocationExpressionSyntax invoke, string memberName) =>
		invoke.Expression switch {
			MemberAccessExpressionSyntax memberAccess => SimpleMemberAccessExpression(memberAccess.Expression, memberName).WithTriviaFrom(memberAccess),
			IdentifierNameSyntax identifierName => IdentifierName(memberName).WithTriviaFrom(identifierName),
			_ => throw new Exception($"Cannot convert {invoke.Expression.GetType()} to member access")
		};


	public static RewriteInvoke GetterSetterToProperty(string propName, string constantType = null, string constantName = null) => (rw, invoke, methodName) => {
		if (invoke.ArgumentList.Arguments.Count > 1)
			return invoke;

		ExpressionSyntax constantExpression = null;
		if (constantType != null) {
			constantExpression = SimpleMemberAccessExpression(rw.UseType(constantType), constantName);
		}

		switch (invoke.ArgumentList.Arguments.Count) {
			case 0:
				var result = ConvertInvokeToMemberReference(invoke, propName);
				if (constantType != null)
					result = Parens(SimpleBinaryExpression(SyntaxKind.EqualsExpression, result, constantExpression));

				return result.WithTriviaFrom(invoke);

			case 1:
				var arg = invoke.ArgumentList.Arguments[0].Expression;
				if (constantType != null) {
					if (rw.model.GetOperation(arg) is ILiteralOperation { ConstantValue: { Value: true } }) {
						invoke = invoke.ReplaceNode(arg, constantExpression);
					}
					else {
						return AddComment($"Suggestion: {propName} = ...")(rw, invoke, methodName);
					}
				}

				return SimpleAssignmentExpression(
					ConvertInvokeToMemberReference(invoke, propName),
					invoke.ArgumentList.Arguments[0].Expression
				).WithTriviaFrom(invoke);

			default:
				throw new Exception("Unreachable");
		}
	};

	public static RewriteInvoke GetterToProperty(string propName) => (_, invoke, methodName) => {
		if (invoke.ArgumentList.Arguments.Count > 0	)
			return invoke;

		return ConvertInvokeToMemberReference(invoke, propName).WithTriviaFrom(invoke);
	};

	public static RewriteInvoke ComparisonFunctionToPropertyEquality(string propName) => (_, invoke, methodName) => {
		return invoke;
	};

	public static RewriteInvoke ToFindTypeCall(string type) => (rw, invoke, methodName) => {
		// TODO: we should replace the entire NameSyntax with a GenericName, to avoid breaking the tree, rather than making an invalid IdentifierNameSyntax
		// might be a problem for recursive calls
		invoke = invoke.ReplaceToken(methodName, methodName.WithText($"Find<{rw.UseType(type)}>"));
		return SimpleMemberAccessExpression(invoke.WithoutTrivia(), "Type").WithTriviaFrom(invoke);
	};
	#endregion
}

