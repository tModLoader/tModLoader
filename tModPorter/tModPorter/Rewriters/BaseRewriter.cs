using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

		if (node != newNode && extraNodeVisitors.Remove(node, out var list)) {
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

	public TypeSyntax UseType(ITypeSymbol sym) =>
		sym is INamedTypeSymbol named ?
			UseType(named) :
			IdentifierName(sym.ToString());

	public TypeSyntax UseType(INamedTypeSymbol sym) {
		if (sym.ConstructedFrom is INamedTypeSymbol genericTemplate && genericTemplate.SpecialType == SpecialType.System_Nullable_T)
			return NullableType(UseType(sym.TypeArguments[0]));

		var specialKind = sym.SpecialType.SpecialTypeKind();
		if (specialKind != SyntaxKind.None)
			return PredefinedType(Token(specialKind));

		if (sym.ContainingNamespace != null) {
			UsingNamespace(sym.ContainingNamespace);
		}

		return IdentifierName(sym.Name);
	}

	public IdentifierNameSyntax UseType(string fullname) => (IdentifierNameSyntax)UseType(model.Compilation.GetTypeByMetadataName(fullname));

	public bool IsUsingNamespace(string @namespace) => usings.Contains(@namespace);

	public ParameterSyntax Parameter(IParameterSymbol p) =>
		SyntaxFactory.Parameter(default, new(ModifierToken(p.RefKind)), UseType(p.Type).WithTrailingTrivia(Space), Identifier(p.Name), default);

	public void RegisterAction<T>(SyntaxNode node, Func<T, T> rewrite) where T : SyntaxNode => RegisterAction(node, (n) => rewrite((T)n));

	public void RegisterAction(SyntaxNode node, Func<SyntaxNode, SyntaxNode> rewrite) {

		if (!extraNodeVisitors.TryGetValue(node, out var list))
			extraNodeVisitors[node] = list = new();

		list.AddLast(rewrite);
	}
}
