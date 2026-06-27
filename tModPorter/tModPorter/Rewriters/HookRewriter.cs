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
		public Dictionary<string, string> parameterRenames;

		public RefactorEntry RenameParameter(string from, string to) {
			parameterRenames ??= new();
			parameterRenames[from] = to;
			return this;
		}
	}

	private static List<RefactorEntry> refactors = new();

	private static RefactorEntry AddRefactor(string type, string method, string comment) {
		RefactorEntry entry = new() { type = type, method = method, comment = comment };
		refactors.Add(entry);
		return entry;
	}

	public static RefactorEntry ChangeHookSignature(string type, string member, string comment = null) => AddRefactor(type, member, comment);
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
		RegisterParameterRenames(sym, node);
		RegisterBaseMethodInvocationRewrites(sym, node);
		node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);
		if (!SelectRefactor(sym, out var refactor))
			return node;

		if ((refactor.removed || RewriteSignature(ref node, sym)) && refactor.comment != null)
			node = node.WithParameterList(node.ParameterList.WithBlockComment(refactor.comment));

		return node;
	}

	private void RegisterParameterRenames(IMethodSymbol sym, MethodDeclarationSyntax node) {
		if (!SelectRefactor(sym, out var refactor) || refactor.removed || refactor.parameterRenames == null)
			return;

		var renames = new Dictionary<IParameterSymbol, string>(SymbolEqualityComparer.Default);
		foreach (var param in sym.Parameters) {
			if (refactor.parameterRenames.TryGetValue(param.Name, out var newName))
				renames[param] = newName;
		}

		if (renames.Count == 0)
			return;

		var body = (SyntaxNode)node.Body ?? node.ExpressionBody;
		if (body == null)
			return;

		foreach (var nameSyntax in body.DescendantNodes().OfType<IdentifierNameSyntax>()) {
			if (model.GetSymbolInfo(nameSyntax).Symbol is IParameterSymbol param && renames.TryGetValue(param, out var newName))
				RegisterAction<IdentifierNameSyntax>(nameSyntax, n => n.WithIdentifier(newName));
		}
	}

	private void RegisterBaseMethodInvocationRewrites(IMethodSymbol sym, MethodDeclarationSyntax node)
	{
		if (!SelectRefactor(sym, out var refactor) || refactor.removed)
			return;

		var body = (SyntaxNode)node.Body ?? node.ExpressionBody;
		if (body == null)
			return;

		if (!SelectBaseSym(sym, sym.OverriddenMethod, out var baseSym))
			return;

		var newParamCount = baseSym.Parameters.Length;
		if (newParamCount <= 0) 
			return;
		
		foreach (var invoke in body.DescendantNodes().OfType<InvocationExpressionSyntax>()) {
			// Match base.<MethodName>(...)
			if (invoke.Expression is not MemberAccessExpressionSyntax memberAccess ||
				memberAccess.Expression is not BaseExpressionSyntax ||
				memberAccess.Name.Identifier.Text != sym.Name) {
				continue;
			}

			var origNode = invoke;
			if (ParametersEqual(sym, baseSym))
				continue;

			// If any parameters in the original method don't exist in the new method (because the modder renamed them) skip rewriting this invocation since we won't be able to map all arguments reliably.
			if (sym.Parameters.Any(x => !baseSym.Parameters.Select(x=>x.Name).Contains(x.Name)))
				continue;

			var existingArgs = invoke.ArgumentList?.Arguments.ToArray() ?? Array.Empty<ArgumentSyntax>();
			var builtArgs = new ArgumentSyntax[newParamCount];

			var originalArgumentsByName = new Dictionary<string, ArgumentSyntax>();
			var parameterExpressionsToKeep = new Dictionary<string, ExpressionSyntax>();

			// Make a mapping of parameter name to original argument syntax for all parameters in the original method to preserve trivia, and track any arguments that will need to be preserved as-is because they aren't simple identifier arguments matching the parameter name
			for (int i = 0; i < sym.Parameters.Length; i++) {
				originalArgumentsByName[sym.Parameters[i].Name] = existingArgs[i];
				if (existingArgs[i].Expression is not IdentifierNameSyntax identifier ||
					identifier.Identifier.Text != sym.Parameters[i].Name) {
					parameterExpressionsToKeep[sym.Parameters[i].Name] = existingArgs[i].Expression;
					break;
				}
			}

			// Initialize built args with identifier arguments using the base parameter names and ref/out keywords when necessary
			for (int i = 0; i < newParamCount; i++) {
				var p = baseSym.Parameters[i];
				ExpressionSyntax expr = IdentifierName(p.Name);
				if(parameterExpressionsToKeep.TryGetValue(p.Name, out var existingCustomExpression))
					expr = existingCustomExpression;
				var arg = Argument(expr);
				if (p.RefKind == RefKind.Ref)
					arg = arg.WithRefKindKeyword(TokenSpace(SyntaxKind.RefKeyword));
				else if (p.RefKind == RefKind.Out)
					arg = arg.WithRefKindKeyword(TokenSpace(SyntaxKind.OutKeyword));
				if (originalArgumentsByName.TryGetValue(p.Name, out var existingExpr) && (existingExpr.HasLeadingTrivia || existingExpr.HasTrailingTrivia)) {
					arg = arg.WithTriviaFrom(existingExpr);
					// Note: We could preserve trivia on ref/out keywords and before the 1st argument if we wanted to, but it's a bit more complex
				}
				builtArgs[i] = arg;
			}

			var newArgList = ArgumentList(SimpleSyntaxFactory.SeparatedList(builtArgs)).WithTriviaFrom(invoke.ArgumentList);
			RegisterAction<InvocationExpressionSyntax>(invoke, n => n.WithArgumentList(newArgList));
		}
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
