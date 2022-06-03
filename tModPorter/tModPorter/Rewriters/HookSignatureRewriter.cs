using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static tModPorter.Rewriters.SimpleSyntaxFactory;

namespace tModPorter.Rewriters;

public class HookSignatureRewriter : BaseRewriter
{
	public class SignatureChange
	{
		public string type { get; init; }
		public string method { get; init; }
		public string comment { get; init; }
	}

	private static List<SignatureChange> refactors = new();

	public static void ChangeHookSignature(string type, string method, string comment = null) => refactors.Add(new() { type = type, method = method, comment = comment });

	public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node) {
		var sym = model.GetDeclaredSymbol(node);
		node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);
		if (!sym.IsOverride || sym.OverriddenMethod != null && !ReturnTypeMismatch(sym, sym.OverriddenMethod))
			return node;

		var refactor = refactors.SingleOrDefault(refactor => sym.Name == refactor.method && sym.ContainingType.InheritsFrom(refactor.type));
		if (refactor == null)
			return node;

		var baseSym = sym.OverriddenMethod ?? sym.ContainingType.BaseType.LookupMethod(refactor.method);
		if (baseSym == null)
			return node;

		var origNode = node;
		if (!ParametersEqual(sym, baseSym)) {
			node = node.WithParameterList(ParameterList(baseSym.Parameters.Select(Parameter)).WithTriviaFrom(node.ParameterList));
		}

		if (!SymbolEqualityComparer.Default.Equals(sym.ReturnType, baseSym.ReturnType)) {
			node = node.WithReturnType(UseType(baseSym.ReturnType).WithTriviaFrom(node.ReturnType));
		}

		if (node != origNode && refactor.comment != null && !node.ParameterList.GetTrailingTrivia().Any(SyntaxKind.MultiLineCommentTrivia)) {
			node = node.WithParameterList(node.ParameterList.WithBlockComment(refactor.comment));
		}

		return node;
	}

	private bool ReturnTypeMismatch(IMethodSymbol sym, IMethodSymbol baseSym) {
		return !model.Compilation.ClassifyConversion(sym.ReturnType, baseSym.ReturnType).IsIdentity;
	}

	private static bool ParametersEqual(IMethodSymbol sym1, IMethodSymbol sym2) =>
		sym1.Parameters.SequenceEqual(sym2.Parameters, (p1, p2) => SymbolEqualityComparer.Default.Equals(p1.Type, p2.Type) && p1.RefKind == p2.RefKind);
}
