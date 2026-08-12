using System.Diagnostics;
using Terraria.ModLoader.Core;

namespace Terraria.Localization;

/// <summary>
/// Contains the localization value corresponding to a key for the current game language. Automatically updates as language, mods, and resource packs change. The <see href="https://github.com/tModLoader/tModLoader/wiki/Localization">Localization Guide</see> teaches more about localization.
/// </summary>
public partial class LocalizedText
{
	static LocalizedText()
	{
		TypeCaching.OnClear += _propertyLookupCache.Clear;
	}

	/// <summary>
	/// Returns the args used with <see cref="WithFormatArgs"/> to create this text, if any.
	/// </summary>
	public object[] BoundArgs { get; private set; }

	/// <summary>
	/// Creates a new LocalizedText with the supplied arguments formatted into the value (via <see cref="string.Format(string, object?[])"/>)<br/>
	/// Will automatically update to re-format the string with cached args when language changes. <br/>
	///<br/>
	/// The resulting LocalizedText should be stored statically. Should not be used to create 'throwaway' LocalizedText instances. <br/>
	/// Use <see cref="Format(object[])"/> instead for repeated on-demand formatting with different args.
	/// <br/> The <see href="https://github.com/tModLoader/tModLoader/wiki/Localization#string-formatting">Localization Guide</see> teaches more about using placeholders in localization.
	/// </summary>
	/// <param name="args">The substitution args</param>
	/// <returns></returns>
	public LocalizedText WithFormatArgs(params object[] args) => LanguageManager.Instance.BindFormatArgs(Key, args);

	internal void BindArgs(LocalizedText original, object[] args)
	{
		Debug.Assert(Key == original.Key);
		// TODO, consider if we should do partial binding, shifting the higher args down
		SetValue(original.Format(args));
		EnglishValue = original.EnglishValue; // keep the unformatted english value on all langs, for consistency, though we don't expect it to be used for anything other than HasValue
		BoundArgs = args;
	}
}
