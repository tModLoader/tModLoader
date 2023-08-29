using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.UI.Config.Elements;

namespace Terraria.ModLoader.UI.Config;
public static class ConfigElementRegistry
{
	private static readonly List<UIConfigElement> configElements = new();

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
	public static UIConfigElement GetConfigElement(object parent, PropertyFieldWrapper memberInfo, int indexInUIList, int indexInCollection = -1)
	{
		// Custom UI attributes
		var customUI = ConfigManager.GetCustomAttribute<CustomConfigUIAttribute>(memberInfo);
		if (customUI != null) {
			// Checking if it inherits from UIConfigElement
			var customUIType = customUI.Type;
			if (!typeof(UIConfigElement).IsAssignableFrom(customUIType)) {
				return new UITextElement {// TODO: localize
					Text = $"{customUIType.Name} specified via CustomConfigUI for {memberInfo.Name} does not inherit from UIConfigElement",
					Tooltip = "",
				}.Bind(parent, memberInfo, indexInUIList, indexInCollection);
			}

			// Checking if it has an empty constructor
			var ctor = customUIType.GetConstructor(Array.Empty<Type>());
			if (ctor is null) {
				return new UITextElement {// TODO: localize
					Text = $"{customUIType.Name} specified via CustomConfigUI for {memberInfo.Name} does not have an empty constructor",
					Tooltip = "",
				}.Bind(parent, memberInfo, indexInUIList, indexInCollection);
			}

			// Creating the element
			var element = ctor.Invoke(Array.Empty<object>()) as UIConfigElement;
			element.Bind(parent, memberInfo, indexInUIList, indexInCollection);
			return element;
		}

		// Modded config elements - Only affect the mod that adds them
		// TODO

		// tML config elements
		foreach (var configElement in configElements) {
			if (configElement.FitsType(memberInfo.Type)) {
				var element = Activator.CreateInstance(configElement.GetType()) as UIConfigElement;
				element.Bind(parent, memberInfo, indexInUIList, indexInCollection);
				return element;
			}
		}

		// Not handled message
		return new UITextElement {
			Text = $"{memberInfo.Name}",
			Tooltip = Language.GetTextValue("tModLoader.ModConfigNotHandled", memberInfo.Name, memberInfo.Type),
		}.Bind(parent, memberInfo, indexInUIList, indexInCollection);
	}

	// Gets a header if it exists on the element
	public static UIHeaderElement GetHeader(object parent, PropertyFieldWrapper memberInfo, int indexInUIList, int indexInCollection = -1)
	{
		var headerAttribute = ConfigManager.GetLocalizedHeader(memberInfo.MemberInfo);
		if (headerAttribute is null)
			return null;

		var header = new UIHeaderElement {
			Header = headerAttribute.Header
		};
		header.Bind(parent, memberInfo, indexInUIList, indexInCollection);
		return header;
	}

	// Adds the given element to the list
	// TODO: rename
	public static void HandleElement(IList<UIConfigElement> list, object parent, PropertyFieldWrapper memberInfo, int indexInUIList, int indexInCollection = -1)
	{
		var header = GetHeader(parent, memberInfo, indexInUIList, indexInCollection);
		if (header is not null)
			list.Add(header);

		var element = GetConfigElement(parent, memberInfo, indexInUIList, indexInCollection);
		list.Add(element);
	}

	// Loops over all of the serialiable fields on an object and adds them to a list
	// TODO: rename
	public static void HandleElements(IList<UIConfigElement> list, object parent)
	{
		int i = 0;
		foreach (var memberInfo in ConfigManager.GetDisplayedVariables(parent)) {
			HandleElement(list, parent, memberInfo, i);
			i++;
		}
	}
}
