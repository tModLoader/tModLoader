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
	/// Used to set the character gender based on hairstyle when randomizing a new character. <br />
	/// If <see cref="Gender.Unspecified" />, the gender will be randomly rolled. <br />
	/// Note that all hairstyles can be selected with either gender. THis is just a defualt for quick randomization.
	/// </summary>
	public virtual Gender RandomizedCharacterCreationGender => Gender.Unspecified;

	/// <summary>
	/// Determines in what UIs this hairstyle is visible once unlocked.
	/// </summary>
	public virtual HairVisibility Visibility => HairVisibility.All;

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
	/// Gets the unlock conditions for this hairstyle. No conditions by default. <br />
	/// Ignored and not called if <see cref="Visibility"/> does includes <see cref="HairVisibility.CharacterCreation"/>.
	/// </summary>
	public virtual IEnumerable<Condition> GetUnlockConditions()
	{
		return Enumerable.Empty<Condition>();
	}
}