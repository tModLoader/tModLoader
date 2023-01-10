using System;
using Terraria.Localization;

namespace Terraria.ModLoader;

public interface ILocalizedModType : IModType
{
	public abstract string LocalizationCategory { get; }
}

public static class ILocalizedModTypeExtensions
{
	public static string GetLocalizationKey(this ILocalizedModType self, string suffix) =>
		self.Mod.GetLocalizationKey($"{self.LocalizationCategory}.{self.Name}.{suffix}");

	public static LocalizedText GetOrRegisterLocalization(this ILocalizedModType self, string suffix, Func<string> makeDefaultValue = null) =>
		Language.GetOrRegister(self.GetLocalizationKey(suffix), makeDefaultValue);

	public static string GetLocalizedValue(this ILocalizedModType self, string suffix) =>
		Language.GetTextValue(self.GetLocalizationKey(suffix));
}
