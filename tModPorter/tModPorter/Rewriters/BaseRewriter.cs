using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Threading.Tasks;

namespace tModPorter.Rewriters;

public abstract class BaseRewriter : CSharpSyntaxRewriter
{
	protected SemanticModel model;

	public async Task<Document> Rewrite(Document doc) {
		model = await doc.GetSemanticModelAsync() ?? throw new Exception("No semantic model: " + doc.FilePath);
		var root = await doc.GetSyntaxRootAsync() ?? throw new Exception("No syntax root: " + doc.FilePath);
		return doc.WithSyntaxRoot(Visit(root));
	}

	public override SyntaxToken VisitToken(SyntaxToken token) => token;
}