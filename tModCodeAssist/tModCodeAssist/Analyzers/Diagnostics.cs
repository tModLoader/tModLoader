using Microsoft.CodeAnalysis;

namespace tModCodeAssist.Analyzers;

public static class Diagnostics
{
	public static class Categories
	{
		public const string Maintenance = nameof(Maintenance);
		public const string Readability = nameof(Readability);
	}

	public static readonly DiagnosticDescriptor ChangeMagicNumberToID = new(
		id: nameof(ChangeMagicNumberToID),
		title: CreateResourceString(nameof(Resources.ChangeMagicNumberToIDTitle)),
		messageFormat: CreateResourceString(nameof(Resources.ChangeMagicNumberToIDMessageFormat)),
		description: CreateResourceString(nameof(Resources.ChangeMagicNumberToIDDescription)),
		category: Categories.Maintenance,
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor SimplifyUnifiedRandom = new(
		id: nameof(SimplifyUnifiedRandom),
		title: CreateResourceString(nameof(Resources.SimplifyUnifiedRandomTitle)),
		messageFormat: CreateResourceString(nameof(Resources.SimplifyUnifiedRandomMessageFormat)),
		description: CreateResourceString(nameof(Resources.SimplifyUnifiedRandomDescription)),
		category: Categories.Readability,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true
	);

	private static LocalizableResourceString CreateResourceString(string nameOfLocalizableResource)
	{
		return new LocalizableResourceString(nameOfLocalizableResource, Resources.ResourceManager, typeof(Resources));
	}
}
