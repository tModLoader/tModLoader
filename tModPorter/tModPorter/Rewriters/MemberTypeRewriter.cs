using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static tModPorter.Rewriters.HookRewriter;
using static tModPorter.Rewriters.RenameRewriter;

namespace tModPorter.Rewriters;
public class MemberTypeRewriter : BaseRewriter
{
	public class MemberTypeChange
	{
		public string type { get; init; }    // declaring type, e.g. "Terraria.Main"
		public string member { get; init; }  // member name, e.g. "item" or "ActiveItems"
		public string to { get; init; }      // new type fullname, e.g. "Terraria.WorldItem"
		public string from { get; init; }    // old type fullname, e.g. "Terraria.Item"
	}

	private static List<MemberTypeChange> fieldTypeChanges = new();

	private static MemberTypeChange ChangeMemberType(string type, string member, string from, string to)
	{
		MemberTypeChange entry = new() { type = type, member = member, from = from, to = to };
		fieldTypeChanges.Add(entry);
		return entry;
	}

	public static MemberTypeChange ChangeStaticFieldType(string type, string member, string from, string to) => ChangeMemberType(type, member, from, to);

	// Return the element type for arrays or IEnumerable<T>-like types, otherwise null.
	// Also supports pattern-based enumerables where the collection type exposes a GetEnumerator()
	// whose returned enumerator has a 'Current' property or implements IEnumerator<T>.
	private static ITypeSymbol GetElementType(ITypeSymbol t)
	{
		if (t is IArrayTypeSymbol arr)
			return arr.ElementType;

		if (t is INamedTypeSymbol named) {
			// Direct generic enumerable
			if (named.IsGenericType && named.ConstructedFrom != null && named.ConstructedFrom.ToString().StartsWith("System.Collections.Generic.IEnumerable"))
				return named.TypeArguments.Length > 0 ? named.TypeArguments[0] : null;

			// Check implemented interfaces for IEnumerable<T>
			foreach (var iface in named.AllInterfaces) {
				if (iface.IsGenericType && iface.ConstructedFrom != null && iface.ConstructedFrom.ToString().StartsWith("System.Collections.Generic.IEnumerable"))
					return iface.TypeArguments.Length > 0 ? iface.TypeArguments[0] : null;
			}

			// Handle pattern-based enumeration: look for a GetEnumerator method and inspect its return type.
			var getEnumMethod = named.LookupMember<IMethodSymbol>("GetEnumerator");
			if (getEnumMethod != null) {
				var ret = getEnumMethod.ReturnType;
				if (ret is INamedTypeSymbol retNamed) {
					// If it's IEnumerator<T>, use the T
					if (retNamed.IsGenericType && retNamed.ConstructedFrom != null && retNamed.ConstructedFrom.ToString().StartsWith("System.Collections.Generic.IEnumerator"))
						return retNamed.TypeArguments.Length > 0 ? retNamed.TypeArguments[0] : null;

					// Otherwise try to read a Current property on the enumerator type
					var currentProp = ret.LookupMember<IPropertySymbol>("Current");
					if (currentProp != null)
						return currentProp.Type;
				}
				else {
					// fallback: non-named return type, still try to lookup Current
					var currentProp = ret.LookupMember<IPropertySymbol>("Current");
					if (currentProp != null)
						return currentProp.Type;
				}
			}
		}

		return null;
	}

	private static string ShortName(string fullname) {
		if (string.IsNullOrEmpty(fullname))
			return fullname;
		int i = fullname.LastIndexOf('.');
		return i == -1 ? fullname : fullname[(i + 1)..];
	}

	// Try to resolve a type symbol for an arbitrary expression, using both GetTypeInfo and GetSymbolInfo fallbacks.
	private ITypeSymbol GetTypeForExpression(ExpressionSyntax expr)
	{
		if (expr == null)
			return null;

		var t = model.GetTypeInfo(expr).Type;
		if (t != null)
			return t;

		var sym = model.GetSymbolInfo(expr).Symbol;
		switch (sym) {
			case ILocalSymbol ls: return ls.Type;
			case IFieldSymbol fs: return fs.Type;
			case IPropertySymbol ps: return ps.Type;
			case IParameterSymbol prs: return prs.Type;
			case IMethodSymbol ms: return ms.ReturnType;
			case INamedTypeSymbol nts: return nts; // e.g. Terraria.Main referenced as a type
			case IAliasSymbol alias:
				return alias.Target as ITypeSymbol ?? model.Compilation.GetTypeByMetadataName(alias.Target?.ToString() ?? "");
			default:
				return null;
		}
	}

	public override SyntaxNode VisitForEachStatement(ForEachStatementSyntax node)
	{
		// visit children first
		node = (ForEachStatementSyntax)base.VisitForEachStatement(node);

		// Only handle explicit typed foreach, not 'var'
		var declaredTypeSymbol = model.GetTypeInfo(node.Type).Type;
		foreach (var change in fieldTypeChanges) {
			var fromShort = ShortName(change.from);
			var toShort = ShortName(change.to);

			// if we couldn't resolve the declared type symbol, fall back to text comparison
			bool declaredMatches = declaredTypeSymbol != null
				? (declaredTypeSymbol.ToString() == change.from || declaredTypeSymbol.InheritsFrom(change.from) || ShortName(declaredTypeSymbol.ToString()) == fromShort)
				: (node.Type is IdentifierNameSyntax id && id.Identifier.Text == fromShort) || (node.Type is QualifiedNameSyntax q && ShortName(q.ToString()) == fromShort);

			if (!declaredMatches)
				continue;

			// Validate expression member access if present
			var exprType = GetTypeForExpression(node.Expression);
			if (exprType == null)
				continue;

			if (node.Expression is MemberAccessExpressionSyntax memberAccess) {
				if (memberAccess.Name.Identifier.Text != change.member)
					continue;

				var targetType = GetTypeForExpression(memberAccess.Expression);
				if (targetType == null || !targetType.InheritsFrom(change.type))
					continue;
			}

			var element = GetElementType(exprType);
			if (element == null) {
				// sometimes the expression itself might already be the element type
				if (exprType.InheritsFrom(change.to) || ShortName(exprType.ToString()) == toShort) {
					var newType = UseType(exprType).WithTriviaFrom(node.Type);
					return node.WithType(newType);
				}
				continue;
			}

			if (element.InheritsFrom(change.to) || element.Name == toShort) {
				var newType = UseType(element).WithTriviaFrom(node.Type);
				return node.WithType(newType);
			}
		}

		return node;
	}

	public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
	{
		// visit children first
		node = (LocalDeclarationStatementSyntax)base.VisitLocalDeclarationStatement(node);

		// Only handle explicit typed locals (not 'var')
		var decl = node.Declaration;
		if (decl == null || decl.Type == null)
			return node;

		var declaredTypeSymbol = model.GetTypeInfo(decl.Type).Type;

		// For each registered change where the declared type matches "from", attempt to detect an initializer
		// that yields the new type (or element type of it), and update the declared type accordingly.
		foreach (var change in fieldTypeChanges) {
			var fromShort = ShortName(change.from);
			var toShort = ShortName(change.to);

			bool declaredMatches = declaredTypeSymbol != null
				? (declaredTypeSymbol.ToString() == change.from || declaredTypeSymbol.InheritsFrom(change.from) || ShortName(declaredTypeSymbol.ToString()) == fromShort)
				: (decl.Type is IdentifierNameSyntax id && id.Identifier.Text == fromShort) || (decl.Type is QualifiedNameSyntax q && ShortName(q.ToString()) == fromShort);

			if (!declaredMatches)
				continue;

			// Only handle cases with at least one variable with an initializer we can inspect
			var v = decl.Variables.FirstOrDefault(x => x.Initializer != null);
			if (v == null)
				continue;

			var initExpr = v.Initializer.Value;
			ITypeSymbol initType = null;

			// Prefer operation type if available
			var op = model.GetOperation(initExpr);
			if (op != null)
				initType = op.Type;
			if (initType == null)
				initType = GetTypeForExpression(initExpr);

			if (initType == null)
				continue;

			// If initializer is a member element access like Main.item[i], prefer validating the member/type
			if (initExpr is ElementAccessExpressionSyntax elemAccess && elemAccess.Expression is MemberAccessExpressionSyntax membExpr) {
				if (membExpr.Name.Identifier.Text != change.member)
					continue;

				var containerType = GetTypeForExpression(membExpr.Expression);
				if (containerType == null || !containerType.InheritsFrom(change.type))
					continue;
			}
			else if (initExpr is MemberAccessExpressionSyntax singleMember) {
				// e.g. assigning Main.ActiveItems (rare for locals) - ensure member matches
				if (singleMember.Name.Identifier.Text != change.member)
					continue;

				var containerType = GetTypeForExpression(singleMember.Expression);
				if (containerType == null || !containerType.InheritsFrom(change.type))
					continue;
			}

			// If the initializer itself already yields the desired 'to' type (or a subtype), use it
			if (initType.InheritsFrom(change.to) || initType.Name == toShort) {
				var newType = UseType(initType).WithTriviaFrom(decl.Type);
				var newDecl = decl.WithType(newType);
				return node.WithDeclaration(newDecl);
			}

			// Otherwise check element type (array / IEnumerable<T> -> T)
			var element = GetElementType(initType);
			if (element != null && (element.InheritsFrom(change.to) || element.Name == toShort)) {
				var newType = UseType(element).WithTriviaFrom(decl.Type);
				var newDecl = decl.WithType(newType);
				return node.WithDeclaration(newDecl);
			}
		}

		return node;
	}
}