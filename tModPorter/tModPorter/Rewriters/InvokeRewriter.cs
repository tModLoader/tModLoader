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

public partial class InvokeRewriter : BaseRewriter
{
	public delegate SyntaxNode RewriteInvoke(InvokeRewriter rw, InvocationExpressionSyntax invoke, NameSyntax methodName);

	private static List<(string type, string name, bool isStatic, RewriteInvoke handler)> handlers = new();


	public static void RefactorInstanceMethodCall(string type, string name, RewriteInvoke handler) => handlers.Add((type, name, isStatic: false, handler));
	public static void RefactorStaticMethodCall(string type, string name, RewriteInvoke handler) => handlers.Add((type, name, isStatic: true, handler));

	private static RewriteInvoke ToApplicator(AddComment comment) => (_, invoke, _) => invoke.ReplaceNode(invoke.ArgumentList, comment.Apply(invoke.ArgumentList));
	public static void RefactorInstanceMethodCall(string type, string name, AddComment comment) => RefactorInstanceMethodCall(type, name, ToApplicator(comment));
	public static void RefactorStaticMethodCall(string type, string name, AddComment comment) => RefactorStaticMethodCall(type, name, ToApplicator(comment));

	public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node) {
		if (base.VisitInvocationExpression(node) is SyntaxNode newNode && newNode != node) // fix arguments and expression first
			return newNode;

		IOperation op = model.GetOperation(node);
		if (!IsInvalidOrObsolete(op))
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
		if (targetType == null)
			return node;

		var nameToken = nameSyntax.Identifier;
		foreach (var (type, name, isStatic, handler) in handlers) {
			if (name != nameToken.Text || !targetType.InheritsFrom(type))
				continue;

			return handler(this, node, nameSyntax);
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

			return handler(this, node, nameSyntax);
		}

		return node;
	}

	#region Handlers
	private static ExpressionSyntax ConvertInvokeToMemberReference(InvocationExpressionSyntax invoke, string memberName) =>
		invoke.Expression switch {
			MemberAccessExpressionSyntax memberAccess => MemberAccessExpression(memberAccess.Expression, memberName).WithTriviaFrom(memberAccess),
			IdentifierNameSyntax identifierName => IdentifierName(memberName).WithTriviaFrom(identifierName),
			_ => throw new Exception($"Cannot convert {invoke.Expression.GetType()} to member access")
		};


	public static RewriteInvoke GetterSetterToProperty(string propName, string constantType = null, string constantName = null) => (rw, invoke, methodName) => {
		if (invoke.ArgumentList.Arguments.Count > 1)
			return invoke;

		ExpressionSyntax constantExpression = null;
		if (constantType != null) {
			constantExpression = MemberAccessExpression(rw.UseType(constantType), constantName);
		}

		switch (invoke.ArgumentList.Arguments.Count) {
			case 0:
				var result = ConvertInvokeToMemberReference(invoke, propName);
				if (constantType != null)
					result = Parens(SimpleSyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, result, constantExpression));

				return result.WithTriviaFrom(invoke);

			case 1:
				var arg = invoke.ArgumentList.Arguments[0].Expression;
				if (constantType != null) {
					if (rw.model.GetOperation(arg) is ILiteralOperation { ConstantValue.Value: true }) {
						invoke = invoke.ReplaceNode(arg, constantExpression);
					}
					else {
						return invoke.ReplaceNode(methodName, methodName.WithBlockComment($"Suggestion: {propName} = ..."));
					}
				}

				return AssignmentExpression(
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
		return invoke; // TODO
	};

	public static RewriteInvoke ToFindTypeCall(string type) => (rw, invoke, methodName) => {
		if (methodName is not IdentifierNameSyntax nameSyntax)
			return invoke;

		invoke = invoke.ReplaceNode(nameSyntax, GenericName("Find", rw.UseType(type)));
		return MemberAccessExpression(invoke.WithoutTrivia(), "Type").WithTriviaFrom(invoke);
	};

	public static RewriteInvoke ToStaticMethodCall(string onType, string newName, bool targetBecomesFirstArg = false) => (rw, invoke, _) => {
		var targetExpr = invoke.Expression switch {
			MemberAccessExpressionSyntax memberAccess => memberAccess.Expression,
			NameSyntax _ => ThisExpression(),
			_ => throw new ArgumentException("Strange invoke target")
		};

		var args = invoke.ArgumentList;
		if (targetBecomesFirstArg) {
			args = ArgumentList(new[] { targetExpr.WithoutTrivia() }).Concat(args);
		}
		else if (SuspectSideEffects(invoke.Expression, out var concern)) {
			invoke = invoke.WithLeadingTrivia(invoke.GetLeadingTrivia().Add(Comment($"/* {concern} */")));
		}

		var member = MemberAccessExpression(rw.UseType(onType), newName);
		return InvocationExpression(member, args).WithTriviaFrom(invoke);
	};

	public static RewriteInvoke ConvertAddEquipTexture => (rw, invoke, methodName) => {
		var paramOps = invoke.ArgumentList.Arguments.Select(arg => rw.model.GetOperation(arg.Expression)).ToArray();
		var method = rw.model.Compilation.GetTypeByMetadataName("Terraria.ModLoader.EquipLoader").LookupMember<IMethodSymbol>("AddEquipTexture");
		if (method == null)
			return invoke;

		invoke = (InvocationExpressionSyntax)ToStaticMethodCall(method.ContainingType.ToString(), method.Name, targetBecomesFirstArg: true)(rw, invoke, methodName);
		if (paramOps.Any(op => op == null || op is IInvalidOperation) || paramOps.Length < 4)
			return invoke;

		var args = invoke.ArgumentList.Arguments;
		int offset = 0;
		ExpressionSyntax equipTexture = null;
		if (paramOps[2].Type.ToString() == "Terraria.ModLoader.EquipType") {
			offset++;
			equipTexture = args[1].Expression;
		}

		static ExpressionSyntax ReplaceNullLiteral(ExpressionSyntax expr) => expr.IsKind(SyntaxKind.NullLiteralExpression) ? null : expr;

		// arg map (ModItem variant)
		// 0 -> 0 (mod)
		// 1 -> 3 (item)
		// 2 -> 2 (type)
		// 3 -> 4 (name)
		// 4 -> 1 (texture)
		// optional overload 1 -> 5 (equipTexture)
		var newArgs = new ExpressionSyntax[6] {
			args[0].Expression,			// mod
			args[offset+4].Expression,	// texture
			args[offset+2].Expression,	// type
			ReplaceNullLiteral(args[offset+1].Expression),	// item
			ReplaceNullLiteral(args[offset+3].Expression),	// name
			equipTexture
		};

		if (paramOps.Length > offset + 4)
			invoke = invoke.WithBlockComment("Note: armTexture and femaleTexture now part of new spritesheet. https://github.com/tModLoader/tModLoader/wiki/Armor-Texture-Migration-Guide");

		return invoke.WithArgumentList(ArgumentList(method, newArgs).WithTriviaFrom(invoke.ArgumentList));
	};

	public static RewriteInvoke ToTryGet(string newName) => (rw, invoke, methodName) => {
		var op = rw.model.GetOperation(invoke);

		ExpressionSyntax outExpr;
		if (invoke.Parent is AssignmentExpressionSyntax assignmentExpr && assignmentExpr.Right == invoke) {
			outExpr = assignmentExpr.Left.WithoutLeadingTrivia().TrimTrailingSpace();
			rw.RegisterAction(assignmentExpr, n => invoke.WithTriviaFrom(n));
		}
		else if (invoke.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax {
				Identifier: var varIdentifier, Parent: VariableDeclarationSyntax { Variables.Count: 1, Type: var varType, Parent: LocalDeclarationStatementSyntax localDecl } } }) {

			outExpr = DeclarationExpression(varType.WithoutLeadingTrivia(), SingleVariableDesignation(varIdentifier).TrimTrailingSpace());
			rw.RegisterAction(localDecl, n => ExpressionStatement(invoke).WithTriviaFrom(n));
		}
		else {
			outExpr = IdentifierName("_");
		}

		invoke = invoke.ReplaceNode(methodName, IdentifierName(newName).WithTriviaFrom(methodName));

		invoke = invoke.WithArgumentList(
			invoke.ArgumentList.Concat(
				ArgumentList(new[] { Argument(null, TokenSpace(SyntaxKind.OutKeyword), outExpr) })));

		return invoke;
	};

	public static RewriteInvoke CommentOut => (rw, invoke, methodName) => {
		// This doesn't actually do the right thing, which would be to remove the entire StatementSyntax from the block, and then add the comment trivia in the right spot to the previous node, but that's really hard...
		// So instead, we just chuck some line comment trivia in front of this and hope no-one else gets too confused
		if (invoke.Parent is StatementSyntax stmt)
			rw.RegisterAction(stmt, CommentOutNode);
		else if (invoke.Parent is ArrowExpressionClauseSyntax arrowExpr)
			rw.RegisterAction(arrowExpr.Parent, CommentOutNode); // whole method is now redundant

		return invoke;
	};

	private static SyntaxNode CommentOutNode(SyntaxNode node) {

		var t = node.GetLeadingTrivia();
		if (t.LastOrDefault().Kind() is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia)
			return node; // prevents infinite recursion

		bool multiline = node.ToString().Contains('\n');
		if (multiline) {
			return node
				.WithLeadingTrivia(t.Add(Comment("/* ")))
				.WithTrailingTrivia(node.GetTrailingTrivia().Insert(0, Comment(" */")));
		}
		else {
			return node.WithLeadingTrivia(t.Add(Comment("// ")));
		}
	}
	#endregion
}

