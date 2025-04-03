using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.BigProgressBar;

namespace Terraria.ModLoader;

/// <summary>
/// A class that is used to swap out the entire boss bar display system with your own implementation
/// </summary>
public abstract class ModBossBarStyle : ModType
{
	/// <summary>
	/// Checks if the selected style matches this ModBossBarStyle.
	/// </summary>
	public bool IsSelected => BossBarLoader.CurrentStyle == this;

	//TODO Localization
	/// <summary>
	/// Controls the name that shows up in the menu selection. If not overridden, it will use this mod's display name.
	/// </summary>
	public virtual string DisplayName => Mod.DisplayNameClean;

	/// <summary>
	/// Return true to skip update code for boss bars. Useful if you want to use your own code for finding out which NPCs to track. Returns false by default.
	/// </summary>
	public virtual bool PreventUpdate => false;

	/// <summary>
	/// Return true to skip draw code for boss bars. Useful if you want to use your own code for drawing boss bars. Returns false by default.
	/// </summary>
	public virtual bool PreventDraw => false;

	protected sealed override void Register() => BossBarLoader.AddBossBarStyle(this);

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// Runs after update code for boss bars (skipped if PreventUpdate returns true), can be used to identify which NPCs to draw.
	/// </summary>
	/// <param name="currentBar">The boss bar that vanilla update code decided to draw. Can be null if skipped or if no suitable NPCs found. Can be casted to ModBossBar</param>
	/// <param name="info">Contains the index of the NPC the game decided to focus on</param>
	public virtual void Update(IBigProgressBar currentBar, ref BigProgressBarInfo info)
	{
	}

	/// <summary>
	/// Called when this ModBossBarStyle is selected
	/// </summary>
	public virtual void OnSelected()
	{
	}

	/// <summary>
	/// Called when this ModBossBarStyle is deselected
	/// </summary>
	public virtual void OnDeselected()
	{
	}

	/// <summary>
	/// Runs after draw code for boss bars (skipped if PreventDraw returns true), can be used to draw your own bars, or reinvoke draw for currently selected-to-draw bar
	/// </summary>
	/// <param name="spriteBatch">The spriteBatch that is drawn on</param>
	/// <param name="currentBar">The boss bar that vanilla update code decided to draw. Can be null if skipped or if no suitable NPCs found. Can be casted to ModBossBar</param>
	/// <param name="info">Contains the index of the NPC the game decided to focus on</param>
	public virtual void Draw(SpriteBatch spriteBatch, IBigProgressBar currentBar, BigProgressBarInfo info)
	{
	}
}
