using System.Collections.Generic;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader;

public abstract class ModSeedType : ModType
{
	/// <summary>
	/// Whether this seed is enabled for the current world.
	/// </summary>
	public ref bool Enabled => ref _enabled;
	private bool _enabled;

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
}