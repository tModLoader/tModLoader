using System;
using Terraria.Localization;

namespace Terraria.ModLoader;

public interface ILocalizedModType : IModType
{
	/// <summary>
	/// The category used by this modded content for use in localization keys. Localization keys follow the pattern of "Mods.{ModName}.{Category}.{ContentName}.{DataName}". The <see href="https://github.com/tModLoader/tModLoader/wiki/Localization#modtype-and-ilocalizedmodtype">Localization wiki page</see> explains how custom <see cref="ModType"/> classes can utilize this.
	/// </summary>
	public abstract string LocalizationCategory { get; }
}

public static class ILocalizedModTypeExtensions
{
	/// <summary>
	/// Gets a suitable localization key belonging to this piece of content. <br/><br/>Localization keys follow the pattern of "Mods.{ModName}.{Category}.{ContentName}.{DataName}", in this case the <paramref name="suffix"/> corresponds to the DataName.
	/// </summary>
	/// <param name="self"></param>
	/// <param name="suffix"></param>
	/// <returns></returns>
	public static string GetLocalizationKey(this ILocalizedModType self, string suffix) =>
		self.Mod.GetLocalizationKey($"{self.LocalizationCategory}.{self.Name}.{suffix}");


	/// <summary>
	/// Returns a <see cref="LocalizedText"/> for this piece of content with the provided <paramref name="suffix"/>.
	/// <br/>If no existing localization exists for the key, it will be defined so it can be exported to a matching mod localization file.
	/// </summary>
	/// <param name="self"></param>
	/// <param name="suffix"></param>
	/// <param name="makeDefaultValue">A factory method for creating the default value, used to update localization files with missing entries</param>
	/// <returns></returns>
	public static LocalizedText GetLocalization(this ILocalizedModType self, string suffix, Func<string> makeDefaultValue = null) =>
		Language.GetOrRegister(self.GetLocalizationKey(suffix), makeDefaultValue);

	/// <summary>
	/// Retrieves the text value for a localization key belonging to this piece of content with the given <paramref name="suffix"/>. The text returned will be for the currently selected language.
	/// </summary>
	/// <param name="self"></param>
	/// <param name="suffix"></param>
	/// <returns></returns>
	public static string GetLocalizedValue(this ILocalizedModType self, string suffix) =>
		Language.GetTextValue(self.GetLocalizationKey(suffix));
}
