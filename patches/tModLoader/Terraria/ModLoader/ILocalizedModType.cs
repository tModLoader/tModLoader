using System;
using System.Runtime.CompilerServices;
using Terraria.Localization;

namespace Terraria.ModLoader;

public interface ILocalizedModType : IModType
{
	public abstract string Category { get; }
}

public static class ILocalizedModTypeExtensions
{
	// Idea: use [CallerMemberName]?
	public static string GetLocalizationKey(this ILocalizedModType self, string suffix) =>
		$"Mods.{self.Mod.Name}.{self.Category}.{self.Name}.{suffix}";

	public static LocalizedText GetOrAddLocalization(this ILocalizedModType self, string suffix, Func<string> makeDefaultValue = null) =>
		LanguageManager.Instance.GetOrAddText(self.GetLocalizationKey(suffix), makeDefaultValue);
}
