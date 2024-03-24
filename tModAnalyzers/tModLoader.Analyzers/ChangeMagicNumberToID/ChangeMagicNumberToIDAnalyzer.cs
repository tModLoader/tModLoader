using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using ReLogic.Reflection;
using static tModLoader.Analyzers.Constants;

namespace tModLoader.Analyzers.ChangeMagicNumberToID;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ChangeMagicNumberToIDAnalyzer() : AbstractDiagnosticAnalyzer(Rule)
{
	public const string Id = nameof(ChangeMagicNumberToID);

	public const string IdClassParameter = "IdClass";
	public const string NamesParameter = "Names";

	private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.ChangeMagicNumberToIDTitle), Resources.ResourceManager, typeof(Resources));
	private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.ChangeMagicNumberToIDMessageFormat), Resources.ResourceManager, typeof(Resources));
	private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.ChangeMagicNumberToIDDescription), Resources.ResourceManager, typeof(Resources));
	public static readonly DiagnosticDescriptor Rule = new(Id, Title, MessageFormat, Categories.Maintenance, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

	protected override void InitializeWorker(AnalysisContext ctx)
	{
		/*
			item.type = 1;

					=>

			item.type = ItemID.IronPickaxe;
		 */
		ctx.RegisterOperationAction(ctx => {
			var op = (IAssignmentOperation)ctx.Operation;
			
			var left = op.Target;
			var right = op.Value;

			if (!right.ConstantValue.HasValue)
				return;

			if (!IsValidOperationType(left, out var target))
				return;

			if (!HasAssociatedIdType(ctx.Compilation, target, out string idClassMetadataName, out var search))
				return;

			TryReport(ctx.ReportDiagnostic, right, idClassMetadataName, search);
		}, OperationKind.SimpleAssignment);

		/*
			item.type == 1

					=>

			item.type == ItemID.IronPickaxe
		 */
		ctx.RegisterOperationAction(ctx => {
			var op = (IBinaryOperation)ctx.Operation;

			var memberSymbol = default(ISymbol);
			var constantOperation = default(IOperation);

			if (IsValidOperationType(op.LeftOperand, out var target) && op.RightOperand.ConstantValue.HasValue) {
				memberSymbol = target;
				constantOperation = op.RightOperand;
			}
			else if (IsValidOperationType(op.RightOperand, out target) && op.LeftOperand.ConstantValue.HasValue) {
				memberSymbol = target;
				constantOperation = op.LeftOperand;
			}
			else {
				return;
			}

			if (!HasAssociatedIdType(ctx.Compilation, memberSymbol, out string idClassMetadataName, out var search))
				return;

			TryReport(ctx.ReportDiagnostic, constantOperation, idClassMetadataName, search);
		}, OperationKind.Binary);

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
		ctx.RegisterOperationAction(ctx => {
			var op = (ISwitchOperation)ctx.Operation;

			if (!IsValidOperationType(op.Value, out var target))
				return;

			if (!HasAssociatedIdType(ctx.Compilation, target, out string idClassMetadataName, out var search))
				return;

			foreach (var caseOperation in op.Cases) {
				foreach (var clauseOperation in caseOperation.Clauses) {
					if (clauseOperation is ISingleValueCaseClauseOperation singleValueCaseClause && singleValueCaseClause.Value.ConstantValue.HasValue) {
						TryReport(ctx.ReportDiagnostic, singleValueCaseClause.Value, idClassMetadataName, search);
					}
					else if (clauseOperation is IRangeCaseClauseOperation rangeCaseClauseOperation) {
						if (rangeCaseClauseOperation.MinimumValue.ConstantValue.HasValue)
							TryReport(ctx.ReportDiagnostic, rangeCaseClauseOperation.MinimumValue, idClassMetadataName, search);

						if (rangeCaseClauseOperation.MaximumValue.ConstantValue.HasValue)
							TryReport(ctx.ReportDiagnostic, rangeCaseClauseOperation.MaximumValue, idClassMetadataName, search);
					}
				}
			}
		}, OperationKind.Switch);

		/*
			Foo(1);

					=>

			Foo(ItemID.IronPickaxe);
		 */
		ctx.RegisterOperationAction(ctx => {
			var op = ctx.Operation;

			if (op is not IObjectCreationOperation && !IsValidOperationType(op))
				return;

			var args = (op as IObjectCreationOperation)?.Arguments
				?? (op as IInvocationOperation)?.Arguments;

			foreach (var arg in args) {
				if (!arg.Value.ConstantValue.HasValue)
					continue;

				if (!HasAssociatedIdType(ctx.Compilation, arg.Parameter, out string idClassMetadataName, out var search))
					continue;

				TryReport(ctx.ReportDiagnostic, arg.Value, idClassMetadataName, search);
			}
		}, OperationKind.ObjectCreation, OperationKind.Invocation);
	}

	private static void TryReport(Action<Diagnostic> report, IOperation operation, string idClassMetadataName, IdDictionary search)
	{
		int id = Convert.ToInt32(operation.ConstantValue.Value);

		if (!search.TryGetNames(id, out var names))
			return;

		foreach (var child in operation.ChildOperations) {
			if (child is IMemberReferenceOperation { Member.Name: var memberName } && names.Contains(memberName)) {
				return;
			}
		}

		string formattedNamesForCodeFixer = string.Join(",", names);
		string formattedIdClassMetadataName = idClassMetadataName.Replace('+', '.');
		string readableNamesForUser = string.Join(", or ", names.Select(x => $"{formattedIdClassMetadataName}.{x}"));

		var properties = new DiagnosticProperties();

		properties.Add(IdClassParameter, idClassMetadataName);
		properties.Add(NamesParameter, formattedNamesForCodeFixer);

		report(Diagnostic.Create(
			Rule,
			operation.Syntax.GetLocation(),
			properties,
			[id, readableNamesForUser]
			));
	}

	private static bool IsValidOperationType(IOperation operation)
	{
		return operation is IInvocationOperation or IMemberReferenceOperation or IParameterReferenceOperation;
	}

	private static bool IsValidOperationType(IOperation operation, out ISymbol target)
	{
		target = null;

		if (IsValidOperationType(operation)) {
			target = (operation as IInvocationOperation)?.TargetMethod
				?? (operation as IMemberReferenceOperation)?.Member
				?? (operation as IParameterReferenceOperation)?.Parameter;
		}

		return target != null;
	}

	private static bool HasAssociatedIdType(Compilation compilation, ISymbol symbol, out string idClassMetadataName, out IdDictionary search)
	{
		string containigTypeMetadataName = ToMetadataName(symbol.ContainingType.OriginalDefinition);
		BuiltinData.Key formattedName;

		if (symbol is IParameterSymbol parameterSymbol) {
			var methodSymbol = (IMethodSymbol)parameterSymbol.ContainingSymbol;

			formattedName = new BuiltinData.Key(containigTypeMetadataName, methodSymbol.MetadataName, symbol.Name);

			// If no entry with parameter name exists, then format to the one without overload.
			if (!BuiltinData.ContainsKey(formattedName))
				formattedName = new BuiltinData.Key(containigTypeMetadataName, methodSymbol.Name, parameterSymbol.Ordinal);
		}
		else {
			formattedName = new BuiltinData.Key(containigTypeMetadataName, symbol.Name);
		}

		return LookGeneric(formattedName, out idClassMetadataName, out search)
			|| LookIntoAttributes(compilation, symbol, out idClassMetadataName, out search);

		static bool LookGeneric(BuiltinData.Key formattedName, out string idClassMetadataName, out IdDictionary search)
		{
			idClassMetadataName = null;
			search = null;

			if (BuiltinData.TryGetValue(formattedName, out var memberInfo)) {
				idClassMetadataName = memberInfo.IdClassMetadataName;
				search = memberInfo.Search;
				return true;
			}

			return false;
		}

		static bool LookIntoAttributes(Compilation compilation, ISymbol symbol, out string idClassMetadataName, out IdDictionary search)
		{
			idClassMetadataName = null;
			search = null;

			var attributes = symbol is IMethodSymbol methodSymbol ? methodSymbol.GetReturnTypeAttributes() : symbol.GetAttributes();
			var attributeSymbol = compilation.GetTypeByMetadataName(IDTypeAttribute1MetadataName);

			foreach (var attributeData in attributes) {
				if (!SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass.OriginalDefinition, attributeSymbol))
					continue;

				var idTypeSymbol = attributeData.AttributeClass.TypeArguments[0];
				string idTypeMetadataName = ToMetadataName(idTypeSymbol);

				if (!Searches.TryGetByMetadataName(idTypeMetadataName, out search))
					continue;

				idClassMetadataName = idTypeMetadataName;
				return true;
			}

			return false;
		}

		static string ToMetadataName(ISymbol symbol)
		{
			return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
		}
	}
}
