using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.UI;

namespace Terraria.ModLoader;

/// <summary>
/// A class that is used to customize aesthetic features of the main menu, such as the logo, background and music.
/// </summary>
public abstract class ModMenu : ModType
{
	internal static Asset<Texture2D> modLoaderLogo;

	public UserInterface UserInterface { get; } = new UserInterface();

	public bool IsNew { get; internal set; }

	protected sealed override void Register()
	{
		MenuLoader.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// The logo texture shown when this ModMenu is active. If not overridden, it will use the tModLoader logo.
	/// </summary>
	public virtual Asset<Texture2D> Logo => modLoaderLogo ??= ModLoader.ManifestAssets.Request<Texture2D>("Terraria.ModLoader.Logo");

	/// <summary>
	/// The sun texture shown when this ModMenu is active. If not overridden, it will use the vanilla sun.
	/// </summary>
	public virtual Asset<Texture2D> SunTexture => TextureAssets.Sun;

	/// <summary>
	/// The moon texture shown when this ModMenu is active. If not overridden, it will use the vanilla moon.
	/// </summary>
	public virtual Asset<Texture2D> MoonTexture => TextureAssets.Moon[Utils.Clamp(Main.moonType, 0, 8)];

	/// <summary>
	/// The music that will be played while this ModMenu is active. If not overridden, it will use the vanilla music.
	/// </summary>
	public virtual int Music => 50; // Default 1.4 main menu music - used a magic number since the MusicID entry doesn't exist.

	/// <summary>
	/// The background style that will be used when this ModMenu is active. If not overridden, it will use the vanilla background.
	/// </summary>
	public virtual ModSurfaceBackgroundStyle MenuBackgroundStyle => null;

	/// <summary>
	/// Controls whether this ModMenu will be available to switch to. Useful if you want this menu to only be available at specific times.
	/// </summary>
	public virtual bool IsAvailable => true;

	//TODO Localization
	/// <summary>
	/// Controls the name that shows up at the base of the screen when this ModMenu is active. If not overridden, it will use this mod's display name.
	/// </summary>
	public virtual string DisplayName => Mod.DisplayName;

	public bool IsSelected => MenuLoader.CurrentMenu == this;

	/// <summary>
	/// Called when this ModMenu is selected. Set the state of the UserInterface to a given UIState to make that UIState appear on the main menu.
	/// </summary>
	public virtual void OnSelected()
	{
	}

	/// <summary>
	/// Called when this ModMenu is deselected.
	/// </summary>
	public virtual void OnDeselected()
	{
	}

	/// <summary>
	/// Called when this ModMenu's logic is updated.
	/// </summary>
	public virtual void Update(bool isOnTitleScreen)
	{
	}

	/// <summary>
	/// Called just before the logo is drawn, and allows you to modify some of the parameters of the logo draw code.
	/// </summary>
	public virtual bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor) => true;

	/// <summary>
	/// Called just after the logo is drawn, and gives the values of some of the parameters of the logo draw code.
	/// </summary>
	public virtual void PostDrawLogo(SpriteBatch spriteBatch, Vector2 logoDrawCenter, float logoRotation, float logoScale, Color drawColor)
	{
	}
}
