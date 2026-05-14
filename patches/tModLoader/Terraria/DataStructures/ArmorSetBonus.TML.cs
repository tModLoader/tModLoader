using Terraria.Localization;

namespace Terraria.DataStructures;

public partial class ArmorSetBonus
{
	public partial class Builder
	{
		private LocalizedText LocalizedText;
		private string Identifier;

		public Builder(ArmorSetEffect effect, LocalizedText localizedText, PartType primaryPart, string identifier)
		{
			Effect = effect;
			LocalizedText = localizedText;
			PrimaryPart = primaryPart;
			Identifier = identifier;
		}
	}

	/// <summary>
	/// Identifies an armor effect. Not unique. Armor sets with multiple options will create multiple ArmorSetEffect that share the same Key.
	/// </summary>
	public string Identifier { get; internal set; }

	public static Builder Create(ArmorSetEffect Effect, LocalizedText LocalizedText, PartType PrimaryPart = PartType.None, string Identifier = null) => new Builder(Effect, LocalizedText, PrimaryPart, Identifier);
}
