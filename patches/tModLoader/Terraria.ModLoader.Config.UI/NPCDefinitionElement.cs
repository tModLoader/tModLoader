using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI
{
	class NPCDefinitionElement : DefinitionElement<NPCDefinition>
	{
		protected override DefinitionOptionElement<NPCDefinition> CreateDefinitionOptionElement() => new NPCDefinitionOptionElement(Value, 0.5f);

		protected override List<DefinitionOptionElement<NPCDefinition>> CreateDefinitionOptionElementList() {
			optionScale = 0.8f;
			var options = new List<DefinitionOptionElement<NPCDefinition>>();
			for (int i = 0; i < NPCLoader.NPCCount; i++) {
				var optionElement = new NPCDefinitionOptionElement(new NPCDefinition(i), optionScale);
				optionElement.OnClick += (a, b) => {
					Value = optionElement.definition;
					updateNeeded = true;
					selectionExpanded = false;
				};
				options.Add(optionElement);
			}
			return options;
		}

		protected override List<DefinitionOptionElement<NPCDefinition>> GetPassedOptionElements() {
			var passed = new List<DefinitionOptionElement<NPCDefinition>>();
			foreach (var option in options) {
				// Should this be the localized NPC name?
				if (Lang.GetNPCName(option.type).Value.IndexOf(chooserFilter.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
					continue;
				string modname = option.definition.mod;
				if (option.type > NPCID.Count) {
					modname = NPCLoader.GetNPC(option.type).mod.DisplayName; // or internal name?
				}
				if (modname.IndexOf(chooserFilterMod.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
					continue;
				passed.Add(option);
			}
			return passed;
		}
	}

	internal class NPCDefinitionOptionElement : DefinitionOptionElement<NPCDefinition>
	{
		public NPCDefinitionOptionElement(NPCDefinition definition, float scale = .75f) : base(definition, scale) {
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			CalculatedStyle dimensions = base.GetInnerDimensions();
			spriteBatch.Draw(backgroundTexture, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			if (definition != null) {
				int type = unloaded ? NPCID.Count : this.type;
				Main.instance.LoadNPC(type);
				Texture2D npcTexture = Main.npcTexture[type];

				int frameCounter = Interface.modConfig.updateCount / 8;
				int frames = Main.npcFrameCount[type];
				if (unloaded) {
					npcTexture = Main.itemTexture[ItemID.Count];
					frames = 1;
				}
				int height = npcTexture.Height / frames;
				int width = npcTexture.Width;
				int frame = frameCounter % frames;
				int y = height * frame;
				Rectangle rectangle2 = new Rectangle(0, y, width, height);

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
				Vector2 origin = rectangle2.Size() * 0;

				//Color color = (npc.color != new Color(byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue)) ? new Color(npc.color.R, npc.color.G, npc.color.B, 255f) : new Color(1f, 1f, 1f);

				spriteBatch.Draw(npcTexture, position2, rectangle2, Color.White, 0f, origin, drawScale, SpriteEffects.None, 0f);
			}
			if (IsMouseHovering)
				UIModConfig.tooltip = tooltip;
		}
	}
}
