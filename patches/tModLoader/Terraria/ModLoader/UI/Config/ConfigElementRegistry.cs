using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader.Config.UI.Elements;

namespace Terraria.ModLoader.Config.UI;
public static class ConfigElementRegistry
{
	private static List<UIConfigElement> configElements = new();

	public static void Load()
	{
		// Autoloading dummy config elements
		var assembly = Assembly.GetAssembly(typeof(Main));
		var configElementTypes = from type in assembly.GetTypes().ToList()
								 where !type.IsAbstract && !type.ContainsGenericParameters
								 where type.IsSubclassOf(typeof(UIConfigElement))
								 orderby type.FullName, StringComparer.InvariantCulture
								 select type;

		foreach (var type in configElementTypes) {
			var element = Activator.CreateInstance(type, true) as UIConfigElement;
			element.IsDummy = true;
			configElements.Add(element);
		}

		// TODO: load mod config elements?
	}

	public static void Unload()
	{
		configElements.Clear();
	}

	// Gets a config element for the member info
	public static UIConfigElement GetConfigElement(object parent, PropertyFieldWrapper memberInfo, int indexInCollection = -1)
	{
		// TODO: allow setting custom ui

		foreach (var configElement in configElements) {
			if (configElement.FitsType(memberInfo.Type)) {
				var element = Activator.CreateInstance(configElement.GetType()) as UIConfigElement;
				element.Bind(parent, memberInfo, indexInCollection);
				return element;
			}
		}

		var textElement = new UITextElement {
			Text = $"{memberInfo.Name} - \"{memberInfo.Type}\" is not handled yet",// TODO: localize, add special message for structs and enums and whatnot
		};
		textElement.Bind(parent, memberInfo, indexInCollection);
		return textElement;
	}

	// Gets a header if it exists on the element
	public static UIHeaderElement GetHeader(object parent, PropertyFieldWrapper memberInfo, int indexInCollection = -1)
	{
		var headerAttribute = ConfigManager.GetLocalizedHeader(memberInfo.MemberInfo);
		if (headerAttribute is null)
			return null;

		var header = new UIHeaderElement {
			Header = headerAttribute.Header
		};
		header.Bind(parent, memberInfo, indexInCollection);
		return header;
	}

	// Adds the given element to the list
	// TODO: rename
	public static void HandleElement(IList<UIConfigElement> list, object parent, PropertyFieldWrapper memberInfo, int indexInCollection = -1)
	{
		var header = GetHeader(parent, memberInfo, indexInCollection);
		if (header is not null)
			list.Add(header);

		var element = GetConfigElement(parent, memberInfo, indexInCollection);
		list.Add(element);
	}

	// TODO: method that loops over all of the serialiable fields on an object and adds them to a list
}
