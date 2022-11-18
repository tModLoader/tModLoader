using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;

namespace Terraria.ModLoader;

/// <summary>
/// A class that is used to modify existing boss health bars. To add them, use ModBossBar instead.
/// </summary>
public abstract class GlobalBossBar : ModType
{
	protected sealed override void Register() => BossBarLoader.AddGlobalBossBar(this);

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// Allows you to draw things before the default draw code is ran. Return false to prevent drawing the bar. Returns true by default.
	/// </summary>
	/// <param name="spriteBatch">The spriteBatch that is drawn on</param>
	/// <param name="npc">The NPC this bar is focused on</param>
	/// <param name="drawParams">The draw parameters for the boss bar</param>
	/// <returns><see langword="true"/> for allowing drawing, <see langword="false"/> for preventing drawing</returns>
	public virtual bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) => true;

	/// <summary>
	/// Allows you to draw things after the bar has been drawn. skipped is true if you or another mod has skipped drawing the bar (possibly hiding it or in favor of new visuals).
	/// </summary>
	/// <param name="spriteBatch">The spriteBatch that is drawn on</param>
	/// <param name="npc">The NPC this bar is focused on</param>
	/// <param name="drawParams">The draw parameters for the boss bar</param>
	public virtual void PostDraw(SpriteBatch spriteBatch, NPC npc, BossBarDrawParams drawParams)
	{
	}
}
