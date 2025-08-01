using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using ReLogic.Reflection;
using Terraria.ID;

namespace tModCodeAssist.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ChangeMagicNumberToIDAnalyzer() : AbstractDiagnosticAnalyzer(Diagnostics.ChangeMagicNumberToID)
{
	public readonly record struct Properties(in string ShortIdType, in string FullIdType, in string Name)
	{
		public static Properties FromImmutable(ImmutableDictionary<string, string> properties)
		{
			return new Properties(
				properties["ShortIdType"],
				properties["FullIdType"],
				properties["Name"]
			);
		}

		public ImmutableDictionary<string, string> ToImmutable()
		{
			var properties = ImmutableDictionary.CreateBuilder<string, string>();
			properties["ShortIdType"] = ShortIdType;
			properties["FullIdType"] = FullIdType;
			properties["Name"] = Name;
			return properties.ToImmutable();
		}
	}

	// TODO: Switch to attributes in source instead of using bindings
	#region Bindings
	private abstract class Binding(Binding.CreationContext context)
	{
		public readonly record struct CreationContext(
			in string OwningClassName,
			in string MemberName,
			in string ShortIdType,
			in string FullIdType,
			in IdDictionary Search
		);

		public string ShortIdType => context.ShortIdType;
		public string FullIdType => context.FullIdType;
		public IdDictionary Search => context.Search;
	}
	private sealed class FieldBinding(Binding.CreationContext context) : Binding(context)
	{
	}
	private sealed class MethodParameterBinding(Binding.CreationContext context, int parameterOrder) : Binding(context)
	{
		public int ParameterOrder => parameterOrder;
	}

	private Dictionary<string, Dictionary<string, Binding>> bindingByMemberByOwningClass;

	private void AddBinding(string owningClassName, string memberName, Func<Binding.CreationContext, Binding> func, string shortIdType, string fullIdType, IdDictionary search)
	{
		var context = new Binding.CreationContext(owningClassName, memberName, shortIdType, fullIdType, search);
		var binding = func(context);

		if (bindingByMemberByOwningClass.TryGetValue(owningClassName, out var bindingByMember)) {
			bindingByMember.Add(memberName, binding);
		}
		else {
			bindingByMemberByOwningClass[owningClassName] = new() { [memberName] = binding };
		}
	}

	private bool TryGetBinding(ISymbol symbol, out Binding binding)
	{
		binding = null;

		static string BuildQualifiedName(ISymbol symbol)
		{
			return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
		}

		if (symbol is IFieldSymbol fieldSymbol && bindingByMemberByOwningClass.TryGetValue(BuildQualifiedName(symbol.ContainingType), out Dictionary<string, Binding> bindingByMember)) {
			if (bindingByMember.TryGetValue(fieldSymbol.MetadataName, out binding)) {
				return true;
			}
		}
		else if (symbol is IPropertySymbol propertySymbol && bindingByMemberByOwningClass.TryGetValue(BuildQualifiedName(symbol.ContainingType), out bindingByMember)) {
			if (bindingByMember.TryGetValue(propertySymbol.MetadataName, out binding)) {
				return true;
			}
		}
		else if (symbol is IMethodSymbol methodSymbol && bindingByMemberByOwningClass.TryGetValue(BuildQualifiedName(symbol.ContainingType), out bindingByMember)) {
			var qualifiedName = methodSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat
				.WithMemberOptions(SymbolDisplayMemberOptions.None)
				.WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters)
				.WithParameterOptions(SymbolDisplayParameterOptions.None)
			);
			if (bindingByMember.TryGetValue(qualifiedName, out binding)) {
				return true;
			}

			qualifiedName = methodSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
				.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
				.WithMemberOptions(SymbolDisplayMemberOptions.IncludeParameters)
				.WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters)
				.WithParameterOptions(SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeExtensionThis)
			);
			if (bindingByMember.TryGetValue(qualifiedName, out binding)) {
				return true;
			}
		}
		else if (symbol is IParameterSymbol parameterSymbol) {
			if (TryGetBinding(parameterSymbol.ContainingSymbol, out binding) && parameterSymbol.Ordinal == ((MethodParameterBinding)binding).ParameterOrder) {
				return true;
			}
		}

		return false;
	}
	#endregion

	protected override void InitializeWorker(AnalysisContext ctx)
	{
		bindingByMemberByOwningClass = [];
		AddBinding("Terraria.Item", "createTile", (ctx) => new FieldBinding(ctx), nameof(TileID), typeof(TileID).FullName, TileID.Search);
		AddBinding("Terraria.Item", "type", (ctx) => new FieldBinding(ctx), nameof(ItemID), typeof(ItemID).FullName, ItemID.Search);
		AddBinding("Terraria.Player", "cursorItemIconID", (ctx) => new FieldBinding(ctx), nameof(ItemID), typeof(ItemID).FullName, ItemID.Search);
		AddBinding("Terraria.Item", "shoot", (ctx) => new FieldBinding(ctx), nameof(ProjectileID), typeof(ProjectileID).FullName, ProjectileID.Search);
		AddBinding("Terraria.Item", "useStyle", (ctx) => new FieldBinding(ctx), nameof(ItemUseStyleID), typeof(ItemUseStyleID).FullName, ItemUseStyleID.Search);
		AddBinding("Terraria.Item", "rare", (ctx) => new FieldBinding(ctx), nameof(ItemRarityID), typeof(ItemRarityID).FullName, ItemRarityID.Search);
		AddBinding("Terraria.NPC", "type", (ctx) => new FieldBinding(ctx), nameof(NPCID), typeof(NPCID).FullName, NPCID.Search);
		AddBinding("Terraria.Main", "netMode", (ctx) => new FieldBinding(ctx), nameof(NetmodeID), typeof(NetmodeID).FullName, NetmodeID.Search);

		AddBinding("Terraria.ModLoader.ModBlockType", "DustType", (ctx) => new FieldBinding(ctx), nameof(DustID), typeof(DustID).FullName, DustID.Search);

		AddBinding("Terraria.Item", "CloneDefaults", (ctx) => new MethodParameterBinding(ctx, 0), nameof(ItemID), typeof(ItemID).FullName, ItemID.Search);
		AddBinding("Terraria.NetMessage", "SendData", (ctx) => new MethodParameterBinding(ctx, 0), nameof(MessageID), typeof(MessageID).FullName, MessageID.Search);
		AddBinding("Terraria.Dust", "NewDust", (ctx) => new MethodParameterBinding(ctx, 3), nameof(DustID), typeof(DustID).FullName, DustID.Search);
		AddBinding("Terraria.Dust", "NewDustDirect", (ctx) => new MethodParameterBinding(ctx, 3), nameof(DustID), typeof(DustID).FullName, DustID.Search);

		AddBinding("Terraria.Recipe", "Create", (ctx) => new MethodParameterBinding(ctx, 0), nameof(ItemID), typeof(ItemID).FullName, ItemID.Search);
		AddBinding("Terraria.Recipe", "AddTile", (ctx) => new MethodParameterBinding(ctx, 0), nameof(TileID), typeof(TileID).FullName, TileID.Search);
		AddBinding("Terraria.Recipe", "AddIngredient", (ctx) => new MethodParameterBinding(ctx, 0), nameof(ItemID), typeof(ItemID).FullName, ItemID.Search);
		AddBinding("Terraria.Recipe", "HasResult", (ctx) => new MethodParameterBinding(ctx, 0), nameof(ItemID), typeof(ItemID).FullName, ItemID.Search);
		AddBinding("Terraria.ModLoader.Mod", "CreateRecipe", (ctx) => new MethodParameterBinding(ctx, 0), nameof(ItemID), typeof(ItemID).FullName, ItemID.Search);
		AddBinding("Terraria.Projectile", "NewProjectile(Terraria.DataStructures.IEntitySource, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, int, int, float, int, float, float, float)", (ctx) => new MethodParameterBinding(ctx, 3), nameof(ProjectileID), typeof(ProjectileID).FullName, ProjectileID.Search);
		AddBinding("Terraria.Projectile", "NewProjectile(Terraria.DataStructures.IEntitySource, float, float, float, float, int, int, float, int, float, float, float)", (ctx) => new MethodParameterBinding(ctx, 5), nameof(ProjectileID), typeof(ProjectileID).FullName, ProjectileID.Search);
		AddBinding("Terraria.Projectile", "NewProjectileDirect(Terraria.DataStructures.IEntitySource, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, int, int, float, int, float, float, float)", (ctx) => new MethodParameterBinding(ctx, 3), nameof(ProjectileID), typeof(ProjectileID).FullName, ProjectileID.Search);

		/*
			item.type = 1;

					=>

			item.type = ItemID.IronPickaxe;
		 */
		ctx.RegisterSyntaxNodeAction(ctx => {
			var node = (AssignmentExpressionSyntax)ctx.Node;

			var leftSymbolInfo = ctx.SemanticModel.GetSymbolInfo(node.Left, ctx.CancellationToken);
			if (leftSymbolInfo.Symbol is not { } leftSymbol || !TryGetBinding(leftSymbol, out var binding)) return;

			if (!node.Right.IsKind(SyntaxKind.NumericLiteralExpression)) return;

			var constant = ctx.SemanticModel.GetConstantValue(node.Right, ctx.CancellationToken);
			if (!constant.HasValue) return;

			int id = Convert.ToInt32(constant.Value);

			ReportDiagnostic(ctx.ReportDiagnostic, node.Right, binding, id);
		}, SyntaxKind.SimpleAssignmentExpression);

		/*
			item.type == 1
			item.type <= 1
			item.type > 1

					=>

			item.type == ItemID.IronPickaxe
			item.type <= ItemID.IronPickaxe
			item.type > ItemID.IronPickaxe
		 */
		ctx.RegisterSyntaxNodeAction(ctx => {
			var node = (BinaryExpressionSyntax)ctx.Node;

			Binding binding;
			SyntaxNode literalNode;
			Optional<object> constant;

			if (node.Left.IsKind(SyntaxKind.NumericLiteralExpression)) {
				var rightSymbolInfo = ctx.SemanticModel.GetSymbolInfo(node.Right, ctx.CancellationToken);
				if (rightSymbolInfo.Symbol is not { } rightSymbol || !TryGetBinding(rightSymbol, out binding))
					return;

				literalNode = node.Left;
				constant = ctx.SemanticModel.GetConstantValue(node.Left, ctx.CancellationToken);
			}
			else if (node.Right.IsKind(SyntaxKind.NumericLiteralExpression)) {
				var leftSymbolInfo = ctx.SemanticModel.GetSymbolInfo(node.Left, ctx.CancellationToken);
				if (leftSymbolInfo.Symbol is not { } leftSymbol || !TryGetBinding(leftSymbol, out binding))
					return;

				literalNode = node.Right;
				constant = ctx.SemanticModel.GetConstantValue(node.Right, ctx.CancellationToken);
			}
			else {
				return;
			}

			if (!constant.HasValue)
				return;

			int id = Convert.ToInt32(constant.Value);

			ReportDiagnostic(ctx.ReportDiagnostic, literalNode, binding, id);
		}, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression, SyntaxKind.GreaterThanExpression, SyntaxKind.GreaterThanOrEqualExpression, SyntaxKind.LessThanExpression, SyntaxKind.LessThanOrEqualExpression);

		/*
			AddIngredient(1)

					=>

			AddIngredient(ItemID.IronPickaxe)
		 */
		ctx.RegisterSyntaxNodeAction(ctx => {
			var node = (InvocationExpressionSyntax)ctx.Node;

			if (ctx.SemanticModel.GetSymbolInfo(node, ctx.CancellationToken).Symbol as IMethodSymbol is not { } invokedMethodSymbol) return;

			if (!TryGetBinding(invokedMethodSymbol, out _)) return;

			for (int i = 0; i < node.ArgumentList.Arguments.Count; i++) {
				var argument = node.ArgumentList.Arguments[i];
				if (!argument.Expression.IsKind(SyntaxKind.NumericLiteralExpression)) continue;
				
				var argumentOperation = (IArgumentOperation)ctx.SemanticModel.GetOperation(argument, ctx.CancellationToken);
				if (!TryGetBinding(argumentOperation.Parameter, out var binding)) continue;

				int id = Convert.ToInt32(argumentOperation.Value.ConstantValue.Value);

				ReportDiagnostic(ctx.ReportDiagnostic, argument.Expression, binding, id);
				break; // with the way bindings currently work, only 1 argument can be binded, thus immediately terminate loop once found.
			}
		}, SyntaxKind.InvocationExpression);

		// TODO: handle constructor arguments

		/*
			switch (item.type) {
				case 1:
					break;
			}

					=>
		
			switch (item.type) {
				case ItemID.IronPickaxe:
					break;
			}
		 */
		ctx.RegisterSyntaxNodeAction(ctx => {
			var node = (CaseSwitchLabelSyntax)ctx.Node;

			var operatedExpression = node.Parent;
			Debug.Assert(operatedExpression is SwitchSectionSyntax);
			operatedExpression = operatedExpression.Parent;
			Debug.Assert(operatedExpression is SwitchStatementSyntax);
			operatedExpression = ((SwitchStatementSyntax)operatedExpression).Expression;

			if (ctx.SemanticModel.GetSymbolInfo(operatedExpression, ctx.CancellationToken).Symbol is not { } operatedSymbol) return;
			if (!TryGetBinding(operatedSymbol, out var binding)) return;

			if (!node.Value.IsKind(SyntaxKind.NumericLiteralExpression) || node.Value is not LiteralExpressionSyntax literalExpressionSyntax)
				return;

			int id = Convert.ToInt32(literalExpressionSyntax.Token.Value);

			ReportDiagnostic(ctx.ReportDiagnostic, node.Value, binding, id);
		}, SyntaxKind.CaseSwitchLabel);
	}

	private void ReportDiagnostic(Action<Diagnostic> report, SyntaxNode literalNode, Binding binding, int id)
	{
		Debug.Assert(literalNode is LiteralExpressionSyntax);

		if (!binding.Search.ContainsId(id)) return;
		var literalName = binding.Search.GetName(id);

		object[] args = [id, $"{binding.ShortIdType}.{literalName}"];
		var properties = new Properties(
			binding.ShortIdType,
			binding.FullIdType,
			literalName
		);

		report(Diagnostic.Create(
			Diagnostics.ChangeMagicNumberToID,
			literalNode.GetLocation(),
			properties.ToImmutable(),
			args
		));
	}
}
