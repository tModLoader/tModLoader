using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using static tModCodeAssist.Constants;

namespace tModCodeAssist.ChangeMagicNumberToID;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ChangeMagicNumberToIDAnalyzer() : AbstractDiagnosticAnalyzer(Rule)
{
	public const string Id = nameof(ChangeMagicNumberToID);

	public const string IdClassParameter = "IdClass";
	public const string NameParameter = "Name";

	private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.ChangeMagicNumberToIDTitle), Resources.ResourceManager, typeof(Resources));
	private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.ChangeMagicNumberToIDMessageFormat), Resources.ResourceManager, typeof(Resources));
	private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.ChangeMagicNumberToIDDescription), Resources.ResourceManager, typeof(Resources));
	public static readonly DiagnosticDescriptor Rule = new(Id, Title, MessageFormat, Categories.Maintenance, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

	protected override void InitializeWorker(AnalysisContext ctx)
	{
		ctx.RegisterCompilationStartAction(static ctx => {
			var dataEntries = ReadDataEntries(ctx);
			
			var compilation = ctx.Compilation;
			var attributeSymbol = compilation.GetTypeByMetadataName(AssociatedIdTypeAttributeMetadataName);

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

			bool HasAssociatedIdType(ISymbol symbol, out string idClassMetadataName, out IdDictionary search)
			{
				idClassMetadataName = null;
				search = null;

				foreach (var attributeData in symbol.GetAttributes()) {
					if (!SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, attributeSymbol))
						continue;

					var idTypeSymbol = (ISymbol)attributeData.ConstructorArguments[0].Value;

					string metadataName = idTypeSymbol.ToDisplayString(
						SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
					);

					if (!dataEntries.TryGetValue(metadataName, out var innerEntries))
						continue;

					idClassMetadataName = metadataName;
					search = innerEntries.FirstOrDefault()?.Search;

					return search != null;
				}

				{
					string metadataName = symbol.ContainingType.OriginalDefinition.ToDisplayString(
						SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
					);

					foreach (var dataEntry in dataEntries) {
						foreach (var innerEntry in dataEntry.Value) {
							if (innerEntry.MetadataName != metadataName)
								continue;

							if (!innerEntry.Members.ContainsKey(symbol.Name))
								continue;

							idClassMetadataName = innerEntry.IdClassName;
							search = innerEntry.Search;
							return true;
						}
					}
				}

				return false;
			}
		});
	}

	private static void TryReport(Action<Diagnostic> report, IOperation operation, string idClassMetadataName, IdDictionary search)
	{
		string name = search.GetName(Convert.ToInt64(operation.ConstantValue.Value));
		foreach (var child in operation.ChildOperations) {
			if (child is IMemberReferenceOperation { Member.Name: var memberName } && name == memberName) {
				return;
			}
		}

		var properties = new DiagnosticProperties();

		properties.Add(IdClassParameter, idClassMetadataName);
		properties.Add(NameParameter, name);

		report(Diagnostic.Create(Rule, operation.Syntax.GetLocation(), properties));
	}

	private static bool IsValidOperationType(IOperation operation)
	{
		return operation is IInvocationOperation or IMemberReferenceOperation;
	}

	private static bool IsValidOperationType(IOperation operation, out ISymbol target)
	{
		target = null;

		if (IsValidOperationType(operation)) {
			target = (operation as IInvocationOperation)?.TargetMethod ?? ((IMemberReferenceOperation)operation).Member;
		}

		return target != null;
	}

#pragma warning disable RS1012 // Start action has no registered actions
	private static Dictionary<string, ImmutableArray<DataEntry>> ReadDataEntries(CompilationStartAnalysisContext ctx)
	{
		var compilation = ctx.Compilation;

		var data = new Dictionary<string, ImmutableArray<DataEntry>>();
		var cacheSearch = new Dictionary<string, IdDictionary>();

		var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip };

		foreach (var additionalFile in ctx.Options.AdditionalFiles) {
			ctx.CancellationToken.ThrowIfCancellationRequested();

			string analyzerName = nameof(ChangeMagicNumberToID);
			string fileName = Path.GetFileNameWithoutExtension(additionalFile.Path);
			string extension = Path.GetExtension(additionalFile.Path);

			if (fileName is null || !fileName.StartsWith($"{analyzerName}", StringComparison.Ordinal) || extension is not ".json")
				continue;

			string srcText = additionalFile.GetText(ctx.CancellationToken)?.ToString();
			if (srcText is null)
				continue;

			string associatedIdClassType = fileName[(fileName.LastIndexOf('-') + 1)..];

			var dataEntries = JsonSerializer.Deserialize<DataEntry[]>(srcText, options);
			foreach (var dataEntry in dataEntries) {
				dataEntry.IdClassName = associatedIdClassType;

				if (cacheSearch.TryGetValue(dataEntry.IdClassName, out var search)) {
					dataEntry.Search = search;
				}
				else {
					dataEntry.Initialize(compilation, ctx.CancellationToken);

					cacheSearch[dataEntry.IdClassName] = dataEntry.Search;
				}
			}

			// Merge if have to.
			if (data.TryGetValue(associatedIdClassType, out var value)) {
				data[associatedIdClassType] = value.Concat(dataEntries).ToImmutableArray();
			}
			else {
				data[associatedIdClassType] = dataEntries.ToImmutableArray();
			}
		}

		return data;
	}
}
