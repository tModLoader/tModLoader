using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace tModCodeAssist.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CommonCollisionNamespaceAnalyzer() : AbstractDiagnosticAnalyzer(Diagnostics.CommonCollisionNamespace)
{
	protected override void InitializeWorker(AnalysisContext ctx)
	{
		ctx.RegisterSymbolAction(ctx => {
			var symbol = (INamespaceSymbol)ctx.Symbol;

			if (symbol.Name is "Main" or "Mod"
				or "Item" or "NPC" or "Projectile" or "Dust" or "Entity" or "Liquid" or "Mount" or "Tile"
				or "ModItem" or "ModNPC" or "ModProjectile" or "ModDust" or "ModType" or "ModMount" or "ModTile" or "ModWall"
			) {
				foreach (var location in symbol.Locations) {
					ctx.ReportDiagnostic(Diagnostic.Create(
						Diagnostics.CommonCollisionNamespace,
						location,
						[symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)]
					));
				}
			}
		}, SymbolKind.Namespace);
	}
}
