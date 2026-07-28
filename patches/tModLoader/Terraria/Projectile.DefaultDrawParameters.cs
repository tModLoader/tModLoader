using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Terraria;


public partial class Projectile
{
	/// <summary>
	/// The values vanilla uses to draw a projectile through the generic draw path.
	/// Projectiles that vanilla draws specially (by some <see cref="type"/> values and by some
	/// <see cref="aiStyle"/> values) are not replicated.
	///	</summary>
	public struct DefaultDrawParameters
	{
		public Texture2D texture;
		public Rectangle sourceRectangle;
		public Vector2 position;
		public Vector2 origin;
		public SpriteEffects effects;
	}

	/// <summary>
	/// Calculates the values vanilla would pass to <c>Main.EntitySpriteDraw</c> for this projectile,
	/// so custom drawings can reuse them instead of recreating the math.
	/// Omitted rotation, scale, and color: maybe use <see cref="rotation"/>, <see cref="scale"/>, and <see cref="GetAlpha"/>
	/// </summary>
	/// <param name="player">
	/// Owner of the projectile.
	/// </param>
	/// <returns>Vanilla's default draw parameters for this projectile</returns>
	public DefaultDrawParameters GetDefaultDrawParameters(Player player = null)
	{
		Texture2D texture = TextureAssets.Projectile[type].Value;

		// Mirrors the offset setup at the top of Main.DrawProj_DrawNormalProjs
		// Vanilla starts at zero and applies specific overrides depending on the type, then calls DrawOffset
		// The type-specific overrides are intentionally skipped here.
		int drawOffsetX = 0;
		int originOffsetY = 0;

		float originX = (texture.Width - width) * 0.5f + width * 0.5f; // Vanilla computes: (texWidth - width) * 0.5f + width * 0.5f

		// Same hook vanilla calls, so ModProjectile's DrawOffsetX/DrawOriginOffsetY/DrawOriginOffsetX applies
		ProjectileLoader.DrawOffset(this, ref drawOffsetX, ref originOffsetY, ref originX);

		SpriteEffects effects = spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

		// Vanilla splits these into two separate paths in DrawProj_DrawNormalProjs:
		// Animated projectiles draw a single frame while everything else draws the whole texture
		Rectangle sourceRectangle;

		if (Main.projFrames[type] > 1) {
			int frameHeight = texture.Height / Main.projFrames[type];
			sourceRectangle = new Rectangle(0, frameHeight * frame, texture.Width, frameHeight - 1);
		}
		else {
			sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);

			if (player != null && ownerHitCheck && player.gravDir == -1f) {
				if (player.direction == 1)
					effects = SpriteEffects.FlipHorizontally;
				else if (player.direction == -1)
					effects = SpriteEffects.None;
			}
		}

		return new DefaultDrawParameters {
			texture = texture,
			sourceRectangle = sourceRectangle,
			position = new Vector2(
				position.X - Main.screenPosition.X + originX + drawOffsetX,
				position.Y - Main.screenPosition.Y + height / 2 + gfxOffY),
			origin = new Vector2(originX, height / 2 + originOffsetY),
			effects = effects
		};
	}
}


