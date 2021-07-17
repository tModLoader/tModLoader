using System;

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

		public string FullName => $"{Mod?.Name ?? "Terraria"}/{Name}";

		void ILoadable.Load(Mod mod) {
			Mod = mod;
			Load();
			Register();
		}

		public virtual void Load(){}

		protected abstract void Register();

		public virtual void SetupContent() {}

		public virtual void Unload(){}
	}

}
