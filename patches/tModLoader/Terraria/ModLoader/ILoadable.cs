using JetBrains.Annotations;

namespace Terraria.ModLoader;

/// <summary>
/// Allows for implementing types to be loaded and unloaded.
/// </summary>
[UsedImplicitly(
	ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature,
	ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors
)]
public interface ILoadable
{
	/// <summary>
	/// Called when loading the type.
	/// </summary>
	/// <param name="mod">The mod instance associated with this type.</param>
	public abstract void Load(Mod mod);

	/// <summary>
	/// Whether or not this type should be loaded when it's told to. Returning false disables <see cref="Mod.AddContent"/> from actually loading this type.
	/// </summary>
	/// <param name="mod">The mod instance trying to add this content</param>
	public virtual bool IsLoadingEnabled(Mod mod) => true;

	/// <summary>
	/// Called during unloading when needed.
	/// </summary>
	public abstract void Unload();
}
