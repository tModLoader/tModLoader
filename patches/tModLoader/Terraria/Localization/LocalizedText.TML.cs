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

	private bool TryFormat(VariableText variableText, Func<string, object> lookup, out string formatted)
	{
		try {
			return variableText.TryFormat(lookup, out formatted);
		}
		catch (Exception e) {
			ReportFormatException([], e);
			formatted = UnformattedValue;
			return true;
		}
	}

	private bool CheckConditionsMet(VariableText variableText, Func<string, object> lookup)
	{
		try {
			return variableText.ConditionsMet(lookup);
		}
		catch (Exception e) {
			ReportFormatException([], e);
			return true; // most likely, the conditions were met, but format encountered an exception. Showing the text will allow the user to diagnose the bad localization
		}
	}

	private void ReportFormatException(object[] args, Exception e)
	{
		try {
			// Rely on Logging.FirstChanceExceptionHandler to report and deduplicate these
			throw new Exception($"The localization key:\n  \"{Key}\"\nwith a value of:\n  \"{UnformattedValue}\"\nfailed to be formatted with the inputs:\n  [{string.Join(", ", args)}]", e);
		}
		catch (Exception) { }
	}

	internal void BindArgs(LocalizedText original, object[] args)
	{
		Debug.Assert(Key == original.Key);

		try {
			switch (original._value) {
				case VariableText variableText:
					_value = variableText.Bind(args);
					break;
				case CompositeText compositeText:
					// SetValue re-parses, giving a plain string once every placeholder has been supplied
					SetValue(compositeText.Bind(args));
					break;
				default:
					SetValue(original.UnformattedValue);
					break;
			}
		}
		catch (Exception e) {
			original.ReportFormatException(args, e);
			SetValue(original.UnformattedValue); // carry on with the placeholders visible, so the remaining texts still re-bind on a language change
		}

		EnglishValue = original.EnglishValue; // keep the unformatted english value on all langs, for consistency, though we don't expect it to be used for anything other than HasValue
		BoundArgs = args;
	}
}
