using System;
using System.Collections.Generic;
using System.Diagnostics;
using ReLogic.Reflection;

namespace tModLoader.Analyzers.ChangeMagicNumberToID;

// Do not rename here anything without changing in BuiltinDataGenerator too
// (this warning includes namespace)
internal static partial class BuiltinData
{
	public readonly record struct Key
	{
		private readonly string metadataName;
		private readonly string memberName;
		private readonly bool hasParameter;
		private readonly (string Name, int Index) parameterNameOrIndex;

		public string MetadataName => metadataName;
		public string MemberName => memberName;
		public bool HasParameter => hasParameter;
		public bool HasParameterName => parameterNameOrIndex.Name != default;
		public bool HasParameterIndex => parameterNameOrIndex.Name == default;

		public Key(string metadataName, string memberName)
		{
			this.metadataName = metadataName;
			this.memberName = memberName;

			hasParameter = false;
		}

		public Key(string metadataName, string memberName, string parameterName)
		{
			this.metadataName = metadataName;
			this.memberName = memberName;

			hasParameter = true;
			parameterNameOrIndex = (parameterName, default);
		}

		public Key(string metadataName, string memberName, int parameterIndex)
		{
			this.metadataName = metadataName;
			this.memberName = memberName;

			hasParameter = true;
			parameterNameOrIndex = (default, parameterIndex);
		}

		public override int GetHashCode()
		{
			var hashCode = new HashCode();

			foreach (char @char in metadataName) {
				hashCode.Add(@char);
			}

			foreach (char @char in memberName) {
				hashCode.Add(@char);
			}

			if (HasParameterName) {
				foreach (char @char in parameterNameOrIndex.Name) {
					hashCode.Add(@char);
				}
			}
			else {
				hashCode.Add(parameterNameOrIndex.Index);
			}

			return hashCode.ToHashCode();
		}
	}

	public readonly struct Entry
	{
		public string IdClassMetadataName { get; }
		public IdDictionary Search { get; }

		public Entry(string metadataName)
		{
			IdClassMetadataName = metadataName;

			bool result = Searches.TryGetByMetadataName(IdClassMetadataName, out var search);
			Debug.Assert(result, $"Failed to find IdDictionary for {IdClassMetadataName} class");
			Search = search;
		}
	}

	private static readonly Dictionary<Key, Entry> data = [];

	public static bool ContainsKey(Key key)
	{
		return data.ContainsKey(key);
	}

	public static bool TryGetValue(Key key, out Entry entry)
	{
		return data.TryGetValue(key, out entry);
	}
}
