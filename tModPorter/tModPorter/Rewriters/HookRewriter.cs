using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static tModPorter.Rewriters.SimpleSyntaxFactory;

namespace tModPorter.Rewriters;

public class HookRewriter : BaseRewriter
{
	internal delegate bool RewriteHook(ref MethodDeclarationSyntax node, IMethodSymbol sym, IMethodSymbol baseSym);

	public class RefactorEntry
	{
		public string type { get; init; }
		public string method { get; init; }
		public string comment { get; init; }

		public bool removed;
	}

	private static List<RefactorEntry> refactors = new();

	private static RefactorEntry AddRefactor(string type, string method, string comment) {
		RefactorEntry entry = new() { type = type, method = method, comment = comment };
		refactors.Add(entry);
		return entry;
	}

	public static void ChangeHookSignature(string type, string member, string comment = null) => AddRefactor(type, member, comment);
	public static void HookRemoved(string type, string member, string comment) => AddRefactor(type, member, "Note: Removed. " + comment).removed = true;

	private static bool SelectRefactor(ISymbol sym, out RefactorEntry refactor) {
		refactor = null;
		if (!sym.IsOverride)
			return false;

		refactor = refactors.SingleOrDefault(refactor => sym.Name == refactor.method && sym.ContainingType.InheritsFrom(refactor.type));
		return refactor != null;
	}

	private static bool SelectBaseSym<T>(T sym, T overriddenSym, out T baseSym) where T : class, ISymbol {
		baseSym = overriddenSym;
		if (baseSym == null || baseSym.IsObsolete())
			baseSym = sym.ContainingType.BaseType.LookupMember<T>(sym.Name);

		return baseSym != null;
	}

	public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node) {
		var sym = model.GetDeclaredSymbol(node);
		node = (PropertyDeclarationSyntax)base.VisitPropertyDeclaration(node);
		if (!SelectRefactor(sym, out var refactor))
			return node;

		if ((refactor.removed || RewriteSignature(ref node, sym)) && refactor.comment != null)
			node = node.WithIdentifier(node.Identifier.WithBlockComment(refactor.comment));

		return node;
	}

	public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node) {
		var sym = model.GetDeclaredSymbol(node);
		node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);
		if (!SelectRefactor(sym, out var refactor))
			return node;

		if ((refactor.removed || RewriteSignature(ref node, sym)) && refactor.comment != null)
			node = node.WithParameterList(node.ParameterList.WithBlockComment(refactor.comment));

		return node;
	}

	private bool AccessibilityMismatch(ISymbol sym, ISymbol baseSym) =>
		sym.DeclaredAccessibility != baseSym.DeclaredAccessibility;

	private bool TypeMismatch(ITypeSymbol t1, ITypeSymbol t2) =>
		!model.Compilation.ClassifyConversion(t1, t2).IsIdentity;

	private static bool ParametersEqual(IMethodSymbol sym1, IMethodSymbol sym2) =>
		sym1.Parameters.SequenceEqual(sym2.Parameters, (p1, p2) => SymbolEqualityComparer.Default.Equals(p1.Type, p2.Type) && p1.RefKind == p2.RefKind);

	private bool RewriteModifiers(ISymbol sym, ISymbol baseSym, SyntaxTokenList modifiers, out SyntaxTokenList newModifiers) {
		if (!AccessibilityMismatch(sym, baseSym)) {
			newModifiers = default;
			return false;
		}

		newModifiers = ModifierList(baseSym.DeclaredAccessibility);
		if (newModifiers.Any())
			newModifiers = newModifiers.Replace(newModifiers.Last(), newModifiers.Last().WithTrailingTrivia(Space));

		newModifiers = newModifiers
			.Add(Token(SyntaxKind.OverrideKeyword))
			.WithTriviaFrom(modifiers);

		return true;
	}

	private bool RewriteSignature(ref MethodDeclarationSyntax node, IMethodSymbol sym) {
		if (!SelectBaseSym(sym, sym.OverriddenMethod, out var baseSym))
			return false;

		var origNode = node;
		if (!ParametersEqual(sym, baseSym))
			node = node.WithParameterList(ParameterList(baseSym.Parameters.Select(Parameter)).WithTriviaFrom(node.ParameterList));

		if (TypeMismatch(sym.ReturnType, baseSym.ReturnType))
			node = node.WithReturnType(UseType(baseSym.ReturnType).WithTriviaFrom(node.ReturnType));

		if (RewriteModifiers(sym, baseSym, node.Modifiers, out var newModifiers))
			node = node.WithModifiers(newModifiers);

		return node != origNode;
	}

	private bool RewriteSignature(ref PropertyDeclarationSyntax node, IPropertySymbol sym) {
		if (!SelectBaseSym(sym, sym.OverriddenProperty, out var baseSym))
			return false;

		var origNode = node;
		if (TypeMismatch(sym.Type, baseSym.Type))
			node = node.WithType(UseType(baseSym.Type).WithTriviaFrom(node.Type));

		if (RewriteModifiers(sym, baseSym, node.Modifiers, out var newModifiers))
			node = node.WithModifiers(newModifiers);

		return node != origNode;
	}
}
