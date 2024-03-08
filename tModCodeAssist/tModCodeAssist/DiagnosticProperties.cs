using System.Collections.Immutable;

namespace tModCodeAssist;

public struct DiagnosticProperties
{
	private ImmutableDictionary<string, string>.Builder builder;

	public void Add(string key)
	{
		Add(key, null);
	}

	public void Add(string key, string value)
	{
		builder ??= ImmutableDictionary.CreateBuilder<string, string>();
		builder.Add(key, value);
	}

	public static implicit operator ImmutableDictionary<string, string>(DiagnosticProperties self)
	{
		return self.builder?.ToImmutable() ?? ImmutableDictionary<string, string>.Empty;
	}
}
