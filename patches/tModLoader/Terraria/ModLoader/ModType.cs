using System;

namespace Terraria.ModLoader
{
	/// <summary>
	/// The base type for most modded things.
	/// </summary>
	public abstract class ModType:IAutoloadable
	{
		///<summary>
		/// The mod this belongs to.
		/// </summary>
		public Mod mod{get;internal set;}

		/// <summary>
		/// The internal name of this instance.
		/// </summary>
		public string Name{get;internal set;}

		bool IAutoloadable.Autoload(Mod mod)
		{
			Type type = GetType();
			this.mod = mod;
			string name = type.Name;
			if(Autoload(ref name))
			{
				AddInstance(name);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Allows you to automatically load this instead of manually adding it via the Mod class. Return true to allow autoloading; by default returns the mod's autoload property. Name is initialized to the overriding class name. Use this method to either force or stop an autoload or to control the internal name.
		/// </summary>
		/// <param name="name">The internal name.</param>
		public virtual bool Autoload(ref string name) => mod.Properties.Autoload;

		void IAutoloadable.Unload(){}

		/// <summary>
		/// DO NOT CALL THIS! This is called automatically if Autoload returns true.
		/// </summary>
		/// <param name="name">Instance name.</param>
		protected abstract void AddInstance(string name);
	}

}
