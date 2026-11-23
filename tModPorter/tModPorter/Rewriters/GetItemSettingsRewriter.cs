using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace tModPorter.Rewriters;

public class GetItemSettingsRewriter : BaseRewriter
{
	private const string SettingsType = "Terraria.GetItemSettings";

	public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
	{
		if (node.Identifier.ValueText != "InventoryUIToInventorySettings" || model.GetSymbolInfo(node).Symbol != null)
			return node;

		ExpressionSyntax access = node;
		if (node.Parent is MemberAccessExpressionSyntax memberAccess && memberAccess.Name == node) {
			if (model.GetTypeInfo(memberAccess.Expression).Type?.ToString() != SettingsType)
				return node;

			access = memberAccess;
		}
		else if (!model.LookupSymbols(node.SpanStart).Any(symbol => symbol.IsStatic && symbol.ContainingType?.ToString() == SettingsType)) {
			return node;
		}

		if (access.Parent is AssignmentExpressionSyntax assignment && assignment.Left == access ||
			access.Parent is ArgumentSyntax argument && argument.RefKindKeyword.RawKind != 0 ||
			access.Parent is ArgumentSyntax { Parent.Parent: InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "nameof" } } })
			return node.WithBlockComment("Note: Removed. Use default(GetItemSettings) for the preset value.");

		RegisterAction(access, n => DefaultExpression(UseType(SettingsType)).WithTriviaFrom(n));
		return node;
	}

	public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
	{
		if (base.VisitObjectCreationExpression(node) is ObjectCreationExpressionSyntax updated && updated != node)
			return updated;

		return node.WithArgumentList(RewriteArguments(node, node.ArgumentList));
	}

	public override SyntaxNode VisitImplicitObjectCreationExpression(ImplicitObjectCreationExpressionSyntax node)
	{
		if (base.VisitImplicitObjectCreationExpression(node) is ImplicitObjectCreationExpressionSyntax updated && updated != node)
			return updated;

		return node.WithArgumentList(RewriteArguments(node, node.ArgumentList));
	}

	private ArgumentListSyntax RewriteArguments(ExpressionSyntax node, ArgumentListSyntax argumentList)
	{
		if (argumentList == null || argumentList.Arguments.Count < 4 ||
			model.GetTypeInfo(node).Type?.ToString() != SettingsType || !IsInvalidOrObsolete(model.GetOperation(node)))
			return argumentList;

		var args = argumentList.Arguments;
		if (args[3].NameColon?.Name.Identifier.ValueText is not (null or "StepAfterHandlingSlotNormally"))
			return argumentList;

		var callbackType = model.Compilation.GetTypeByMetadataName("System.Action`1")?.Construct(model.Compilation.GetTypeByMetadataName("Terraria.Item"));
		if (callbackType == null || !args[3].Expression.IsKind(SyntaxKind.DefaultLiteralExpression) && !model.ClassifyConversion(args[3].Expression, callbackType).IsImplicit)
			return argumentList;

		if (args.Take(3).Any(arg => !model.ClassifyConversion(arg.Expression, model.Compilation.GetSpecialType(SpecialType.System_Boolean)).IsImplicit))
			return argumentList;

		if (args.Count > 5 || args.Count == 5 && (args[4].NameColon?.Name.Identifier.ValueText is not (null or "NoCoinMerge") ||
			!model.ClassifyConversion(args[4].Expression, model.Compilation.GetSpecialType(SpecialType.System_Boolean)).IsImplicit))
			return argumentList;

		if (args[3].NameColon == null)
			args = args.Replace(args[3], args[3].WithNameColon(NameColon("StepAfterHandlingSlotNormally").WithTrailingTrivia(Space)));
		if (args.Count == 5 && args[4].NameColon == null)
			args = args.Replace(args[4], args[4].WithNameColon(NameColon("NoCoinMerge").WithTrailingTrivia(Space)));

		return argumentList.WithArguments(args);
	}
}
