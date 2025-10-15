using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace tModCodeAssist.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SimplifyLocalPlayerAnalyzer() : AbstractDiagnosticAnalyzer(Diagnostics.SimplifyLocalPlayer)
{
	protected override void InitializeWorker(AnalysisContext ctx)
	{
		ctx.RegisterSyntaxNodeAction(ctx => {
			var node = (ElementAccessExpressionSyntax)ctx.Node;

			var mainTypeSymbol = ctx.SemanticModel.Compilation.GetTypeByMetadataName("Terraria.Main");
			if (mainTypeSymbol is null)
				return;

			var mainPlayerMember = mainTypeSymbol.GetMembers("player").FirstOrDefault();
			if (mainPlayerMember is null)
				return;

			var mainMyPlayerMember = mainTypeSymbol.GetMembers("myPlayer").FirstOrDefault();
			if (mainMyPlayerMember is null)
				return;

			var expSymbol = ctx.SemanticModel.GetSymbolInfo(node.Expression).Symbol;
			if (expSymbol is null)
				return;

			if (!SymbolEqualityComparer.Default.Equals(expSymbol, mainPlayerMember))
				return;

			var argExp = node.ArgumentList.Arguments.FirstOrDefault()?.Expression;
			if (argExp is null)
				return;

			var argSymbol = ctx.SemanticModel.GetSymbolInfo(argExp).Symbol;
			if (argSymbol is not null && SymbolEqualityComparer.Default.Equals(argSymbol, mainMyPlayerMember)) {
				ctx.ReportDiagnostic(Diagnostic.Create(
					Diagnostics.SimplifyLocalPlayer,
					node.GetLocation(),
					[node.ToString()]
				));
			}
		}, SyntaxKind.ElementAccessExpression);

	}
}
