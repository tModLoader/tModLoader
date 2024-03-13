using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using static tModCodeAssist.Constants;

namespace tModCodeAssist.SimplifyUnifiedRandom;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SimplifyUnifiedRandomAnalyzer() : AbstractDiagnosticAnalyzer(Rule)
{
	public const string Id = nameof(SimplifyUnifiedRandom);

	public const string IsLeftParameter = "IsLeft";
	public const string NegateParameter = "Negate";

	private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.SimplifyUnifiedRandomTitle), Resources.ResourceManager, typeof(Resources));
	private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.SimplifyUnifiedRandomMessageFormat), Resources.ResourceManager, typeof(Resources));
	private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.SimplifyUnifiedRandomDescription), Resources.ResourceManager, typeof(Resources));
	public static readonly DiagnosticDescriptor Rule = new(Id, Title, MessageFormat, Categories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

	protected override void InitializeWorker(AnalysisContext ctx)
	{
		// Main.rand.Next(x) == 0 => Main.rand.NextBool(x)

		ctx.RegisterCompilationStartAction(static ctx => {
			var unifiedRandomTypeSymbol = ctx.Compilation.GetTypeByMetadataName(UnifiedRandomMetadataName);
			var nextMethodSymbol = unifiedRandomTypeSymbol?.GetMembers("Next").FirstOrDefault(MatchNextMethod);

			// Checking for whether or not `unifiedRandomTypeSymbol` is null is redundant.
			if (nextMethodSymbol is null)
				return;

			ctx.RegisterOperationAction(ctx => {
				// Currently, only equality and inequality are supported.
				if (ctx.Operation is not IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals } op)
					return;

				var leftMethodSymbol = (op.LeftOperand as IInvocationOperation)?.TargetMethod;
				var rightMethodSymbol = (op.RightOperand as IInvocationOperation)?.TargetMethod;

				bool isLeft = SymbolEqualityComparer.Default.Equals(leftMethodSymbol, nextMethodSymbol);
				bool isRight = SymbolEqualityComparer.Default.Equals(rightMethodSymbol, nextMethodSymbol);

				// One of operands must be `UnifiedRandom.Next(Int32)`, but never both of them.
				// This check can realistically be optimized to one xor.
				if (!((isLeft || isRight) && !SymbolEqualityComparer.Default.Equals(leftMethodSymbol, rightMethodSymbol)))
					return;

				bool isNegated = op.OperatorKind is BinaryOperatorKind.NotEquals;

				// Store the `UnifiedRandom.Next(Int32)` operand.
				var nextMethodOperand = isLeft ? op.LeftOperand : op.RightOperand;

				// Store the other one.
				var nonMethodOperand = isLeft ? op.RightOperand : op.LeftOperand;

				if (nonMethodOperand is not (IInvocationOperation or IMemberReferenceOperation or { ConstantValue.HasValue: true }))
					return;

				var nextMethodOperandSyntax = nextMethodOperand.Syntax;
				var nonMethodOperandSyntax = nonMethodOperand.Syntax;

				// It's time to report!
				var properties = new DiagnosticProperties();

				if (isLeft)
					properties.Add(IsLeftParameter);

				if (isNegated)
					properties.Add(NegateParameter);

				ctx.ReportDiagnostic(Diagnostic.Create(Rule, op.Syntax.GetLocation(), properties));
			}, OperationKind.Binary);
		});

		static bool MatchNextMethod(ISymbol symbol)
		{
			if (symbol is not IMethodSymbol methodSymbol)
				return false;

			return methodSymbol is { Parameters: [{ Type.SpecialType: SpecialType.System_Int32 }] };
		}
	}
}
