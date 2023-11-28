using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static tModPorter.Rewriters.SimpleSyntaxFactory;

namespace tModPorter.Rewriters;

public abstract class BaseRewriter : CSharpSyntaxRewriter
{
	protected SemanticModel model;

	private Dictionary<SyntaxNode, LinkedList<Func<SyntaxNode, SyntaxNode>>> extraNodeVisitors = new();

	private SyntaxList<UsingDirectiveSyntax> usings;

	public async Task<Document> Rewrite(Document doc) {
		extraNodeVisitors.Clear();
		model = await doc.GetSemanticModelAsync() ?? throw new Exception("No semantic model: " + doc.FilePath);
		var root = await doc.GetSyntaxRootAsync() ?? throw new Exception("No syntax root: " + doc.FilePath);
		return doc.WithSyntaxRoot(Visit(root));
	}

	[return: NotNullIfNotNull("node")]
	public override SyntaxNode Visit(SyntaxNode node) {
		SyntaxNode newNode = base.Visit(node);

		if (node != null && extraNodeVisitors.Count > 0 && extraNodeVisitors.Remove(node, out var list)) {
			foreach (var f in list)
				newNode = f(newNode);
		}

		return newNode;
	}

	public override SyntaxToken VisitToken(SyntaxToken token) => token;

	public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node) {
		usings = VisitUsingList(node.Usings);
		return ((CompilationUnitSyntax)base.VisitCompilationUnit(node)).WithUsings(usings);
	}

	protected virtual SyntaxList<UsingDirectiveSyntax> VisitUsingList(SyntaxList<UsingDirectiveSyntax> usings) => usings;

	private void UsingNamespace(INamespaceSymbol ns) {
		usings = usings.WithUsingNamespace(ns.ToString());
	}

	public TypeSyntax UseType(ITypeSymbol sym) => sym switch {
		INamedTypeSymbol named => UseType(named),
		IArrayTypeSymbol array => UseType(array),
		_ => IdentifierName(sym.ToString())
	};

	public TypeSyntax UseType(INamedTypeSymbol sym) {
		if (sym.ConstructedFrom is INamedTypeSymbol genericTemplate && genericTemplate.SpecialType == SpecialType.System_Nullable_T)
			return NullableType(UseType(sym.TypeArguments[0]));

		var specialKind = sym.SpecialType.SpecialTypeKind();
		if (specialKind != SyntaxKind.None)
			return PredefinedType(Token(specialKind));

		if (sym.ContainingNamespace != null)
			UsingNamespace(sym.ContainingNamespace);

		return Name(sym);
	}

	private NameSyntax Name(INamedTypeSymbol sym)
	{
		SimpleNameSyntax name = sym.TypeArguments.Length > 0
			? GenericName(Identifier(sym.Name), TypeArgumentList(sym.TypeArguments.Select(UseType)))
			: IdentifierName(sym.Name);

		return sym.ContainingType != null
			? QualifiedName(Name(sym.ContainingType), name)
			: name;
	}

	public IdentifierNameSyntax UseType(string fullname) => (IdentifierNameSyntax)UseType(model.Compilation.GetTypeByMetadataName(fullname));

	public TypeSyntax UseType(IArrayTypeSymbol arrayType) => ArrayTypeRank1(UseType(arrayType.ElementType));

	public bool IsUsingNamespace(string @namespace) => usings.Contains(@namespace);

	public ParameterSyntax Parameter(IParameterSymbol p) =>
		SyntaxFactory.Parameter(default, new(ModifierToken(p.RefKind)), UseType(p.Type).WithTrailingTrivia(Space), Identifier(p.Name), default);

	public void RegisterAction<T>(SyntaxNode node, Func<T, SyntaxNode> rewrite) where T : SyntaxNode => RegisterAction(node, (n) => rewrite((T)n));

	public void RegisterAction(SyntaxNode node, Func<SyntaxNode, SyntaxNode> rewrite) {

		if (!extraNodeVisitors.TryGetValue(node, out var list))
			extraNodeVisitors[node] = list = new();

		list.AddLast(rewrite);
	}

	protected bool IdentifierNameInvalid(IdentifierNameSyntax node, out IOperation op, out ITypeSymbol targetType, out bool isInvoke) {
		switch (node.Parent) {
			case MemberAccessExpressionSyntax memberAccess when node == memberAccess.Name && MemberReferenceInvalid(memberAccess, out op, out isInvoke):
				targetType = model.GetTypeInfo(memberAccess.Expression).Type;
				return true;

			case MemberBindingExpressionSyntax memberBinding when MemberReferenceInvalid(memberBinding, out op, out isInvoke) && op.ChildOperations.First() is IConditionalAccessInstanceOperation target:
				targetType = target.Type;
				return true;

			// getting the operation for errored identifiers as part of the lhs of an initializer expression is difficult directly, need to go via the assignment
			case AssignmentExpressionSyntax assignment when assignment.Parent is InitializerExpressionSyntax && model.GetOperation(assignment) is IAssignmentOperation { Target: IInvalidOperation targetOp, Parent: var parent }:
				op = targetOp;
				targetType = parent.Type;
				isInvoke = false;
				return true;
		};

		targetType = null;
		isInvoke = false;
		op = model.GetOperation(node);
		if (IsInvalidOrObsolete(op)) {
			return true;
		}

		op = null;
		return model.GetSymbolInfo(node).Symbol == null;
	}

	protected bool MemberReferenceInvalid(SyntaxNode memberRefExpr, out IOperation op, out bool isInvoke) {
		// for MemberAccessExpressionSyntax and MemberBindingExpressionSyntax which are the target of InvocationExpressionSyntax, there is no operation for the member access, only the invocation
		// We check that all the arguments for the invocation are valid as a way of determining that it's the member access which is causing the failure (though this may fail if they rely on generic type inference I guess?)
		// If only there was a way to get 'member not found diagnostics...'
		// A different option would be to find the target type of the invoke/member access and see if the named member is missing
		if (memberRefExpr.Parent is InvocationExpressionSyntax invoke && memberRefExpr == invoke.Expression) {
			isInvoke = true;
			op = model.GetOperation(invoke);
			return IsInvalidOrObsolete(op) && invoke.ArgumentList.Arguments.All(arg => model.GetOperation(arg) is not IInvalidOperation);
		}

		isInvoke = false;
		op = model.GetOperation(memberRefExpr);
		return IsInvalidOrObsolete(op);
	}

	public static bool IsInvalidOrObsolete(IOperation op) =>
		op is IInvalidOperation ||
		op is IInvocationOperation invocation && invocation.TargetMethod.IsObsolete() ||
		op is IMemberReferenceOperation reference && reference.Member.IsObsolete();

	public static bool SuspectSideEffects(ExpressionSyntax expr, out ExpressionSyntax concern) {
		switch (expr) {
			case MemberAccessExpressionSyntax memberAccess:
				return SuspectSideEffects(memberAccess.Expression, out concern);
			case NameSyntax:
				concern = null;
				return false;
			default:
				concern = expr;
				return true;
		};
	}

	public static bool EqualWithNoSuspectedSideEffects(ExpressionSyntax x, ExpressionSyntax y) =>
		x.ToString() == y.ToString() && !SuspectSideEffects(x, out _) && !SuspectSideEffects(y, out _);

	public static (ExpressionSyntax[] positional, ArgumentSyntax[] named) ParseArgs(SeparatedSyntaxList<ArgumentSyntax> arguments) {
		var positional = new List<ArgumentSyntax>();
		var named = new List<ArgumentSyntax>();

		foreach (var arg in arguments) {
			if (arg.NameColon != null) {
				named.Add(arg);
			} else {
				positional.AddRange(named);
				named.Clear();
				positional.Add(arg);
			}
		}

		return (positional.Select(arg => arg.Expression).ToArray(), named.ToArray());
	}

	public static void ArrangeArgs(ref ExpressionSyntax[] positional, ArgumentSyntax[] named, ReadOnlySpan<string> span) {
		Array.Resize(ref positional, span.Length);
		foreach (var n in named) {
			int i = span.IndexOf(n.NameColon.Name.Identifier.Text);
			positional[i] = n.Expression;
		}
	}

	public static ExpressionSyntax UnwrapPredefinedCast(ExpressionSyntax x, string typeName) =>
		x is CastExpressionSyntax { Expression: var expr, Type: PredefinedTypeSyntax { Keyword.Text: var t } } && t == typeName ? expr : x;
}
