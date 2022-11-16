using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Terraria.ModLoader.Setup.Formatting
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
					node.Members) // rely on the formatter to fix the indentation of the members for us
				.WithTriviaFrom(node);
			return fs;
		}
	}
}
