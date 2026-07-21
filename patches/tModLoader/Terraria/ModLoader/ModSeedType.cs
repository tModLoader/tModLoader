using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader;

/// <summary>
/// This is the superclass for ModSpecialSeed and ModSecretSeed, combining common code
/// </summary>
public abstract class ModSeedType : ModType
{
	/// <summary>
	/// Whether this seed is enabled for the current world.
	/// </summary>
	public ref bool Enabled => ref _enabled;
	private bool _enabled;

	/// <summary>
	/// The translation for the description used for this seed
	/// </summary>
	public virtual LocalizedText Description => Language.GetOrRegister($"Mods.{Mod.Name}.Seeds.{Name}.{nameof(Description)}", () => "");

	/// <summary>
	/// <inheritdoc cref="ModSystem.ModifyWorldGenTasks"/>
	/// <br/><br/>This only applies for worlds with this seed enabled. It is called before ModSystem.ModifyWorldGenTasks.
	/// </summary>
	public virtual void ModifyWorldGenTasks(List<GenPass> tasks) { }

	/// <summary>
	/// <inheritdoc cref="ModSystem.PostWorldGen"/>
	/// <br/><br/>This only applies for worlds with this seed enabled. It is called before ModSystem.PostWorldGen.
	/// </summary>
	public virtual void PostWorldGen() { }

	/// <summary>
	/// Allows you to modify the loading screen tips that are displayed at the bottom of the screen while a world with this seed is generating.
	/// Vanilla usages of this include the Drunk seed replacing the text with random numbers, or For the Worthy flipping it backwards.
	/// <seealso cref="ModifyWorldProgressText"/>
	/// </summary>
	public virtual void ModifyLoadingTips(ref string text, ref Color drawColor)
	{
	}

	/// <summary>
	/// Allows you to modify the text that is displayed during world generation to inform world generation status.
	/// Vanilla usages of this include the Drunk seed replacing the text with random numbers, or For the Worthy flipping it backwards.
	/// <seealso cref="ModifyLoadingTips"/>
	/// </summary>
	public virtual void ModifyWorldProgressText(ref string text, ref Color drawColor)
	{
	}
}