using Hjson;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using static Terraria.ModLoader.LocalizationLoader;

namespace Terraria.ModLoader.Utilities;

// HEAVILY cleaned up and modified code from https://github.com/hjson/hjson-cs
// The default stringify method from the Hjson library has zero customization, and I found the output to be too ugly
// -- Mirsario
internal static class HjsonExtensions
{
	public struct HjsonStyle
	{
		public bool WriteComments;
		public bool EmitRootBraces;
		public bool NoIndentaion;
		public string Separator;
	}

	private delegate bool TryParseNumericLiteralDelegate(string text, bool stopAtNext, out JsonValue value);

	private const string JsonWriter = "Hjson.JsonWriter";
	private const string HjsonReader = "Hjson.HjsonReader";
	private const string HjsonWriter = "Hjson.HjsonWriter";
	private const string HjsonValue = "Hjson.HjsonValue";

	public static readonly HjsonStyle DefaultHjsonStyle = new() {
		WriteComments = true,
		EmitRootBraces = false,
		NoIndentaion = false,
		Separator = " ",
	};

	private static readonly TryParseNumericLiteralDelegate tryParseNumericLiteral = GetDelegateOfMethod<TryParseNumericLiteralDelegate>(HjsonReader, "TryParseNumericLiteral");
	private static readonly Func<string, string> escapeString = GetDelegateOfMethod<Func<string, string>>(JsonWriter, "EscapeString");
	private static readonly Func<string, string> escapeName = GetDelegateOfMethod<Func<string, string>>(HjsonWriter, nameof(escapeName));
	private static readonly Func<string, bool> startsWithKeyword = GetDelegateOfMethod<Func<string, bool>>(HjsonWriter, nameof(startsWithKeyword));
	private static readonly Func<char, bool> needsEscapeML = GetDelegateOfMethod<Func<char, bool>>(HjsonWriter, nameof(needsEscapeML));
	private static readonly Func<char, bool> needsEscape = GetDelegateOfMethod<Func<char, bool>>(HjsonWriter, nameof(needsEscape));
	private static readonly Func<char, bool> needsQuotes = GetDelegateOfMethod<Func<char, bool>>(HjsonWriter, nameof(needsQuotes));
	private static readonly Func<char, bool> isPunctuatorChar = GetDelegateOfMethod<Func<char, bool>>(HjsonValue, "IsPunctuatorChar");

	public static string ToFancyHjsonString(this JsonValue value, HjsonStyle? style = null)
	{
		var stringWriter = new StringWriter();
		var usedStyle = style ?? DefaultHjsonStyle;

		WriteFancyHjsonValue(stringWriter, value, 0, in usedStyle, noIndentation: true, isRootObject: true);

		return stringWriter.ToString();
	}

	private static void WriteFancyHjsonValue(TextWriter tw, JsonValue value, int level, in HjsonStyle style, bool hasComments = false, bool noIndentation = false, bool isRootObject = false)
	{
		switch (value.JsonType) {
			case JsonType.Object:
				var jObject = value.Qo();
				var commentedObject = style.WriteComments ? jObject as WscJsonObject : null;
				bool showBraces = !isRootObject || (commentedObject != null ? commentedObject.RootBraces : style.EmitRootBraces);

				if (!noIndentation || showBraces) {
					tw.Write(style.Separator);
				}

				if (showBraces) {
					tw.Write("{");
				}

				if (commentedObject != null) {
					bool skipFirst = !showBraces;
					string kwl = GetComments(commentedObject.Comments, "");
					JsonType? lastJsonType = null;

					foreach (string key in commentedObject.Order.Concat(commentedObject.Keys).Distinct()) {
						if (!jObject.ContainsKey(key))
							continue;

						var val = jObject[key];

						if ((val.JsonType != lastJsonType && lastJsonType != null) || lastJsonType == JsonType.Object) {
							NewLine(tw, 0);
						}

						if (!skipFirst)
							NewLine(tw, level + (showBraces ? 1 : 0));
						else
							skipFirst = false;

						if (!string.IsNullOrWhiteSpace(kwl)) {
							// Keep empty lines, properly indent all comment lines
							string indentation = new string('\t', level + (showBraces ? 1 : 0));
							var lines = kwl.TrimStart().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.TrimEntries).Select(s => s.Trim()).ToArray();
							kwl = string.Join("\n" + indentation, lines);
							tw.Write(kwl);
							NewLine(tw, level + (showBraces ? 1 : 0));
						}

						lastJsonType = val.JsonType;
						
						kwl = GetComments(commentedObject.Comments, key);

						bool commentedOut = jObject is CommentedWscJsonObject commented && commented.CommentedOut.Contains(key);
						bool commentIsMultiline = false;
						if (commentedOut) {
							commentIsMultiline = val.GetRawString().IndexOf('\n') != -1;

							if (commentIsMultiline)
								tw.Write("/* ");
							else
								tw.Write("// ");
						}

						tw.Write(escapeName(key));
						tw.Write(':');

						WriteFancyHjsonValue(tw, val, level + (showBraces ? 1 : 0), in style, hasComments: TestCommentString(kwl));

						if (commentedOut && commentIsMultiline)
							tw.Write(" */");
					}

					tw.Write(kwl);

					if (showBraces)
						NewLine(tw, level);
				}
				else {
					bool skipFirst = !showBraces;
					JsonType? lastJsonType = null;

					foreach (KeyValuePair<string, JsonValue> pair in jObject) {
						if ((pair.Value.JsonType != lastJsonType && lastJsonType != null) || lastJsonType == JsonType.Object) {
							NewLine(tw, 0);
						}

						lastJsonType = pair.Value.JsonType;

						if (!skipFirst)
							NewLine(tw, level + 1);
						else
							skipFirst = false;

						tw.Write(escapeName(pair.Key));
						tw.Write(':');

						WriteFancyHjsonValue(tw, pair.Value, level + (showBraces ? 1 : 0), in style, noIndentation: true);
					}

					if (showBraces && jObject.Count > 0)
						NewLine(tw, level);
				}

				if (showBraces)
					tw.Write('}');

				break;
			case JsonType.Array:
				int i = 0, n = value.Count;

				if (!style.NoIndentaion) {
					if (n > 0)
						NewLine(tw, level);
					else
						tw.Write(style.Separator);
				}

				tw.Write('[');
				WscJsonArray whiteL = null;
				string wsl = null;

				if (style.WriteComments) {
					whiteL = value as WscJsonArray;

					if (whiteL != null)
						wsl = GetComments(whiteL.Comments, 0);
				}

				for (; i < n; i++) {
					var v = value[i];

					if (whiteL != null) {
						tw.Write(wsl);
						wsl = GetComments(whiteL.Comments, i + 1);
					}

					NewLine(tw, level + 1);
					WriteFancyHjsonValue(tw, v, level + 1, in style, hasComments: wsl != null && TestCommentString(wsl));
				}

				if (whiteL != null)
					tw.Write(wsl);

				if (n > 0)
					NewLine(tw, level);

				tw.Write(']');

				break;
			case JsonType.Boolean:
				tw.Write(style.Separator);
				tw.Write(value ? "true" : "false");
				break;
			case JsonType.String:
				WriteString(tw, value.GetRawString(), level, hasComments, style.Separator);
				break;
			default:
				tw.Write(style.Separator);
				tw.Write(value.GetRawString());
				break;
		}
	}

	public static string GetRawString(this JsonValue value)
	{
		return value.JsonType switch {
			JsonType.String => ((string)value) ?? "",
			JsonType.Number => ((IFormattable)value).ToString("G", NumberFormatInfo.InvariantInfo).ToLowerInvariant(),
			_ => value.JsonType.ToString(),
		};
	}

	private static void NewLine(TextWriter tw, int level)
	{
		tw.Write("\r\n");
		tw.Write(new string('\t', level));
	}

	private static void WriteString(TextWriter tw, string value, int level, bool hasComment, string separator)
	{
		if (value == "") {
			tw.Write(separator + "\"\"");
			return;
		}

		char left = value[0], right = value[value.Length - 1];
		char left1 = value.Length > 1 ? value[1] : '\0', left2 = value.Length > 2 ? value[2] : '\0';
		bool doEscape = hasComment || value.Any(c => needsQuotes(c));

		if (doEscape
		|| char.IsWhiteSpace(left)
		|| char.IsWhiteSpace(right)
		|| left == '"'
		|| left == '\''
		|| left == '#'
		|| (left == '/' && (left1 == '*' || left1 == '/'))
		|| isPunctuatorChar(left)
		|| tryParseNumericLiteral(value, true, out JsonValue dummy)
		|| startsWithKeyword(value)
		) {
			if (!value.Any(c => needsEscape(c)))
				tw.Write(separator + "\"" + value + "\"");
			else if (!value.Any(c => needsEscapeML(c)) && !value.Contains("'''") && !value.All(c => char.IsWhiteSpace(c)))
				WriteMultiLineString(value, tw, level, separator);
			else
				tw.Write(separator + "\"" + escapeString(value) + "\"");
		}
		else {
			tw.Write(separator + value);
		}
	}

	private static void WriteMultiLineString(string value, TextWriter tw, int level, string separator)
	{
		string[] lines = value.Replace("\r", "").Split('\n');

		if (lines.Length == 1) {
			tw.Write(separator + "'''");
			tw.Write(lines[0]);
			tw.Write("'''");
		}
		else {
			level++;
			NewLine(tw, level);
			tw.Write("'''");

			foreach (string line in lines) {
				NewLine(tw, !string.IsNullOrEmpty(line) ? level : 0);
				tw.Write(line);
			}

			NewLine(tw, level);
			tw.Write("'''");
		}
	}

	private static string GetComments(Dictionary<string, string> comments, string key)
	{
		return comments.ContainsKey(key) ? GetComments(comments[key]) : "";
	}

	private static string GetComments(List<string> comments, int index)
	{
		return comments.Count > index ? GetComments(comments[index]) : "";
	}

	private static string GetComments(string text)
	{
		if (string.IsNullOrEmpty(text))
			return string.Empty;

		for (int i = 0; i < text.Length; i++) {
			char c = text[i];

			if (c == '\n' || c == '#' || c == '/' && i + 1 < text.Length && (text[i + 1] == '/' || text[i + 1] == '*'))
				break;

			if (c > ' ')
				return $" # {text}";
		}

		return text;
	}

	private static bool TestCommentString(string text)
		=> text.Length > 0 && text[text[0] == '\r' && text.Length > 1 ? 1 : 0] != '\n';

	private static T GetDelegateOfMethod<T>(string type, string methodName) where T : Delegate
	{
		return typeof(HjsonValue)
			.Assembly
			.GetType(type)
			.GetMethod(methodName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			.CreateDelegate<T>();
	}
}

