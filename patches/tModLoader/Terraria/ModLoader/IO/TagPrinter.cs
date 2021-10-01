using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terraria.ModLoader.IO
{
	public class TagPrinter
	{
		private string indent = "";
		private StringBuilder sb = new StringBuilder();

		public override string ToString() {
			return sb.ToString();
		}

		private static string TypeString(Type type) {
			if (type == typeof(byte)) return "byte";
			if (type == typeof(short)) return "short";
			if (type == typeof(int)) return "int";
			if (type == typeof(long)) return "long";
			if (type == typeof(float)) return "float";
			if (type == typeof(double)) return "double";
			if (type == typeof(string)) return "string";
			if (type == typeof(byte[])) return "byte[]";
			if (type == typeof(int[])) return "int[]";
			if (type.IsAssignableTo(typeof(IReadOnlyTagCompound))) return "object";
			if (type == typeof(IList)) return "list";
			throw new ArgumentException("Unknown Type: " + type);
		}

		private void WriteList<T>(char start, char end, bool multiline, IEnumerable<T> list, Action<T> write) {
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

		private void WriteEntry(KeyValuePair<string, object> entry) {
			(string key, object value) = entry;
			if (value == null) {
				sb.Append('"').Append(key).Append("\" = null");
				return;
			}

			var type = value.GetType();
			var isList = value is IList and not Array;
			sb.Append(TypeString(isList ? type.GetGenericArguments()[0] : type));

			sb.Append(" \"").Append(key).Append("\" ");

			if (value is IReadOnlyTagCompound && !isList)
				sb.Append("= ");

			WriteValue(value);
		}

		private void WriteValue(object elem) {
			switch (elem)
			{
				case byte b:
					sb.Append(b);
					break;
				case short s:
					sb.Append(s);
					break;
				case int i:
					sb.Append(i);
					break;
				case long l:
					sb.Append(l);
					break;
				case float f:
					sb.Append(f);
					break;
				case double d:
					sb.Append(d);
					break;
				case string s:
					sb.Append('"').Append(s).Append('"');
					break;
				case byte[] bytes:
					sb.Append('[').Append(string.Join(", ", bytes)).Append(']');
					break;
				case int[] ints:
					sb.Append('[').Append(string.Join(", ", ints)).Append(']');
					break;
				case IReadOnlyTagCompound compound:
					WriteList('{', '}', true, compound, WriteEntry);
					break;
				case IList list:
					var type = list.GetType().GetGenericArguments()[0];
					WriteList('[', ']',
						type == typeof(string) || type == typeof(TagCompound) || type == typeof(IReadOnlyTagCompound) || typeof(IList).IsAssignableFrom(type),
						list.Cast<object>(),
						o => {
							if (type == typeof(IList)) //lists of lists need their subtype printed
								sb.Append(TypeString(o.GetType().GetGenericArguments()[0])).Append(' ');

							WriteValue(o);
						});
					break;
			}
		}

		public static string Print(IReadOnlyTagCompound tag) {
			var printer = new TagPrinter();
			printer.WriteValue(tag);
			return printer.ToString();
		}

		public static string Print(KeyValuePair<string, object> entry) {
			var printer = new TagPrinter();
			printer.WriteEntry(entry);
			return printer.ToString();
		}
	}
}
