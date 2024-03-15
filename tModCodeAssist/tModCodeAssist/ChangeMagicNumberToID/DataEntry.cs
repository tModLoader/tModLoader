#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using ReLogic.Reflection;

namespace tModCodeAssist.ChangeMagicNumberToID;

public sealed class DataEntries
{
	public sealed record class MemberInfo : RawDataEntry.MemberInfo
	{
		public string IdClassMetadataName { get; }
		public IdDictionary Search { get; }

		public MemberInfo(RawDataEntry.MemberInfo rawMemberInfo, string idClassMetadataName, IdDictionary search)
		{
			Target = rawMemberInfo.Target;
			ParameterName = rawMemberInfo.ParameterName;
			IdClassMetadataName = idClassMetadataName;
			Search = search;
		}
	}

	public Dictionary<string, MemberInfo> MemberByItsFullyQualifiedName { get; }

	public DataEntries(Searches searches, Dictionary<string, ImmutableArray<RawDataEntry>> rawDataEntries)
	{
		MemberByItsFullyQualifiedName = [];

		foreach (var rawDataEntry in rawDataEntries) {
			string associatedIdClass = rawDataEntry.Key;
			var dataEntries = rawDataEntry.Value;

			foreach (var dataEntry in dataEntries) {
				foreach (var member in dataEntry.Members) {
					if (!searches.TryGetByMetadataName(associatedIdClass, out var search))
						continue;

					string key = FormatName(dataEntry.MetadataName, member.Key, member.Value.ParameterName);

					if (MemberByItsFullyQualifiedName.ContainsKey(key))
						continue;

					MemberByItsFullyQualifiedName.Add(key, new(member.Value, associatedIdClass, search));
				}
			}
		}
	}

	public bool ContainsKey(string formattedName)
	{
		return MemberByItsFullyQualifiedName.ContainsKey(formattedName);
	}

	public bool TryGetValue(string formattedName, [NotNullWhen(true)] out MemberInfo? memberInfo)
	{
		return MemberByItsFullyQualifiedName.TryGetValue(formattedName, out memberInfo);
	}

	public static string FormatName(string containingClassName, string memberName, string? parameterName = null)
	{
		if (parameterName != null)
			return $"{containingClassName}.{memberName} {parameterName}";

		return $"{containingClassName}.{memberName}";
	}
}

public sealed record class RawDataEntry
{
	public record class MemberInfo
	{
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public AttributeTargets Target { get; set; }

		public string? ParameterName { get; set; }
	}

	public string MetadataName { get; set; } = null!;
	public Dictionary<string, MemberInfo> Members { get; set; } = null!;
}
