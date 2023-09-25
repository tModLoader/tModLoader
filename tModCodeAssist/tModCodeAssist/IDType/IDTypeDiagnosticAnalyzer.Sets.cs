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

	private static ImmutableDictionary<string, IDSet> idSets;
	private static ImmutableDictionary<string, string> idSetNameByFullName;

	private static void PopulateIDSets()
	{
		var idSets = ImmutableDictionary.CreateBuilder<string, IDSet>();
		var idSetNameByFullName = ImmutableDictionary.CreateBuilder<string, string>();

		foreach (var idType in typeof(ItemID).Assembly.GetExportedTypes().Where(static x => x.Namespace.StartsWith("Terraria.ID"))) {
			var associatedNameAttribute = idType.GetCustomAttribute(typeof(AssociatedNameAttribute), false);
			var searchDictionary = idType.GetField("Search");

			if (associatedNameAttribute == null) {
				continue;
			}

			if (searchDictionary == null || searchDictionary.GetValue(null) is not IdDictionary idSet) {
				continue;
			}

			string associatedName = ((AssociatedNameAttribute)associatedNameAttribute).Name;

			idSets.Add(associatedName, new(idType.FullName, idSet));
			idSetNameByFullName.Add(idType.FullName.Replace('+', '.'), associatedName);
		}

		IDTypeDiagnosticAnalyzer.idSets = idSets.ToImmutable();
		IDTypeDiagnosticAnalyzer.idSetNameByFullName = idSetNameByFullName.ToImmutable();
	}
}
