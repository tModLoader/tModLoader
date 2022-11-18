using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.ModLoader.Default;

/// <summary>
/// This is the default modmenu - the one that tML uses and the default one upon entering the game for the first time.
/// </summary>
[Autoload(false)]
internal class MenutML : ModMenu
{
	public override string DisplayName => "tModLoader";

	public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
	{
		logoScale *= 0.84f;

		return true;
	}
}

/// <summary>
/// The Journey's End theme converted into a ModMenu, so that it better fits with the new system.
/// </summary>
[Autoload(false)]
internal class MenuJourneysEnd : ModMenu
{
	public override string DisplayName => "Journey's End";

	public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor) => false;
}

/// <summary>
/// The Terraria 1.3.5.3 theme converted into a ModMenu, so that it better fits with the new system.
/// </summary>
[Autoload(false)]
internal class MenuOldVanilla : ModMenu
{
	public override bool IsAvailable => Main.instance.playOldTile;

	public override string DisplayName => "Terraria 1.3.5.3";

	public override int Music => MusicID.Title;

	public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor) => false;
}
