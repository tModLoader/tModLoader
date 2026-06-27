using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ID;

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
	/// Note that all hairstyles can be selected with either gender. This is just a default for quick randomization.
	/// </summary>
	public virtual Gender RandomizedCharacterCreationGender => Gender.Unspecified;

	/// <summary>
	/// Determines whether this hairstyle is available during character creation. <br />
	/// This is distinctly different from <see cref="GetUnlockConditions" />, which determines whether the hairstyle
	/// is available in-game in the Stylist UI.
	/// </summary>
	public virtual bool AvailableDuringCharacterCreation => true;

	protected sealed override void Register()
	{
		ModTypeLookup<ModHair>.Register(this);
		Type = HairLoader.Register(this);
		HairID.Search.Add(FullName, Type);
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
	/// These conditions are used exclusively for the Stylist UI in-game; see <see cref="AvailableDuringCharacterCreation" /> for character creation.
	/// </summary>
	public virtual IEnumerable<Condition> GetUnlockConditions()
	{
		return Enumerable.Empty<Condition>();
	}
}