using Terraria.Localization;
using Terraria.ModLoader;

namespace Terraria.DataStructures;

public partial class ArmorSetBonus
{
	public partial class Builder
	{
		private LocalizedText LocalizedText;
		private string Identifier;

		public Builder(string identifier, LocalizedText localizedText, ArmorSetEffect effect, PartType primaryPart)
		{
			Effect = effect;
			LocalizedText = localizedText;
			PrimaryPart = primaryPart;
			Identifier = identifier;
		}

		/// <inheritdoc cref="Set(int, int, int)"/>
		public Builder Set<THead, TBody, TLegs>()
			where THead : ModItem
			where TBody : ModItem
			where TLegs : ModItem
		{
			return Set(ModContent.ItemType<THead>(), ModContent.ItemType<TBody>(), ModContent.ItemType<TLegs>());
		}
	}

	/// <summary>
	/// Identifies an armor effect. Not unique. Armor sets with multiple options will create multiple ArmorSetEffect that share the same Key.
	/// </summary>
	public string Identifier { get; internal set; }
}
