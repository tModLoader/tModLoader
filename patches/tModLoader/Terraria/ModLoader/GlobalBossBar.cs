using Microsoft.Xna.Framework.Graphics;

namespace Terraria.ModLoader
{
	/// <summary>
	/// A class that is used to modify existing boss health bars. To add them, use ModBossBar instead.
	/// </summary>
	public abstract class GlobalBossBar : ModType
	{
		protected sealed override void Register() => BossBarLoader.AddGlobalBossBar(this);

		/// <summary>
		/// Allows you to draw things before the default draw code is ran. Return false to prevent drawing the ModBossBar. Returns true by default.
		/// </summary>
		/// <param name="spriteBatch">The spriteBatch that is drawn on</param>
		/// <param name="npc">The NPC this ModBossBar is focused on</param>
		/// <param name="drawParams">The draw parameters for the boss bar</param>
		/// <returns><see langword="true"/> for allowing drawing, <see langword="false"/> for preventing drawing</returns>
		public virtual bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) => true;

		/// <summary>
		/// Allows you to draw things after the bar has been drawn. skipped is true if you or another mod has skipped drawing the bar (possibly hiding it or in favor of new visuals).
		/// </summary>
		/// <param name="skipped"><see langword="true"/> if you or another mod has skipped drawing the bar in PreDraw (possibly hiding it or in favor of new visuals)</param>
		/// <param name="spriteBatch">The spriteBatch that is drawn on</param>
		/// <param name="npc">The NPC this ModBossBar is focused on</param>
		/// <param name="drawParams">The draw parameters for the boss bar</param>
		public virtual void PostDraw(bool skipped, SpriteBatch spriteBatch, NPC npc, BossBarDrawParams drawParams) {
		}
	}
}
