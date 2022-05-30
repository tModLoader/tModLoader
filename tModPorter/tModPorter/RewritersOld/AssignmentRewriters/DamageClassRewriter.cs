using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace tModPorter.Rewriters.AssignmentRewriters;

public class DamageClassRewriter : BaseRewriter {
	private readonly Dictionary<string, string> _fieldToDamageClass = new() {
		{"magic", "DamageClass.Magic"},
		{"melee", "DamageClass.Melee"},
		{"ranged", "DamageClass.Ranged"},
		{"summon", "DamageClass.Summon"},
		{"thrown", "DamageClass.Throwing"},
	};

	public DamageClassRewriter(SemanticModel model, List<string> usingList,
		HashSet<(BaseRewriter rewriter, SyntaxNode originalNode)> nodesToRewrite,
		HashSet<(BaseRewriter rewriter, SyntaxToken originalToken)> tokensToRewrite)
		: base(model, usingList, nodesToRewrite, tokensToRewrite) { }

	public override RewriterType RewriterType => RewriterType.Assignment;

	public override void VisitNode(SyntaxNode node) {
		// Find `x.y = true` or `x.y = false`
		if (node is not AssignmentExpressionSyntax nodeAssignment)
			return;

		if (nodeAssignment.Left is not MemberAccessExpressionSyntax accessExpression)
			return;

		if (_fieldToDamageClass.Any(f => f.Key == accessExpression.Name.ToString())) {
			if (nodeAssignment.Right.Kind() == SyntaxKind.FalseLiteralExpression) {
				AddNodeToRewrite(nodeAssignment);
				return;
			}
			AddNodeToRewrite(accessExpression.Name);
			AddNodeToRewrite(nodeAssignment.Right);
		}
	}

	public override SyntaxNode RewriteNode(SyntaxNode node) {
		// Do different rewrites depending on the node
		return node switch {
			AssignmentExpressionSyntax => CommentOutFalseAssigment(node),
			IdentifierNameSyntax {Parent: MemberAccessExpressionSyntax} fieldIdentifier => RewriteAccessField(fieldIdentifier),
			LiteralExpressionSyntax {
					RawKind: (int) SyntaxKind.FalseLiteralExpression or (int) SyntaxKind.TrueLiteralExpression,
				}
				literalValue => RewriteAssignedValue(literalValue),
			_ => node,
		};
	}

	private static SyntaxNode CommentOutFalseAssigment(SyntaxNode oldNode) {
		if (oldNode is not AssignmentExpressionSyntax assigment) return oldNode;

		SyntaxTriviaList leadingTrivia = assigment.GetLeadingTrivia();

		// This will place the "... redundant ..." comment before the semicolon (moving it to the end of the line)
		// It could be fixed by rewriting the trivia of the parent node, but I'd prefer not to do that.
		SyntaxTriviaList trailingTrivia = assigment.GetTrailingTrivia();

		if (!assigment.GetLeadingTrivia().ToString().Contains("// ")) {
			leadingTrivia = leadingTrivia.Add(Comment("// "));
			trailingTrivia = trailingTrivia.Insert(0,
				Comment(" /* tModPorter - this is redundant, for more info see https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide#damage-classes */ "));
		}

		ExpressionSyntax commentedNode = assigment.WithLeadingTrivia(leadingTrivia).WithTrailingTrivia(trailingTrivia);
		return commentedNode;
	}

	private SyntaxNode RewriteAccessField(SyntaxNode oldNode) {
		// <oldNode> is `y` (`x.y = true`)
		if (oldNode is not IdentifierNameSyntax) return oldNode;

		SyntaxNode newNode = IdentifierName("DamageType").WithTriviaFrom(oldNode);
		return newNode;
	}

	private SyntaxNode RewriteAssignedValue(SyntaxNode oldNode) {
		// <oldNode> should be `true`, get parent node (x.y = true), and then get `x.y` (<fieldName>)
		if (oldNode is not LiteralExpressionSyntax {
				Parent: AssignmentExpressionSyntax {Left: MemberAccessExpressionSyntax fieldName},
			})
			return oldNode;

		KeyValuePair<string, string> newDamage = _fieldToDamageClass.First(f => f.Key == fieldName.Name.ToString());
		ExpressionSyntax newValue = ParseExpression(newDamage.Value).WithTriviaFrom(oldNode);
		return newValue;
	}
}