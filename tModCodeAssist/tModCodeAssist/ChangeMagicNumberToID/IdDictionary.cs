using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace tModCodeAssist.ChangeMagicNumberToID;

public sealed class IdDictionary
{
	private readonly Dictionary<string, long> _nameToId = [];
	private readonly Dictionary<long, string> _idToName = [];

	public int Count { get; private set; }

	public IEnumerable<string> Names => _nameToId.Keys;

	public bool TryGetName(long id, out string name) => _idToName.TryGetValue(id, out name);
	public bool TryGetId(string name, out long id) => _nameToId.TryGetValue(name, out id);
	public bool ContainsName(string name) => _nameToId.ContainsKey(name);
	public bool ContainsId(long id) => _idToName.ContainsKey(id);
	public string GetName(long id) => _idToName[id];
	public long GetId(string name) => _nameToId[name];

	public void Add(string name, long id)
	{
		_idToName.Add(id, name);
		_nameToId.Add(name, id);
	}

	public static IdDictionary Create(INamedTypeSymbol obsoleteAttributeSymbol, INamedTypeSymbol symbol, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var search = new IdDictionary();
		int manualCount = 0;

		foreach (var member in symbol.GetMembers()) {
			// Skip non constants
			if (member is not IFieldSymbol { HasConstantValue: true } field)
				continue;

			// Skip non integers
			if (field.Type.SpecialType is not (>= SpecialType.System_SByte and <= SpecialType.System_UInt64))
				continue;

			// Skip if field is marked as `Obsolete`
			foreach (var attributeData in member.GetAttributes()) {
				if (SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, obsoleteAttributeSymbol)) {
					continue;
				}
			}

			long constValue = Convert.ToInt64(field.ConstantValue);

			if (!search._nameToId.ContainsKey(member.Name)) {
				search._nameToId.Add(member.Name, constValue);
				search._idToName.Add(constValue, member.Name);
			}

			manualCount++;
		}

		search.Count = manualCount;

		return search;
	}
}
