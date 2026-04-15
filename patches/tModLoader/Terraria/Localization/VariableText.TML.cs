using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Terraria.Localization;

internal partial class VariableText
{
	private static readonly Regex _namedPluralRegex = new Regex(@"{\^([a-zA-Z][\w\.]*):([^\r\n]+?)}", RegexOptions.Compiled); // "{Count} {^Count:item;items}"

	private static string ConvertNamedPlurals(string format, List<string> variables)
	{
		return _namedPluralRegex.Replace(format, delegate (Match match) {
			string varName = match.Groups[1].Value;
			int idx = variables.IndexOf(varName);
			if (idx < 0) {
				idx = variables.Count;
				variables.Add(varName);
			}
			return "{^" + idx + ":" + match.Groups[2].Value + "}";
		});
	}
}
