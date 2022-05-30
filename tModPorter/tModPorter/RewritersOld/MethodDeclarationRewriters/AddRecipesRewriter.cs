using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace tModPorter.Rewriters.MethodDeclarationRewriters;

public class AddRecipesRewriter : BaseRewriter {
	private static readonly List<string> ValidModRecipeMethods = new() {
		"AddIngredient",
		"AddRecipe",
		"AddRecipeGroup",
		"AddTile",
		"SetResult",
	};
	
	public AddRecipesRewriter(SemanticModel model, List<string> usingList,
		HashSet<(BaseRewriter rewriter, SyntaxNode originalNode)> nodesToRewrite,
		HashSet<(BaseRewriter rewriter, SyntaxToken originalToken)> tokensToRewrite)
		: base(model, usingList, nodesToRewrite, tokensToRewrite) { }

	public override RewriterType RewriterType => RewriterType.Method;

	public override void VisitNode(SyntaxNode node) {
		if (node is not MethodDeclarationSyntax nodeMethod)
			return;

		// Make sure the body isn't null, that the method name is "AddRecipes", that it has statements, and that it hasn't already been ported
		if (nodeMethod.Body != null && nodeMethod.Identifier.Text == "AddRecipes"
									&& nodeMethod.Body.Statements.Count != 0 && !nodeMethod.Body.ToString().Contains("CreateRecipe"))
			AddNodeToRewrite(node);
	}

	public override SyntaxNode RewriteNode(SyntaxNode node) {
		MethodDeclarationSyntax nodeMethod = (MethodDeclarationSyntax) node;
		// SyntaxTriviaList leading = nodeMethod.Body.Statements.First().GetLeadingTrivia();
		SyntaxList<StatementSyntax> newStatements = new();

		string expression = "";
		int resultAmount = 1;
		string result = null;

		foreach (StatementSyntax statementSyntax in nodeMethod.Body.Statements) {
			if (HasSymbol(statementSyntax, out _)) {
				newStatements = newStatements.Add(statementSyntax);
				continue;
			}

			if (statementSyntax is LocalDeclarationStatementSyntax or ExpressionStatementSyntax {Expression: AssignmentExpressionSyntax}) {
				if (!statementSyntax.ToString().Contains("ModRecipe"))
					newStatements = newStatements.Add(statementSyntax);
				continue;
			}

			if (statementSyntax is not ExpressionStatementSyntax {Expression: InvocationExpressionSyntax invocationExpressionSyntax}) {
				newStatements = newStatements.Add(statementSyntax);
				continue;
			}

			if (invocationExpressionSyntax.Expression is not MemberAccessExpressionSyntax memberAccessSyntax) {
				newStatements = newStatements.Add(statementSyntax);
				continue;
			}

			if (!ValidModRecipeMethods.Contains(memberAccessSyntax.Name.ToString().Trim())) {
				newStatements = newStatements.Add(statementSyntax);
				continue;
			}

			string eol = memberAccessSyntax.SyntaxTree.GetRoot().DescendantTrivia().Where(t => t.IsKind(SyntaxKind.EndOfLineTrivia)).Select(t => t.ToFullString()).GroupBy(x => x).MaxBy(g => g.Count()).First();
			string leadingTrivia = memberAccessSyntax.GetLeadingTrivia().ToString();
			int numIndentations;
			char indentChar;
			if (leadingTrivia.Contains('\t')) {
				indentChar = '\t';
				numIndentations = leadingTrivia.Count(c => c == '\t');
			}
			else {
				// 4 spaces mean 1 indent
				indentChar = ' ';
				numIndentations = leadingTrivia.Count(c => c == ' ') / 4;
			}

			string GenerateIndent() {
				string indent = eol;
				for (int i = 0; i < numIndentations + 1; i++) {
					if (indentChar == '\t')
						indent += "\t";
					else
						indent += "	";
				}
				return indent;
			}

			// Parse the existing recipe
			switch (memberAccessSyntax.Name.ToString()) {
				case "AddIngredient":
				case "AddTile":
				case "AddRecipeGroup":
					string[] splitExpression = invocationExpressionSyntax.ToString().Split('.', 2);
					expression += GenerateIndent() + "." + splitExpression[1];
					break;
				case "SetResult":
					string[] arguments = invocationExpressionSyntax.ArgumentList.Arguments.Select(a => a.ToString()).ToArray();

					if (arguments[0] != "this")
						result = arguments[0];
					if (arguments.Length == 2)
						resultAmount = int.Parse(arguments[1]);
					break;
				case "AddRecipe":
					string parsedExpression;

					if (string.IsNullOrEmpty(result))
						parsedExpression = $"CreateRecipe({resultAmount})";
					else
						parsedExpression = $"Mod.CreateRecipe({result}, {resultAmount})";

					parsedExpression += expression + GenerateIndent() + ".Register()";

					newStatements = newStatements.Add(ExpressionStatement(ParseExpression(parsedExpression)).WithTriviaFrom(statementSyntax));

					expression = "";
					resultAmount = 1;
					result = "";
					break;
			}
		}

		BlockSyntax modifierBody = nodeMethod.Body.WithStatements(newStatements);
		return nodeMethod.WithBody(modifierBody);
	}
}