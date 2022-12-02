using Microsoft.Xna.Framework;

namespace Terraria.ModLoader;

public abstract class ModRarity : ModType
{
	public int Type { get; internal set; }

	protected sealed override void Register()
	{
		ModTypeLookup<ModRarity>.Register(this);
		Type = RarityLoader.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

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
