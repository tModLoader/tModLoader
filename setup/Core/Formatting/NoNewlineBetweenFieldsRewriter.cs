using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Terraria.ModLoader.Setup.Core.Formatting
{
	internal class NoNewlineBetweenFieldsRewriter : CSharpSyntaxRewriter
	{
		private readonly HashSet<SyntaxToken> modifyTokens = [];

		public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node) {
			TagFieldTokens(node.Members);
			return base.VisitClassDeclaration(node);
		}

		public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node) {
			TagFieldTokens(node.Members);
			return base.VisitStructDeclaration(node);
		}

		private void TagFieldTokens(SyntaxList<MemberDeclarationSyntax> members) {
			for (int i = 0; i < members.Count - 1; i++) {
				if ((members[i] is FieldDeclarationSyntax || members[i] is EventFieldDeclarationSyntax) && members[i].Kind() == members[i + 1].Kind()) {
					Tag(members[i + 1].GetFirstToken());
				}
			}
		}

		private void Tag(SyntaxToken token) {
			if (token.HasLeadingTrivia && token.LeadingTrivia[0].IsKind(SyntaxKind.EndOfLineTrivia) && token.LeadingTrivia.All(SyntaxUtils.IsWhitespace))
				modifyTokens.Add(token);
		}

		public override SyntaxToken VisitToken(SyntaxToken token) {
			if (modifyTokens.Contains(token))
				token = token.WithLeadingTrivia(token.LeadingTrivia.Skip(1));

			return base.VisitToken(token);
		}
	}
}