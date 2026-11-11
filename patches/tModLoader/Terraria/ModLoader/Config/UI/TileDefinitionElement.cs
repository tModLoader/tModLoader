using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.UI;
using Terraria.ObjectData;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;

internal class TileDefinitionElement : DefinitionElement<TileDefinition>
{
	protected override DefinitionOptionElement<TileDefinition> CreateDefinitionOptionElement() => new TileDefinitionOptionElement(Value, 0.5f);

	protected override List<DefinitionOptionElement<TileDefinition>> CreateDefinitionOptionElementList()
	{
		var options = new List<DefinitionOptionElement<TileDefinition>>();

		for (int i = -1; i < TileLoader.TileCount; i++) {
			var optionElement = new TileDefinitionOptionElement(i == -1 ? new TileDefinition() : new TileDefinition(i), OptionScale);
			optionElement.OnLeftClick += (a, b) => {
				Value = optionElement.Definition;
				UpdateNeeded = true;
				SelectionExpanded = false;
			};
			options.Add(optionElement);
		}

		return options;
	}

	protected override List<DefinitionOptionElement<TileDefinition>> GetPassedOptionElements()
	{
		var passed = new List<DefinitionOptionElement<TileDefinition>>();

		foreach (var option in Options) {
			// Should this be the localized tile name?
			if (!(option.Definition?.DisplayName ?? "").Contains(ChooserFilter.CurrentString, StringComparison.OrdinalIgnoreCase))
				continue;

			string modname = "Terraria";

			if (option.Type >= TileID.Count) {
				modname = TileLoader.GetTile(option.Type).Mod.DisplayNameClean; // or internal name?
			}

			if (modname.IndexOf(ChooserFilterMod.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
				continue;

			passed.Add(option);
		}

		return passed;
	}
}

internal class TileDefinitionOptionElement : DefinitionOptionElement<TileDefinition>
{
	public TileDefinitionOptionElement(TileDefinition definition, float scale = 0.5f) : base(definition, scale)
	{
	}

	public override void SetItem(TileDefinition definition)
	{
		NullID = -1;
		base.SetItem(definition);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		CalculatedStyle dimensions = GetInnerDimensions();
		spriteBatch.Draw(BackgroundTexture.Value, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

		if (Definition != null && (Type > NullID || Unloaded)) {
			int type = Unloaded ? ModContent.TileType<UnloadedSolidTile>() : Definition.Type;
			if (TextureAssets.Tile[type].State == AssetState.NotLoaded)
				Main.Assets.Request<Texture2D>(TextureAssets.Tile[type].Name, AssetRequestMode.AsyncLoad);

			// Framed tiles: draw a part of the texture making it look like one solitary block
			// FrameImportant tiles: use TileObjectData to get the correct parts of the texture and draw them
			if (Main.tileFrameImportant[type]) {
				var objData = TileObjectData.GetTileData(type, 0);

				if (objData != null) {
					DrawMultiTile(type, objData);
				}
				else {
					// Default to top left tile for FrameImportant tiles without TileObjectData
					Draw1x1Tile(type, new Point(0, 0));
				}
			}
			else {
				// Coordinates for where a tile isn't attached to anything (eg in the air)
				Draw1x1Tile(type, new Point(9, 3));
			}
		}

		if (IsMouseHovering)
			UIModConfig.Tooltip = Tooltip;

		void Draw1x1Tile(int type, Point coords, Point? offsetFromCenter = null)
		{
			var offset = offsetFromCenter ?? new Point(0, 0);
			var texture = TextureAssets.Tile[type];

			int frameX = coords.X * 18;
			int frameY = coords.Y * 18;
			var sourceRect = new Rectangle(frameX, frameY, 16, 16);
			var position = dimensions.Center() + offset.ToVector2() * 16;

			spriteBatch.Draw(texture.Value, position, sourceRect, Color.White, 0f, Vector2.One * 8, Scale * 2, SpriteEffects.None, 0);
		}

		// Adapted from TileObject.DrawPreview to draw at a specific location instead of tile coordinate
		void DrawMultiTile(int type, TileObjectData tileData)
		{
			// Fixes pink padding pixels drawing
			RasterizerState rasterizerState = spriteBatch.GraphicsDevice.RasterizerState;
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, rasterizerState, null, Main.UIScaleMatrix);

			Vector2 positionTopLeft = dimensions.Position() + new Vector2(4, 4);
			float drawDimensionHeight = dimensions.Height - 8;
			float drawDimensionWidth = dimensions.Width - 8;
			// Note: dimensions.Width already takes DefinitionOptionElement.Scale into account
			float drawScale = Math.Min(drawDimensionWidth / (tileData.CoordinateWidth * tileData.Width) , drawDimensionHeight / tileData.CoordinateHeights.Sum());
			float adjustX = tileData.Width < tileData.Height ? (tileData.Height - tileData.Width) / (tileData.Height * 2f) : 0f;
			float adjustY = tileData.Height < tileData.Width ? (tileData.Width - tileData.Height) / (tileData.Width * 2f) : 0f;

			int frameCounter = Interface.modConfig.UpdateCount / 60;
			int frames = tileData.SubTiles?.Count ?? 0; // For some reason, some TOD don't have SubTiles (389)
			int frame = frames > 0 ? frameCounter % frames : 0; // This cycles styles for tiles with declared subtiles, but actually doesn't cycle all styles, it's impossible to know how many styles a tile has.

			Texture2D tileTexture = TextureAssets.Tile[type].Value;
			int placeStyle = tileData.CalculatePlacementStyle(frame, 0, 0);
			int row = 0;
			int drawYOffset = tileData.DrawYOffset;
			int drawXOffset = tileData.DrawXOffset;
			placeStyle += tileData.DrawStyleOffset;
			int styleWrapLimit = tileData.StyleWrapLimit;
			int styleLineSkip = tileData.StyleLineSkip;
			if (tileData.StyleWrapLimitVisualOverride.HasValue)
				styleWrapLimit = tileData.StyleWrapLimitVisualOverride.Value;

			if (tileData.styleLineSkipVisualOverride.HasValue)
				styleLineSkip = tileData.styleLineSkipVisualOverride.Value;

			if (styleWrapLimit > 0) {
				row = placeStyle / styleWrapLimit * styleLineSkip;
				placeStyle %= styleWrapLimit;
			}

			int topLeftX;
			int topLeftY;
			if (tileData.StyleHorizontal) {
				topLeftX = tileData.CoordinateFullWidth * placeStyle;
				topLeftY = tileData.CoordinateFullHeight * row;
			}
			else {
				topLeftX = tileData.CoordinateFullWidth * row;
				topLeftY = tileData.CoordinateFullHeight * placeStyle;
			}

			int tileWidth = tileData.Width;
			int tileHeight = tileData.Height;
			int maxTileDimension = Math.Max(tileData.Width, tileData.Height);

			for (int i = 0; i < tileWidth; i++) {
				int x = topLeftX + i * (tileData.CoordinateWidth + tileData.CoordinatePadding);
				int y = topLeftY;
				for (int j = 0; j < tileHeight; j++) {
					if (j == 0 && tileData.DrawStepDown != 0)
						drawYOffset += tileData.DrawStepDown;

					if (type == 567)
						drawYOffset = (j != 0) ? tileData.DrawYOffset : (tileData.DrawYOffset - 2);

					int drawWidth = tileData.CoordinateWidth;
					int drawHeight = tileData.CoordinateHeights[j];
					if (type == 114 && j == 1)
						drawHeight += 2;

					spriteBatch.Draw(
						sourceRectangle: new Rectangle(x, y, drawWidth, drawHeight),
						texture: tileTexture,
						position: new Vector2(
							positionTopLeft.X + ((float)i / maxTileDimension + adjustX) * drawDimensionWidth /*+ drawXOffset*/,
							positionTopLeft.Y + ((float)j / maxTileDimension + adjustY) * drawDimensionHeight /*+ drawYOffset*/
						),
						color: Color.White, rotation: 0f, origin: Vector2.Zero, scale: drawScale, effects: SpriteEffects.None, layerDepth: 0f);
					y += drawHeight + tileData.CoordinatePadding;
				}
			}
		}
	}
}
