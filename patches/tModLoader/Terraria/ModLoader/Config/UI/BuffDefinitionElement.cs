using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;

internal class BuffDefinitionElement : DefinitionElement<BuffDefinition>
{
	protected override DefinitionOptionElement<BuffDefinition> CreateDefinitionOptionElement() => new BuffDefinitionOptionElement(Value, 0.5f);

	protected override List<DefinitionOptionElement<BuffDefinition>> CreateDefinitionOptionElementList()
	{
		var options = new List<DefinitionOptionElement<BuffDefinition>>();

		for (int i = 0; i < BuffLoader.BuffCount; i++) {
			// The first buff from BuffID is null, so it's better to create an empty BuffDefinition.
			var buffDefinition = i == 0 ? new BuffDefinition() : new BuffDefinition(i);
			var optionElement = new BuffDefinitionOptionElement(buffDefinition, OptionScale);
			optionElement.OnLeftClick += (a, b) => {
				Value = optionElement.Definition;
				UpdateNeeded = true;
				SelectionExpanded = false;
			};
			options.Add(optionElement);
		}

		return options;
	}

	protected override List<DefinitionOptionElement<BuffDefinition>> GetPassedOptionElements()
	{
		var passed = new List<DefinitionOptionElement<BuffDefinition>>();

		foreach (var option in Options) {
			// Should this be the localized buff name?
			if (!Lang.GetBuffName(option.Type).Contains(ChooserFilter.CurrentString, StringComparison.OrdinalIgnoreCase))
				continue;

			string modname = "Terraria";

			if (option.Type >= BuffID.Count) {
				modname = BuffLoader.GetBuff(option.Type).Mod.DisplayNameClean; // or internal name?
			}

			if (modname.IndexOf(ChooserFilterMod.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
				continue;

			passed.Add(option);
		}
		return passed;
	}
}

internal class BuffDefinitionOptionElement : DefinitionOptionElement<BuffDefinition>
{
	public BuffDefinitionOptionElement(BuffDefinition definition, float scale = 0.5f) : base(definition, scale)
	{
	}

	public override void SetItem(BuffDefinition definition)
	{
		base.SetItem(definition);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		CalculatedStyle dimensions = GetInnerDimensions();

		spriteBatch.Draw(BackgroundTexture.Value, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

		if (Definition != null) {
			int type = Unloaded ? 0 : Type;

			Texture2D buffTexture;

			if (type == 0) {
				// Use ItemID.None as the empty buff texture.
				buffTexture = TextureAssets.Item[ItemID.None].Value;
			}
			else {
				buffTexture = TextureAssets.Buff[type].Value;
			}

			int frameCounter = Interface.modConfig.UpdateCount / 4;
			//int frames = Main.projFrames[type];
			int frames = 1;

			if (Unloaded) {
				buffTexture = TextureAssets.Item[ModContent.ItemType<UnloadedItem>()].Value;
				frames = 1;
			}

			int height = buffTexture.Height / frames;
			int width = buffTexture.Width;
			int frame = frameCounter % frames;
			int y = height * frame;
			var rectangle2 = new Rectangle(0, y, width, height);

			float drawScale = 1f;
			float availableWidth = (float)DefaultBackgroundTexture.Width() * Scale;

			if (width > availableWidth || height > availableWidth) {
				if (width > height) {
					drawScale = availableWidth / width;
				}
				else {
					drawScale = availableWidth / height;
				}
			}

			Vector2 vector = BackgroundTexture.Size() * Scale;
			Vector2 position2 = dimensions.Position() + vector / 2f - rectangle2.Size() * drawScale / 2f;
			Vector2 origin = rectangle2.Size() * 0/* * (pulseScale / 2f - 0.5f)*/;

			spriteBatch.Draw(buffTexture, position2, rectangle2, Color.White, 0f, origin, drawScale, SpriteEffects.None, 0f);
		}

		if (IsMouseHovering)
			UIModConfig.Tooltip = Tooltip;
	}
}
