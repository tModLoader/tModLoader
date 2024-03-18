#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using ReLogic.Reflection;

namespace tModLoader.Analyzers.ChangeMagicNumberToID;

public readonly struct DataEntries(Dictionary<string, DataEntries.MemberInfo> memberByItsFullyQualifiedName)
{
	public readonly struct MemberInfo(RawData.MemberInfo rawMemberInfo, string idClassMetadataName, IdDictionary search)
	{
		public AttributeTargets Target { get; } = rawMemberInfo.Target;
		public string? ParameterName { get; } = rawMemberInfo.ParameterName;
		public string IdClassMetadataName { get; } = idClassMetadataName;
		public IdDictionary Search { get; } = search;
	}

	public Dictionary<string, MemberInfo> MemberByItsFullyQualifiedName { get; } = memberByItsFullyQualifiedName;

	public bool ContainsKey(string formattedName)
	{
		return MemberByItsFullyQualifiedName?.ContainsKey(formattedName) ?? false;
	}

	public bool TryGetValue(string formattedName, [NotNullWhen(true)] out MemberInfo memberInfo)
	{
		memberInfo = default;

		return MemberByItsFullyQualifiedName?.TryGetValue(formattedName, out memberInfo) ?? false;
	}

	public static string FormatName(string containingClassName, string memberName, string? parameterName = null)
	{
		if (parameterName != null)
			return $"{containingClassName}.{memberName} {parameterName}";

		return $"{containingClassName}.{memberName}";
	}
}

public sealed record class RawData
{
	public sealed record class Entry
	{
		public string MetadataName { get; set; } = null!;
		public Dictionary<string, MemberInfo> Members { get; set; } = null!;
	}

	public record class MemberInfo
	{
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public AttributeTargets Target { get; set; }

		public string? ParameterName { get; set; }
	}

	public string IdClass { get; set; } = null!;
	public List<Entry> Data { get; set; } = null!;
}
