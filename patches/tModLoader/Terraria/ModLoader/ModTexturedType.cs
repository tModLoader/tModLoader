using System;

namespace Terraria.ModLoader
{
	/// <summary>
	/// The base type for most modded things with textures.
	/// </summary>
	public abstract class ModTexturedType:IAutoloadable
	{
		///<summary>
		/// The mod this belongs to.
		/// </summary>
		public Mod mod{get;internal set;}
		
		/// <summary>
		/// The internal name of this instance.
		/// </summary>
		public string Name{get;internal set;}

		//public string Texture{get;internal set;}

		bool IAutoloadable.Autoload(Mod mod)
		{
			Type type = GetType();
			this.mod = mod;
			string name = type.Name;
			string texture = type.FullName.Replace('.', '/');
			if(Autoload(ref name, ref texture))
			{
				AddInstance(name, texture);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Allows you to automatically load this instead of manually adding it via the Mod instance. Return true to allow autoloading; by default returns the mod's autoload property. Name is initialized to the overriding class name and texture is initialized to the namespace and overriding class name with periods replaced with slashes. Use this method to either force or stop an autoload, or to change the default display name and texture path.
		/// </summary>
		/// <param name="name">The internal name.</param>
		/// <param name="texture">The texture path.</param>
		public virtual bool Autoload(ref string name, ref string texture) => mod.Properties.Autoload;
		
		void IAutoloadable.Unload(){}

		/// <summary>
		/// Called if Autoload returns true.
		/// DO NOT CALL THIS MANUALLY!
		/// </summary>
		/// <param name="name">Instance name.</param>
		/// <param name="texture">Texture path.</param>
		protected abstract void AddInstance(string name, string texture);
	}
}
