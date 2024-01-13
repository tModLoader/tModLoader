using System;
using System.Collections.Immutable;
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

	public const string Category = Categories.Maintainability;
	private static readonly LocalizableString Title = new LocalizableResourceString(nameof(MyResources.Title), MyResources.ResourceManager, typeof(MyResources));
	private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(MyResources.MessageFormat), MyResources.ResourceManager, typeof(MyResources));
	private static readonly LocalizableString Description = new LocalizableResourceString(nameof(MyResources.Description), MyResources.ResourceManager, typeof(MyResources));
	public static readonly DiagnosticDescriptor Rule = new(DiagnosticIDs.IDType, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

	public IDTypeDiagnosticAnalyzer() : base(Rule)
	{
	}

	protected sealed override void InitializeWorker(AnalysisContext context)
	{
		context.RegisterOperationAction(AnalyzeInvocationOperation, OperationKind.Literal);

		context.RegisterSyntaxNodeAction(AnalyzeAssignment, SyntaxKind.SimpleAssignmentExpression);
	}

	private static void AnalyzeInvocationOperation(OperationAnalysisContext context)
	{
		var operation = (ILiteralOperation)context.Operation;

		// Ignore compiler-generated literals.
		if (operation.IsImplicit)
			return;

		var argumentOperation = default(IArgumentOperation);

		// Check if literal is from arguments.
		for (var parentOperation = operation.Parent; parentOperation != null; parentOperation = parentOperation.Parent) {
			if (parentOperation is IArgumentOperation) {
				argumentOperation = (IArgumentOperation)parentOperation;
				break;
			}
		}

		// Skip if not.
		if (argumentOperation is null)
			return;

		// Try find IDType attribute
		var argumentAttributes = argumentOperation.Parameter.GetAttributes();
		var idTypeAttributeData = argumentAttributes.FirstOrDefault(static x => x.AttributeClass.IsSameAsFullyQualifiedString(IDTypeAttributeFullyQualifiedName));

		// If wasn't found then skip
		if (idTypeAttributeData is null)
			return;

		// Get IDType name and the magic number
		string idTypeName = (string)idTypeAttributeData.ConstructorArguments[0].Value;
		int idNumber = Convert.ToInt32(operation.ConstantValue.Value);

		if (idSets.TryGetValue(idTypeName, out var idSet) && idSet.Constants.TryGetName(idNumber, out string idLiteral)) {
			var properties = ImmutableDictionary.CreateBuilder<string, string>();
			properties[IDFullyQualifiedNameProperty] = idSet.FullyQualifiedName;
			properties[IDLiteralProperty] = idLiteral;

			context.ReportDiagnostic(Diagnostic.Create(Rule, operation.Syntax.GetLocation(), properties.ToImmutable()));
		}
	}

	private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
	{
		var node = (AssignmentExpressionSyntax)context.Node;

		if (!node.Right.IsKind(SyntaxKind.NumericLiteralExpression))
			return;

		var assignedSymbol = context.SemanticModel.GetSymbolInfo(node.Left, context.CancellationToken).Symbol;

		// Skip if not field or property
		if (assignedSymbol is not IFieldSymbol and not IPropertySymbol)
			return;

		var argumentAttributes = assignedSymbol.GetAttributes();
		var idTypeAttributeData = argumentAttributes.FirstOrDefault(static x => x.AttributeClass.IsSameAsFullyQualifiedString(IDTypeAttributeFullyQualifiedName));

		// Skip if no IDType attribute
		if (idTypeAttributeData == null)
			return;

		string idTypeName = (string)idTypeAttributeData.ConstructorArguments[0].Value;
		int idNumber = Convert.ToInt32(((LiteralExpressionSyntax)node.Right).Token.Value);

		if (idSets.TryGetValue(idTypeName, out var idSet) && idSet.Constants.TryGetName(idNumber, out string idLiteral)) {
			var properties = ImmutableDictionary.CreateBuilder<string, string>();
			properties[IDFullyQualifiedNameProperty] = idSet.FullyQualifiedName;
			properties[IDLiteralProperty] = idLiteral;

			context.ReportDiagnostic(Diagnostic.Create(Rule, node.Right.GetLocation(), properties.ToImmutable()));
		}
	}
}
