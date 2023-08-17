using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static tModPorter.Rewriters.SimpleSyntaxFactory;

namespace tModPorter.Rewriters;

public class HookGenRewriter : BaseRewriter {

	private List<string> refactoredUsingPrefixes = new();

	public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node) {
		if (!IdentifierNameInvalid(node, out var op, out var targetType, out bool isInvoke))
			return node;

		if (op != null && targetType != null)
			return node;

		var newType = refactoredUsingPrefixes.Select(pre => model.Compilation.GetTypeByMetadataName(pre + node.Identifier.Text)).Where(t => t != null).FirstOrDefault();
		if (newType == null)
			return node;

		return IdentifierName(newType.Name).WithTriviaFrom(node);
	}

	public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)	{
		if (!IsFullySimple(node) || !MatchQualifiedTypeName(node.ToString(), out var newName))
			return base.VisitMemberAccessExpression(node);

		return Name(newName).WithTriviaFrom(node);
	}

	private static bool IsFullySimple(MemberAccessExpressionSyntax node) => node.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
		(node.Expression is MemberAccessExpressionSyntax mAccess && IsFullySimple(mAccess) || node.Expression is IdentifierNameSyntax);

	public override SyntaxNode VisitQualifiedName(QualifiedNameSyntax node) {
		if (model.GetSymbolInfo(node).Symbol != null)
			return node;

		if (!MatchQualifiedTypeName(node.ToString(), out var newName))
			return base.VisitQualifiedName(node);

		return Name(newName).WithTriviaFrom(node);
	}

	protected override SyntaxList<UsingDirectiveSyntax> VisitUsingList(SyntaxList<UsingDirectiveSyntax> usings) {
		var renamed = usings.Where(u => MatchOldHookgenNamespace(u.Name.ToString(), out _, out _)).ToArray();
		if (renamed.Length == 0)
			return usings;

		usings = List(usings.Except(renamed));
		foreach (var u in renamed) {
			MatchOldHookgenNamespace(u.Name.ToString(), out string new_ns, out string prefix);
			usings = usings.WithUsingNamespace(new_ns);

			refactoredUsingPrefixes.Add(new_ns + '.' + prefix + '_');
		}

		return base.VisitUsingList(usings);
	}

	private bool MatchQualifiedTypeName(string name, out string newName)
	{
		newName = null;
		if (!MatchOldHookgenNamespace(name, out string new_ns, out string prefix))
			return false;

		newName = new_ns.Insert(new_ns.LastIndexOf('.') + 1, prefix + '_');
		return model.Compilation.GetTypeByMetadataName(newName) != null;
	}

	private static bool MatchOldHookgenNamespace(string ns, out string new_ns, out string prefix)
	{
		if (ns.StartsWith("IL.") || ns.StartsWith("On.")) {
			prefix = ns[..2];
			new_ns = ns[3..];
			return true;
		}

		prefix = null;
		new_ns = null;
		return false;
	}
}
