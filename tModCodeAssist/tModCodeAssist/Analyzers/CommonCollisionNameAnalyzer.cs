using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace tModCodeAssist.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CommonCollisionNameAnalyzer() : AbstractDiagnosticAnalyzer(Diagnostics.CommonCollisionName)
{
	protected override void InitializeWorker(AnalysisContext ctx)
	{
		ctx.RegisterSymbolAction(ctx => {
			var symbol = ctx.Symbol;
			if (symbol.MetadataName is not ("Main" or "Mod"
				or "Player" or "Item" or "NPC" or "Projectile" or "Gore" or "Dust" or "Entity" or "Liquid" or "Mount" or "Tile" or "Recipe"
				or "ModPlayer" or "ModItem" or "ModNPC" or "ModProjectile" or "ModGore" or "ModDust" or "ModType" or "ModMount" or "ModTile" or "ModWall"
			)) {
				return;
			}

			object[] args = [symbol.MetadataName, symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)];
			foreach (var location in symbol.Locations) {
				ctx.ReportDiagnostic(Diagnostic.Create(Diagnostics.CommonCollisionName, location, args));
			}
		}, SymbolKind.Namespace, SymbolKind.NamedType);
	}
}
