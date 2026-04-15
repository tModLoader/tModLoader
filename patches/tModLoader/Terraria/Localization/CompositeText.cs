using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Terraria.Localization;

internal class CompositeText
{
	private struct Plural
	{
		public int SourceArgIndex;
		public int FormatArgIndex;
		public string[] Options;
	}

	private readonly string _original;
	private readonly CompositeFormat _compositeFormat;
	private readonly Plural[] _plurals;
	private readonly object[] _extendedArgBuffer;

	private static readonly Regex _positionalRegex = new Regex(@"\{(\d+)", RegexOptions.Compiled);
	private static readonly Regex _positionalPluralRegex = new Regex(@"{\^(\d+):([^\r\n]+?)}", RegexOptions.Compiled); // "{0} {^0:item;items}"

	public CompositeText(string format)
	{
		_original = format;
		_plurals = ProcessPlurals(ref format);
		_compositeFormat = CompositeFormat.Parse(format);
		_extendedArgBuffer = _plurals.Length > 0 ? new object[_compositeFormat.MinimumArgumentCount] : null;
	}

	public static bool TryCreate(string s, out CompositeText text)
	{
		if (!_positionalRegex.IsMatch(s) && !_positionalPluralRegex.IsMatch(s)) {
			text = null;
			return false;
		}

		text = new CompositeText(s);
		return true;
	}

	private static Plural[] ProcessPlurals(ref string format)
	{
		if (!_positionalPluralRegex.IsMatch(format))
			return [];

		var plurals = new List<Plural>();
		int nextSlot = _positionalRegex.Matches(format).Max(m => int.Parse(m.Groups[1].Value)) + 1;

		format = _positionalPluralRegex.Replace(format, delegate (Match match) {
			plurals.Add(new Plural {
				SourceArgIndex = int.Parse(match.Groups[1].Value),
				FormatArgIndex = nextSlot,
				Options = match.Groups[2].Value.Split(';')
			});
			return "{" + nextSlot++ + "}";
		});

		return plurals.ToArray();
	}

	public string Format(params object[] args)
	{
		if (_extendedArgBuffer == null)
			return string.Format(null, _compositeFormat, args);

		Array.Copy(args, _extendedArgBuffer, args.Length);
		foreach (var p in _plurals)
			_extendedArgBuffer[p.FormatArgIndex] = Pluralization.SelectPlural(p.Options, args[p.SourceArgIndex]);

		return string.Format(null, _compositeFormat, _extendedArgBuffer);
	}

	public override string ToString() => _original;
}
