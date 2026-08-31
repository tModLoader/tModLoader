using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Minimap;
using Terraria.Localization;

namespace Terraria.ModLoader;

/// <summary>
/// Defines a custom minimap border that is added to the vanilla minimap border selection menu.
/// </summary>
[Autoload(true, Side = ModSide.Client)]
public abstract class ModMinimapFrame : ModTexturedType, IConfigKeyHolder, ILocalizedModType
{
	/// <summary>
	/// The asset path of the frame texture, without the extension. By default this is derived from the concrete type's namespace and name.
	/// Use a <c>Terraria/...</c> path to reuse a vanilla asset.
	/// </summary>
	public override string Texture => base.Texture;

	/// <summary>The asset path of the zoom-in button texture, without the extension.</summary>
	public virtual string ZoomInTexture => Texture + "_ZoomIn";

	/// <summary>The asset path of the zoom-out button texture, without the extension.</summary>
	public virtual string ZoomOutTexture => Texture + "_ZoomOut";

	/// <summary>The asset path of the reset button texture, without the extension.</summary>
	public virtual string ResetTexture => Texture + "_Reset";

	/// <summary>The frame position offset relative to the map background.</summary>
	public virtual Vector2 FrameOffset => new(-10f, -10f);

	/// <summary>The position of the reset button relative to the frame texture.</summary>
	public virtual Vector2 ResetButtonPosition => new(200f, 234f);

	/// <summary>The position of the zoom-in button relative to the frame texture.</summary>
	public virtual Vector2 ZoomInButtonPosition => new(148f, 234f);

	/// <summary>The position of the zoom-out button relative to the frame texture.</summary>
	public virtual Vector2 ZoomOutButtonPosition => new(174f, 234f);

	public string ConfigKey => FullName;
	public string NameKey => DisplayName.Key;
	public virtual string LocalizationCategory => "MinimapFrames";
	public virtual LocalizedText DisplayName => this.GetLocalization(nameof(DisplayName), PrettyPrintName);

	internal MinimapFrame CreateInstance(AssetRequestMode mode)
	{
		var frame = new MinimapFrame(ModContent.Request<Texture2D>(Texture, mode), FrameOffset) {
			ConfigKey = ConfigKey,
			NameKey = NameKey
		};
		frame.SetResetButton(ModContent.Request<Texture2D>(ResetTexture, mode), ResetButtonPosition);
		frame.SetZoomInButton(ModContent.Request<Texture2D>(ZoomInTexture, mode), ZoomInButtonPosition);
		frame.SetZoomOutButton(ModContent.Request<Texture2D>(ZoomOutTexture, mode), ZoomOutButtonPosition);
		return frame;
	}

	protected sealed override void Register()
	{
		ModTypeLookup<ModMinimapFrame>.Register(this);
		MinimapFrameLoader.Add(this);
	}

	public sealed override void SetupContent()
	{
		// Register the default display name so localization files are updated automatically.
		_ = DisplayName;
		SetStaticDefaults();
	}
}
