using Mono.Cecil;

namespace Terraria.ModLoader
{
	/// <summary>
	///		Handles modifying assemblies as they're loaded.
	/// </summary>
	public abstract class Coremod
	{
		/// <summary>
		///		Transforms the passed <paramref name="module"/> in the context of this <see cref="Coremod"/>. Invoked for every loaded assembly that is not blacklisted.
		/// </summary>
		/// <param name="module">The <see cref="ModuleDefinition"/> to transform.</param>
		/// <returns><see langword="true"/> if this <see cref="ModuleDefinition"/> was modified, otherwise <see langword="false"/>.</returns>
		/// <remarks>
		///		Return values should be handled appropriately. The return value indicates whether your coremod has modified the assembly, which is used for logging, debugging, and much more. <br />
		///		Furthermore, if no invocations return <see langword="true"/>, the assembly will be loaded by file instead of by memory, meaning any changes applied by this <see cref="Coremod"/> will be ignored.
		/// </remarks>
		public abstract bool Transform(ModuleDefinition module);
	}
}