using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Common.Systems
{
	// This class showcases the "variation" feature of the localization system.
	// Variation is a feature that allows a single text key to have several variant options. This is used in many non-English languages to support prefixes/adjectives adjusting their spelling to the gender of the item/noun. The wiki (https://github.com/tModLoader/tModLoader/wiki/Localization) has more information on this feature.
	// This feature is not limited to gendered item prefixes, variant text can be used to support alternate localizations or even metadata. This class demonstrates some potential ideas for novel uses of this feature.
	public class LanguageVariantSystem : ModSystem
	{
		// Since the "variation" feature is quite niche, this method is not called anywhere in the mod currently. Uncomment the call below to see the feature in action.
		public override void PostSetupContent() {
			//VariationFeatureShowcase();
		}

		// The examples shown here implement collective nouns ("flock of birds", "school of fish") and gendered text.
		// The gendered text example isn't really gendered text since English doesn't do that, but the example showcases how the feature works in other languages. See the Terraria localization files for items and prefixes in a gendered language to see how this feature is used in practice, as well as the code of Lang.GetPrefixedItemName.
		public void VariationFeatureShowcase() {
			var nouns = Language.FindAll(Lang.CreateDialogFilter("Mods.ExampleMod.VariationShowcase.Noun"));
			(string key, string variationKeyword, bool plural)[] messages = [
				("Mods.ExampleMod.VariationShowcase.LooksNiceMessage", variationKeyword: "Gender", plural: false),
				("Mods.ExampleMod.VariationShowcase.CollectiveNounMessage", variationKeyword: "Collective", plural: true)
			];
			foreach (var message in messages) {
				foreach (var noun in nouns) {
					string result = GetVariantMessageForNoun(message.key, noun.Key, message.variationKeyword, message.plural);
					Mod.Logger.Info($"VariationFeatureShowcase.cs: {result}");
				}
			}
		}

		public static string GetVariantMessageForNoun(string messageKey, string nounKey, string variationKeyword, bool plural) {
			LocalizedText nounText = Language.GetText(nounKey);
			LocalizedText messageText = Language.GetText(messageKey);
			string messageTextValue = messageText.Value;
			// The TryGetVariation method is used to retrieve the variant text for a given key and variation keyword.
			// In this case, we first look up the variant for the noun, the "Gender" or "Collective" value, and then use that variant to look up the variant for the message.
			if (Language.TryGetVariation(nounText.Key, variationKeyword, out var nounVariation) && Language.TryGetVariation(messageText.Key, nounVariation, out var messageVariation))
				messageTextValue = messageVariation;

			string pluralizedNoun = nounText.Format(!plural ? 1 : 2);
			return LocalizedText.Literal(messageTextValue).Format(pluralizedNoun);
		}
	}
}
