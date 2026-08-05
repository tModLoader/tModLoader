using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Terraria.Localization;

internal partial class VariableText
{
	public int PositionalArgCount { get; } // positional args occupy slots [0, PositionalArgCount), followed by named variables

	private static readonly Regex _namedPluralRegex = new Regex(@"{\^([a-zA-Z][\w\.]*):([^\r\n]+?)}", RegexOptions.Compiled); // "{Count} {^Count:item;items}"
	private static readonly Regex _argIndexRegex = new Regex(@"(?<=\{\^?)\d+", RegexOptions.Compiled); // Matches just the index of "{0}" or "{^0:item;items}", so it can be mapped back to a variable name

	/// <summary>
	/// Formats <paramref name="args"/> into the leading placeholders, shifting the rest down. Leaves variables and conditions untouched
	/// </summary>
	public VariableText Bind(object[] args)
	{
		// discard excess positional args so they don't overflow into the named args
		if (args.Length > PositionalArgCount)
			Array.Resize(ref args, PositionalArgCount);

		int positionalArgCount = PositionalArgCount - args.Length;
		string format = _compositeText.Bind(args);
		string original = string.Join("", _conditions) + _argIndexRegex.Replace(format, match => {
			int variable = int.Parse(match.Value) - positionalArgCount;
			return variable >= 0 && variable < _variables.Length ? _variables[variable] : match.Value;
		});

		return new VariableText(original, format, _conditions, _variables, positionalArgCount);
	}

	private static string ConvertNamedPlurals(string format, List<string> variables, int positionalArgCount)
	{
		return _namedPluralRegex.Replace(format, delegate (Match match) {
			string varName = match.Groups[1].Value;
			int idx = variables.IndexOf(varName);
			if (idx < 0) {
				idx = variables.Count;
				variables.Add(varName);
			}
			return "{^" + (positionalArgCount + idx) + ":" + match.Groups[2].Value + "}";
		});
	}
}
