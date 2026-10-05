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

		public override string ToString() => $"{{^{SourceArgIndex}:{string.Join(';', Options)}}}"; // Reproduces the pattern this was parsed from. See Bind
	}

	/// <summary>
	/// Helper for <see cref="Bind"/>
	/// </summary>
	internal struct PlaceholderArg
	{
		public int Index;
		public override string ToString() => $"{{{Index}}}";
	}

	private readonly string _original;
	private readonly CompositeFormat _compositeFormat;
	private readonly Plural[] _plurals;
	private readonly object[] _extendedArgBuffer;

	private static readonly Regex _argIndexRegex = new Regex(@"\{\^?(\d+)", RegexOptions.Compiled); // Matches the arg index of both "{0}" and the pluralization regex below
	private static readonly Regex _positionalPluralRegex = new Regex(@"{\^(\d+):([^\r\n]+?)}", RegexOptions.Compiled); // Matches "{^0:item;items}" -> (0, "item;items")

	/// <summary> The number of args <see cref="Format"/> requires, one more than the highest index the text references. </summary>
	public int ArgCount { get; }

	public CompositeText(string format)
	{
		_original = format;
		ArgCount = CountArgs(format);
		_plurals = ProcessPlurals(ref format, ArgCount);
		_compositeFormat = CompositeFormat.Parse(format);
		_extendedArgBuffer = _plurals.Length > 0 ? new object[_compositeFormat.MinimumArgumentCount] : null;
	}

	public static bool TryCreate(string s, out CompositeText text)
	{
		if (!_argIndexRegex.IsMatch(s)) {
			text = null;
			return false;
		}

		text = new CompositeText(s);
		return true;
	}

	/// <summary> The number of args <paramref name="format"/> requires, one more than the highest index it references. Unreferenced indices below that still count, eg "{2}" requires 3. </summary>
	public static int CountArgs(string format)
		=> _argIndexRegex.Matches(format).Select(m => int.Parse(m.Groups[1].Value) + 1).DefaultIfEmpty(0).Max();

	private static Plural[] ProcessPlurals(ref string format, int nextSlot)
	{
		if (!_positionalPluralRegex.IsMatch(format))
			return [];

		var plurals = new List<Plural>();

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

	public static void PadArgs(ref object[] args, int count, int shift = 0)
	{
		int supplied = args.Length;
		if (supplied >= count)
			return;

		Array.Resize(ref args, count);
		for (int i = supplied; i < count; i++)
			args[i] = new PlaceholderArg { Index = i - shift };
	}

	public string Format(params object[] args)
	{
		PadArgs(ref args, ArgCount);
		if (_extendedArgBuffer == null)
			return string.Format(null, _compositeFormat, args);

		Array.Copy(args, _extendedArgBuffer, ArgCount);
		foreach (var p in _plurals) {
			_extendedArgBuffer[p.FormatArgIndex] = args[p.SourceArgIndex] switch {
				PlaceholderArg unbound => p with { SourceArgIndex = unbound.Index },
				var count => Pluralization.SelectPlural(p.Options, count),
			};
		}

		return string.Format(null, _compositeFormat, _extendedArgBuffer);
	}

	/// <summary>
	/// Formats <paramref name="args"/> into the leading placeholders, shifting the rest down.
	/// </summary>
	public string Bind(object[] args)
	{
		PadArgs(ref args, ArgCount, shift: args.Length);
		return Format(args);
	}

	public override string ToString() => _original;
}
