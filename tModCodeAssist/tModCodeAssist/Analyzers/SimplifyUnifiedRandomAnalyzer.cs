using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace tModCodeAssist.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SimplifyUnifiedRandomAnalyzer() : AbstractDiagnosticAnalyzer(Diagnostics.SimplifyUnifiedRandom)
{
	public readonly record struct Properties(in bool IsLeftConstant)
	{
		public static Properties FromImmutable(ImmutableDictionary<string, string> properties)
		{
			return new Properties(
				bool.Parse(properties["IsLeftConstant"])
			);
		}
		public ImmutableDictionary<string, string> ToImmutable()
		{
			var properties = ImmutableDictionary.CreateBuilder<string, string>();
			properties["IsLeftConstant"] = IsLeftConstant ? "true" : "false";
			return properties.ToImmutable();
		}
	}

	protected override void InitializeWorker(AnalysisContext ctx)
	{
		ctx.RegisterSyntaxNodeAction(ctx => {
			var node = (BinaryExpressionSyntax)ctx.Node;

			InvocationExpressionSyntax invocation;
			IMethodSymbol invocationSymbol;
			Optional<object> constant;
			bool isLeftConstant;

			if (node.Left.IsKind(SyntaxKind.NumericLiteralExpression)) {
				var rightSymbolInfo = ctx.SemanticModel.GetSymbolInfo(node.Right, ctx.CancellationToken);
				if (rightSymbolInfo.Symbol is not { })
					return;

				invocation = node.Right as InvocationExpressionSyntax;
				invocationSymbol = rightSymbolInfo.Symbol as IMethodSymbol;
				constant = ctx.SemanticModel.GetConstantValue(node.Left, ctx.CancellationToken);
				isLeftConstant = true;
			}
			else if (node.Right.IsKind(SyntaxKind.NumericLiteralExpression)) {
				var leftSymbolInfo = ctx.SemanticModel.GetSymbolInfo(node.Left, ctx.CancellationToken);
				if (leftSymbolInfo.Symbol is not { })
					return;

				invocation = node.Left as InvocationExpressionSyntax;
				invocationSymbol = leftSymbolInfo.Symbol as IMethodSymbol;
				constant = ctx.SemanticModel.GetConstantValue(node.Right, ctx.CancellationToken);
				isLeftConstant = false;
			}
			else {
				return;
			}

			if (!constant.HasValue)
				return;

			static bool Is_Next(IMethodSymbol symbol)
			{
				return symbol?.OriginalDefinition is {
					Name: "Next",
					ContainingSymbol: {
						Name: "UnifiedRandom",
						ContainingSymbol: {
							Name: "Utilities",
							ContainingSymbol: {
								Name: "Terraria",
								ContainingSymbol: INamespaceSymbol {
									IsNamespace: true,
									IsGlobalNamespace: true
								}
							}
						}
					}
				};
			}

			if (!Is_Next(invocationSymbol))
				return;

			int value = Convert.ToInt32(constant.Value);
			Debug.Assert(invocation.IsKind(SyntaxKind.InvocationExpression));

			if (invocationSymbol.Parameters.Length == 2) {
				// probably best not to try and simplify this, abort.
			}
			else if (invocationSymbol.Parameters.Length == 1) {
				if (value < 0)
					return;

				var operation = (IInvocationOperation)ctx.SemanticModel.GetOperation(invocation, ctx.CancellationToken);
				if (operation.Arguments[0] is { Value.ConstantValue: { HasValue: true, Value: var argumentConstant } }) {
					ulong argumentConstantValue = Convert.ToUInt64(argumentConstant);

					if ((ulong)value >= argumentConstantValue) {
						// Impossible to ever succeed.
						return;
					}

					object[] args = [node.ToString()];
					var properties = new Properties(isLeftConstant);

					ctx.ReportDiagnostic(Diagnostic.Create(
						Diagnostics.SimplifyUnifiedRandom,
						node.GetLocation(),
						properties.ToImmutable(),
						args
					));
				}
			}
		}, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression);

	}
}
