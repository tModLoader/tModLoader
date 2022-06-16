using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static tModPorter.Rewriters.SimpleSyntaxFactory;

namespace tModPorter.Rewriters;

public class MemberUseRewriter : BaseRewriter {

	public delegate SyntaxNode RewriteMemberUse(MemberUseRewriter rw, IInvalidOperation op, IdentifierNameSyntax memberName);

	private static List<(string type, string name, RewriteMemberUse handler)> handlers = new();

	public static void RefactorInstanceMember(string type, string name, RewriteMemberUse handler) => handlers.Add((type, name, handler));
	public static void RefactorInstanceMember(string type, string name, AddComment comment) => RefactorInstanceMember(type, name, (_, _, n) => comment.Apply(n));


	public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node) {
		if (!IdentifierNameInvalid(node, out var op, out var targetType, out bool isInvoke) || op == null || isInvoke)
			return node;

		if (targetType == null)
			targetType = model.GetEnclosingSymbol(node.SpanStart).ContainingType;

		var nameToken = node.Identifier;
		var handler = handlers.SingleOrDefault(h => nameToken.Text == h.name && targetType.InheritsFrom(h.type));
		if (handler == default)
			return node;

		return handler.handler.Invoke(this, op, node);
	}

	public static RewriteMemberUse DamageTypeField(string className, string comment = null) => (rw, op, memberName) => {
		var damageClassExpr = MemberAccessExpression(rw.UseType("Terraria.ModLoader.DamageClass"), className);

		if (op.Parent is IAssignmentOperation assign && assign.Target == op) {
			var expr = assign.Syntax;
			if (assign.Value is not ILiteralOperation { ConstantValue.Value: bool constantValue })
				return memberName.WithBlockComment("Suggestion: DamageType = ..."); // some other literal assignment

			if (!constantValue) { // = false
				rw.RegisterAction(assign.Syntax, n => n.WithBlockComment("Suggestion: Remove. See Item.DamageType"));
				return memberName;
			}

			rw.RegisterAction(assign.Value.Syntax, n => damageClassExpr.WithTriviaFrom(n).WithBlockComment(comment));
			return memberName.WithIdentifier("DamageType");
		}
		else { // plain identifier or member access
			
			var rootExpr = (ExpressionSyntax)op.Syntax;
			rw.RegisterAction<ExpressionSyntax>(rootExpr, n => InvocationExpression(n.WithoutTrivia(), damageClassExpr).WithTriviaFrom(n));
			return memberName.WithIdentifier("CountsAsClass");
		}
	};

	public static RewriteMemberUse DamageModifier(string className, string methodName, string subMember = null) => (rw, op, memberName) => {
		var damageClassExpr = MemberAccessExpression(rw.UseType("Terraria.ModLoader.DamageClass"), className);

		var rootExpr = (ExpressionSyntax)op.Syntax;
		rw.RegisterAction<ExpressionSyntax>(rootExpr, n => {
			n = InvocationExpression(n.WithoutTrivia(), damageClassExpr).WithTriviaFrom(n);
			if (subMember != null)
				n = MemberAccessExpression(n.WithoutTrivia(), subMember).WithTriviaFrom(n);

			return n;
		});

		return memberName.WithIdentifier(methodName);
	};
}
