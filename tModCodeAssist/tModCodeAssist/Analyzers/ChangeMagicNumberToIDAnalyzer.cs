using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
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

			TryReportVariedDiagnostics(ctx.ReportDiagnostic, ctx.SemanticModel, node.Right, binding, ctx.CancellationToken);
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

			if (IsNumber(node.Right)) {
				if (ctx.SemanticModel.GetSymbolInfo(node.Left, ctx.CancellationToken).Symbol is not { } leftSymbol) return;
				if (!MagicNumberBindings.TryGetBinding(leftSymbol, out var binding)) return;

				TryReportVariedDiagnostics(ctx.ReportDiagnostic, ctx.SemanticModel, node.Right, binding, ctx.CancellationToken);
			}
			else if (IsNumber(node.Left)) {
				if (ctx.SemanticModel.GetSymbolInfo(node.Right, ctx.CancellationToken).Symbol is not { } rightSymbol) return;
				if (!MagicNumberBindings.TryGetBinding(rightSymbol, out var binding)) return;

				TryReportVariedDiagnostics(ctx.ReportDiagnostic, ctx.SemanticModel, node.Left, binding, ctx.CancellationToken);
			}
			else {
				MagicNumberBindings.Binding leftBinding = null, rightBinding = null;
				_ = ctx.SemanticModel.GetSymbolInfo(node.Left, ctx.CancellationToken).Symbol is { } leftSymbol && MagicNumberBindings.TryGetBinding(leftSymbol, out leftBinding);
				_ = ctx.SemanticModel.GetSymbolInfo(node.Right, ctx.CancellationToken).Symbol is { } rightSymbol && MagicNumberBindings.TryGetBinding(rightSymbol, out rightBinding);

				switch (leftBinding, rightBinding) {
					case (not null, not null):
						// TODO: report different types?
						break;
					case (null, not null):
						TryReportVariedDiagnostics(ctx.ReportDiagnostic, ctx.SemanticModel, node.Left, rightBinding, ctx.CancellationToken);
						break;
					case (not null, null):
						TryReportVariedDiagnostics(ctx.ReportDiagnostic, ctx.SemanticModel, node.Right, leftBinding, ctx.CancellationToken);
						break;
					case (null, null):
						break;
				}
			}
		}, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression, SyntaxKind.GreaterThanExpression, SyntaxKind.GreaterThanOrEqualExpression, SyntaxKind.LessThanExpression, SyntaxKind.LessThanOrEqualExpression);

		/*
			AddIngredient(1)

					=>

			AddIngredient(ItemID.IronPickaxe)
		 */
		ctx.RegisterSyntaxNodeAction(ctx => {
			var node = (InvocationExpressionSyntax)ctx.Node;

			if (ctx.SemanticModel.GetSymbolInfo(node, ctx.CancellationToken).Symbol as IMethodSymbol is not { } invokedMethodSymbol) return;
			if (!MagicNumberBindings.HasBindingsForSymbol(invokedMethodSymbol)) return;

			if (ctx.SemanticModel.GetOperation(node) is not IInvocationOperation invokeOperation)
				return;

			foreach (IArgumentOperation argument in invokeOperation.Arguments)
			{
				ctx.CancellationToken.ThrowIfCancellationRequested();

				if (argument.Parameter is null || argument.Syntax is not ArgumentSyntax argumentSyntax)
					continue;

				if (!MagicNumberBindings.TryGetBinding(argument.Parameter, out var binding))
					continue;

				TryReportVariedDiagnostics(ctx.ReportDiagnostic, ctx.SemanticModel, argumentSyntax.Expression, binding, ctx.CancellationToken);
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

			TryReportVariedDiagnostics(ctx.ReportDiagnostic, ctx.SemanticModel, node.Value, binding, ctx.CancellationToken);
		}, SyntaxKind.CaseSwitchLabel);

		/*
			ItemID.Sets.StaffMinionSlotsRequired[1309] = 2f;
				=>
			ItemID.Sets.StaffMinionSlotsRequired[ItemID.SlimeStaff] = 2f;
		*/
		ctx.RegisterSyntaxNodeAction(ctx => {
			var node = (ElementAccessExpressionSyntax)ctx.Node;

			var leftSymbolInfo = ctx.SemanticModel.GetSymbolInfo(node.Expression, ctx.CancellationToken);
			if (leftSymbolInfo.Symbol is not { } leftSymbol || !MagicNumberBindings.TryGetBinding(leftSymbol, out var binding))
				return;

			if (node.ArgumentList is not {
				RawKind: (int)SyntaxKind.BracketedArgumentList,
				Arguments: [{ Expression: var indexExpression }]
			})
				return;

			TryReportVariedDiagnostics(ctx.ReportDiagnostic, ctx.SemanticModel, indexExpression, binding, ctx.CancellationToken);
		}, SyntaxKind.ElementAccessExpression);
	}

	private static bool IsNumber(SyntaxNode node)
	{
		return node is { RawKind: (int)SyntaxKind.NumericLiteralExpression } or PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.UnaryMinusExpression, Operand.RawKind: (int)SyntaxKind.NumericLiteralExpression };
	}

	private void TryReportVariedDiagnostics(Action<Diagnostic> report, SemanticModel semanticModel, SyntaxNode constantNode, MagicNumberBindings.Binding binding, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		if (IsNumber(constantNode)) {
			var constant = semanticModel.GetConstantValue(constantNode, cancellationToken);
			if (!constant.HasValue)
				return;

			ReportDiagnostic(report, constantNode, binding, Convert.ToInt32(constant.Value));
		}
		else if (semanticModel.GetSymbolInfo(constantNode, cancellationToken) is { Symbol: var argumentSymbol } && argumentSymbol is IFieldSymbol { IsConst: true }) {
			var displayString = argumentSymbol.ContainingType.ToDisplayString();
			if (!displayString.StartsWith("Terraria.") || binding.FullIdType.Equals(displayString))
				return;

			ReportBadTypeDiagnostic(report, constantNode, binding);
		}
	}

	private void ReportDiagnostic(Action<Diagnostic> report, SyntaxNode literalNode, MagicNumberBindings.Binding binding, int id)
	{
		if (!binding.AllowNegativeIDs && id < 0) return;
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
