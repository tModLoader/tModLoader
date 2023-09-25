using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using static tModCodeAssist.Constants;
using MyResources = tModCodeAssist.IDType.IDTypeResources;

namespace tModCodeAssist.IDType;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed partial class IDTypeDiagnosticAnalyzer : AbstractDiagnosticAnalyzer
{
	public const string IDFullyQualifiedNameProperty = "IDFullyQualifiedName";
	public const string IDLiteralProperty = "IDLiteral";

	private static readonly LocalizableString Title = new LocalizableResourceString(nameof(MyResources.Title), MyResources.ResourceManager, typeof(MyResources));
	private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(MyResources.MessageFormat), MyResources.ResourceManager, typeof(MyResources));
	private static readonly LocalizableString Description = new LocalizableResourceString(nameof(MyResources.Description), MyResources.ResourceManager, typeof(MyResources));
	public static readonly DiagnosticDescriptor Rule = new(DiagnosticIDs.IDType, Title, MessageFormat, Categories.Maintainability, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

	private static readonly LocalizableString BadTitle = new LocalizableResourceString(nameof(MyResources.BadTitle), MyResources.ResourceManager, typeof(MyResources));
	private static readonly LocalizableString BadMessageFormat = new LocalizableResourceString(nameof(MyResources.BadMessageFormat), MyResources.ResourceManager, typeof(MyResources));
	private static readonly LocalizableString BadDescription = new LocalizableResourceString(nameof(MyResources.BadDescription), MyResources.ResourceManager, typeof(MyResources));
	public static readonly DiagnosticDescriptor BadRule = new(DiagnosticIDs.BadIDType, BadTitle, BadMessageFormat, Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true, description: BadDescription);

	static IDTypeDiagnosticAnalyzer()
	{
		PopulateIDSets();
	}

	public IDTypeDiagnosticAnalyzer() : base(Rule, BadRule)
	{
	}

	protected sealed override void InitializeWorker(AnalysisContext context)
	{
		// TEMPORARILY DISABLED
		// item.type = 5 (-> item.type = ItemID.Mushroom)
		//context.RegisterOperationAction(AnalyzeAssignment, OperationKind.SimpleAssignment);

		// AddTile(18) (-> AddTile(TileID.WorkBenches))
		context.RegisterOperationAction(AnalyzeArgument_MagicNumber, OperationKind.Literal, OperationKind.FieldReference, OperationKind.PropertyReference);

		// item.type == 5 (-> item.type == ItemID.Mushroom)
		context.RegisterOperationAction(AnalyzeComparison_MagicNumber, OperationKind.Binary);

		// switch (item.type) {
		//	case 5: (-> case ItemID.Mushroom:)
		//		break;
		// }
		context.RegisterSyntaxNodeAction(AnalyzeSwitchLabel_MagicNumber, SyntaxKind.SwitchStatement);
	}

	private static bool TryGetIDAssociatedName(ISymbol symbol, out string associatedName)
	{
		associatedName = default;

		// Try find IDType attribute
		var argumentAttributes = symbol.GetAttributes();
		var idTypeAttributeData = argumentAttributes.FirstOrDefault(static x => x.AttributeClass.IsSameAsFullyQualifiedString(IDTypeAttributeFullyQualifiedName));

		// If wasn't found then skip
		if (idTypeAttributeData is null)
			return false;

		associatedName = (string)idTypeAttributeData.ConstructorArguments[0].Value;

		return true;
	}

	private static void AnalyzeArgument_MagicNumber(OperationAnalysisContext context)
	{
		// Ignore compiler-generated literals.
		if (context.Operation.IsImplicit)
			return;

		var argumentOperation = default(IArgumentOperation);

		// Check if operation is from arguments.
		for (var parentOperation = context.Operation.Parent; parentOperation != null; parentOperation = parentOperation.Parent) {
			if (parentOperation is IArgumentOperation) {
				argumentOperation = (IArgumentOperation)parentOperation;
				break;
			}
		}

		// Skip if it isn't.
		if (argumentOperation is null)
			return;

		// Try find IDType attribute
		if (!TryGetIDAssociatedName(argumentOperation.Parameter, out string idTypeName)) {
			return;
		}

		// A magic number
		if (context.Operation is ILiteralOperation) {
			AnalyzeLiteral(context, idTypeName);
		}
		// ItemID.Something
		else if (context.Operation is IMemberReferenceOperation or IParameterReferenceOperation) {
			AnalyzeID(context, idTypeName);
		}
		else {
			Debug.Fail($"Somehow, operation kind was: {context.Operation.Kind}");
		}

		static void AnalyzeLiteral(OperationAnalysisContext context, string idTypeName)
		{
			var operation = context.Operation;
			int idNumber = Convert.ToInt32(operation.ConstantValue.Value);

			if (idSets.TryGetValue(idTypeName, out var idSet) && idSet.Constants.TryGetName(idNumber, out string idLiteral)) {
				var properties = ImmutableDictionary.CreateBuilder<string, string>();
				properties[IDFullyQualifiedNameProperty] = idSet.FullyQualifiedName;
				properties[IDLiteralProperty] = idLiteral;

				context.ReportDiagnostic(Diagnostic.Create(Rule, operation.Syntax.GetLocation(), properties.ToImmutable()));
			}
		}

		static void AnalyzeID(OperationAnalysisContext context, string idTypeName)
		{
			var operation = context.Operation;

			var symbol = default(ISymbol);
			if (operation is IMemberReferenceOperation memberReferenceOperation) {
				symbol = memberReferenceOperation.Member;
			}
			else if (operation is IParameterReferenceOperation parameterReferenceOperation) {
				symbol = parameterReferenceOperation.Parameter;
			}

			var containingType = symbol.ContainingType;
			string fullyQualfieidTypeName = containingType.ToErrorFormatString();

			if (!idSetNameByFullName.TryGetValue(fullyQualfieidTypeName, out string associatedContainingType)) {
				return;
			}

			// If they match, we can just skip.
			if (associatedContainingType.Equals(idTypeName)) {
				return;
			}

			context.ReportDiagnostic(Diagnostic.Create(BadRule, operation.Syntax.GetLocation()));
		}
	}

	private static void AnalyzeComparison_MagicNumber(OperationAnalysisContext context)
	{
		var operation = (IBinaryOperation)context.Operation;

		var leftOperand = default(IOperation);
		var rightOperand = default(IOperation);

		for (leftOperand = operation.LeftOperand; leftOperand.IsImplicit;) {
			leftOperand = leftOperand.ChildOperations.First();
		}

		for (rightOperand = operation.RightOperand; rightOperand.IsImplicit;) {
			rightOperand = rightOperand.ChildOperations.First();
		}

		// ItemID.DirtBlock == 1
		if (rightOperand is ILiteralOperation) {
			// Must reference parameter or any member.
			if (leftOperand is not IParameterReferenceOperation and not IMemberReferenceOperation)
				return;

			var left = leftOperand;
			var right = (ILiteralOperation)rightOperand;

			var leftMember = GetMember(left);

			if (!idSetNameByFullName.TryGetValue(leftMember.ContainingType.ToErrorFormatString(), out string associatedName)) {
				TryGetIDAssociatedName(leftMember, out associatedName);
			}

			if (associatedName == null) {
				return;
			}

			int idNumber = Convert.ToInt32(right.ConstantValue.Value);

			if (idSets.TryGetValue(associatedName, out var idSet) && idSet.Constants.TryGetName(idNumber, out string idLiteral)) {
				var properties = ImmutableDictionary.CreateBuilder<string, string>();
				properties[IDFullyQualifiedNameProperty] = idSet.FullyQualifiedName;
				properties[IDLiteralProperty] = idLiteral;

				context.ReportDiagnostic(Diagnostic.Create(Rule, right.Syntax.GetLocation(), properties.ToImmutable()));
			}
		}
		// 1 == ItemID.DirtBlock
		else if (leftOperand is ILiteralOperation) {
			// Must reference parameter or any member.
			if (rightOperand is not IParameterReferenceOperation and not IMemberReferenceOperation)
				return;

			var left = (ILiteralOperation)leftOperand;
			var right = rightOperand;

			var rightMember = GetMember(right);

			if (!idSetNameByFullName.TryGetValue(rightMember.ContainingType.ToErrorFormatString(), out string associatedName)) {
				TryGetIDAssociatedName(rightMember, out associatedName);
			}

			if (associatedName == null) {
				return;
			}

			int idNumber = Convert.ToInt32(right.ConstantValue.Value);

			if (idSets.TryGetValue(associatedName, out var idSet) && idSet.Constants.TryGetName(idNumber, out string idLiteral)) {
				var properties = ImmutableDictionary.CreateBuilder<string, string>();
				properties[IDFullyQualifiedNameProperty] = idSet.FullyQualifiedName;
				properties[IDLiteralProperty] = idLiteral;

				context.ReportDiagnostic(Diagnostic.Create(Rule, left.Syntax.GetLocation(), properties.ToImmutable()));
			}
		}
		// ItemID.DirtBlock == TileID.DirtBlock
		// item.type == ItemID.DirtBlock
		else if (leftOperand is IMemberReferenceOperation or IParameterReferenceOperation && rightOperand is IMemberReferenceOperation or IParameterReferenceOperation) {
			var left = leftOperand;
			var right = rightOperand;

			var leftMember = GetMember(left);
			var rightMember = GetMember(right);

			if (!idSetNameByFullName.TryGetValue(leftMember.ContainingType.ToErrorFormatString(), out string leftAssociatedName)) {
				TryGetIDAssociatedName(leftMember, out leftAssociatedName);
			}

			if (!idSetNameByFullName.TryGetValue(rightMember.ContainingType.ToErrorFormatString(), out string rightAssociatedName)) {
				TryGetIDAssociatedName(rightMember, out rightAssociatedName);
			}

			if (leftAssociatedName == null || rightAssociatedName == null || leftAssociatedName.Equals(rightAssociatedName)) {
				return;
			}

			context.ReportDiagnostic(Diagnostic.Create(BadRule, operation.Syntax.GetLocation()));
		}

		static ISymbol GetMember(IOperation operation)
		{
			return operation is IParameterReferenceOperation
				? ((IParameterReferenceOperation)operation).Parameter
				: ((IMemberReferenceOperation)operation).Member;
		}
	}

	private static void AnalyzeAssignment(OperationAnalysisContext context)
	{
		var operation = (ISimpleAssignmentOperation)context.Operation;

		var target = operation.Target;
		var value = operation.Value;

		for (; target != null && target.IsImplicit;) {
			target = target.ChildOperations.FirstOrDefault();
		}

		for (; value != null && value.IsImplicit;) {
			value = value.ChildOperations.FirstOrDefault();
		}

		if (target is not IMemberReferenceOperation and not IParameterReferenceOperation) {
			return;
		}

		if (value is not ILiteralOperation and not IMemberReferenceOperation and not IParameterReferenceOperation) {
			return;
		}

		var targetSymbol = GetSymbol(target);
		var valueSymbol = GetSymbol(value);

		bool isValidToReport = false;

		if (!idSetNameByFullName.TryGetValue(targetSymbol.ContainingType.ToErrorFormatString(), out string leftAssociatedName)) {
			isValidToReport = TryGetIDAssociatedName(targetSymbol, out leftAssociatedName);
		}

		if (value is ILiteralOperation) {
			if (!idSets.TryGetValue(leftAssociatedName, out var idSet))
				return;

			if (!idSet.Constants.TryGetName(Convert.ToInt32(value.ConstantValue.Value), out string idLiteral)) {
				return;
			}

			var properties = ImmutableDictionary.CreateBuilder<string, string>();
			properties[IDFullyQualifiedNameProperty] = idSet.FullyQualifiedName;
			properties[IDLiteralProperty] = idLiteral;

			context.ReportDiagnostic(Diagnostic.Create(Rule, value.Syntax.GetLocation(), properties.ToImmutable()));
			return;
		}

		if (!idSetNameByFullName.TryGetValue(valueSymbol.ContainingType.ToErrorFormatString(), out string rightAssociatedName)) {
			isValidToReport &= TryGetIDAssociatedName(valueSymbol, out rightAssociatedName);
		}

		if (!isValidToReport || leftAssociatedName.Equals(rightAssociatedName)) {
			return;
		}

		context.ReportDiagnostic(Diagnostic.Create(BadRule, value.Syntax.GetLocation()));

		static ISymbol GetSymbol(IOperation operation)
		{
			if (operation is IMemberReferenceOperation memberReferenceOperation) {
				return memberReferenceOperation.Member;
			}
			else if (operation is IParameterReferenceOperation parameterReferenceOperation) {
				return parameterReferenceOperation.Parameter;
			}

			return null;
		}
	}

	private static void AnalyzeSwitchLabel_MagicNumber(SyntaxNodeAnalysisContext context)
	{
		var node = (SwitchStatementSyntax)context.Node;

		var expressionSymbol = context.SemanticModel.GetSymbolInfo(node.Expression, context.CancellationToken).Symbol;

		// Skip if not field
		if (expressionSymbol is not IFieldSymbol) {
			return;
		}

		if (!TryGetIDAssociatedName(expressionSymbol, out string idTypeName))
			return;

		foreach (var caseLabelNode in node.Sections.SelectMany(static x => x.Labels).Where(static x => x is CaseSwitchLabelSyntax).Select(static x => x as CaseSwitchLabelSyntax)) {
			var operation = context.SemanticModel.GetOperation(caseLabelNode.Value, context.CancellationToken);

			if (operation is not IFieldReferenceOperation and not ILiteralOperation) {
				continue;
			}

			int? idNumber = null;

			if (operation is ILiteralOperation) {
				idNumber = Convert.ToInt32(((ILiteralOperation)operation).ConstantValue.Value);
			}
			else if (operation is IFieldReferenceOperation) {
				if (!TryGetIDAssociatedName(((IFieldReferenceOperation)operation).Member, out string associatedName))
					continue;

				if (idTypeName.Equals(associatedName))
					continue;

				context.ReportDiagnostic(Diagnostic.Create(BadRule, caseLabelNode.Value.GetLocation()));
			}

			if (!idNumber.HasValue) {
				continue;
			}

			if (idSets.TryGetValue(idTypeName, out var idSet) && idSet.Constants.TryGetName(idNumber.Value, out string idLiteral)) {
				var properties = ImmutableDictionary.CreateBuilder<string, string>();
				properties[IDFullyQualifiedNameProperty] = idSet.FullyQualifiedName;
				properties[IDLiteralProperty] = idLiteral;

				context.ReportDiagnostic(Diagnostic.Create(Rule, caseLabelNode.Value.GetLocation(), properties.ToImmutable()));
			}
		}
	}
}
