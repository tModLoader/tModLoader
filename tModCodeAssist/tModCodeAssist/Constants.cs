namespace tModCodeAssist;

public static class Constants
{
	public static class DiagnosticIDs
	{
		public const string IDType = nameof(IDType);
		public const string BadIDType = nameof(BadIDType);
		public const string DefaultResearchCount = nameof(DefaultResearchCount);
	}

	public static class Categories
	{
		public const string Maintainability = nameof(Maintainability);
		public const string Usage = nameof(Usage);
	}

	public const string IDTypeAttributeFullyQualifiedName = "global::Terraria.ModLoader.Annotations.IDTypeAttribute";

	public const string SystemRandomFullyQualifiedName = "global::System.Random";
	public const string ItemFullyQualifiedName = "global::Terraria.Item";

	public const string ResearchUnlockCountPropertyName = "ResearchUnlockCount";
}
