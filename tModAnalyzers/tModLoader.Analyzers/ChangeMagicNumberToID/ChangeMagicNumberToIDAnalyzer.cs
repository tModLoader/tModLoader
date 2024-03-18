using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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

	private static readonly JsonSerializerOptions DeserializerOptions = new JsonSerializerOptions {
		ReadCommentHandling = JsonCommentHandling.Skip,
		AllowTrailingCommas = true,
	};

	private static readonly SourceTextValueProvider<DataEntries> DeserializationProvider = new(sourceText => {
		var rawDatas = JsonSerializer.Deserialize<RawData[]>(sourceText.ToString(), DeserializerOptions);
		var data = new Dictionary<string, DataEntries.MemberInfo>();

		foreach (var rawData in rawDatas) {
			string associatedIdClass = rawData.IdClass;

			if (!Searches.TryGetByMetadataName(associatedIdClass, out var search))
				continue;

			foreach (var rawDataEntry in rawData.Data) {
				string metadataName = rawDataEntry.MetadataName;

				foreach (var rawMember in rawDataEntry.Members) {
					string formattedName = DataEntries.FormatName(metadataName, rawMember.Key, rawMember.Value.ParameterName);

					data[formattedName] = new DataEntries.MemberInfo(rawMember.Value, associatedIdClass, search);
				}
			}
		}

		return new DataEntries(data);
	});

	protected override void InitializeWorker(AnalysisContext ctx)
	{
		ctx.RegisterCompilationStartAction(ctx => {
			var dataEntries = ReadDataEntries(ctx);

			var compilation = ctx.Compilation;
			var attributeSymbol = compilation.GetTypeByMetadataName(AssociatedIdTypeAttributeMetadataName);

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

				if (!HasAssociatedIdType(target, out string idClassMetadataName, out var search))
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

				if (!HasAssociatedIdType(memberSymbol, out string idClassMetadataName, out var search))
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

				if (!HasAssociatedIdType(target, out string idClassMetadataName, out var search))
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

					if (!HasAssociatedIdType(arg.Parameter, out string idClassMetadataName, out var search))
						continue;

					TryReport(ctx.ReportDiagnostic, arg.Value, idClassMetadataName, search);
				}
			}, OperationKind.ObjectCreation, OperationKind.Invocation);

			bool HasAssociatedIdType(ISymbol symbol, out string idClassMetadataName, out IdDictionary search)
			{
				idClassMetadataName = null;
				search = null;

				string containigTypeMetadataName = ToMetadataName(symbol.ContainingType.OriginalDefinition);
				string formattedName;

				if (symbol is IParameterSymbol parameterSymbol) {
					var methodSymbol = (IMethodSymbol)parameterSymbol.ContainingSymbol;

					formattedName = DataEntries.FormatName(containigTypeMetadataName, FormatMethodNameWithArguments(methodSymbol), symbol.Name);

					// If no entry with specific overload exists, then format to the one without overload.
					if (!dataEntries.ContainsKey(formattedName))
						formattedName = DataEntries.FormatName(containigTypeMetadataName, methodSymbol.Name, symbol.Name);

					// TODO: Use custom SymbolDisplayFormat instead of... this.
					static string FormatMethodNameWithArguments(IMethodSymbol methodSymbol)
					{
						var sb = new StringBuilder(16);

						sb.Append(methodSymbol.MetadataName);

						sb.Append('(');

						for (int i = 0, c = methodSymbol.Parameters.Length; i < c; i++) {
							var param = methodSymbol.Parameters[i];

							sb.Append(param.RefKind switch {
								RefKind.Ref => "ref ",
								RefKind.Out => "out ",
								RefKind.In => "in ",
								_ => string.Empty
							});

							sb.Append(param.Type.MetadataName);

							if (i != c - 1) {
								sb.Append(", ");
							}
						}

						sb.Append(')');

						return sb.ToString();
					}
				}
				else {
					formattedName = DataEntries.FormatName(containigTypeMetadataName, symbol.Name);
				}

				return LookGeneric(symbol, out idClassMetadataName, out search)
					|| LookIntoAttributes(symbol, out idClassMetadataName, out search);

				bool LookGeneric(ISymbol symbol, out string idClassMetadataName, out IdDictionary search)
				{
					idClassMetadataName = null;
					search = null;

					if (dataEntries.TryGetValue(formattedName, out var memberInfo)) {
						if (symbol is IMethodSymbol && !memberInfo.Target.HasFlag(AttributeTargets.ReturnValue))
							return false;

						idClassMetadataName = memberInfo.IdClassMetadataName;
						search = memberInfo.Search;
						return true;
					}

					return false;
				}

				bool LookIntoAttributes(ISymbol symbol, out string idClassMetadataName, out IdDictionary search)
				{
					idClassMetadataName = null;
					search = null;

					var attributes = symbol is IMethodSymbol methodSymbol ? methodSymbol.GetReturnTypeAttributes() : symbol.GetAttributes();

					foreach (var attributeData in attributes) {
						if (!SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, attributeSymbol))
							continue;

						var idTypeSymbol = (ISymbol)attributeData.ConstructorArguments[0].Value;
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
		});
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

#pragma warning disable RS1012 // Start action has no registered actions
	private static DataEntries ReadDataEntries(CompilationStartAnalysisContext ctx)
	{
		foreach (var additionalFile in ctx.Options.AdditionalFiles) {
			ctx.CancellationToken.ThrowIfCancellationRequested();

			string name = Path.GetFileNameWithoutExtension(additionalFile.Path);
			string extension = Path.GetExtension(additionalFile.Path);

			if (name is not "ChangeMagicNumberToID.Data" || extension is not ".json")
				continue;

			var additionalFileText = additionalFile.GetText(ctx.CancellationToken);
			if (additionalFileText == null)
				continue;

			ctx.CancellationToken.ThrowIfCancellationRequested();

			if (!ctx.TryGetValue(additionalFileText, DeserializationProvider, out var dataEntries))
				continue;

			return dataEntries;
		}

		return default;
	}
}
