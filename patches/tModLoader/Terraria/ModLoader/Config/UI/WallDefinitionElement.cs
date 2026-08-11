using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader.Default;
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

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		CalculatedStyle dimensions = GetInnerDimensions();
		spriteBatch.Draw(BackgroundTexture.Value, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

		if (Definition != null && Definition.Type != 0) {
			int type = Unloaded ? ModContent.WallType<UnloadedWall>() : Definition.Type;
			if (TextureAssets.Wall[type].State == AssetState.NotLoaded)
				Main.Assets.Request<Texture2D>(TextureAssets.Wall[type].Name, AssetRequestMode.AsyncLoad);
			Texture2D wallTexture = TextureAssets.Wall[type].Value;

			if (wallTexture != null) {
				int size = 32;
				Rectangle sourceRectangle = new Rectangle(324, 108, size, size);
				var position = dimensions.Center();

				spriteBatch.Draw(wallTexture, position, sourceRectangle, Color.White, 0f, Vector2.One * 16, Scale, SpriteEffects.None, 0f);

				if (!Main.wallHouse[type]) {
					Vector2 unsafeIndicatorOffset = dimensions.ToRectangle().Size() * 0.2f * new Vector2(1f, -1f);
					Texture2D unsafeIndicatorTexture = TextureAssets.Extra[ExtrasID.UnsafeIndicator].Value;
					Rectangle unsafeIndicatorFrame = unsafeIndicatorTexture.Frame();
					spriteBatch.Draw(unsafeIndicatorTexture, position + unsafeIndicatorOffset, unsafeIndicatorFrame, Color.White, 0f, unsafeIndicatorFrame.Size() / 2f, Scale, SpriteEffects.None, 0f);
				}
			}
		}

		if (IsMouseHovering)
			UIModConfig.Tooltip = Tooltip;
	}
}
