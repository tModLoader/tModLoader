using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;

internal class WallDefinitionElement : DefinitionElement<WallDefinition>
{
	protected override DefinitionOptionElement<WallDefinition> CreateDefinitionOptionElement() => new WallDefinitionOptionElement(Value, 0.5f);

	protected override List<DefinitionOptionElement<WallDefinition>> CreateDefinitionOptionElementList()
	{
		var options = new List<DefinitionOptionElement<WallDefinition>>();

		for (int i = 0; i < WallLoader.WallCount; i++) {
			var optionElement = new WallDefinitionOptionElement(new WallDefinition(i), OptionScale);
			optionElement.OnLeftClick += (a, b) => {
				Value = optionElement.Definition;
				UpdateNeeded = true;
				SelectionExpanded = false;
			};
			options.Add(optionElement);
		}

		return options;
	}

	protected override List<DefinitionOptionElement<WallDefinition>> GetPassedOptionElements()
	{
		var passed = new List<DefinitionOptionElement<WallDefinition>>();

		foreach (var option in Options) {
			if (!(option.Definition?.DisplayName ?? "").Contains(ChooserFilter.CurrentString, StringComparison.OrdinalIgnoreCase))
				continue;

			passed.Add(option);
		}

		return passed;
	}
}

internal class WallDefinitionOptionElement : DefinitionOptionElement<WallDefinition>
{
	public WallDefinitionOptionElement(WallDefinition definition, float scale = 0.5f) : base(definition, scale) { }

	public override void SetItem(WallDefinition definition)
	{
		NullID = 0;
		base.SetItem(definition);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		CalculatedStyle dimensions = GetInnerDimensions();
		spriteBatch.Draw(BackgroundTexture.Value, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

		if (Definition != null && Definition.Type > 0) {
			Main.instance.LoadWall(Definition.Type);
			Texture2D wallTexture = TextureAssets.Wall[Definition.Type].Value;

			if (wallTexture != null) {
				int size = 32;
				Rectangle sourceRectangle = new Rectangle(0, 0, size, size);
				var position = dimensions.Center();

				spriteBatch.Draw(wallTexture, position, sourceRectangle, Color.White, 0f, Vector2.One * 16, Scale, SpriteEffects.None, 0f);
			}
		}

		if (IsMouseHovering)
			UIModConfig.Tooltip = Definition.DisplayName;
	}
}
