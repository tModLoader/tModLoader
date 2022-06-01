using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static tModPorter.Rewriters.SimpleSyntaxFactory;

namespace tModPorter.Rewriters;

public class RenameRewriter : BaseRewriter {
	public class MemberRename {
		public string type { get; init; }
		public string from { get; init; }
		public string to { get; init; }
		public bool isMethod { get; init; }

		public AdditionalRenameAction followup { get; private set; }

		public MemberRename FollowBy(AdditionalRenameAction onRenamed) {
			followup += onRenamed;
			return this;
		}
	}

	public delegate void AdditionalRenameAction(RenameRewriter rw, SyntaxToken name);

	private static List<MemberRename> memberRenames = new();
	private static List<(string from, string to)> typeRenames = new();

	private static MemberRename RenameMember(MemberRename entry) {
		memberRenames.Add(entry);
		return entry;
	}

	public static MemberRename RenameInstanceField(string type, string from, string to) => RenameMember(new() { type = type, from = from, to = to });
	public static MemberRename RenameStaticField(string type, string from, string to) => RenameMember(new() { type = type, from = from, to = to });
	public static MemberRename RenameMethod(string type, string from, string to) => RenameMember(new() { type = type, from = from, to = to, isMethod = true });
	public static void RenameType(string from, string to) => typeRenames.Add((from, to));

	public static MemberRename RenameStaticField(string type, string from, string to, string newType) => RenameStaticField(type, from, to).FollowBy(OnType(newType));
	public static MemberRename RenameMethod(string type, string from, string to, string newType) => RenameMethod(type, from, to).FollowBy(OnType(newType));

	public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node) {
		return node.Parent switch {
			MemberAccessExpressionSyntax memberAccess when node == memberAccess.Name && MemberReferenceInvalid(memberAccess, out _, out bool isInvoke) =>
				Refactor(node, model.GetTypeInfo(memberAccess.Expression).Type, isInvoke),

			MemberBindingExpressionSyntax memberBinding when MemberReferenceInvalid(memberBinding, out var op, out bool isInvoke) && op.ChildOperations.First() is IConditionalAccessInstanceOperation target =>
				Refactor(node, target.Type, isInvoke),

			// getting the operation for errored identifiers as part of the lhs of an initializer expression is difficult directly, need to go via the assignment
			AssignmentExpressionSyntax assignment when assignment.Parent is InitializerExpressionSyntax && model.GetOperation(assignment) is IAssignmentOperation { Target: IInvalidOperation, Parent: var parent } =>
				Refactor(node, parent.Type, refactoringMethod: false),

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
			node = node.WithIdentifier(Refactor(node.Identifier, sym.ContainingType, refactoringMethod: true));
		}

		return node;
	}

	private bool MemberReferenceInvalid(SyntaxNode memberRefExpr, out IInvalidOperation op, out bool isInvoke) {
		// for MemberAccessExpressionSyntax and MemberBindingExpressionSyntax which are the target of InvocationExpressionSyntax, there is no operation for the member access, only the invocation
		// We check that all the arguments for the invocation are valid as a way of determining that it's the member access which is causing the failure (though this may fail if they rely on generic type inference I guess?)
		// If only there was a way to get 'member not found diagnostics...'
		// A different option would be to find the target type of the invoke/member access and see if the named member is missing
		if (memberRefExpr.Parent is InvocationExpressionSyntax invoke && memberRefExpr == invoke.Expression) {
			isInvoke = true;
			return (op = model.GetOperation(invoke) as IInvalidOperation) != null && invoke.ArgumentList.Arguments.All(arg => model.GetOperation(arg) is not IInvalidOperation);
		}

		isInvoke = false;
		return (op = model.GetOperation(memberRefExpr) as IInvalidOperation) != null;
	}

	private IdentifierNameSyntax Refactor(IdentifierNameSyntax nameSyntax, ITypeSymbol instType, bool refactoringMethod) =>
		nameSyntax.WithIdentifier(Refactor(nameSyntax.Identifier, instType, refactoringMethod));

	private SyntaxToken Refactor(SyntaxToken nameToken, ITypeSymbol instType, bool refactoringMethod) {
		if (instType == null)
			return nameToken;

		foreach (var entry in memberRenames) {
			if (entry.from != nameToken.Text || refactoringMethod && !entry.isMethod || !instType.InheritsFrom(entry.type))
				continue;

			entry.followup?.Invoke(this, nameToken);
			return nameToken.WithText(entry.to);
		}

		return nameToken;
	}

	private IdentifierNameSyntax RefactorSpeculative(IdentifierNameSyntax nameSyntax) {
		var nameToken = nameSyntax.Identifier;

		foreach (var entry in memberRenames) {
			if (entry.from != nameToken.Text)
				continue;

			var repl = nameSyntax.WithIdentifier(nameToken.WithText(entry.to));
			var speculate = model.GetSpeculativeSymbolInfo(nameSyntax.SpanStart, repl, SpeculativeBindingOption.BindAsExpression);
			if (speculate.Symbol?.ContainingType?.ToString() == entry.type) {
				entry.followup?.Invoke(this, nameToken);
				return repl;
			}
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

	public static AdditionalRenameAction OnType(string newType) => (rw, node) => {
		if (node.Parent.Parent is not MemberAccessExpressionSyntax memberAccess || memberAccess.Expression is not SimpleNameSyntax onType)
			return;
		
		rw.RegisterAction<MemberAccessExpressionSyntax>(memberAccess, (newNode) =>
			newNode.WithExpression(IdentifierName(rw.UseTypeName(newType)).WithTriviaFrom(newNode.Expression))
		);
	};

	public static AdditionalRenameAction AccessMember(string memberName) => (rw, node) => {
		if (node.Parent is not IdentifierNameSyntax nameSyntax || nameSyntax.Parent is not ExpressionSyntax usage)
			return;

		usage = usage.Parent switch {
			ElementAccessExpressionSyntax e when usage == e.Expression => e,
			InvocationExpressionSyntax e when usage == e.Expression => e,
			_ => usage
		};

		rw.RegisterAction<ExpressionSyntax>(usage, (newNode) =>
			SimpleMemberAccessExpression(newNode.WithoutTrivia(), memberName).WithTriviaFrom(newNode)
		);
	};
}
