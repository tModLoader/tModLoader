using System;
using System.Linq;
using System.Reflection;
using Terraria.ID;

namespace Terraria;

public partial class RecipeGroup
{
	/// <summary>
	/// A unique key for this RecipeGroup.
	/// </summary>
	public string Key { get; internal set; } // internal set so we can set it for vanilla groups without large patches.

	// Modded API: Forces assigning a Key parameter to facilitate ad hoc merging of recipe groups.
	/// <summary>
	/// Creates and registers a RecipeGroup with the given key, group descriptor key, and valid items. If an existing RecipeGroup with the same <paramref name="key"/> exists, the valid items will be merged into the existing group instead of creating a new one.
	/// <br/><br/> The <paramref name="groupDescriptorKey"/> is a localization key that will be interpolated into the "CombineFormat.RecipeGroup" key, which in English will result in "Any [groupDescriptorKeyValue]". To deviate from this format, use <see cref="Register(string, Func{string}, int[])"/> instead and provide a custom <c>Func&lt;string&gt; getName</c> function.
	/// <br/><br/> The first item in the <paramref name="validItems"/> array will be used as the placeholder item for the group.
	/// </summary>
	/// <param name="key">The unique key for the RecipeGroup.</param>
	/// <param name="groupDescriptorKey">The key for the group's descriptor text.</param>
	/// <param name="validItems">The valid items for the group.</param>
	/// <returns>The registered RecipeGroup.</returns>
	public static RecipeGroup Register(string key, string groupDescriptorKey, params int[] validItems) => new RecipeGroup(groupDescriptorKey, validItems) { Key = key }.Register();

	/// <summary>
	/// Creates and registers a RecipeGroup with the given key, name function, and valid items. If an existing RecipeGroup with the same <paramref name="key"/> exists, the valid items will be merged into the existing group instead of creating a new one.
	/// <br/><br/> The <paramref name="getName"/> function is the display value. When using <see cref="Register(string, string, int[])"/> the display value will automatically be "Any [groupDescriptorKeyValue]", but this function provides complete control over the display value.
	/// <br/><br/> The first item in the <paramref name="validItems"/> array will be used as the placeholder item for the group.
	/// </summary>
	/// <param name="key">The unique key for the RecipeGroup.</param>
	/// <param name="getName">A function that will return the display value.</param>
	/// <param name="validItems">The valid items for the group.</param>
	/// <returns>The registered RecipeGroup.</returns>
	public static RecipeGroup Register(string key, Func<string> getName, params int[] validItems) => new RecipeGroup(getName, validItems) { Key = key }.Register();

	public static void AssignKeysToVanillaGroups()
	{
		// The RecipeGroup in ConditionalDialogue don't register, so they aren't suitable for use in recipes anyway since they have no RegisteredId. They also are not currently reinitialized, so changes will persist between mod reloads.
		// If we wanted to support them, they'd have to be registered and we'd need to make sure they are reset on mod reload, but they aren't used anywhere anyway yet.

		foreach (var fieldInfo in typeof(RecipeGroups).GetFields(BindingFlags.Static | BindingFlags.Public).Where(f => f.FieldType == typeof(RecipeGroup))) {
			(fieldInfo.GetValue(null) as RecipeGroup).Key = fieldInfo.Name;
		}
	}
}
