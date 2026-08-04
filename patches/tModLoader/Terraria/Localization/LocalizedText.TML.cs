using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
	/// Creates a <see cref="LocalizedText"/> with empty <see cref="Key"/> a given <see cref="Value"/> <br/>
	/// <b>Only use as a last resort to call an API that requires a LocalizedText with an unlocalizable value</b>
	/// </summary>
	public static LocalizedText Literal(string text) => new LocalizedText("", text);

	/// <summary>
	/// Returns the args used with <see cref="WithFormatArgs"/> or <see cref="WithPartialFormatArgs"/> to create this text, if any.
	/// </summary>
	public object[] BoundArgs { get; private set; }

	/// <summary>
	/// The number of args required by <see cref="Format(object[])"/>
	/// </summary>
	public int ArgCount => _value switch {
		VariableText variableText => variableText.PositionalArgCount,
		CompositeText compositeText => compositeText.ArgCount,
		_ => 0
	};

	private void ThrowInvalidLiteralOperation([CallerMemberName] string methodName = default)
	{
		throw new InvalidOperationException($"{methodName} on literal text \"{UnformattedValue}\"");
	}

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
	public LocalizedText WithFormatArgs(params object[] args)
	{
		if (args.Length < ArgCount)
			throw new ArgumentException($"The localization key:\n  \"{Key}\"\nwith a value of:\n  \"{UnformattedValue}\"\nrequires {ArgCount} args, but only {args.Length} were supplied:\n  [{string.Join(", ", args)}]\nUse WithPartialFormatArgs to supply the rest later.");

		return WithPartialFormatArgs(args);
	}

	/// <summary>
	/// Version of <see cref="WithFormatArgs"/> which supplies only the leading args, shifting the rest down. <br/>
	/// The unfilled placeholders are renumbered from 0, so <see cref="Format(object[])"/> or <see cref="ToNetworkText(object[])"/> on the result take only the args which are still missing. <br/>
	/// Eg <c>Language.GetText("Key").WithPartialFormatArgs("red")</c> turns "{0} and {1}" into "red and {0}"
	/// </summary>
	/// <param name="args">The leading substitution args</param>
	public LocalizedText WithPartialFormatArgs(params object[] args)
	{
		if (string.IsNullOrEmpty(Key)) ThrowInvalidLiteralOperation();
		return LanguageManager.Instance.BindFormatArgs(Key, [.. BoundArgs ?? [], .. args]);
	}

	/// <summary>
	/// Formats <paramref name="args"/> into the leading placeholders, shifting the rest down.
	/// </summary>
	private string FormatPartial(object[] args)
	{
		if (args.Length >= ArgCount)
			return Format(args);

		var padded = new object[ArgCount];
		Array.Copy(args, padded, args.Length);
		for (int i = args.Length; i < padded.Length; i++)
			padded[i] = new CompositeText.PlaceholderArg { Index = i - args.Length };

		return Format(padded);
	}

	internal void BindArgs(LocalizedText original, object[] args)
	{
		Debug.Assert(Key == original.Key);
		SetValue(original.FormatPartial(args));
		EnglishValue = original.EnglishValue; // keep the unformatted english value on all langs, for consistency, though we don't expect it to be used for anything other than HasValue
		BoundArgs = args;
	}
}
