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

	public static MemberAccessExpressionSyntax MemberAccessExpression(ExpressionSyntax expression, string memberName) =>
		SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, IdentifierName(memberName));

	public static AssignmentExpressionSyntax AssignmentExpression(ExpressionSyntax left, ExpressionSyntax right) =>
		SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, OperatorToken(SyntaxKind.EqualsToken), right);

	public static BinaryExpressionSyntax BinaryExpression(SyntaxKind kind, ExpressionSyntax left, ExpressionSyntax right) {
		var expr = SyntaxFactory.BinaryExpression(kind, left, right);
		return expr.WithOperatorToken(expr.OperatorToken.WithLeadingTrivia(Space).WithTrailingTrivia(Space));
	}

	public static InvocationExpressionSyntax InvocationExpression(ExpressionSyntax target, string methodName, params ExpressionSyntax[] args) =>
		InvocationExpression(MemberAccessExpression(target, methodName), args);

	public static InvocationExpressionSyntax InvocationExpression(ExpressionSyntax target, params ExpressionSyntax[] args) =>
		SyntaxFactory.InvocationExpression(target, ArgumentList(SeparatedList(args.Select(Argument))));

	public static NameSyntax Name(string s) {
		int l = s.LastIndexOf('.');
		if (l == -1)
			return IdentifierName(s);

		return QualifiedName(Name(s[..l]), IdentifierName(s[(l + 1)..]));
	}

	public static UsingDirectiveSyntax SimpleUsing(string ns) {
		var u = UsingDirective(Name(ns));
		return u.WithUsingKeyword(u.UsingKeyword.WithTrailingTrivia(Space)).WithTrailingTrivia(CarriageReturnLineFeed);
	}

	public static ExpressionSyntax Parens(ExpressionSyntax expr) {
		// TODO, only wrap if necessary
		return ParenthesizedExpression(expr);
	}

	public static SeparatedSyntaxList<T> SeparatedList<T>(IEnumerable<T> items) where T : SyntaxNode {
		var arr = items.ToArray();
		if (arr.Length == 0)
			return SyntaxFactory.SeparatedList<T>();

		return SyntaxFactory.SeparatedList(arr, Enumerable.Repeat(Token(SyntaxKind.CommaToken).WithTrailingTrivia(Space), arr.Length - 1));
	}

	public static ParameterListSyntax ParameterList(IEnumerable<ParameterSyntax> items) => SyntaxFactory.ParameterList(SeparatedList(items));
	public static TypeArgumentListSyntax TypeArgumentList(IEnumerable<TypeSyntax> items) => SyntaxFactory.TypeArgumentList(SeparatedList(items));
	public static GenericNameSyntax GenericName(string name, params TypeSyntax[] args) => SyntaxFactory.GenericName(Identifier(name), TypeArgumentList(args));

	public static SyntaxToken ModifierToken(RefKind refKind) => refKind switch {
		RefKind.None => default,
		RefKind.Ref => Token(SyntaxKind.RefKeyword).WithTrailingTrivia(Space),
		RefKind.Out => Token(SyntaxKind.OutKeyword).WithTrailingTrivia(Space),
		RefKind.In => Token(SyntaxKind.InKeyword).WithTrailingTrivia(Space),
		_ => throw new Exception("Unreachable")
	};

	public static ArgumentListSyntax WithAdditionalArguments(this ArgumentListSyntax args, ArgumentListSyntax other) {
		if (other.Arguments.Count == 0)
			return args;

		var argList = args.Arguments;
		if (argList.Count == 0)
			return other;

		var l = argList.GetWithSeparators()
			.Add(Token(SyntaxKind.CommaToken).WithTrailingTrivia(Space))
			.AddRange(other.Arguments.GetWithSeparators());

		return args.WithArguments(SyntaxFactory.SeparatedList<ArgumentSyntax>(l));
	}
}
