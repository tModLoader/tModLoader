using System;
using JetBrains.Annotations;

namespace Terraria.ModLoader;

/// <summary>
/// Generalized marker interface for autoloading.
/// The corresponding <typeparamref name="TImpl"/>.<see cref="IAutoloader.Autoload(Mod, Type)"/> method will be called on all non-abstract subtypes which implement this interface
/// </summary>
/// <typeparam name="TImpl">Concrete type containing a static <see cref="IAutoloader.Autoload(Mod, Type)"/> implementation method</typeparam>
[UsedImplicitly(
	ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature,
	ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors
)]
public interface IAutoload<TImpl> where TImpl : IAutoloader
{
}

/// <summary>
/// <see cref="IAutoload{TImpl}"/>. Used by <see cref="ILoadable"/> in tML
/// </summary>
public interface IAutoloader
{
	public static abstract void Autoload(Mod mod, Type type);
}
