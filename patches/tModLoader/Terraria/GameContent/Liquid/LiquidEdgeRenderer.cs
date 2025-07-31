#nullable enable

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ID;

namespace Terraria.GameContent.Liquid;

/// <summary>
/// Responsible for special rendering of liquid edges/slopes for the rewritten
/// liquid slope handling.
/// </summary>
/// <remarks>
/// See the related pull request:
/// https://github.com/tModLoader/tModLoader/pull/4714
/// </remarks>
public static class LiquidEdgeRenderer
{
	/// <summary>
	/// Whether the special edge rendering logic is enabled.
	/// <br />
	/// Even if it's enabled, it will only apply if <see cref="Active"/>
	/// is <see langword="true"/>.
	/// </summary>
	public static bool Enabled = true;

	/// <summary>
	/// Whether the new rendering is actually active for this frame.
	/// </summary>
	public static bool Active => !Main.keyState.PressingShift() && Lighting.Mode is Graphics.Light.LightMode.Color or Graphics.Light.LightMode.White;

	public static Effect MaskShader => (maskShaderAsset ??= ModLoader.ModLoader.ManifestAssets.Request<Effect>("Terraria.GameContent.Liquid.LiquidMask", AssetRequestMode.ImmediateLoad)).Value;

	private static Asset<Effect>? maskShaderAsset;

	public static Texture2D MaskTile => (maskTileAsset ??= ModLoader.ModLoader.ManifestAssets.Request<Texture2D>("Terraria.GameContent.Liquid.DefaultTileLiquidMask", AssetRequestMode.ImmediateLoad)).Value;

	private static Asset<Texture2D>? maskTileAsset;

	public static Dictionary<int, Asset<Texture2D>> CustomTileMasks = new Dictionary<int, Asset<Texture2D>>();

	public static readonly BlendState MaskingBlendState = new BlendState() {
		ColorSourceBlend = Blend.Zero,
		AlphaSourceBlend = Blend.Zero,
		ColorDestinationBlend = Blend.InverseSourceAlpha,
		AlphaDestinationBlend = Blend.InverseSourceAlpha
	};

	/// <summary>
	/// Tiles which mask rendered liquid (tiles on the edge of bodies of
	/// liquid).
	/// </summary>
	public static HashSet<Point> Edges { get; } = [];

	public static void DrawSingleTileMask(SpriteBatch spriteBatch, int tileX, int tileY)
	{
		Tile tileCache = Main.tile[tileX, tileY];

		Texture2D texture = MaskTile;

		//if (CustomTileMasks.TryGetValue(tileCache.TileType, out Asset<Texture2D>? newTexture) && newTexture != null) {
		//	if (!newTexture.IsLoaded) {
		//		// Load masks here
		//	}
		//	texture = newTexture.Value;
		//}

		Vector2 position = new Vector2(tileX * 16, tileY * 16) + new Vector2(Main.drawToScreen ? 0 : Main.offScreenRange) - Main.screenPosition;

		if (tileCache.Slope != SlopeType.Solid && !TileID.Sets.HasSlopeFrames[tileCache.TileType]) {
			int slopeType = (int)tileCache.Slope;
			for (int i = 0; i < 8; i++) {
				int slopePosY = i * -2;
				int slopeHeight = 16 - i * 2;
				int slopeOffsetY = 16 - slopeHeight;
				int slopePosX;
				switch (slopeType) {
					case 1:
						slopePosY = 0;
						slopePosX = i * 2;
						slopeHeight = 14 - i * 2;
						slopeOffsetY = 0;
						break;
					case 2:
						slopePosY = 0;
						slopePosX = 16 - i * 2 - 2;
						slopeHeight = 14 - i * 2;
						slopeOffsetY = 0;
						break;
					case 3:
						slopePosX = i * 2;
						break;
					default:
						slopePosX = 16 - i * 2 - 2;
						break;
				}

				spriteBatch.Draw(texture, position + new Vector2(slopePosX, i * 2 + slopePosY), new Rectangle(tileCache.TileFrameX + slopePosX, tileCache.TileFrameY + slopeOffsetY, 2, slopeHeight), Color.White, 0f, Vector2.Zero, 1f, 0, 0f);
			}

			int slopeTopOrBottom = ((slopeType <= 2) ? 14 : 0);
			spriteBatch.Draw(texture, position + new Vector2(0f, slopeTopOrBottom), new Rectangle(tileCache.TileFrameX, tileCache.TileFrameY + slopeTopOrBottom, 16, 2), Color.White, 0f, Vector2.Zero, 1f, 0, 0f);
		}
		else {
			int fullTileHeight = 0;
			if (tileCache.IsHalfBlock) {
				fullTileHeight += 8;
			}

			spriteBatch.Draw(texture, position + new Vector2(0, fullTileHeight), new Rectangle(tileCache.TileFrameX, tileCache.TileFrameY + fullTileHeight, 16, 16 - fullTileHeight), Color.White, 0f, Vector2.Zero, 1f, 0, 0f);
		}
	}
}
