#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
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

	/// <summary>
	/// Turns all pixels with alpha above zero white, and all others transparent.
	/// </summary>
	public static Effect MaskShader => (maskShaderAsset ??= ModLoader.ModLoader.ManifestAssets.Request<Effect>("Terraria.GameContent.Liquid.LiquidMask", AssetRequestMode.ImmediateLoad)).Value;

	private static Asset<Effect>? maskShaderAsset;

	/// <summary>
	/// The default liquid mask tile for tiles in <see cref="TileID.Sets.BlocksWaterDrawingBehindSelf"/>.
	/// </summary>
	public static Texture2D MaskTile => (maskTileAsset ??= ModLoader.ModLoader.ManifestAssets.Request<Texture2D>("Terraria.GameContent.Liquid.DefaultTileLiquidMask", AssetRequestMode.ImmediateLoad)).Value;

	private static Asset<Texture2D>? maskTileAsset;

	/// <summary>
	/// Contains liquid mask textures for specific block types that have funny shapes but still need to hide water.
	/// <br />
	/// Only shows up when the tile is part of the <see cref="TileID.Sets.BlocksWaterDrawingBehindSelf"/> set.
	/// </summary>
	public static Asset<Texture2D>[] TileLiquidMasks = [];

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
	public static List<Point> Edges { get; } = [];

	public static void DrawSingleTileMask(SpriteBatch spriteBatch, int tileX, int tileY)
	{
		Tile tileCache = Main.tile[tileX, tileY];

		Texture2D texture = MaskTile;

		// Check if a custom mask is loaded for the tile and use it if so
		if (TileLiquidMasks.IndexInRange(tileCache.type) && (TileLiquidMasks[tileCache.type]?.IsLoaded ?? false)) {
			texture = TileLiquidMasks[tileCache.type].Value;
		}
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

	public static unsafe void CollectEdgeData(LiquidRenderer.LiquidCache* pCache, Tile tileCache, int tileX, int tileY)
	{
		pCache->EdgeData = null;

		if (!Active)
			return;

		Tile tileRightCache = Main.tile[tileX + 1, tileY];
		Tile tileLeftCache = Main.tile[tileX - 1, tileY];
		Tile tileUpCache = Main.tile[tileX, tileY - 1];
		Tile tileDownCache = Main.tile[tileX, tileY + 1];

		if (!tileCache.HasTile || tileCache.IsActuated || Main.tileSolidTop[tileCache.type] || (tileCache.IsHalfBlock && (tileLeftCache.liquid > 160 || tileRightCache.liquid > 160) && Main.instance.waterfallManager.CheckForWaterfall(tileX, tileY)))
			return;

		int liquidType = 0;

		int highLiquid = 0;
		bool left = false;
		bool right = false;
		bool up = false;
		bool down = false;
		bool self = false;
		SlopeType slope = tileCache.Slope;
		BlockType blockType = tileCache.BlockType;

		if (tileCache.type == TileID.Grate && tileCache.LiquidAmount > 0) {
			self = true;
			down = true;
			left = true;
			right = true;
			highLiquid = tileCache.LiquidAmount;
			liquidType = tileCache.LiquidType;
		}
		else {
			if (tileCache.LiquidAmount > 0 && blockType != BlockType.Solid && (blockType != BlockType.HalfBlock || tileCache.liquid > 160)) {
				//self = true;

				if (tileCache.LiquidAmount >= highLiquid) {
					highLiquid = tileCache.LiquidAmount;
					liquidType = tileCache.LiquidType;
				}
			}

			if (tileLeftCache.LiquidAmount > 0) {
				left = true;

				if (tileLeftCache.LiquidAmount >= highLiquid) {
					highLiquid = tileLeftCache.LiquidAmount;
					liquidType = tileLeftCache.LiquidType;
				}
			}

			if (tileRightCache.LiquidAmount > 0) {
				right = true;

				if (tileRightCache.LiquidAmount >= highLiquid) {
					highLiquid = tileRightCache.LiquidAmount;
					liquidType = tileRightCache.LiquidType;
				}
			}

			if (tileUpCache.LiquidAmount > 0) {
				up = true;

				// Always treat directly above as most important.
				highLiquid = 255;
				liquidType = tileUpCache.LiquidType;
			}

			if (tileDownCache.LiquidAmount > 252) {
				if (tileDownCache.LiquidType == liquidType || !up) {
					down = true;
					liquidType = tileDownCache.LiquidType;
				}
			}
		}

		if (!up && !down && !left && !right && !self)
			return;

		var exempt = tileCache.HasTile && (Main.tileSolidTop[tileCache.type] || !Main.tileSolid[tileCache.type]);
		if (exempt)
			return;

		Tile tileUpLeftCache = Main.tile[tileX - 1, tileY - 1];
		Tile tileUpRightCache = Main.tile[tileX + 1, tileY - 1];

		bool leftEmpty = !left && !(tileLeftCache.HasTile && Main.tileSolid[tileLeftCache.TileType] && tileLeftCache.BlockType == BlockType.Solid)
			&& !(tileLeftCache.BlockType is not BlockType.Solid && tileUpLeftCache.LiquidAmount > 0);

		bool rightEmpty = !right && !(tileRightCache.HasTile && Main.tileSolid[tileRightCache.TileType] && tileRightCache.BlockType == BlockType.Solid)
			&& !(tileRightCache.BlockType is not BlockType.Solid && tileUpRightCache.LiquidAmount > 0);

		if (slope == SlopeType.SlopeUpLeft && !left && rightEmpty)
			return;

		if (slope == SlopeType.SlopeUpRight && !right && leftEmpty)
			return;

		bool upLeftEmpty = left && !(tileUpLeftCache.HasTile && Main.tileSolid[tileUpLeftCache.TileType]) && tileUpLeftCache.LiquidAmount <= 0;
		bool upRightEmpty = right && !(tileUpRightCache.HasTile && Main.tileSolid[tileUpRightCache.TileType]) && tileUpRightCache.LiquidAmount <= 0;
		bool leftOrRightNotFull = (tileLeftCache.LiquidAmount > 0 && tileLeftCache.LiquidAmount < 250) || (tileRightCache.LiquidAmount < 250 && tileRightCache.LiquidAmount > 0);

		bool similarHeights = left && right ? Math.Abs(tileLeftCache.LiquidAmount - tileRightCache.LiquidAmount) < 100 : true;

		bool isSurfaceLiquid = !up && (similarHeights || !(tileUpCache.HasTile && Main.tileSolid[tileUpCache.TileType])) && (upLeftEmpty || upRightEmpty || leftOrRightNotFull);

		Rectangle size = new Rectangle(0, 0, 16, 16);
		Vector2 offset = Vector2.Zero;

		if (up && (left || right)) {
			size = new Rectangle(0, 6, 16, 16);
			if (!tileCache.IsHalfBlock && !down && !(tileDownCache.HasTile && Main.tileSolid[tileDownCache.TileType])) {
				size.Height = 12;
			}
		}
		else if (down && up) {
			size = new Rectangle(0, 0, 16, 16);
		}
		else if (up) {
			size = new Rectangle(0, 6, 16, 10);

			if (tileCache.IsHalfBlock || slope != SlopeType.Solid) {
				if (slope is SlopeType.SlopeUpLeft or SlopeType.SlopeUpRight) {
					size = new Rectangle(0, 6, 16, 2);
				}
				else if (slope is SlopeType.SlopeDownLeft or SlopeType.SlopeDownRight) {
					size = new Rectangle(0, 6, 16, 12);
				}
			}
		}
		else if (down && !left && !right) {
			offset = new Vector2(0, 12);
			size = new Rectangle(0, 12, 16, 4);
			highLiquid = 255;
			isSurfaceLiquid = false;
		}
		else {
			float depth = 256 - highLiquid;
			depth /= 32f;

			int width = down && tileDownCache.LiquidAmount > 250 ? 16 : 4;

			var depthPush = (int)(depth * 2);
			depthPush = Math.Min(12, depthPush);

			if (slope != SlopeType.Solid) {
				offset = new Vector2(0, depthPush);
				size = new Rectangle(0, depthPush, 16, 16 - depthPush);

				if (left && right) {
					if (slope is SlopeType.SlopeUpLeft or SlopeType.SlopeDownLeft) {
						highLiquid = tileRightCache.LiquidAmount;
					}
					else if (slope is SlopeType.SlopeUpRight or SlopeType.SlopeDownRight) {
						highLiquid = tileLeftCache.LiquidAmount;
					}

					int avgDepth = (int)((256 - highLiquid) / 32f) * 2;
					offset = new Vector2(0, avgDepth);
					size = new Rectangle(0, avgDepth, 16, 16 - avgDepth);
				}
				else if (left) {
					if (slope == SlopeType.SlopeDownLeft || slope == SlopeType.SlopeUpLeft) {
						offset = new Vector2(0, depthPush);
						size = new Rectangle(0, depthPush, 2, 16 - depthPush);
					}
					if (slope == SlopeType.SlopeDownRight || slope == SlopeType.SlopeUpRight) {
						offset = new Vector2(0, depthPush);
						size = new Rectangle(14, depthPush, 14, 16 - depthPush);
					}
	
				}
				else if (right) {
					if (slope == SlopeType.SlopeDownLeft || slope == SlopeType.SlopeUpLeft) {
						offset = new Vector2(2, depthPush);
						size = new Rectangle(2, depthPush, 14, 16 - depthPush);
					}
					if (slope == SlopeType.SlopeDownRight || slope == SlopeType.SlopeUpRight) {
						offset = new Vector2(14, depthPush);
						size = new Rectangle(14, depthPush, 2, 16 - depthPush);
					}
				}
			}
			else if ((left && right) || tileCache.IsHalfBlock) {
				highLiquid = (tileLeftCache.LiquidAmount + tileRightCache.LiquidAmount) / 2;
				int avgDepth = (int)((256 - highLiquid) / 32f) * 2;
				if (tileCache.IsHalfBlock)
					avgDepth = depthPush;
				offset = new Vector2(0, avgDepth);
				size = new Rectangle(0, 4, 16, 16 - avgDepth);
			}
			else if (left) {
				offset = new Vector2(0, depthPush);
				size = new Rectangle(0, 4, width, 16 - depthPush);
				if (rightEmpty && down) {
					size.Width -= 4;
				}
			}
			else if (right) {
				offset = new Vector2(16 - width, depthPush);
				size = new Rectangle(16 - width, 4, width, 16 - depthPush);
				if (leftEmpty && down) {
					offset.X += 4;
					size.Width -= 4;
				}
			}
		}

		size.X = 16;
		size.Y = isSurfaceLiquid ? 0 : 64;

		if (tileCache.IsHalfBlock && !down) {
			if (leftEmpty || rightEmpty)
				return;
		}

		var newEdgeData = new LiquidRenderer.LiquidEdgeData() {
			LiquidOffset = offset,
			SourceRectangle = size
		};

		Edges.Add(new Point(tileX, tileY));

		if (blockType is BlockType.HalfBlock) {
			if (!pCache->IsHalfBrick) {
				pCache->LiquidLevel = highLiquid / 255f;
				pCache->Type = (byte)liquidType;
			}
			pCache->EdgeData = newEdgeData;
		}
		else if (blockType is not BlockType.Solid) {
			Debug.Assert(pCache->IsSolid);

			pCache->LiquidLevel = highLiquid / 255f;
			pCache->Type = (byte)liquidType;
			pCache->EdgeData = newEdgeData;
		}
		else {
			Debug.Assert(pCache->IsSolid);

			pCache->LiquidLevel = highLiquid / 255f;
			pCache->Type = (byte)liquidType;
			pCache->EdgeData = newEdgeData;
		}
	}
}
