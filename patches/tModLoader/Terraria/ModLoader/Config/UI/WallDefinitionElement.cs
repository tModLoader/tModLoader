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

	protected override List<DefinitionOptionElement<WallDefinition>> GetPassedOptionElements()
	{
		var passed = new List<DefinitionOptionElement<WallDefinition>>();
		foreach (var option in Options) {
			// Filter by the text in the search bar
			if (option.Definition.DisplayName.IndexOf(ChooserFilter.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
				continue;

			passed.Add(option);
		}
		return passed;
	}

	protected override List<DefinitionOptionElement<WallDefinition>> CreateDefinitionOptionElementList()
	{
		return WallDefinitionOptionElement.GetWallOptionsSetup();
	}
}

internal class WallDefinitionOptionElement : DefinitionOptionElement<WallDefinition>
{
	public WallDefinitionOptionElement(WallDefinition definition, float scale) : base(definition, scale) { }

	public static List<DefinitionOptionElement<WallDefinition>> GetWallOptionsSetup()
	{
		var options = new List<DefinitionOptionElement<WallDefinition>>();

		options.Add(new WallDefinitionOptionElement(new WallDefinition(0), 0.5f));

		for (int i = 1; i < WallLoader.WallCount; i++) {
			options.Add(new WallDefinitionOptionElement(new WallDefinition(i), 0.5f));
		}

		return options;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		CalculatedStyle dimensions = GetInnerDimensions();

		if (IsMouseHovering) {
			UIModConfig.Tooltip = Definition.DisplayName;
		}

		if (Definition.Type <= 0)
			return;

		Main.instance.LoadWall(Definition.Type);
		Texture2D wallTexture = TextureAssets.Wall[Definition.Type].Value;

		if (wallTexture != null) {
			// Walls use 32x32 frames typically, we grab the first frame
			int size = 32;
			Rectangle sourceRectangle = new Rectangle(0, 0, size, size);

			// Center the texture snippet inside the UI box
			Vector2 position = new Vector2(
				dimensions.X + dimensions.Width / 2f - size / 2f,
				dimensions.Y + dimensions.Height / 2f - size / 2f
			);

			spriteBatch.Draw(wallTexture, position, sourceRectangle, Color.White);
		}
	}
}