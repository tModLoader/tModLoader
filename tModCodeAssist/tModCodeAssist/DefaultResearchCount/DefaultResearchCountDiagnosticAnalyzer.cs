using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using static tModCodeAssist.Constants;
using MyResources = tModCodeAssist.DefaultResearchCount.DefaultResearchCountResources;

namespace tModCodeAssist.DefaultResearchCount;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DefaultResearchCountDiagnosticAnalyzer : AbstractDiagnosticAnalyzer
{
	private static readonly LocalizableString Title = new LocalizableResourceString(nameof(MyResources.Title), MyResources.ResourceManager, typeof(MyResources));
	private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(MyResources.MessageFormat), MyResources.ResourceManager, typeof(MyResources));
	private static readonly LocalizableString Description = new LocalizableResourceString(nameof(MyResources.Description), MyResources.ResourceManager, typeof(MyResources));
	public static readonly DiagnosticDescriptor Rule = new(DiagnosticIDs.DefaultResearchCount, Title, MessageFormat, Categories.Usage, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);

	public DefaultResearchCountDiagnosticAnalyzer() : base(Rule)
	{
	}

	protected sealed override void InitializeWorker(AnalysisContext context)
	{
		context.RegisterOperationAction(static (context) => {
			var operation = (ISimpleAssignmentOperation)context.Operation;

			if (operation.Target is not IPropertyReferenceOperation target) {
				return;
			}

			if (operation.Value is not ILiteralOperation { ConstantValue: { HasValue: true, Value: 1 } }) {
				return;
			}

			var propertySymbol = target.Property;

			if (propertySymbol.ContainingType.IsSameAsFullyQualifiedString(ItemFullyQualifiedName) && propertySymbol.Name.Equals(ResearchUnlockCountPropertyName)) {
				context.ReportDiagnostic(Diagnostic.Create(Rule, operation.Syntax.GetLocation()));
			}
		}, OperationKind.SimpleAssignment);
	}
}
