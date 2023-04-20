using System;

namespace Terraria.ModLoader;
public class DefaultLocalizedValueAttribute : Attribute
{
	/*
	[DefaultLocalizedValue(typeof(DisplayNameGenerator))]
	public virtual LocalizedText DisplayName => GetLocalizedText(nameof(DisplayName));

	[DefaultLocalizedValue("")]
	public virtual LocalizedText Tooltip => GetLocalizedText(nameof(Tooltip));

	[DefaultLocalizedValue("Bestiary text for {DisplayName}")]
	public virtual LocalizedText BestiaryText => GetLocalizedText(nameof(BestiaryText));
	*/

	private string defaultValue;

	public DefaultLocalizedValueAttribute(string defaultValue)
	{
		this.defaultValue = defaultValue;
	}

	private Type type;
	private string methodName;

	public DefaultLocalizedValueAttribute(Type type, string methodName = "GetDefaultLocalizedValue")
	{
		this.type = type;
		this.methodName = methodName;
	}
}