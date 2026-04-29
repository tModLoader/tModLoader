using System;
using Terraria.GameContent;
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
		/* These RecipeGroup don't register, so they aren't suitable for use in recipes anyway since they have no RegisteredId. If we wanted, we'd need to register and assign a key, which might look similar to this.
		ConditionalDialogue.ItemGroups.Ore = new RecipeGroup("RecipeGroups.Ore", 699, 12, 11, 700, 14, 701, 13, 702) { Key = "Ore" }.Register();
		ConditionalDialogue.ItemGroups.Ore.Key = "Ore";
		ConditionalDialogue.ItemGroups.Bars.Key = "Bars";
		ConditionalDialogue.ItemGroups.Anvils.Key = "Anvils";
		ConditionalDialogue.ItemGroups.Whips.Key = "Whips";
		ConditionalDialogue.ItemGroups.Mounts.Key = "Mounts";
		*/

		// Could use reflection, but this should be faster.
		RecipeGroups.Birds.Key = "Birds";
		RecipeGroups.Scorpions.Key = "Scorpion";
		RecipeGroups.Squirrels.Key = "Squirrel";
		RecipeGroups.Bugs.Key = "Bugs";
		RecipeGroups.Ducks.Key = "Ducks";
		RecipeGroups.Butterflies.Key = "Butterflies";
		RecipeGroups.Fireflies.Key = "Fireflies";
		RecipeGroups.Snails.Key = "Snails";
		RecipeGroups.Dragonflies.Key = "Dragonflies";
		RecipeGroups.Turtles.Key = "Turtles";
		RecipeGroups.Macaws.Key = "Macaws";
		RecipeGroups.Cockatiels.Key = "Cockatiels";
		RecipeGroups.CloudBalloons.Key = "CloudBalloons";
		RecipeGroups.BlizzardBalloons.Key = "BlizzardBalloons";
		RecipeGroups.SandstormBalloons.Key = "SandstormBalloons";
		RecipeGroups.CritterGuides.Key = "CritterGuides";
		RecipeGroups.NatureGuides.Key = "NatureGuides";
		RecipeGroups.Seashells.Key = "Seashells";
		RecipeGroups.Fruit.Key = "Fruit";
		RecipeGroups.Balloons.Key = "Balloons";
		RecipeGroups.CobaltBar.Key = "CobaltBar";
		RecipeGroups.MythrilBar.Key = "MythrilBar";
		RecipeGroups.GemCritter.Key = "GemCritter";
		RecipeGroups.MagicMirror.Key = "MagicMirror";
		RecipeGroups.Wood.Key = "Wood";
		RecipeGroups.Stone.Key = "Stone";
		RecipeGroups.Sand.Key = "Sand";
		RecipeGroups.IronBar.Key = "IronBar";
		RecipeGroups.Fragment.Key = "Fragment";
		RecipeGroups.PressurePlate.Key = "PressurePlate";
		RecipeGroups.Jellyfish.Key = "Jellyfish";
	}
}
