#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace tModCodeAssist.ChangeMagicNumberToID;

public sealed class DataEntry
{
	public struct MemberInfo
	{
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public AttributeTargets Target { get; set; }

		public string? ParameterName { get; set; }
	}

	public string MetadataName { get; set; } = null!;
	public Dictionary<string, MemberInfo> Members { get; set; } = null!;

	[JsonIgnore] public string IdClassName { get; set; } = null!;
	[JsonIgnore] public IdDictionary Search { get; set; } = null!;

	public void Initialize(Compilation compilation, CancellationToken cancellationToken)
	{
		var type = compilation.GetTypeByMetadataName(IdClassName);

		if (type == null)
			return;

		Search = IdDictionary.Create(compilation.GetTypeByMetadataName(Constants.ObsoleteAttributeMetadataName), type, cancellationToken);
	}
}
