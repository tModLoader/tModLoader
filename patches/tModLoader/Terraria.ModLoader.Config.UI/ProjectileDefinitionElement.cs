using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI
{
	class ProjectileDefinitionElement : DefinitionElement<ProjectileDefinition>
	{
		protected override DefinitionOptionElement<ProjectileDefinition> CreateDefinitionOptionElement() => new ProjectileDefinitionOptionElement(Value, 0.5f);

		protected override List<DefinitionOptionElement<ProjectileDefinition>> CreateDefinitionOptionElementList() {
			var options = new List<DefinitionOptionElement<ProjectileDefinition>>();
			for (int i = 0; i < ProjectileLoader.ProjectileCount; i++) {
				var optionElement = new ProjectileDefinitionOptionElement(new ProjectileDefinition(i), optionScale);
				optionElement.OnClick += (a, b) => {
					Value = optionElement.definition;
					updateNeeded = true;
					selectionExpanded = false;
				};
				options.Add(optionElement);
			}
			return options;
		}

		protected override List<DefinitionOptionElement<ProjectileDefinition>> GetPassedOptionElements() {
			var passed = new List<DefinitionOptionElement<ProjectileDefinition>>();
			foreach (var option in options) {
				// Should this be the localized projectile name?
				if (Lang.GetProjectileName(option.type).Value.IndexOf(chooserFilter.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
					continue;
				string modname = option.definition.mod;
				if (option.type > ProjectileID.Count) {
					modname = ProjectileLoader.GetProjectile(option.type).mod.DisplayName; // or internal name?
				}
				if (modname.IndexOf(chooserFilterMod.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
					continue;
				passed.Add(option);
			}
			return passed;
		}
	}

	internal class ProjectileDefinitionOptionElement : DefinitionOptionElement<ProjectileDefinition>
	{
		public ProjectileDefinitionOptionElement(ProjectileDefinition definition, float scale = .75f) : base(definition, scale) {
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			CalculatedStyle dimensions = base.GetInnerDimensions();
			spriteBatch.Draw(backgroundTexture, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			if (definition != null) {
				int type = unloaded ? ProjectileID.Count : this.type;
				Main.instance.LoadProjectile(type);
				Texture2D projectileTexture = Main.projectileTexture[type];

				int frameCounter = Interface.modConfig.updateCount / 4;
				int frames = Main.projFrames[type];
				if (unloaded) {
					projectileTexture = Main.itemTexture[ItemID.Count];
					frames = 1;
				}
				int height = projectileTexture.Height / frames;
				int width = projectileTexture.Width;
				int frame = frameCounter % frames;
				int y = height * frame;
				var rectangle2 = new Rectangle(0, y, width, height);

				float drawScale = 1f;
				float availableWidth = (float)defaultBackgroundTexture.Width * scale;
				if (width > availableWidth || height > availableWidth) {
					if (width > height) {
						drawScale = availableWidth / width;
					}
					else {
						drawScale = availableWidth / height;
					}
				}
				drawScale *= scale;
				Vector2 vector = backgroundTexture.Size() * scale;
				Vector2 position2 = dimensions.Position() + vector / 2f - rectangle2.Size() * drawScale / 2f;
				Vector2 origin = rectangle2.Size() * 0/* * (pulseScale / 2f - 0.5f)*/;

				spriteBatch.Draw(projectileTexture, position2, rectangle2, Color.White, 0f, origin, drawScale, SpriteEffects.None, 0f);
			}
			if (IsMouseHovering)
				UIModConfig.tooltip = tooltip;
		}
	}
}
