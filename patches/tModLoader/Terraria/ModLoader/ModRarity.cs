using Microsoft.Xna.Framework;
using System;

namespace Terraria.ModLoader
{
	public abstract class ModRarity : ModType
	{
		public int Type { get; internal set; }

		protected sealed override void Register() {
			if (!Mod.loading)
				throw new Exception("AddRarity can only be called from Mod.Load or Mod.Autoload");

			if (Mod.rarities.ContainsKey(Name))
				throw new Exception($"You tried to add 2 ModRarities with the same name: {Name}. Maybe 2 classes share a classname but in different namespaces while autoloading or you manually called AddRarity with 2 rarities of the same name.");
			
			Mod.rarities[Name] = this;
			Type = RarityLoader.Add(this);
			ContentInstance.Register(this);
		}

		/// <summary>
		/// Your ModRarity's color.
		/// Returns White by default.
		/// </summary>
		/// <returns></returns>
		public virtual Color RarityColor => Color.White;

		/// <summary>
		/// Allows you to modify which rarities will come before and after this when a modifier is applied (since modifiers can affect rarity)
		/// </summary>
		/// <param name="offset">The amount by which the rarity would be offset in vanilla. -2 is the most it can go down, and +2 is the most it can go up by.</param>
		/// <param name="valueMult">The combined stat and prefix value scale. Can be used to implement super high or low value rarity adjustments outside normal vanilla ranges</param>
		/// <returns>The adjusted rarity type. Return <code>Type</code> for no change.</returns>
		public virtual int GetPrefixedRarity(int offset, float valueMult) => Type;
	}
}
