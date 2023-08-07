using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace tModPorter.Rewriters;

public static class SimpleSyntaxFactory
{
	public static SyntaxToken OperatorToken(SyntaxKind kind) =>
		Token(new(Space), kind, new(Space));

	public static SyntaxToken TokenSpace(SyntaxKind kind) =>
		Token(default, kind, new(Space));

	public static MemberAccessExpressionSyntax MemberAccessExpression(ExpressionSyntax expression, string memberName) =>
		MemberAccessExpression(expression, IdentifierName(memberName));

	public static MemberAccessExpressionSyntax MemberAccessExpression(ExpressionSyntax expression, SimpleNameSyntax memberName) =>
		SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, memberName);

	public static AssignmentExpressionSyntax AssignmentExpression(ExpressionSyntax left, ExpressionSyntax right) =>
		SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, OperatorToken(SyntaxKind.EqualsToken), right);

	public static BinaryExpressionSyntax BinaryExpression(SyntaxKind kind, ExpressionSyntax left, ExpressionSyntax right) {
		var expr = SyntaxFactory.BinaryExpression(kind, left, right);
		return expr.WithOperatorToken(OperatorToken(expr.OperatorToken.Kind()));
	}

	public static InvocationExpressionSyntax InvocationExpression(ExpressionSyntax target, string methodName, params ExpressionSyntax[] args) =>
		InvocationExpression(MemberAccessExpression(target, methodName), args);

	public static InvocationExpressionSyntax InvocationExpression(ExpressionSyntax target, params ExpressionSyntax[] args) =>
		SyntaxFactory.InvocationExpression(target, ArgumentList(args));

	public static ObjectCreationExpressionSyntax ObjectCreationExpression(TypeSyntax type, params ExpressionSyntax[] args) =>
		SyntaxFactory.ObjectCreationExpression(TokenSpace(SyntaxKind.NewKeyword), type, ArgumentList(args), null);

	public static ElementAccessExpressionSyntax ElementAccessExpression(ExpressionSyntax target, params ExpressionSyntax[] args) =>
		SyntaxFactory.ElementAccessExpression(target, BracketedArgumentList(args));

	public static NameSyntax Name(string s) {
		int l = s.LastIndexOf('.');
		if (l == -1)
			return IdentifierName(s);

		return QualifiedName(Name(s[..l]), IdentifierName(s[(l + 1)..]));
	}

	public static UsingDirectiveSyntax SimpleUsing(string ns) {
		var u = UsingDirective(Name(ns));
		return u.WithUsingKeyword(TokenSpace(SyntaxKind.UsingKeyword)).WithTrailingTrivia(CarriageReturnLineFeed);
	}

	public static ExpressionSyntax Parens(ExpressionSyntax expr) {
		// TODO, only wrap if necessary
		return ParenthesizedExpression(expr);
	}

	public static SeparatedSyntaxList<T> SeparatedList<T>(IEnumerable<T> items) where T : SyntaxNode {
		var arr = items.ToArray();
		if (arr.Length == 0)
			return SyntaxFactory.SeparatedList<T>();

		return SyntaxFactory.SeparatedList(arr, Enumerable.Repeat(TokenSpace(SyntaxKind.CommaToken), arr.Length - 1));
	}

	public static ParameterListSyntax ParameterList(IEnumerable<ParameterSyntax> items) => SyntaxFactory.ParameterList(SeparatedList(items));
	public static TypeArgumentListSyntax TypeArgumentList(IEnumerable<TypeSyntax> items) => SyntaxFactory.TypeArgumentList(SeparatedList(items));
	public static ArgumentListSyntax ArgumentList(IEnumerable<ArgumentSyntax> items) => SyntaxFactory.ArgumentList(SeparatedList(items));
	public static ArgumentListSyntax ArgumentList(IEnumerable<ExpressionSyntax> items) => ArgumentList(items.Select(Argument));
	public static BracketedArgumentListSyntax BracketedArgumentList(IEnumerable<ArgumentSyntax> items) => SyntaxFactory.BracketedArgumentList(SeparatedList(items));
	public static BracketedArgumentListSyntax BracketedArgumentList(IEnumerable<ExpressionSyntax> items) => BracketedArgumentList(items.Select(Argument));
	public static GenericNameSyntax GenericName(string name, params TypeSyntax[] args) => SyntaxFactory.GenericName(Identifier(name), TypeArgumentList(args));

	public static ArrayTypeSyntax ArrayTypeRank1(TypeSyntax elementType) => ArrayType(elementType, new(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression()))));

	public static SyntaxToken ModifierToken(RefKind refKind) => refKind switch {
		RefKind.None => default,
		RefKind.Ref => TokenSpace(SyntaxKind.RefKeyword),
		RefKind.Out => TokenSpace(SyntaxKind.OutKeyword),
		RefKind.In => TokenSpace(SyntaxKind.InKeyword),
		_ => throw new Exception("Unreachable")
	};

	public static SyntaxTokenList ModifierList(Accessibility access) => access switch {
		Accessibility.Private => new(Token(SyntaxKind.PrivateKeyword)),
		Accessibility.ProtectedAndInternal => new(TokenSpace(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.InternalKeyword)),
		Accessibility.Protected => new(Token(SyntaxKind.ProtectedKeyword)),
		Accessibility.Internal => new(Token(SyntaxKind.InternalKeyword)),
		Accessibility.ProtectedOrInternal => default,
		Accessibility.Public => new(Token(SyntaxKind.PublicKeyword)),
		_ => default,
	};

	public static ArgumentListSyntax Concat(this ArgumentListSyntax args, ArgumentListSyntax other) {
		if (other.Arguments.Count == 0)
			return args;

		var argList = args.Arguments;
		if (argList.Count == 0)
			return other;

		var l = argList.GetWithSeparators()
			.Add(TokenSpace(SyntaxKind.CommaToken))
			.AddRange(other.Arguments.GetWithSeparators());

		return args.WithArguments(SyntaxFactory.SeparatedList<ArgumentSyntax>(l));
	}

	public static ArgumentListSyntax ArgumentList(IMethodSymbol method, params ExpressionSyntax[] newArgs) {
		var argSyntaxes = new List<ArgumentSyntax>();

		bool useNamedArgs = false;
		for (int i = 0; i < newArgs.Length; i++) {
			var arg = newArgs[i];
			if (arg == null) {
				useNamedArgs = true;
				continue;
			}

			var param = method.Parameters[i];
			if (useNamedArgs)
				argSyntaxes.Add(Argument(NameColon(param.Name).WithTrailingTrivia(Space), default, arg));
			else
				argSyntaxes.Add(Argument(arg));
		}

		return ArgumentList(argSyntaxes);
	}
}
