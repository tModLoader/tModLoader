using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Terraria.ModLoader.Setup.Formatting
{
	class AddIDUsingRewriter : CSharpSyntaxRewriter
	{
		private SemanticModel model;

		public AddIDUsingRewriter(SemanticModel model) {
			this.model = model;
		}

		public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node) {
			var namespaceDeclaration = node.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
			if(namespaceDeclaration.Name.ToString().StartsWith("Terraria.ID")){
				return base.VisitCompilationUnit(node);
			}

			// TODO: ignore if current namespace is Terraira.ID
			if (!node.Usings.Any(u => u.Name.GetText().ToString() == "Terraria.ID")) {
				var newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Terraria.ID")).NormalizeWhitespace();
				if (node.Usings.Any())
					return node.InsertNodesAfter(node.Usings.Last(), new[] { newUsing });
				else
					return node.AddUsings(newUsing);
			}

			return base.VisitCompilationUnit(node);
		}
	}
}
