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
		isEnabledByDefault: true,
		helpLinkUri: "https://github.com/tModLoader/tModLoader/wiki/tModCodeAssist#changemagicnumbertoid"
	);

	public static readonly DiagnosticDescriptor BadIDType = new(
		id: nameof(BadIDType),
		title: CreateResourceString(nameof(Resources.BadIDTypeTitle)),
		messageFormat: CreateResourceString(nameof(Resources.BadIDTypeMessageFormat)),
		description: CreateResourceString(nameof(Resources.BadIDTypeDescription)),
		category: Categories.Maintenance,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri: "https://github.com/tModLoader/tModLoader/wiki/tModCodeAssist#badidtype"
	);

	public static readonly DiagnosticDescriptor CommonCollisionName = new(
		id: nameof(CommonCollisionName),
		title: CreateResourceString(nameof(Resources.CommonCollisionNameTitle)),
		messageFormat: CreateResourceString(nameof(Resources.CommonCollisionNameMessageFormat)),
		description: CreateResourceString(nameof(Resources.CommonCollisionNameDescription)),
		category: Categories.Maintenance,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri: "https://github.com/tModLoader/tModLoader/wiki/tModCodeAssist#commoncollisionname"
	);

	public static readonly DiagnosticDescriptor SimplifyUnifiedRandom = new(
		id: nameof(SimplifyUnifiedRandom),
		title: CreateResourceString(nameof(Resources.SimplifyUnifiedRandomTitle)),
		messageFormat: CreateResourceString(nameof(Resources.SimplifyUnifiedRandomMessageFormat)),
		description: CreateResourceString(nameof(Resources.SimplifyUnifiedRandomDescription)),
		category: Categories.Readability,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: "https://github.com/tModLoader/tModLoader/wiki/tModCodeAssist#simplifyunifiedrandom"
	);


	public static readonly DiagnosticDescriptor SimplifyLocalPlayer = new(
		id: nameof(SimplifyLocalPlayer),
		title: CreateResourceString(nameof(Resources.SimplifyLocalPlayerTitle)),
		messageFormat: CreateResourceString(nameof(Resources.SimplifyLocalPlayerMessageFormat)),
		description: CreateResourceString(nameof(Resources.SimplifyLocalPlayerDescription)),
		category: Categories.Readability,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: "https://github.com/tModLoader/tModLoader/wiki/tModCodeAssist#simplifylocalplayer"
	);

	private static LocalizableResourceString CreateResourceString(string nameOfLocalizableResource)
	{
		return new LocalizableResourceString(nameOfLocalizableResource, Resources.ResourceManager, typeof(Resources));
	}
}
