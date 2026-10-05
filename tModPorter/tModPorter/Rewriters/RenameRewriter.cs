using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
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
	private static Dictionary<string, string> namespaceRenames = new();

	private static MemberRename RenameMember(MemberRename entry) {
		memberRenames.Add(entry);
		return entry;
	}

	public static MemberRename RenameInstanceField(string type, string from, string to) => RenameMember(new() { type = type, from = from, to = to });
	public static void RenameInstanceFieldMultiple(string type, string[] froms, string to)
	{
		foreach (string from in froms) {
			RenameMember(new() { type = type, from = from, to = to });
		}
	}
	public static MemberRename RenameStaticField(string type, string from, string to) => RenameMember(new() { type = type, from = from, to = to });
	public static void RenameStaticFields(string type, params string[] fromTo)
	{
		if (fromTo.Length % 2 != 0)
			throw new Exception("You have a bad length for inputs on RenameStaticFields");
		for (int i = 0; i < fromTo.Length; i += 2) {
			RenameMember(new() { type = type, from = fromTo[i], to = fromTo[i + 1] });
		}
	}
	public static MemberRename RenameMethod(string type, string from, string to) => RenameMember(new() { type = type, from = from, to = to, isMethod = true });
	public static void RenameType(string from, string to) => typeRenames.Add((from, to));
	public static void RenameNamespace(string from, string to) => namespaceRenames.Add(from, to);

	public static MemberRename RenameStaticField(string type, string from, string to, string newType) => RenameStaticField(type, from, to).FollowBy(OnType(newType));
	public static MemberRename RenameMethod(string type, string from, string to, string newType) => RenameMethod(type, from, to).FollowBy(OnType(newType));

	public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node) {
		if (!IdentifierNameInvalid(node, out var op, out var targetType, out bool isInvoke))
			return node;

		if (op != null) {
			if (targetType != null)
				return Refactor(node, targetType, isInvoke);
			
			return RefactorSpeculative(node);
		}

		return RefactorType(node);
	}

	public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node) {
		var sym = model.GetDeclaredSymbol(node);
		node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);
		if (sym.IsOverride && (sym.OverriddenMethod == null || sym.OverriddenMethod.IsObsolete())) {
			node = node.WithIdentifier(Refactor(node.Identifier, sym.ContainingType, refactoringMethod: true));
		}

		return node;
	}

	public override SyntaxNode VisitQualifiedName(QualifiedNameSyntax node) {
		if (model.GetSymbolInfo(node).Symbol != null)
			return node;

		if (!namespaceRenames.TryGetValue(node.ToString(), out var newNamespace))
			return base.VisitQualifiedName(node);


		return Name(newNamespace);
	}

	protected override SyntaxList<UsingDirectiveSyntax> VisitUsingList(SyntaxList<UsingDirectiveSyntax> usings) {
		var renamed = usings.Where(u => u.Name != null && namespaceRenames.ContainsKey(u.Name.ToString())).ToArray();
		if (renamed.Length == 0)
			return usings;

		usings = List(usings.Except(renamed));
		foreach (var u in renamed) {
			usings = usings.WithUsingNamespace(namespaceRenames[u.Name.ToString()]);
		}

		return base.VisitUsingList(usings);
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

		if (!refactoringMethod)
			RefactorInnerType(nameToken, instType);

		return nameToken;
	}

	private void RefactorInnerType(SyntaxToken nameToken, ITypeSymbol instType)
	{
		if (nameToken.Parent.Parent is not MemberAccessExpressionSyntax memberAccess)
			return;

		var fullname = $"{instType}.{nameToken.Text}";
		if (typeRenames.SingleOrDefault(e => e.from == fullname) is not (_, string to))
			return;

		RegisterAction(memberAccess, node => UseType(to));
	}

	private SyntaxNode RefactorSpeculative(IdentifierNameSyntax nameSyntax) {
		var nameToken = nameSyntax.Identifier;

		foreach (var entry in memberRenames) {
			if (entry.from != nameToken.Text)
				continue;

			var repl = nameSyntax.WithIdentifier(entry.to);
			var speculate = model.GetSpeculativeSymbolInfo(nameSyntax.SpanStart, repl, SpeculativeBindingOption.BindAsExpression);
			if (speculate.Symbol?.ContainingType?.ToString() == entry.type || model.GetEnclosingSymbol(nameSyntax.SpanStart).ContainingType.InheritsFrom(entry.type)) {
				entry.followup?.Invoke(this, nameToken);
				return repl;
			}
		}

		return RefactorType(nameSyntax);
	}

	private TypeSyntax RefactorType(IdentifierNameSyntax nameSyntax) {
		var nameToken = nameSyntax.Identifier;

		foreach (var (from, to) in typeRenames) {
			if (!from.EndsWith(nameToken.Text))
				continue;

			var qualifier = from[..^nameToken.Text.Length];
			if (qualifier[^1] != '.')
				continue;

			if (to.StartsWith(qualifier)) { // check for a nested class or similar
				var repl = nameSyntax.WithIdentifier(to[qualifier.Length..]);
				var speculate = model.GetSpeculativeSymbolInfo(nameSyntax.SpanStart, repl, SpeculativeBindingOption.BindAsTypeOrNamespace);
				if (speculate.Symbol?.ToString() == to)
					return repl;
			}

			if (IsUsingNamespace(qualifier[..^1])) {
				return UseTypeByMetadataName(to).WithTriviaFrom(nameSyntax);
			}
		}

		return nameSyntax;
	}

	public static AdditionalRenameAction OnType(string newType) => (rw, token) => {
		if (token.Parent.Parent is MemberAccessExpressionSyntax { Expression: SimpleNameSyntax } memberAccess) {
			rw.RegisterAction<MemberAccessExpressionSyntax>(memberAccess, n =>
				n.WithExpression(rw.UseType(newType).WithTriviaFrom(n.Expression)));
		}
		else if (token.Parent is SimpleNameSyntax name && rw.model.GetOperation(token.Parent) is IInvalidOperation) { // standalone expr
			rw.RegisterAction<SimpleNameSyntax>(name,
				n => MemberAccessExpression(rw.UseType(newType), n.WithoutTrivia()).WithTriviaFrom(n));
		}
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
			MemberAccessExpression(newNode.WithoutTrivia(), memberName).WithTriviaFrom(newNode)
		);
	};

	public static AdditionalRenameAction AddCommentToMethodInvocation(string comment) => (rw, node) => {
		if (node.Parent.Parent.Parent is InvocationExpressionSyntax invoke)
			rw.RegisterAction<InvocationExpressionSyntax>(invoke, newNode => newNode.WithArgumentList(newNode.ArgumentList.WithBlockComment(comment)));
	};

	public static AdditionalRenameAction AddCommentToOverride(string comment) => (rw, node) => {
		if (node.Parent is MethodDeclarationSyntax decl)
			rw.RegisterAction<MethodDeclarationSyntax>(decl, newNode => newNode.WithParameterList(newNode.ParameterList.WithBlockComment(comment)));
	};

	public static AdditionalRenameAction AddCommentToFieldAccess(string comment) => (rw, node) => {
		if (node.Parent is not IdentifierNameSyntax nameSyntax || nameSyntax.Parent is not ExpressionSyntax usage)
			return;
		usage = usage.Parent switch {
			ElementAccessExpressionSyntax e when usage == e.Expression => e,
			_ => usage
		};
		rw.RegisterAction<ExpressionSyntax>(usage, newNode => newNode.WithBlockComment(comment));
	};

	public static AdditionalRenameAction AccessShimmerBuffIDElem() => (rw, node) => {
		if (node is not { Parent: IdentifierNameSyntax { Parent: ExpressionSyntax { Parent: ElementAccessExpressionSyntax elemAccess} } })
			return;

		var buffIdShimmer = MemberAccessExpression(rw.UseType("Terraria.ID.BuffID"), "Shimmer");
		rw.RegisterAction<ExpressionSyntax>(elemAccess,
			n => ElementAccessExpression(n.WithoutTrivia(), buffIdShimmer).WithTriviaFrom(n));
	};

	public static void ConvertCollectionAddToSetTrue(RenameRewriter rw, SyntaxToken node)
	{
		// node.Add(...)
		// ->
		// node[...] = true

		if (node is not { Parent: IdentifierNameSyntax { Parent: MemberAccessExpressionSyntax {
				Parent: MemberAccessExpressionSyntax { Name.Identifier.Text: "Add", Parent: InvocationExpressionSyntax invoke } } } })
			return;

		if (invoke.ArgumentList.Arguments.Count != 1)
			return;

		rw.RegisterAction<InvocationExpressionSyntax>(invoke, n => {
			var addAccess = (MemberAccessExpressionSyntax)n.Expression;
			var arrayAccess = addAccess.Expression;
			var arg = n.ArgumentList.Arguments[0].Expression;
			return AssignmentExpression(
				ElementAccessExpression(arrayAccess.WithoutTrivia(), arg.WithoutTrivia()),
				LiteralExpression(SyntaxKind.TrueLiteralExpression)
			).WithTriviaFrom(n);
		});
	}

	public static void InvertBool(RenameRewriter rw, SyntaxToken node)
	{
		if (node.Parent is not IdentifierNameSyntax nameSyntax)
			return;

		ExpressionSyntax usage = nameSyntax.Parent is MemberAccessExpressionSyntax memberAccess
			? memberAccess
			: nameSyntax;

		if (usage.Parent is AssignmentExpressionSyntax assign && assign.Left == usage)
			return;

		if (usage.Parent is PrefixUnaryExpressionSyntax notExpr && notExpr.IsKind(SyntaxKind.LogicalNotExpression)) {
			rw.RegisterAction<PrefixUnaryExpressionSyntax>(notExpr,
				n => n.Operand.WithTriviaFrom(n));
		}
		else {
			rw.RegisterAction<ExpressionSyntax>(usage,
				n => PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, n.WithoutTrivia()).WithTriviaFrom(n));
		}
	}
}
