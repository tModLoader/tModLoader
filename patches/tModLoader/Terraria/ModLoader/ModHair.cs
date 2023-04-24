using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace Terraria.ModLoader;

/// <summary>
/// This class allows you to define custom hair styles for a player, controlling the associated gender and unlock conditions.
/// </summary>
public abstract class ModHair : ModTexturedType
{
	public int Type { get; internal set; }

	/// <summary>
	/// The path to the alternative texture used for when the hair is covered by a hat.
	/// </summary>
	public virtual string AltTexture => Texture + "_Alt";

	/// <summary>
	/// Determines whether this hairstyle is considered male, which influences what gender is picked when randomizing a character.
	/// </summary>
	public virtual bool IsMale => true;

	protected sealed override void Register()
	{
		ModTypeLookup<ModHair>.Register(this);
		Type = HairLoader.Register(this);
	}

	public sealed override void SetupContent()
	{
		AutoStaticDefaults();
		SetStaticDefaults();
	}

	/// <summary>
	/// Automatically sets certain static defaults. Override this if you do not want the properties to be set for you.
	/// </summary>
	public virtual void AutoStaticDefaults()
	{
		TextureAssets.PlayerHair[Type] = ModContent.Request<Texture2D>(Texture);
		TextureAssets.PlayerHairAlt[Type] = ModContent.Request<Texture2D>(AltTexture);
	}

	/// <summary>
	/// Gets the unlock conditions for this hairstyle. No conditions by default.
	/// </summary>
	public virtual IEnumerable<Condition> GetUnlockConditions()
	{
		return Enumerable.Empty<Condition>();
	}
}