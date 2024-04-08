using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;

internal class ProjectileDefinitionElement : DefinitionElement<ProjectileDefinition>
{
	protected override DefinitionOptionElement<ProjectileDefinition> CreateDefinitionOptionElement() => new ProjectileDefinitionOptionElement(Value, 0.5f);

	protected override List<DefinitionOptionElement<ProjectileDefinition>> CreateDefinitionOptionElementList()
	{
		var options = new List<DefinitionOptionElement<ProjectileDefinition>>();

		for (int i = 0; i < ProjectileLoader.ProjectileCount; i++) {
			var optionElement = new ProjectileDefinitionOptionElement(new ProjectileDefinition(i), OptionScale);
			optionElement.OnLeftClick += (a, b) => {
				Value = optionElement.Definition;
				UpdateNeeded = true;
				SelectionExpanded = false;
			};
			options.Add(optionElement);
		}

		return options;
	}

	protected override List<DefinitionOptionElement<ProjectileDefinition>> GetPassedOptionElements()
	{
		var passed = new List<DefinitionOptionElement<ProjectileDefinition>>();

		foreach (var option in Options) {
			// Should this be the localized projectile name?
			if (Lang.GetProjectileName(option.Type).Value.IndexOf(ChooserFilter.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
				continue;

			string modname = option.Definition.Mod;

			if (option.Type >= ProjectileID.Count) {
				modname = ProjectileLoader.GetProjectile(option.Type).Mod.DisplayNameClean; // or internal name?
			}

			if (!modname.Contains(ChooserFilterMod.CurrentString, StringComparison.OrdinalIgnoreCase))
				continue;

			passed.Add(option);
		}

		return passed;
	}
}

internal class ProjectileDefinitionOptionElement : DefinitionOptionElement<ProjectileDefinition>
{
	public ProjectileDefinitionOptionElement(ProjectileDefinition definition, float scale = .75f) : base(definition, scale)
	{
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		CalculatedStyle dimensions = GetInnerDimensions();

		spriteBatch.Draw(BackgroundTexture.Value, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

		if (Definition != null) {
			int type = Unloaded ? ProjectileID.None : Type;
			if (TextureAssets.Projectile[type].State == AssetState.NotLoaded)
				Main.Assets.Request<Texture2D>(TextureAssets.Projectile[type].Name, AssetRequestMode.AsyncLoad);
			Texture2D projectileTexture = TextureAssets.Projectile[type].Value;

			int frameCounter = Interface.modConfig.UpdateCount / 4;
			int frames = Main.projFrames[type];

			if (Unloaded) {
				projectileTexture = TextureAssets.Item[ModContent.ItemType<UnloadedItem>()].Value;
				frames = 1;
			}

			int height = projectileTexture.Height / frames;
			int width = projectileTexture.Width;
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

			drawScale *= Scale;

			Vector2 vector = BackgroundTexture.Size() * Scale;
			Vector2 position2 = dimensions.Position() + vector / 2f - rectangle2.Size() * drawScale / 2f;
			Vector2 origin = rectangle2.Size() * 0/* * (pulseScale / 2f - 0.5f)*/;

			spriteBatch.Draw(projectileTexture, position2, rectangle2, Color.White, 0f, origin, drawScale, SpriteEffects.None, 0f);
		}

		if (IsMouseHovering)
			UIModConfig.Tooltip = Tooltip;
	}
}
