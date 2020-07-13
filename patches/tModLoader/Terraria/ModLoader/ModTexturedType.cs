namespace Terraria.ModLoader
{
	/// <summary>
	/// The base type for most modded things with textures.
	/// </summary>
	public abstract class ModTexturedType:IModType
	{
		///<summary>
		/// The mod this belongs to.
		/// </summary>
		public Mod Mod{get;internal set;}
		
		/// <summary>
		/// The internal name of this instance.
		/// </summary>
		public virtual string Name => GetType().Name;

		public virtual string Texture => GetType().FullName.Replace('.', '/');

		void ILoadable.Load(Mod mod)
		{
			Mod = mod;
			Load();
			AddInstance();
		}

		public virtual void Load(){}
		public virtual void Unload(){}

		/// <summary>
		/// Called if Autoload returns true.
		/// DO NOT CALL THIS MANUALLY!
		/// </summary>
		/// <param name="name">Instance name.</param>
		/// <param name="texture">Texture path.</param>
		protected abstract void AddInstance();
	}
}
