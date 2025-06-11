using System;

namespace Terraria.Localization;

/// <summary>
/// Contains methods to access or retrieve localization values. The <see href="https://github.com/tModLoader/tModLoader/wiki/Localization">Localization Guide</see> teaches more about localization.
/// </summary>
public static partial class Language
{
	/// <summary>
	/// Returns a <see cref="LocalizedText"/> for a given key.
	/// <br/>If no existing localization exists for the key, it will be defined so it can be exported to a matching mod localization file.
	/// </summary>
	/// <param name="key">The localization key</param>
	/// <param name="makeDefaultValue">A factory method for creating the default value, used to update localization files with missing entries</param>
	/// <returns></returns>
	public static LocalizedText GetOrRegister(string key, Func<string> makeDefaultValue = null) => LanguageManager.Instance.GetOrRegister(key, makeDefaultValue);

	[Obsolete("Pass mod.GetLocalizationKey(key) directly")]
	public static LocalizedText GetOrRegister(Terraria.ModLoader.Mod mod, string key, Func<string> makeDefaultValue = null) => GetOrRegister(mod.GetLocalizationKey(key), makeDefaultValue);
}
