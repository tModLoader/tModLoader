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

	public static MemberAccessExpressionSyntax SimpleMemberAccessExpression(ExpressionSyntax expression, string memberName) =>
		MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, IdentifierName(memberName));

	public static AssignmentExpressionSyntax SimpleAssignmentExpression(ExpressionSyntax left, ExpressionSyntax right) =>
		AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, OperatorToken(SyntaxKind.EqualsToken), right);

	public static BinaryExpressionSyntax SimpleBinaryExpression(SyntaxKind kind, ExpressionSyntax left, ExpressionSyntax right) {
		var expr = BinaryExpression(kind, left, right);
		return expr.WithOperatorToken(expr.OperatorToken.WithLeadingTrivia(Space).WithTrailingTrivia(Space));
	}

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

	public static ParameterListSyntax ParameterList(IEnumerable<ParameterSyntax> @params) {
		var arr = @params.ToArray();
		if (arr.Length == 0)
			return SyntaxFactory.ParameterList();

		return SyntaxFactory.ParameterList(SeparatedList(
				arr,
				Enumerable.Repeat(Token(SyntaxKind.CommaToken).WithTrailingTrivia(Space), arr.Length - 1)
			));
	}

	public static SyntaxToken ModifierToken(RefKind refKind) => refKind switch {
		RefKind.None => default,
		RefKind.Ref => Token(SyntaxKind.RefKeyword).WithTrailingTrivia(Space),
		RefKind.Out => Token(SyntaxKind.OutKeyword).WithTrailingTrivia(Space),
		RefKind.In => Token(SyntaxKind.InKeyword).WithTrailingTrivia(Space),
		_ => throw new Exception("Unreachable")
	};
}
