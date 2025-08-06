using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using tModCodeAssist.Bindings;

namespace tModCodeAssist.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ChangeMagicNumberToIDAnalyzer() : AbstractDiagnosticAnalyzer(Diagnostics.ChangeMagicNumberToID, Diagnostics.BadIDType)
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

	protected override void InitializeWorker(AnalysisContext ctx)
	{
		MagicNumberBindings.PopulateBindings();

		/*
			item.type = 1;

					=>

			item.type = ItemID.IronPickaxe;
		 */
		ctx.RegisterSyntaxNodeAction(ctx => {
			var node = (AssignmentExpressionSyntax)ctx.Node;

			var leftSymbolInfo = ctx.SemanticModel.GetSymbolInfo(node.Left, ctx.CancellationToken);
			if (leftSymbolInfo.Symbol is not { } leftSymbol || !MagicNumberBindings.TryGetBinding(leftSymbol, out var binding)) return;

			if (node.Right.IsKind(SyntaxKind.NumericLiteralExpression)) {
				var constant = ctx.SemanticModel.GetConstantValue(node.Right, ctx.CancellationToken);
				if (!constant.HasValue)
					return;

				int id = Convert.ToInt32(constant.Value);

				ReportDiagnostic(ctx.ReportDiagnostic, node.Right, binding, id);
			}
			else if (ctx.SemanticModel.GetSymbolInfo(node.Right, ctx.CancellationToken) is { Symbol: var rightSymbol } && rightSymbol is IFieldSymbol { IsConst: true }) {
				var displayString = rightSymbol.ContainingType.ToDisplayString();
				if (!displayString.StartsWith("Terraria.") || binding.FullIdType.Equals(displayString, StringComparison.InvariantCulture))
					return;

				ReportBadTypeDiagnostic(ctx.ReportDiagnostic, node.Right, binding);
			}
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

			MagicNumberBindings.Binding binding;
			SyntaxNode literalNode;
			Optional<object> constant;

			if (node.Left.IsKind(SyntaxKind.NumericLiteralExpression)) {
				var rightSymbolInfo = ctx.SemanticModel.GetSymbolInfo(node.Right, ctx.CancellationToken);
				if (rightSymbolInfo.Symbol is not { } rightSymbol || !MagicNumberBindings.TryGetBinding(rightSymbol, out binding))
					return;

				literalNode = node.Left;
				constant = ctx.SemanticModel.GetConstantValue(node.Left, ctx.CancellationToken);
			}
			else if (node.Right.IsKind(SyntaxKind.NumericLiteralExpression)) {
				var leftSymbolInfo = ctx.SemanticModel.GetSymbolInfo(node.Left, ctx.CancellationToken);
				if (leftSymbolInfo.Symbol is not { } leftSymbol || !MagicNumberBindings.TryGetBinding(leftSymbol, out binding))
					return;

				literalNode = node.Right;
				constant = ctx.SemanticModel.GetConstantValue(node.Right, ctx.CancellationToken);
			}
			else {
				var leftSymbolInfo = ctx.SemanticModel.GetSymbolInfo(node.Left, ctx.CancellationToken);
				var rightSymbolInfo = ctx.SemanticModel.GetSymbolInfo(node.Right, ctx.CancellationToken);

				var nodeToReport = default(SyntaxNode);
				ISymbol a = null, b = null;

				if (leftSymbolInfo.Symbol is IFieldSymbol { IsConst: true }) {
					nodeToReport = node.Left;
					a = rightSymbolInfo.Symbol;
					b = leftSymbolInfo.Symbol;
				}

				if (rightSymbolInfo.Symbol is IFieldSymbol { IsConst: true }) {
					nodeToReport = node.Right;
					a = leftSymbolInfo.Symbol;
					b = rightSymbolInfo.Symbol;
				}

				if (a != null && b != null) {
					if (!MagicNumberBindings.TryGetBinding(a, out binding))
						return;

					var displayString = b.ContainingType.ToDisplayString();
					if (!displayString.StartsWith("Terraria.") || binding.FullIdType.Equals(displayString, StringComparison.InvariantCulture))
						return;

					ReportBadTypeDiagnostic(ctx.ReportDiagnostic, nodeToReport, binding);
				}

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

			if (!MagicNumberBindings.TryGetBinding(invokedMethodSymbol, out _)) return;

			for (int i = 0; i < node.ArgumentList.Arguments.Count; i++) {
				var argument = node.ArgumentList.Arguments[i];
				if (argument.Expression.IsKind(SyntaxKind.NumericLiteralExpression)) {
					var argumentOperation = (IArgumentOperation)ctx.SemanticModel.GetOperation(argument, ctx.CancellationToken);
					if (!MagicNumberBindings.TryGetBinding(argumentOperation.Parameter, out var binding))
						continue;

					int id = Convert.ToInt32(argumentOperation.Value.ConstantValue.Value);

					ReportDiagnostic(ctx.ReportDiagnostic, argument.Expression, binding, id);
					break; // with the way bindings currently work, only 1 argument can be binded, thus immediately terminate loop once found.
				}
				else if (ctx.SemanticModel.GetSymbolInfo(argument.Expression, ctx.CancellationToken) is { Symbol: var rightSymbol } && rightSymbol is IFieldSymbol { IsConst: true }) {
					var argumentOperation = (IArgumentOperation)ctx.SemanticModel.GetOperation(argument, ctx.CancellationToken);
					if (!MagicNumberBindings.TryGetBinding(argumentOperation.Parameter, out var binding))
						continue;

					var displayString = rightSymbol.ContainingType.ToDisplayString();
					if (!displayString.StartsWith("Terraria.") || binding.FullIdType.Equals(displayString, StringComparison.InvariantCulture))
						return;

					ReportBadTypeDiagnostic(ctx.ReportDiagnostic, argument.Expression, binding);
				}
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
			if (!MagicNumberBindings.TryGetBinding(operatedSymbol, out var binding)) return;

			if (node.Value.IsKind(SyntaxKind.NumericLiteralExpression) && node.Value is LiteralExpressionSyntax literalExpressionSyntax) {
				int id = Convert.ToInt32(literalExpressionSyntax.Token.Value);

				ReportDiagnostic(ctx.ReportDiagnostic, node.Value, binding, id);
			}
			else if (ctx.SemanticModel.GetSymbolInfo(node.Value, ctx.CancellationToken) is { Symbol: var rightSymbol } && rightSymbol is IFieldSymbol { IsConst: true }) {
				var displayString = rightSymbol.ContainingType.ToDisplayString();
				if (!displayString.StartsWith("Terraria.") || binding.FullIdType.Equals(displayString, StringComparison.InvariantCulture))
					return;

				ReportBadTypeDiagnostic(ctx.ReportDiagnostic, node.Value, binding);
			}

		}, SyntaxKind.CaseSwitchLabel);
	}

	private void ReportDiagnostic(Action<Diagnostic> report, SyntaxNode literalNode, MagicNumberBindings.Binding binding, int id)
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

	private void ReportBadTypeDiagnostic(Action<Diagnostic> report, SyntaxNode expressionNode, MagicNumberBindings.Binding expected)
	{
		report(Diagnostic.Create(
			Diagnostics.BadIDType,
			expressionNode.GetLocation(),
			[expressionNode.ToString(), expected.ShortIdType]
		));
	}
}
