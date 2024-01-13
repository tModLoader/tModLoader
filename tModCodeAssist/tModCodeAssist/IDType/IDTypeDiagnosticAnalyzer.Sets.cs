using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using ReLogic.Reflection;
using Terraria.ID;
using tModCodeAssist.IDType.Sets;

namespace tModCodeAssist.IDType;

partial class IDTypeDiagnosticAnalyzer
{
	private record struct IDSet(
		in string FullyQualifiedName,
		in IdDictionary Constants
	);

	private static readonly ImmutableDictionary<string, IDSet> idSets;

	static IDTypeDiagnosticAnalyzer()
	{
		var builder = ImmutableDictionary.CreateBuilder<string, IDSet>();

		foreach (var idType in typeof(ItemID).Assembly.GetExportedTypes().Where(static x => x.Namespace.StartsWith("Terraria.ID"))) {
			var associatedName = idType.GetCustomAttribute(typeof(AssociatedNameAttribute), false);
			var searchDictionary = idType.GetField("Search");

			if (associatedName == null) {
				continue;
			}

			if (searchDictionary == null || searchDictionary.GetValue(null) is not IdDictionary idSet) {
				continue;
			}

			builder.Add(((AssociatedNameAttribute)associatedName).Name, new(GetQualifiedName(idType), idSet));
		}

		idSets = builder.ToImmutable();

		static string GetQualifiedName(Type type)
		{
			return type.FullName.Replace('+', '.');
		}
	}
}
