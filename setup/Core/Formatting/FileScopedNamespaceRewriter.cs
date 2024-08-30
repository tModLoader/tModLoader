using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Terraria.ModLoader.Setup.Core.Formatting
{
	public class FileScopedNamespaceRewriter : CSharpSyntaxRewriter
	{
		public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node) {
			var fs = SyntaxFactory.FileScopedNamespaceDeclaration(
					node.AttributeLists,
					node.Modifiers,
					node.NamespaceKeyword,
					node.Name.WithoutTrailingTrivia(),
					SyntaxFactory.Token(SyntaxKind.SemicolonToken).WithTrailingTrivia(
						node.Name.GetTrailingTrivia().Add(SyntaxFactory.EndOfLine(Environment.NewLine))),
					node.Externs,
					node.Usings,
					Unindent(node.Members))
				.WithTriviaFrom(node);
			return fs;
		}

		private static SyntaxList<MemberDeclarationSyntax> Unindent(SyntaxList<MemberDeclarationSyntax> members) {
			return new UnindentRewriter().VisitList(members);
		}
	}
}
