using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terraria.ModLoader.IO;

public class TagPrinter
{
	private string indent = "";
	private StringBuilder sb = new StringBuilder();

	public override string ToString()
	{
		return sb.ToString();
	}

	private string TypeString(Type type)
	{
		if (type == typeof(byte)) return "byte";
		if (type == typeof(short)) return "short";
		if (type == typeof(int)) return "int";
		if (type == typeof(long)) return "long";
		if (type == typeof(float)) return "float";
		if (type == typeof(double)) return "double";
		if (type == typeof(string)) return "string";
		if (type == typeof(byte[])) return "byte[]";
		if (type == typeof(int[])) return "int[]";
		if (type == typeof(TagCompound)) return "object";
		if (type == typeof(IList)) return "list";
		throw new ArgumentException("Unknown Type: " + type);
	}

	private void WriteList<T>(char start, char end, bool multiline, IEnumerable<T> list, Action<T> write)
	{
		sb.Append(start);
		indent += "  ";
		var first = true;
		foreach (var entry in list) {
			if (first) first = false;
			else sb.Append(multiline ? "," : ", ");

			if (multiline) sb.AppendLine().Append(indent);
			write(entry);
		}
		indent = indent.Substring(2);
		if (multiline && !first) sb.AppendLine().Append(indent);
		sb.Append(end);
	}

	private void WriteEntry(KeyValuePair<string, object> entry)
	{
		if (entry.Value == null) {
			sb.Append('"').Append(entry.Key).Append("\" = null");
			return;
		}

		var type = entry.Value.GetType();
		var isList = entry.Value is IList && !(entry.Value is Array);
		sb.Append(TypeString(isList ? type.GetGenericArguments()[0] : type));

		sb.Append(" \"").Append(entry.Key).Append("\" ");

		if (type != typeof(TagCompound) && !isList)
			sb.Append("= ");

		WriteValue(entry.Value);
	}

	private void WriteValue(object elem)
	{
		if (elem is byte)
			sb.Append((byte)elem);
		else if (elem is short)
			sb.Append((short)elem);
		else if (elem is int)
			sb.Append((int)elem);
		else if (elem is long)
			sb.Append((long)elem);
		else if (elem is float)
			sb.Append((float)elem);
		else if (elem is double)
			sb.Append((double)elem);
		else if (elem is string)
			sb.Append('"').Append((string)elem).Append('"');
		else if (elem is byte[])
			sb.Append('[').Append(string.Join(", ", (byte[])elem)).Append(']');
		else if (elem is int[])
			sb.Append('[').Append(string.Join(", ", (int[])elem)).Append(']');
		else if (elem is TagCompound) {
			var tag = (TagCompound)elem;
			WriteList('{', '}', true, tag, WriteEntry);
		}
		else if (elem is IList) {
			var type = elem.GetType().GetGenericArguments()[0];
			WriteList('[', ']',
				type == typeof(string) || type == typeof(TagCompound) || typeof(IList).IsAssignableFrom(type),
				((IList)elem).Cast<object>(),
				o => {
					if (type == typeof(IList)) //lists of lists need their subtype printed
						sb.Append(TypeString(o.GetType().GetGenericArguments()[0])).Append(' ');

					WriteValue(o);
				});
		}
	}

	public static string Print(TagCompound tag)
	{
		var printer = new TagPrinter();
		printer.WriteValue(tag);
		return printer.ToString();
	}

	public static string Print(KeyValuePair<string, object> entry)
	{
		var printer = new TagPrinter();
		printer.WriteEntry(entry);
		return printer.ToString();
	}
}
