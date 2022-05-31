using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Threading.Tasks;
using static tModPorter.Rewriters.SimpleSyntaxFactory;

namespace tModPorter.Rewriters;

public abstract class BaseRewriter : CSharpSyntaxRewriter
{
	protected SemanticModel model;
	private SyntaxList<UsingDirectiveSyntax> usings;

	public async Task<Document> Rewrite(Document doc) {
		model = await doc.GetSemanticModelAsync() ?? throw new Exception("No semantic model: " + doc.FilePath);
		var root = await doc.GetSyntaxRootAsync() ?? throw new Exception("No syntax root: " + doc.FilePath);
		return doc.WithSyntaxRoot(Visit(root));
	}

	public override SyntaxToken VisitToken(SyntaxToken token) => token;

	public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node) {
		usings = node.Usings;
		return ((CompilationUnitSyntax)base.VisitCompilationUnit(node)).WithUsings(usings);
	}

	private void UsingNamespace(INamespaceSymbol ns) {
		if (usings.Any(u => u.Name.ToString() == ns.ToString()))
			return;

		usings = usings.Add(SimpleUsing(ns.ToString()));
	}

	public string UseType(INamedTypeSymbol sym) {
		if (sym.ContainingNamespace != null) {
			UsingNamespace(sym.ContainingNamespace);
		}

		return sym.Name;
	}

	public string UseTypeName(string fullname) => UseType(model.Compilation.GetTypeByMetadataName(fullname));
}
