namespace Terraria.ModLoader
{
	/// <summary>
	/// The base type for most modded things.
	/// </summary>
	public abstract class ModType : IModType
	{
		///<summary>
		/// The mod this belongs to.
		/// </summary>
		public Mod Mod { get; internal set; }

		/// <summary>
		/// The internal name of this.
		/// </summary>
		public virtual string Name => GetType().Name;

		/// <summary>
		/// The internal name of this, including the mod it is from.
		/// </summary>
		public string FullName => $"{Mod?.Name ?? "Terraria"}/{Name}";

		void ILoadable.Load(Mod mod) {
			Mod = mod;
			Load();
			Register();
		}

		/// <summary>
		/// Allows you to perform one-time loading tasks. Beware that mod content has not finished loading here, things like ModContent lookup tables or ID Sets are not fully populated.
		/// <para>Use <see cref="SetStaticDefaults"/> when you need to access content.</para>
		/// </summary>
		public virtual void Load() { }

		/// <summary>
		/// If you make a new ModType, seal this override.
		/// </summary>
		protected abstract void Register();

		/// <summary>
		/// If you make a new ModType, seal this override, and call <see cref="SetStaticDefaults"/> in it.
		/// </summary>
		public virtual void SetupContent() { }

		/// <summary>
		/// Allows you to modify the properties after loading has completed.
		/// </summary>
		public virtual void SetStaticDefaults() { }

		/// <summary>
		/// Allows you to safely unload things you added in <see cref="Load"/>.
		/// </summary>
		public virtual void Unload() { }
	}
}
