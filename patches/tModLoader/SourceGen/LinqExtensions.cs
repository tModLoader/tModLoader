using System.Collections.Generic;

namespace SourceGen
{
	internal static class LinqExtensions
	{
		// Not present in .NET Standard 2.0
		public static IEnumerable<T> Prepend<T>(this IEnumerable<T> enumerable, T value) {
			yield return value;

			foreach (var item in enumerable) {
				yield return item;
			}
		}
	}
}
