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
		$"Mods.{self.Mod.Name}.{self.LocalizationCategory}.{self.Name}.{suffix}";

	public static LocalizedText GetOrAddLocalization(this ILocalizedModType self, string suffix, Func<string> makeDefaultValue = null) =>
		LanguageManager.Instance.GetOrAddText(self.GetLocalizationKey(suffix), makeDefaultValue);
}
