using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
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
			var optionElement = new TileDefinitionOptionElement(i == -1 ? null : new TileDefinition(i), OptionScale);
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
				modname = TileLoader.GetTile(option.Type).Mod.DisplayName; // or internal name?
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

		if (Definition != null) {
			int type = Definition.Type;

			// Framed tiles: draw a part of the texture making it look like one solitary block
			// FrameImportant tiles: use TileObjectData to get the correct parts of the texture and draw tehm
			if (Main.tileFrameImportant[type]) {
				// TODO: frameimportatnt tiles
				var objData = TileObjectData.GetTileData(type, 0);

				Draw1x1Tile(type, new Point(0, 0));
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
	}
}
