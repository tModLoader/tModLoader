using System;

namespace Terraria.Localization;

public static class Pluralization
{
	// https://www.unicode.org/cldr/charts/43/supplemental/language_plural_rules.html
	// implementations extracted from build of https://github.com/xyzsd/cldr-plural-rules
	// English, German, Italian, Spanish, Portuguese, French
	//   one, other
	// Russian, Polish
	//   one, few, many
	// Chinese
	//   other
	public static int CardinalPluralRule(GameCulture culture, int count)
	{
		int mod_i10 = count % 10;
		int mod_i100 = count % 100;
		static bool contains(int i, int a, int b) => i >= a && i <= b;

		switch (culture.LegacyId) {
			case (int)GameCulture.CultureName.Russian:
				// one, few, many
				// _C_RULE_13
				if (mod_i10 == 1 && mod_i100 != 11)
					return 0;

				if (contains(mod_i10, 2, 4) && !contains(mod_i100, 12, 14))
					return 1;

				return 2;

			case (int)GameCulture.CultureName.English:
			case (int)GameCulture.CultureName.German:
			case (int)GameCulture.CultureName.Italian:
			case (int)GameCulture.CultureName.Spanish:
			case (int)GameCulture.CultureName.Portuguese:
				// one, many
				return count == 1 ? 0 : 1;

			case (int)GameCulture.CultureName.French:
				// one, many
				return count == 0 || count == 1 ? 0 : 1;

			case (int)GameCulture.CultureName.Polish:
				// one, few, many
				if (count == 1)
					return 0;

				if (contains(mod_i10, 2, 4) && !contains(mod_i100, 12, 14))
					return 1;

				return 2;

			case (int)GameCulture.CultureName.Chinese:
			default:
				// Chinese
				// other
				return 0;
		}
	}

	public static string SelectPlural(string[] options, object value)
	{
		int count = Convert.ToInt32(value is IConvertible c ? c : value?.ToString());
		int rule = CardinalPluralRule(Language.ActiveCulture, count);
		return options[Math.Min(rule, options.Length - 1)];
	}
}
