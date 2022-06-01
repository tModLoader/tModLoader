using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
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
		usings = node.Usings;
		return ((CompilationUnitSyntax)base.VisitCompilationUnit(node)).WithUsings(usings);
	}

	private void UsingNamespace(INamespaceSymbol ns) {
		var fullname = ns.ToString();
		if (usings.Any(u => u.Name.ToString() == fullname))
			return;

		int idx = 0;
		while (idx < usings.Count && string.Compare(usings[idx].Name.ToString(), fullname) < 0)
			idx++;

		usings = usings.Insert(idx, SimpleUsing(fullname));
	}

	public string UseType(INamedTypeSymbol sym) {
		if (sym.ContainingNamespace != null) {
			UsingNamespace(sym.ContainingNamespace);
		}

		return sym.Name;
	}

	public string UseTypeName(string fullname) => UseType(model.Compilation.GetTypeByMetadataName(fullname));

	public void RegisterAction<T>(SyntaxNode node, Func<T, T> rewrite) where T : SyntaxNode => RegisterAction(node, (n) => rewrite((T)n));

	public void RegisterAction(SyntaxNode node, Func<SyntaxNode, SyntaxNode> rewrite) {

		if (!extraNodeVisitors.TryGetValue(node, out var list))
			extraNodeVisitors[node] = list = new();

		list.AddLast(rewrite);
	}
}
