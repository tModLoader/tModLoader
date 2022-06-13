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
}
